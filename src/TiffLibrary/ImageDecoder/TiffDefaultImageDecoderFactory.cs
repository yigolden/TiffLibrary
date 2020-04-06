using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JpegLibrary;
using TiffLibrary.Compression;
using TiffLibrary.PhotometricInterpreters;

namespace TiffLibrary.ImageDecoder
{
    internal static class TiffDefaultImageDecoderFactory
    {

        public static async Task<TiffImageDecoder> CreateImageDecoderAsync(TiffOperationContext operationContext, ITiffFileContentSource contentSource, TiffImageFileDirectory ifd, TiffImageDecoderOptions? options, CancellationToken cancellationToken)
        {
            TiffFileContentReader reader = await contentSource.OpenReaderAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                return await CreateImageDecoderAsync(operationContext, contentSource, reader, ifd, options, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                await reader.DisposeAsync().ConfigureAwait(false);
            }

            throw new InvalidDataException("Failed to determine offsets to the image data.");
        }

        public static async Task<TiffImageDecoder> CreateImageDecoderAsync(TiffOperationContext operationContext, ITiffFileContentSource contentSource, TiffFileContentReader reader, TiffImageFileDirectory ifd, TiffImageDecoderOptions? options, CancellationToken cancellationToken)
        {
            var fieldReader = new TiffFieldReader(reader, operationContext);
            try
            {
                var tagReader = new TiffTagReader(fieldReader, ifd);

                // Special case for old-style JPEG compression
                TiffCompression compression = await tagReader.ReadCompressionAsync(cancellationToken).ConfigureAwait(false);
                if (compression == TiffCompression.OldJpeg)
                {
                    return await CreateLegacyJpegImageDecoderAsync(tagReader, operationContext, contentSource, reader, options ?? TiffImageDecoderOptions.Default, cancellationToken).ConfigureAwait(false);
                }

                if (!ifd.Contains(TiffTag.PhotometricInterpretation))
                {
                    throw new InvalidDataException("PhotometricInterpretation tag is missing.");
                }
                if (ifd.Contains(TiffTag.TileWidth) && ifd.Contains(TiffTag.TileLength))
                {
                    return await CreateTiledImageDecoderAsync(tagReader, operationContext, contentSource, options ?? TiffImageDecoderOptions.Default, cancellationToken).ConfigureAwait(false);
                }
                if (ifd.Contains(TiffTag.StripOffsets))
                {
                    return await CreateStrippedImageDecoderAsync(tagReader, operationContext, contentSource, options ?? TiffImageDecoderOptions.Default, cancellationToken).ConfigureAwait(false);
                }
            }
            finally
            {
                await fieldReader.DisposeAsync().ConfigureAwait(false);
            }

            throw new InvalidDataException("Failed to determine offsets to the image data.");
        }

        public static async Task<TiffImageDecoder> CreateStrippedImageDecoderAsync(TiffTagReader tagReader, TiffOperationContext operationContext, ITiffFileContentSource contentSource, TiffImageDecoderOptions options, CancellationToken cancellationToken)
        {
            // Basic Informations
            TiffPhotometricInterpretation photometricInterpretation = (await tagReader.ReadPhotometricInterpretationAsync(cancellationToken).ConfigureAwait(false)).GetValueOrDefault();
            TiffCompression compression = await tagReader.ReadCompressionAsync(cancellationToken).ConfigureAwait(false);
            int width = (int)await tagReader.ReadImageWidthAsync(cancellationToken).ConfigureAwait(false);
            int height = (int)await tagReader.ReadImageLengthAsync(cancellationToken).ConfigureAwait(false);
            TiffPlanarConfiguration planarConfiguration = await tagReader.ReadPlanarConfigurationAsync(cancellationToken).ConfigureAwait(false);
            TiffValueCollection<ushort> bitsPerSample = await tagReader.ReadBitsPerSampleAsync(cancellationToken).ConfigureAwait(false);

            // Special case for JPEG
            ITiffImageDecoderMiddleware? jpegBitsExpansionMiddleware = null;
            if (compression == TiffCompression.Jpeg)
            {
                // First, make sure all the componenets have the same bit depth
                if (bitsPerSample.IsEmpty)
                {
                    throw new NotSupportedException("BitsPerSample tag it not specified.");
                }
                ushort firstBitsPerSample = bitsPerSample.GetFirstOrDefault();
                foreach (ushort bits in bitsPerSample)
                {
                    if (bits != firstBitsPerSample)
                    {
                        throw new NotSupportedException("Components have different bits.");
                    }
                }

                // The JpegDecompressionAlgorithm class will output either 8-bit pixels or 16-bit pixels for such images
                // Therefore, we treat it as if it is a 8-bit image or 16-bit image in the following steps.

                if (firstBitsPerSample < 8)
                {
                    var newBitsPerSample = new TiffMutableValueCollection<ushort>(bitsPerSample.Count);
                    for (int i = 0; i < bitsPerSample.Count; i++)
                    {
                        newBitsPerSample[i] = 8;
                    }
                    bitsPerSample = newBitsPerSample.GetReadOnlyView();
                    jpegBitsExpansionMiddleware = new Jpeg8BitSampleExpansionMiddleware(firstBitsPerSample);
                }
                else if (firstBitsPerSample > 8 && firstBitsPerSample < 16)
                {
                    var newBitsPerSample = new TiffMutableValueCollection<ushort>(bitsPerSample.Count);
                    for (int i = 0; i < bitsPerSample.Count; i++)
                    {
                        newBitsPerSample[i] = 16;
                    }
                    bitsPerSample = newBitsPerSample.GetReadOnlyView();
                    jpegBitsExpansionMiddleware = new Jpeg16BitSampleExpansionMiddleware(firstBitsPerSample);
                }
                else if (firstBitsPerSample > 16)
                {
                    throw new NotSupportedException($"JPEG compression does not support {firstBitsPerSample} bits image.");
                }
            }

            // Calculate BytesPerScanline
            TiffValueCollection<int> bytesPerScanline = planarConfiguration == TiffPlanarConfiguration.Planar ? CalculatePlanarBytesPerScanline(photometricInterpretation, bitsPerSample, width) : CalculateChunkyBytesPerScanline(photometricInterpretation, compression, bitsPerSample, width);

            TiffOrientation orientation = default;
            var builder = new TiffImageDecoderPipelineBuilder();

            // Middleware: ApplyOrientation
            if (!options.IgnoreOrientation)
            {
                orientation = await tagReader.ReadOrientationAsync(cancellationToken).ConfigureAwait(false);
                BuildApplyOrientationMiddleware(builder, orientation);
            }

            // Middleware: ImageEnumerator
            await BuildStrippedImageEnumerator(builder, tagReader, compression, height, bytesPerScanline, cancellationToken).ConfigureAwait(false);

            // Middleware: Decompression
            builder.Add(new TiffImageDecompressionMiddleware(photometricInterpretation, bitsPerSample, bytesPerScanline, await ResolveDecompressionAlgorithmAsync(compression, tagReader, cancellationToken).ConfigureAwait(false)));

            // For JPEG: Sample Expansion
            if (!(jpegBitsExpansionMiddleware is null))
            {
                builder.Add(jpegBitsExpansionMiddleware);
            }

            // Middleware: ReverseYCbCrSubsampling
            if (photometricInterpretation == TiffPhotometricInterpretation.YCbCr && compression != TiffCompression.OldJpeg && compression != TiffCompression.Jpeg)
            {
                await BuildReverseYCbCrSubsamlpingMiddleware(builder, planarConfiguration, bitsPerSample, tagReader, cancellationToken).ConfigureAwait(false);
            }

            // Middleware: ReversePredictor
            TiffPredictor prediction = await tagReader.ReadPredictorAsync(cancellationToken);
            if (prediction != TiffPredictor.None)
            {
                builder.Add(new TiffReversePredictorMiddleware(bytesPerScanline, bitsPerSample, prediction));
            }

            // Middleware: Photometric Interpretation
            ITiffImageDecoderMiddleware photometricInterpretationMiddleware = planarConfiguration == TiffPlanarConfiguration.Planar ?
                await ResolvePlanarPhotometricInterpretationMiddlewareAsync(photometricInterpretation, bitsPerSample, tagReader, options, cancellationToken).ConfigureAwait(false) :
                await ResolveChunkyPhotometricInterpretationMiddlewareAsync(photometricInterpretation, compression, bitsPerSample, tagReader, options, cancellationToken).ConfigureAwait(false);
            builder.Add(photometricInterpretationMiddleware);

            var parameters = new TiffImageDecoderParameters()
            {
                MemoryPool = options.MemoryPool ?? MemoryPool<byte>.Shared,
                OperationContext = operationContext,
                ContentSource = contentSource,
                ImageFileDirectory = tagReader.ImageFileDirectory,
                PixelConverterFactory = options.PixelConverterFactory,
                ImageSize = new TiffSize(width, height),
                Orientation = orientation
            };

            return new TiffImageDecoderPipelineAdapter(parameters, builder.Build());
        }

        public static async Task<TiffImageDecoder> CreateTiledImageDecoderAsync(TiffTagReader tagReader, TiffOperationContext operationContext, ITiffFileContentSource contentSource, TiffImageDecoderOptions options, CancellationToken cancellationToken)
        {
            // Basic Informations
            TiffPhotometricInterpretation photometricInterpretation = (await tagReader.ReadPhotometricInterpretationAsync(cancellationToken).ConfigureAwait(false)).GetValueOrDefault();
            TiffCompression compression = await tagReader.ReadCompressionAsync(cancellationToken).ConfigureAwait(false);
            int width = (int)await tagReader.ReadImageWidthAsync(cancellationToken).ConfigureAwait(false);
            int height = (int)await tagReader.ReadImageLengthAsync(cancellationToken).ConfigureAwait(false);
            TiffPlanarConfiguration planarConfiguration = await tagReader.ReadPlanarConfigurationAsync(cancellationToken).ConfigureAwait(false);
            TiffValueCollection<ushort> bitsPerSample = await tagReader.ReadBitsPerSampleAsync(cancellationToken).ConfigureAwait(false);
            uint? tileWidth = await tagReader.ReadTileWidthAsync(cancellationToken).ConfigureAwait(false);
            Debug.Assert(tileWidth.HasValue);

            // Special case for JPEG
            ITiffImageDecoderMiddleware? jpegBitsExpansionMiddleware = null;
            if (compression == TiffCompression.Jpeg)
            {
                // First, make sure all the componenets have the same bit depth
                if (bitsPerSample.IsEmpty)
                {
                    throw new NotSupportedException("BitsPerSample tag it not specified.");
                }
                ushort firstBitsPerSample = bitsPerSample.GetFirstOrDefault();
                foreach (ushort bits in bitsPerSample)
                {
                    if (bits != firstBitsPerSample)
                    {
                        throw new NotSupportedException("Components have different bits.");
                    }
                }

                // The JpegDecompressionAlgorithm class will output either 8-bit pixels or 16-bit pixels for such images
                // Therefore, we treat it as if it is a 8-bit image or 16-bit image in the following steps.

                if (firstBitsPerSample < 8)
                {
                    var newBitsPerSample = new TiffMutableValueCollection<ushort>(bitsPerSample.Count);
                    for (int i = 0; i < bitsPerSample.Count; i++)
                    {
                        newBitsPerSample[i] = firstBitsPerSample;
                    }
                    bitsPerSample = newBitsPerSample.GetReadOnlyView();
                    jpegBitsExpansionMiddleware = new Jpeg8BitSampleExpansionMiddleware(firstBitsPerSample);
                }
                else if (firstBitsPerSample > 8 && firstBitsPerSample < 16)
                {
                    var newBitsPerSample = new TiffMutableValueCollection<ushort>(bitsPerSample.Count);
                    for (int i = 0; i < bitsPerSample.Count; i++)
                    {
                        newBitsPerSample[i] = firstBitsPerSample;
                    }
                    bitsPerSample = newBitsPerSample.GetReadOnlyView();
                    jpegBitsExpansionMiddleware = new Jpeg16BitSampleExpansionMiddleware(firstBitsPerSample);
                }
                else if (firstBitsPerSample > 16)
                {
                    throw new NotSupportedException($"JPEG compression does not support {firstBitsPerSample} bits image.");
                }
            }

            // Calculate BytesPerScanline
            TiffValueCollection<int> bytesPerScanline = planarConfiguration == TiffPlanarConfiguration.Planar ? CalculatePlanarBytesPerScanline(photometricInterpretation, bitsPerSample, (int)tileWidth.GetValueOrDefault()) : CalculateChunkyBytesPerScanline(photometricInterpretation, compression, bitsPerSample, (int)tileWidth.GetValueOrDefault());

            TiffOrientation orientation = default;
            var builder = new TiffImageDecoderPipelineBuilder();

            // Middleware: ApplyOrientation
            if (!options.IgnoreOrientation)
            {
                orientation = await tagReader.ReadOrientationAsync(cancellationToken).ConfigureAwait(false);
                BuildApplyOrientationMiddleware(builder, orientation);
            }

            // Middleware: ImageEnumerator
            await BuildTiledImageEnumerator(builder, tagReader, bytesPerScanline.Count, cancellationToken).ConfigureAwait(false);

            // Middleware: Decompression
            builder.Add(new TiffImageDecompressionMiddleware(photometricInterpretation, bitsPerSample, bytesPerScanline, await ResolveDecompressionAlgorithmAsync(compression, tagReader, cancellationToken).ConfigureAwait(false)));

            // For JPEG: Sample Expansion
            if (!(jpegBitsExpansionMiddleware is null))
            {
                builder.Add(jpegBitsExpansionMiddleware);
            }

            // Middleware: ReverseYCbCrSubsampling
            if (photometricInterpretation == TiffPhotometricInterpretation.YCbCr && compression != TiffCompression.OldJpeg && compression != TiffCompression.Jpeg)
            {
                await BuildReverseYCbCrSubsamlpingMiddleware(builder, planarConfiguration, bitsPerSample, tagReader, cancellationToken).ConfigureAwait(false);
            }

            // Middleware: ReversePredictor
            TiffPredictor prediction = await tagReader.ReadPredictorAsync(cancellationToken);
            if (prediction != TiffPredictor.None)
            {
                builder.Add(new TiffReversePredictorMiddleware(bytesPerScanline, bitsPerSample, prediction));
            }

            // Middleware: Photometric Interpretation
            ITiffImageDecoderMiddleware photometricInterpretationMiddleware = planarConfiguration == TiffPlanarConfiguration.Planar ?
                await ResolvePlanarPhotometricInterpretationMiddlewareAsync(photometricInterpretation, bitsPerSample, tagReader, options, cancellationToken).ConfigureAwait(false) :
                await ResolveChunkyPhotometricInterpretationMiddlewareAsync(photometricInterpretation, compression, bitsPerSample, tagReader, options, cancellationToken).ConfigureAwait(false);
            builder.Add(photometricInterpretationMiddleware);

            var parameters = new TiffImageDecoderParameters()
            {
                MemoryPool = options.MemoryPool ?? MemoryPool<byte>.Shared,
                OperationContext = operationContext,
                ContentSource = contentSource,
                ImageFileDirectory = tagReader.ImageFileDirectory,
                PixelConverterFactory = options.PixelConverterFactory,
                ImageSize = new TiffSize(width, height),
                Orientation = orientation
            };

            return new TiffImageDecoderPipelineAdapter(parameters, builder.Build());
        }

        public static async Task<TiffImageDecoder> CreateLegacyJpegImageDecoderAsync(TiffTagReader tagReader, TiffOperationContext operationContext, ITiffFileContentSource contentSource, TiffFileContentReader contentReader, TiffImageDecoderOptions options, CancellationToken cancellationToken)
        {
            TiffImageFileDirectory ifd = tagReader.ImageFileDirectory;
            TiffPhotometricInterpretation? photometricInterpretation = await tagReader.ReadPhotometricInterpretationAsync(cancellationToken);
            int width = (int)await tagReader.ReadImageWidthAsync(cancellationToken).ConfigureAwait(false);
            int height = (int)await tagReader.ReadImageLengthAsync(cancellationToken).ConfigureAwait(false);
            TiffPlanarConfiguration planarConfiguration = await tagReader.ReadPlanarConfigurationAsync(cancellationToken).ConfigureAwait(false);
            TiffValueCollection<ushort> bitsPerSample = ifd.Contains(TiffTag.BitsPerSample) ? await tagReader.ReadBitsPerSampleAsync(cancellationToken).ConfigureAwait(false) : TiffValueCollection.Empty<ushort>();

            // Validate photometric interpretation tag and planar configuration tag.
            if (photometricInterpretation.HasValue &&
                photometricInterpretation.GetValueOrDefault() != TiffPhotometricInterpretation.WhiteIsZero &&
                photometricInterpretation.GetValueOrDefault() != TiffPhotometricInterpretation.BlackIsZero &&
                photometricInterpretation.GetValueOrDefault() != TiffPhotometricInterpretation.RGB &&
                photometricInterpretation.GetValueOrDefault() != TiffPhotometricInterpretation.YCbCr)
            {
                throw new NotSupportedException("Unsupported photometric interpretation.");
            }
            if (planarConfiguration != TiffPlanarConfiguration.Chunky)
            {
                throw new NotSupportedException("Unsupported planar configuration.");
            }

            // Try find the JPEG stream.
            TiffStreamRegion jpegStream = default;

            if (ifd.Contains(TiffTag.JPEGInterchangeFormat) && ifd.Contains(TiffTag.JPEGInterchangeFormatLength))
            {
                jpegStream = new TiffStreamRegion(await tagReader.ReadJPEGInterchangeFormatAsync(cancellationToken).ConfigureAwait(false), (int)(await tagReader.ReadJPEGInterchangeFormatLengthAsync(cancellationToken).ConfigureAwait(false)));
            }
            else if (!ifd.Contains(TiffTag.JPEGProc))
            {
                TiffImageFileDirectoryEntry offsetEntry = ifd.FindEntry(TiffTag.StripOffsets);
                TiffImageFileDirectoryEntry lengthEntry = ifd.FindEntry(TiffTag.StripByteCounts);
                if (offsetEntry.ValueCount == 1 && lengthEntry.ValueCount == 1)
                {
                    jpegStream = new TiffStreamRegion((long)(await tagReader.ReadStripOffsetsAsync(cancellationToken).ConfigureAwait(false)).GetFirstOrDefault(), (int)(await tagReader.ReadStripByteCountsAsync(cancellationToken).ConfigureAwait(false)).GetFirstOrDefault());
                }
            }

            // Try identity JPEG stream
            MemoryPool<byte> memoryPool = options.MemoryPool ?? MemoryPool<byte>.Shared;
            if (jpegStream.Length > 0)
            {
                // Read JPEG stream.
                const int BufferSize = 81920;
                using var bufferWriter = new MemoryPoolBufferWriter(memoryPool);
                TiffStreamRegion streamRegion = jpegStream;
                do
                {
                    int readSize = Math.Min(streamRegion.Length, BufferSize);
                    Memory<byte> memory = bufferWriter.GetMemory(readSize);
                    readSize = await contentReader.ReadAsync(streamRegion.Offset, memory, cancellationToken).ConfigureAwait(false);
                    bufferWriter.Advance(readSize);
                    streamRegion = new TiffStreamRegion(streamRegion.Offset + readSize, streamRegion.Length - readSize);
                } while (streamRegion.Length > 0);

                // Identify JPEG stream.
                var decoder = new JpegDecoder();
                decoder.SetInput(bufferWriter.GetReadOnlySequence());
                try
                {
                    decoder.Identify();

                    if (decoder.Precision != 8)
                    {
                        throw new NotSupportedException("Only 8-bit JPEG is supported.");
                    }

                    // Try deduce photometric interpretation
                    if (!photometricInterpretation.HasValue)
                    {
                        switch (decoder.NumberOfComponents)
                        {
                            case 1:
                                photometricInterpretation = TiffPhotometricInterpretation.BlackIsZero;
                                break;
                            case 3:
                                photometricInterpretation = TiffPhotometricInterpretation.YCbCr;
                                break;
                            case 4:
                                photometricInterpretation = TiffPhotometricInterpretation.Seperated;
                                break;
                            default:
                                throw new NotSupportedException("Unsupported photometric interpretation.");
                        }
                    }

                    // Reconstruct BitsPerSample tag.
                    if (bitsPerSample.IsEmpty)
                    {
                        //decoder.NumberOfComponents
                        ushort[] bitsPerSampleArray = new ushort[decoder.NumberOfComponents];
                        for (int i = 0; i < bitsPerSampleArray.Length; i++)
                        {
                            bitsPerSampleArray[i] = 8;
                        }
                        bitsPerSample = TiffValueCollection.UnsafeWrap(bitsPerSampleArray);
                    }
                    else
                    {
                        if (bitsPerSample.Count != decoder.NumberOfComponents)
                        {
                            throw new NotSupportedException("Bits per sample does not match.");
                        }
                    }

                    // Reconstruct ImageWidth and ImageLength tag.
                    if (width == 0)
                    {
                        width = decoder.Width;
                    }
                    if (height == 0)
                    {
                        height = decoder.Height;
                    }
                }
                catch (InvalidDataException)
                {
                    // Reset jpegStream to indicate we can not deduce the location of JPEG stream.
                    jpegStream = default;
                }
            }

            // Validate photometric interpretation
            if (!photometricInterpretation.HasValue)
            {
                throw new NotSupportedException("Unsupported photometric interpretation.");
            }

            // Validate width and height.
            if (width == 0 || height == 0)
            {
                throw new NotSupportedException("Can not determine image size.");
            }

            // Validate BitsPerSample tag.
            foreach (ushort item in bitsPerSample)
            {
                if (item != 8)
                {
                    throw new NotSupportedException("Only 8-bit JPEG is supported.");
                }
            }

            TiffOrientation orientation = default;
            var builder = new TiffImageDecoderPipelineBuilder();

            // Middleware: ApplyOrientation
            if (!options.IgnoreOrientation)
            {
                orientation = await tagReader.ReadOrientationAsync(cancellationToken).ConfigureAwait(false);
                BuildApplyOrientationMiddleware(builder, orientation);
            }

            if (jpegStream.Length > 0)
            {
                // Middleware: Directly extract pixel data from JPEG stream.
                builder.Add(new LegacyJpegStreamDecoder(jpegStream));
            }
            else
            {
                // Calculate BytesPerScanline
                TiffValueCollection<int> bytesPerScanline = CalculateChunkyBytesPerScanline(photometricInterpretation.GetValueOrDefault(), TiffCompression.OldJpeg, bitsPerSample, width);

                // Middleware: ImageEnumerator
                if (ifd.Contains(TiffTag.StripOffsets))
                {
                    await BuildStrippedImageEnumerator(builder, tagReader, TiffCompression.OldJpeg, height, bytesPerScanline, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await BuildTiledImageEnumerator(builder, tagReader, bytesPerScanline.Count, cancellationToken).ConfigureAwait(false);
                }


                // Middleware: Decompression
                builder.Add(new TiffImageDecompressionMiddleware(photometricInterpretation.GetValueOrDefault(), bitsPerSample, bytesPerScanline, await ResolveLegacyJpegDecompressionAlgorithmAsync(tagReader, contentReader, memoryPool, cancellationToken).ConfigureAwait(false)));
            }

            // Middleware: Photometric Interpretation
            ITiffImageDecoderMiddleware photometricInterpretationMiddleware = await ResolveChunkyPhotometricInterpretationMiddlewareAsync(photometricInterpretation.GetValueOrDefault(), TiffCompression.OldJpeg, bitsPerSample, tagReader, options, cancellationToken).ConfigureAwait(false);
            builder.Add(photometricInterpretationMiddleware);

            var parameters = new TiffImageDecoderParameters
            {
                MemoryPool = memoryPool,
                OperationContext = operationContext,
                ContentSource = contentSource,
                ImageFileDirectory = ifd,
                PixelConverterFactory = options.PixelConverterFactory,
                ImageSize = new TiffSize(width, height),
                Orientation = orientation
            };

            return new TiffImageDecoderPipelineAdapter(parameters, builder.Build());
        }

        private static async Task BuildStrippedImageEnumerator(TiffImageDecoderPipelineBuilder builder, TiffTagReader tagReader, TiffCompression compression, int height, TiffValueCollection<int> bytesPerScanline, CancellationToken cancellationToken)
        {
            // Strip data
            int rowsPerStrip = (int)(await tagReader.ReadRowsPerStripAsync(cancellationToken).ConfigureAwait(false));
            TiffValueCollection<ulong> stripOffsets = await tagReader.ReadStripOffsetsAsync(cancellationToken).ConfigureAwait(false);
            TiffValueCollection<ulong> stripsByteCount = await tagReader.ReadStripByteCountsAsync(cancellationToken).ConfigureAwait(false);
            int stripCount = stripOffsets.Count;
            if (stripCount == 0)
            {
                throw new InvalidDataException();
            }

            TiffValueCollection<int> convertedStripsByteCount;

            // Special case for mailformed file.
            if (stripCount == 1 && rowsPerStrip == 0)
            {
                rowsPerStrip = height;
            }
            if (stripCount != stripsByteCount.Count)
            {
                // Special case for mailformed file.
                convertedStripsByteCount = InferStripsByteCount();
                if (stripCount != convertedStripsByteCount.Count)
                {
                    throw new InvalidDataException();
                }
            }
            else
            {
                convertedStripsByteCount = stripsByteCount.ConvertAll(v => (int)v);
            }

            builder.Add(new TiffStrippedImageDecoderEnumeratorMiddleware(rowsPerStrip, stripOffsets.ConvertAll(v => (long)v), convertedStripsByteCount, bytesPerScanline.Count));

            TiffValueCollection<int> InferStripsByteCount()
            {
                if (compression != TiffCompression.NoCompression)
                {
                    return default;
                }

                int substripCount = bytesPerScanline.Count;
                int actualStripCount = stripOffsets.Count / substripCount;

                // Infer strips byte count
                int[] stripsByteCount = new int[stripOffsets.Count];
                for (int substripIndex = 0; substripIndex < substripCount; substripIndex++)
                {
                    int bytes = bytesPerScanline[substripIndex];
                    for (int stripIndex = 0; stripIndex < actualStripCount; stripIndex++)
                    {
                        int accessIndex = substripIndex * actualStripCount + stripIndex;
                        int imageHeight = Math.Min(rowsPerStrip, height - stripIndex * rowsPerStrip);
                        stripsByteCount[accessIndex] = imageHeight * bytes;
                    }
                }

                return TiffValueCollection.UnsafeWrap(stripsByteCount);
            }
        }

        private static async Task BuildTiledImageEnumerator(TiffImageDecoderPipelineBuilder builder, TiffTagReader tagReader, int planeCount, CancellationToken cancellationToken)
        {
            // Read Tile Size 
            uint? tileWidth = await tagReader.ReadTileWidthAsync(cancellationToken).ConfigureAwait(false);
            uint? tileHeight = await tagReader.ReadTileLengthAsync(cancellationToken).ConfigureAwait(false);
            // TileWidth and TileHeight can not be null because they were checked in TiffFileReader.InternalCreateImageDecoderInstance
            Debug.Assert(tileWidth != null && tileHeight != null);

            // Read Tile Offsets
            TiffValueCollection<ulong> tileOffsets = await tagReader.ReadTileOffsetsAsync(cancellationToken).ConfigureAwait(false);
            TiffValueCollection<ulong> tileByteCounts = await tagReader.ReadTileByteCountsAsync(cancellationToken).ConfigureAwait(false);
            if (tileOffsets.IsEmpty || tileByteCounts.IsEmpty)
            {
                // Fallback to using StripOffsets and StripByteCounts
                tileOffsets = await tagReader.ReadStripOffsetsAsync(cancellationToken).ConfigureAwait(false);
                tileByteCounts = await tagReader.ReadStripByteCountsAsync(cancellationToken).ConfigureAwait(false);
            }

            // Validate
            if (tileOffsets.Count == 0 || tileOffsets.Count != tileByteCounts.Count)
            {
                throw new InvalidDataException();
            }
            if (tileWidth % 16 != 0 || tileHeight % 16 != 0)
            {
                throw new InvalidDataException();
            }

            builder.Add(new TiffTiledImageDecoderEnumeratorMiddleware((int)tileWidth.GetValueOrDefault(), (int)tileHeight.GetValueOrDefault(), tileOffsets.ConvertAll(v => (long)v), tileByteCounts.ConvertAll(v => (int)v), planeCount));
        }

        private static void BuildApplyOrientationMiddleware(TiffImageDecoderPipelineBuilder builder, TiffOrientation orientation)
        {
            if (orientation != 0 && orientation != TiffOrientation.TopLeft)
            {
                builder.Add(new TiffReverseOrientationMiddleware(orientation));
            }
        }

        private static async Task BuildReverseYCbCrSubsamlpingMiddleware(TiffImageDecoderPipelineBuilder builder, TiffPlanarConfiguration planarConfiguration, TiffValueCollection<ushort> bitsPerSample, TiffTagReader tagReader, CancellationToken cancellationToken)
        {
            ushort[] subsampling = await tagReader.ReadYCbCrSubSamplingAsync(cancellationToken).ConfigureAwait(false);
            if (subsampling.Length == 0)
            {
                return;
            }
            if (subsampling.Length != 2)
            {
                throw new InvalidDataException("YCbCrSubSampling should contains 2 elements.");
            }
            if (bitsPerSample.GetFirstOrDefault() == 8)
            {
                builder.Add(new TiffReverseChromaSubsampling8Middleware(subsampling[0], subsampling[1], planarConfiguration == TiffPlanarConfiguration.Planar));
            }
            else if (bitsPerSample.GetFirstOrDefault() == 16)
            {
                builder.Add(new TiffReverseChromaSubsampling16Middleware(subsampling[0], subsampling[1], planarConfiguration == TiffPlanarConfiguration.Planar));
            }
            else
            {
                throw new NotSupportedException("Unsupported bits per sample.");
            }
        }

        private static TiffValueCollection<int> CalculateChunkyBytesPerScanline(TiffPhotometricInterpretation photometricInterpretation, TiffCompression compression, TiffValueCollection<ushort> bitsPerSample, int width)
        {
            if (compression == TiffCompression.ModifiedHuffmanCompression || compression == TiffCompression.T4Encoding || compression == TiffCompression.T6Encoding)
            {
                return TiffValueCollection.Single(width);
            }

            switch (photometricInterpretation)
            {
                case TiffPhotometricInterpretation.WhiteIsZero:
                case TiffPhotometricInterpretation.BlackIsZero:
                    if (bitsPerSample.GetFirstOrDefault() <= 32)
                    {
                        return TiffValueCollection.Single((bitsPerSample.GetFirstOrDefault() * width + 7) / 8);
                    }
                    break;
                case TiffPhotometricInterpretation.RGB:
                    if (bitsPerSample.Count == 3)
                    {
                        if (bitsPerSample[0] <= 32 && bitsPerSample[1] <= 32 && bitsPerSample[2] <= 32)
                        {
                            return TiffValueCollection.Single((width * (bitsPerSample[0] + bitsPerSample[1] + bitsPerSample[2]) + 7) / 8);
                        }
                    }
                    else if (bitsPerSample.Count == 4)
                    {
                        if (bitsPerSample[0] <= 32 && bitsPerSample[1] <= 32 && bitsPerSample[2] <= 32 && bitsPerSample[3] <= 32)
                        {
                            return TiffValueCollection.Single((width * (bitsPerSample[0] + bitsPerSample[1] + bitsPerSample[2] + bitsPerSample[3]) + 7) / 8);
                        }
                    }
                    break;
                case TiffPhotometricInterpretation.PaletteColor:
                    if (bitsPerSample.GetFirstOrDefault() <= 8)
                    {
                        return TiffValueCollection.Single((bitsPerSample.GetFirstOrDefault() * width + 7) / 8);
                    }
                    break;
                case TiffPhotometricInterpretation.TransparencyMask:
                    if (bitsPerSample[0] == 1)
                    {
                        return TiffValueCollection.Single((width + 7) / 8);
                    }
                    break;
                case TiffPhotometricInterpretation.Seperated:
                    if (bitsPerSample.Count == 4)
                    {
                        if (bitsPerSample[0] == 8 && bitsPerSample[1] == 8 && bitsPerSample[2] == 8 && bitsPerSample[3] == 8)
                            return TiffValueCollection.Single(4 * width);
                        if (bitsPerSample[0] == 16 && bitsPerSample[1] == 16 && bitsPerSample[2] == 16 && bitsPerSample[3] == 16)
                            return TiffValueCollection.Single(8 * width);
                    }
                    break;
                case TiffPhotometricInterpretation.YCbCr:
                    if (bitsPerSample.Count == 3)
                    {
                        if (bitsPerSample[0] == 8 && bitsPerSample[1] == 8 && bitsPerSample[2] == 8)
                            return TiffValueCollection.Single(3 * width);
                        if (bitsPerSample[0] == 16 && bitsPerSample[1] == 16 && bitsPerSample[2] == 16)
                            return TiffValueCollection.Single(6 * width);
                    }
                    break;
            }
            throw new NotSupportedException("Photometric interpretation not supported.");
        }

        private static TiffValueCollection<int> CalculatePlanarBytesPerScanline(TiffPhotometricInterpretation photometricInterpretation, TiffValueCollection<ushort> bitsPerSample, int width)
        {
            switch (photometricInterpretation)
            {
                case TiffPhotometricInterpretation.RGB:
                    if (bitsPerSample.Count == 3)
                    {
                        if (bitsPerSample[0] <= 32 && bitsPerSample[1] <= 32 && bitsPerSample[2] <= 32)
                        {
                            return TiffValueCollection.UnsafeWrap(new int[] { (width * bitsPerSample[0] + 7) / 8, (width * bitsPerSample[1] + 7) / 8, (width * bitsPerSample[2] + 7) / 8 });
                        }
                    }
                    else if (bitsPerSample.Count == 4)
                    {
                        if (bitsPerSample[0] <= 32 && bitsPerSample[1] <= 32 && bitsPerSample[2] <= 32 && bitsPerSample[3] <= 32)
                        {
                            return TiffValueCollection.UnsafeWrap(new int[] { (width * bitsPerSample[0] + 7) / 8, (width * bitsPerSample[1] + 7) / 8, (width * bitsPerSample[2] + 7) / 8, (width * bitsPerSample[3] + 7) / 8 });
                        }
                    }
                    break;
                case TiffPhotometricInterpretation.Seperated:
                    if (bitsPerSample.Count == 4)
                    {
                        if (bitsPerSample[0] == 8 && bitsPerSample[1] == 8 && bitsPerSample[2] == 8 && bitsPerSample[3] == 8)
                            return TiffValueCollection.UnsafeWrap(new int[] { width, width, width, width });
                        if (bitsPerSample[0] == 16 && bitsPerSample[1] == 16 && bitsPerSample[2] == 16 && bitsPerSample[3] == 16)
                            return TiffValueCollection.UnsafeWrap(new int[] { 2 * width, 2 * width, 2 * width, 2 * width });
                    }
                    break;
                case TiffPhotometricInterpretation.YCbCr:
                    if (bitsPerSample.Count == 3)
                    {
                        if (bitsPerSample[0] == 8 && bitsPerSample[1] == 8 && bitsPerSample[2] == 8)
                            return TiffValueCollection.UnsafeWrap(new int[] { width, width, width });
                        if (bitsPerSample[0] == 16 && bitsPerSample[1] == 16 && bitsPerSample[2] == 16)
                            return TiffValueCollection.UnsafeWrap(new int[] { 2 * width, 2 * width, 2 * width });
                    }
                    break;
            }
            throw new NotSupportedException("Photometric interpretation not supported.");
        }

        private static ValueTask<ITiffDecompressionAlgorithm> ResolveDecompressionAlgorithmAsync(TiffCompression compression, TiffTagReader tagReader, CancellationToken cancellationToken)
        {
            ITiffDecompressionAlgorithm decompressionAlgorithm;

            // Select a decompression algorithm
            switch (compression)
            {
                case TiffCompression.NoCompression:
                    decompressionAlgorithm = NoneCompressionAlgorithm.Instance;
                    break;
                case TiffCompression.ModifiedHuffmanCompression:
                    return ResolveModifiedHuffmanDecompressionAlgorithmAsync(tagReader, cancellationToken);
                case TiffCompression.T4Encoding:
                    return ResolveT4DecompressionAlgorithmAsync(tagReader, cancellationToken);
                case TiffCompression.T6Encoding:
                    return ResolveT6DecompressionAlgorithmAsync(tagReader, cancellationToken);
                case TiffCompression.Lzw:
                    decompressionAlgorithm = LzwCompressionAlgorithm.Instance;
                    break;
                case TiffCompression.Deflate:
                case TiffCompression.OldDeflate:
                    decompressionAlgorithm = DeflateCompressionAlgorithm.Instance;
                    break;
                case TiffCompression.PackBits:
                    decompressionAlgorithm = PackBitsCompressionAlgorithm.Instance;
                    break;
                case TiffCompression.Jpeg:
                    return ResolveJpegDecompressionAlgorithmAsync(tagReader, cancellationToken);
                case TiffCompression.ThunderScan:
                    decompressionAlgorithm = ThunderScanCompressionAlgorithm.Instance;
                    break;
                case TiffCompression.NeXT:
                    decompressionAlgorithm = NeXTCompressionAlgorithm.Instance;
                    break;
                default:
                    throw new NotSupportedException("Compression algorithm not supported.");
            }

            return new ValueTask<ITiffDecompressionAlgorithm>(decompressionAlgorithm);

            async ValueTask<ITiffDecompressionAlgorithm> ResolveModifiedHuffmanDecompressionAlgorithmAsync(TiffTagReader tagReader, CancellationToken cancellationToken)
            {
                TiffFillOrder fillOrder = await tagReader.ReadFillOrderAsync(cancellationToken).ConfigureAwait(false);
                return ModifiedHuffmanCompressionAlgorithm.GetSharedInstance(fillOrder);
            }

            static async ValueTask<ITiffDecompressionAlgorithm> ResolveT4DecompressionAlgorithmAsync(TiffTagReader tagReader, CancellationToken cancellationToken)
            {
                TiffFillOrder fillOrder = await tagReader.ReadFillOrderAsync(cancellationToken).ConfigureAwait(false);
                TiffT4Options t4Options = await tagReader.ReadT4OptionsAsync(cancellationToken).ConfigureAwait(false);
                if (t4Options.HasFlag(TiffT4Options.UseUncompressedMode))
                {
                    throw new NotSupportedException("Uncompressed mode is not supported.");
                }
                if (t4Options.HasFlag(TiffT4Options.Is2DimensionalCoding))
                {
                    return CcittGroup3TwoDimensionalCompressionAlgorithm.GetSharedInstance(fillOrder);
                }
                return CcittGroup3OneDimensionalCompressionAlgorithm.GetSharedInstance(fillOrder);
            }

            static async ValueTask<ITiffDecompressionAlgorithm> ResolveT6DecompressionAlgorithmAsync(TiffTagReader tagReader, CancellationToken cancellationToken)
            {
                TiffFillOrder fillOrder = await tagReader.ReadFillOrderAsync(cancellationToken).ConfigureAwait(false);
                TiffT6Options t6Options = await tagReader.ReadT6OptionsAsync(cancellationToken).ConfigureAwait(false);
                if (t6Options.HasFlag(TiffT6Options.AllowUncompressedMode))
                {
                    throw new NotSupportedException("Uncompressed mode is not supported.");
                }
                return CcittGroup4Compression.GetSharedInstance(fillOrder);
            }

            static async ValueTask<ITiffDecompressionAlgorithm> ResolveJpegDecompressionAlgorithmAsync(TiffTagReader tagReader, CancellationToken cancellationToken)
            {
                byte[] jpegTables = await tagReader.ReadJPEGTablesAsync(cancellationToken).ConfigureAwait(false);
                return new JpegDecompressionAlgorithm(jpegTables, 3);
            }
        }

        private static async ValueTask<ITiffDecompressionAlgorithm> ResolveLegacyJpegDecompressionAlgorithmAsync(TiffTagReader tagReader, TiffFileContentReader reader, MemoryPool<byte> memoryPool, CancellationToken cancellationToken)
        {
            ushort jpegProc = await tagReader.ReadJPEGProcAsync(cancellationToken).ConfigureAwait(false);
            if (jpegProc != 1)
            {
                throw new NotSupportedException("Only baseline JPEG is supported.");
            }

            // JPEG decoding parameters
            ushort restartInterval = await tagReader.ReadJPEGRestartIntervalAsync(cancellationToken).ConfigureAwait(false);
            ushort[] subsampling = await tagReader.ReadYCbCrSubSamplingAsync(cancellationToken).ConfigureAwait(false);
            uint[] jpegQTablesOffset = await tagReader.ReadJPEGQTablesAsync(cancellationToken).ConfigureAwait(false);
            uint[] jpegDcTablesOffset = await tagReader.ReadJPEGDCTablesAsync(cancellationToken).ConfigureAwait(false);
            uint[] jpegAcTablesOffset = await tagReader.ReadJPEGACTablesAsync(cancellationToken).ConfigureAwait(false);

            // Validate these parameters.
            int componentCount = jpegQTablesOffset.Length;
            if (componentCount == 0)
            {
                throw new InvalidDataException("Invalid JPEG quantization tables.");
            }
            if (jpegDcTablesOffset.Length != componentCount)
            {
                throw new InvalidDataException("Invalid JPEG DC Tables.");
            }
            if (jpegAcTablesOffset.Length != componentCount)
            {
                throw new InvalidDataException("Invalid JPEG AC Tables.");
            }

            // Initialize the decompression algorithm instance.
            int readCount;
            var algorithm = new LegacyJpegDecompressionAlgorithm(componentCount);
            using (IMemoryOwner<byte> bufferHandle = memoryPool.Rent(369)) // 64+(16+17)+(16+256)
            {
                Memory<byte> buffer = bufferHandle.Memory;
                for (int i = 0; i < componentCount; i++)
                {
                    buffer.Span.Clear();
                    // Quantization Tablse
                    readCount = await reader.ReadAsync(jpegQTablesOffset[i], buffer.Slice(0, 64), cancellationToken).ConfigureAwait(false);
                    if (readCount != 64)
                    {
                        throw new InvalidDataException("Corrupted quantization table is encountered.");
                    }
                    // DC Tables
                    readCount = await reader.ReadAsync(jpegDcTablesOffset[i], buffer.Slice(64, 33), cancellationToken).ConfigureAwait(false);
                    if (readCount < 16)
                    {
                        throw new InvalidDataException("Corrupted DC table is encountered.");
                    }
                    // AC Tables
                    readCount = await reader.ReadAsync(jpegAcTablesOffset[i], buffer.Slice(97, 272), cancellationToken).ConfigureAwait(false);
                    if (readCount < 16)
                    {
                        throw new InvalidDataException("Corrupted AC table is encountered.");
                    }
                    // Initialize this component
                    algorithm.SetComponent(i, buffer.Span.Slice(0, 64), buffer.Span.Slice(64, 33), buffer.Span.Slice(97, 272));
                }
            }

            algorithm.Initialize(restartInterval, subsampling);
            return algorithm;
        }

        private static ValueTask<ITiffImageDecoderMiddleware> ResolveChunkyPhotometricInterpretationMiddlewareAsync(TiffPhotometricInterpretation photometricInterpretation, TiffCompression compression, TiffValueCollection<ushort> bitsPerSample, TiffTagReader tagReader, TiffImageDecoderOptions options, CancellationToken cancellationToken)
        {
            if (compression == TiffCompression.ModifiedHuffmanCompression || compression == TiffCompression.T4Encoding || compression == TiffCompression.T6Encoding)
            {
                switch (photometricInterpretation)
                {
                    case TiffPhotometricInterpretation.WhiteIsZero:
                        if (bitsPerSample.Count == 1 && bitsPerSample.GetFirstOrDefault() == 1)
                        {
                            return new ValueTask<ITiffImageDecoderMiddleware>(TiffWhiteIsZero8Interpreter.Instance);
                        }
                        break;
                    case TiffPhotometricInterpretation.BlackIsZero:
                        if (bitsPerSample.Count == 1 && bitsPerSample.GetFirstOrDefault() == 1)
                        {
                            return new ValueTask<ITiffImageDecoderMiddleware>(TiffBlackIsZero8Interpreter.Instance);
                        }
                        break;
                }
                throw new NotSupportedException(compression.ToString() + " compression does not support this photometric interpretation.");
            }

            switch (photometricInterpretation)
            {
                case TiffPhotometricInterpretation.WhiteIsZero:
                    if (bitsPerSample.Count == 1)
                    {
                        return ResolveWhiteIsZeroAsync(bitsPerSample.GetFirstOrDefault(), tagReader, cancellationToken);
                    }
                    break;
                case TiffPhotometricInterpretation.BlackIsZero:
                    if (bitsPerSample.Count == 1)
                    {
                        return ResolveBlackIsZeroAsync(bitsPerSample.GetFirstOrDefault(), tagReader, cancellationToken);
                    }
                    break;
                case TiffPhotometricInterpretation.RGB:
                    if (bitsPerSample.Count == 3)
                    {
                        return ResolveRgbAsync(bitsPerSample, tagReader, cancellationToken);
                    }
                    if (bitsPerSample.Count == 4)
                    {
                        return ResolveRgbaAsync(bitsPerSample, tagReader, options, cancellationToken);
                    }
                    break;
                case TiffPhotometricInterpretation.PaletteColor:
                    if (bitsPerSample.Count == 1)
                    {
                        return ResolvePaletteColorAsync(bitsPerSample.GetFirstOrDefault(), tagReader, cancellationToken);
                    }
                    break;
                case TiffPhotometricInterpretation.TransparencyMask:
                    if (bitsPerSample.Count == 1 && bitsPerSample.GetFirstOrDefault() == 1)
                    {
                        return ResolveTransparencyMaskAsync(tagReader, cancellationToken);
                    }
                    break;
                case TiffPhotometricInterpretation.Seperated:
                    if (bitsPerSample.Count == 4)
                    {
                        return ResolveCmykAsync(bitsPerSample, tagReader, cancellationToken);
                    }
                    break;
                case TiffPhotometricInterpretation.YCbCr:
                    if (bitsPerSample.Count == 3)
                    {
                        return ResolveYCbCrAsync(bitsPerSample, tagReader, cancellationToken);
                    }
                    break;
            }

            throw new NotSupportedException("Photometric interpretation not supported.");

            static async ValueTask<ITiffImageDecoderMiddleware> ResolveWhiteIsZeroAsync(int bitCount, TiffTagReader tagReader, CancellationToken cancellationToken)
            {
                TiffFillOrder fillOrder = await tagReader.ReadFillOrderAsync(cancellationToken).ConfigureAwait(false);
                if (bitCount == 1)
                {
                    return new TiffWhiteIsZero1Interpreter(fillOrder);
                }

                if (fillOrder == 0 || fillOrder == TiffFillOrder.HigherOrderBitsFirst)
                {
                    switch (bitCount)
                    {
                        case 4:
                            return TiffWhiteIsZero4Interpreter.Instance;
                        case 8:
                            return TiffWhiteIsZero8Interpreter.Instance;
                        case 16:
                            return TiffWhiteIsZero16Interpreter.Instance;
                    }
                }

                if (bitCount <= 8)
                {
                    return new TiffWhiteIsZeroAny8Interpreter(bitCount, fillOrder);
                }
                if (bitCount <= 16)
                {
                    return new TiffWhiteIsZeroAny16Interpreter(bitCount, fillOrder);
                }
                if (bitCount <= 32)
                {
                    return new TiffWhiteIsZeroAny32Interpreter(bitCount, fillOrder);
                }

                throw new NotSupportedException("Photometric interpretation not supported.");
            }
            static async ValueTask<ITiffImageDecoderMiddleware> ResolveBlackIsZeroAsync(int bitCount, TiffTagReader tagReader, CancellationToken cancellationToken)
            {
                TiffFillOrder fillOrder = await tagReader.ReadFillOrderAsync(cancellationToken).ConfigureAwait(false);
                if (bitCount == 1)
                {
                    return new TiffBlackIsZero1Interpreter(fillOrder);
                }

                if (fillOrder == 0 || fillOrder == TiffFillOrder.HigherOrderBitsFirst)
                {
                    switch (bitCount)
                    {
                        case 4:
                            return TiffBlackIsZero4Interpreter.Instance;
                        case 8:
                            return TiffBlackIsZero8Interpreter.Instance;
                        case 16:
                            return TiffBlackIsZero16Interpreter.Instance;
                    }
                }

                if (bitCount <= 8)
                {
                    return new TiffBlackIsZeroAny8Interpreter(bitCount, fillOrder);
                }
                if (bitCount <= 16)
                {
                    return new TiffBlackIsZeroAny16Interpreter(bitCount, fillOrder);
                }
                if (bitCount <= 32)
                {
                    return new TiffBlackIsZeroAny32Interpreter(bitCount, fillOrder);
                }

                throw new NotSupportedException("Photometric interpretation not supported.");
            }

            static async ValueTask<ITiffImageDecoderMiddleware> ResolveRgbAsync(TiffValueCollection<ushort> bitsPerSample, TiffTagReader tagReader, CancellationToken cancellationToken)
            {
                Debug.Assert(bitsPerSample.Count == 3);
                TiffFillOrder fillOrder = await tagReader.ReadFillOrderAsync(cancellationToken).ConfigureAwait(false);

                if (fillOrder == 0 || fillOrder == TiffFillOrder.HigherOrderBitsFirst)
                {
                    if (bitsPerSample[0] == 8 && bitsPerSample[1] == 8 && bitsPerSample[2] == 8)
                    {
                        return TiffChunkyRgb888Interpreter.Instance;
                    }
                    if (bitsPerSample[0] == 16 && bitsPerSample[1] == 16 && bitsPerSample[2] == 16)
                    {
                        return TiffChunkyRgb161616Interpreter.Instance;
                    }
                }

                if (bitsPerSample[0] <= 8 && bitsPerSample[1] <= 8 && bitsPerSample[2] <= 8)
                {
                    return new TiffChunkyRgbAny888Interpreter(bitsPerSample, fillOrder);
                }
                if (bitsPerSample[0] <= 16 && bitsPerSample[1] <= 16 && bitsPerSample[2] <= 16)
                {
                    return new TiffChunkyRgbAny161616Interpreter(bitsPerSample, fillOrder);
                }
                if (bitsPerSample[0] <= 32 && bitsPerSample[1] <= 32 && bitsPerSample[2] <= 32)
                {
                    return new TiffChunkyRgbAny323232Interpreter(bitsPerSample, fillOrder);
                }

                throw new NotSupportedException("Photometric interpretation not supported.");
            }
            static async ValueTask<ITiffImageDecoderMiddleware> ResolveRgbaAsync(TiffValueCollection<ushort> bitsPerSample, TiffTagReader tagReader, TiffImageDecoderOptions options, CancellationToken cancellationToken)
            {
                Debug.Assert(bitsPerSample.Count == 4);
                TiffValueCollection<TiffExtraSample> extraSamples = await tagReader.ReadExtraSamplesAsync(cancellationToken).ConfigureAwait(false);
                TiffFillOrder fillOrder = await tagReader.ReadFillOrderAsync(cancellationToken).ConfigureAwait(false);
                if (extraSamples.Count == 1 && (extraSamples[0] == TiffExtraSample.AssociatedAlphaData || extraSamples[0] == TiffExtraSample.UnassociatedAlphaData))
                {
                    if (fillOrder == 0 || fillOrder == TiffFillOrder.HigherOrderBitsFirst)
                    {
                        if (bitsPerSample[0] == 8 && bitsPerSample[1] == 8 && bitsPerSample[2] == 8 && bitsPerSample[3] == 8)
                        {
                            return new TiffChunkyRgba8888Interpreter(isAlphaAssociated: extraSamples[0] == TiffExtraSample.AssociatedAlphaData, options.UndoColorPreMultiplying);
                        }
                        if (bitsPerSample[0] == 16 && bitsPerSample[1] == 16 && bitsPerSample[2] == 16 && bitsPerSample[3] == 16)
                        {
                            return new TiffChunkyRgba16161616Interpreter(isAlphaAssociated: extraSamples[0] == TiffExtraSample.AssociatedAlphaData, options.UndoColorPreMultiplying);
                        }
                    }

                    if (bitsPerSample[0] <= 8 && bitsPerSample[1] <= 8 && bitsPerSample[2] <= 8 && bitsPerSample[3] <= 8)
                    {
                        return new TiffChunkyRgbaAny8888Interpreter(extraSamples[0] == TiffExtraSample.AssociatedAlphaData, options.UndoColorPreMultiplying, bitsPerSample, fillOrder);
                    }
                    if (bitsPerSample[0] <= 16 && bitsPerSample[1] <= 16 && bitsPerSample[2] <= 16 && bitsPerSample[3] <= 16)
                    {
                        return new TiffChunkyRgbaAny16161616Interpreter(extraSamples[0] == TiffExtraSample.AssociatedAlphaData, options.UndoColorPreMultiplying, bitsPerSample, fillOrder);
                    }
                    if (bitsPerSample[0] <= 32 && bitsPerSample[1] <= 32 && bitsPerSample[2] <= 32 && bitsPerSample[3] <= 32)
                    {
                        return new TiffChunkyRgbaAny32323232Interpreter(extraSamples[0] == TiffExtraSample.AssociatedAlphaData, options.UndoColorPreMultiplying, bitsPerSample, fillOrder);
                    }
                }
                throw new NotSupportedException("Photometric interpretation not supported.");
            }
            static async ValueTask<ITiffImageDecoderMiddleware> ResolvePaletteColorAsync(int bitCount, TiffTagReader tagReader, CancellationToken cancellationToken)
            {
                TiffFillOrder fillOrder = await tagReader.ReadFillOrderAsync(cancellationToken).ConfigureAwait(false);
                if (fillOrder == 0 || fillOrder == TiffFillOrder.HigherOrderBitsFirst)
                {
                    switch (bitCount)
                    {
                        case 4:
                            return new TiffPaletteColor4Interpreter(await tagReader.ReadColorMapAsync(cancellationToken).ConfigureAwait(false));
                        case 8:
                            return new TiffPaletteColor8Interpreter(await tagReader.ReadColorMapAsync(cancellationToken).ConfigureAwait(false));
                    }
                }

                if (bitCount <= 8)
                {
                    return new TiffPaletteColorAny8Interpreter(await tagReader.ReadColorMapAsync(cancellationToken).ConfigureAwait(false), bitCount, fillOrder);
                }

                throw new NotSupportedException("Photometric interpretation not supported.");
            }
            static async ValueTask<ITiffImageDecoderMiddleware> ResolveTransparencyMaskAsync(TiffTagReader tagReader, CancellationToken cancellationToken)
            {
                TiffFillOrder fillOrder = await tagReader.ReadFillOrderAsync(cancellationToken).ConfigureAwait(false);
                return new TiffTransparencyMask1Interpreter(fillOrder);
            }
            static async ValueTask<ITiffImageDecoderMiddleware> ResolveCmykAsync(TiffValueCollection<ushort> bitsPerSample, TiffTagReader tagReader, CancellationToken cancellationToken)
            {
                Debug.Assert(bitsPerSample.Count == 4);
                TiffInkSet inkSet = await tagReader.ReadInkSetAsync(cancellationToken).ConfigureAwait(false);
                if (inkSet == TiffInkSet.CMYK)
                {
                    if (bitsPerSample[0] == 8 && bitsPerSample[1] == 8 && bitsPerSample[2] == 8 && bitsPerSample[3] == 8)
                    {
                        return TiffChunkyCmyk8888Interpreter.Instance;
                    }
                    if (bitsPerSample[0] == 16 && bitsPerSample[1] == 16 && bitsPerSample[2] == 16 && bitsPerSample[3] == 16)
                    {
                        return TiffChunkyCmyk16161616Interpreter.Instance;
                    }
                }
                throw new NotSupportedException("Photometric interpretation not supported.");
            }
            static async ValueTask<ITiffImageDecoderMiddleware> ResolveYCbCrAsync(TiffValueCollection<ushort> bitsPerSample, TiffTagReader tagReader, CancellationToken cancellationToken)
            {
                Debug.Assert(bitsPerSample.Count == 3);
                if (bitsPerSample[0] == 8 && bitsPerSample[1] == 8 && bitsPerSample[2] == 8)
                {
                    TiffRational[] coefficients = await tagReader.ReadYCbCrCoefficientsAsync(cancellationToken).ConfigureAwait(false);
                    TiffRational[] referenceBlackWhite = await tagReader.ReadReferenceBlackWhiteAsync(cancellationToken).ConfigureAwait(false);
                    return new TiffChunkyYCbCr888Interpreter(TiffValueCollection.UnsafeWrap(coefficients), TiffValueCollection.UnsafeWrap(referenceBlackWhite));
                }
                if (bitsPerSample[0] == 16 && bitsPerSample[1] == 16 && bitsPerSample[2] == 16)
                {
                    TiffRational[] coefficients = await tagReader.ReadYCbCrCoefficientsAsync(cancellationToken).ConfigureAwait(false);
                    TiffRational[] referenceBlackWhite = await tagReader.ReadReferenceBlackWhiteAsync(cancellationToken).ConfigureAwait(false);
                    return new TiffChunkyYCbCr161616Interpreter(TiffValueCollection.UnsafeWrap(coefficients), TiffValueCollection.UnsafeWrap(referenceBlackWhite));
                }
                throw new NotSupportedException("Photometric interpretation not supported.");
            }
        }

        private static ValueTask<ITiffImageDecoderMiddleware> ResolvePlanarPhotometricInterpretationMiddlewareAsync(TiffPhotometricInterpretation photometricInterpretation, TiffValueCollection<ushort> bitsPerSample, TiffTagReader tagReader, TiffImageDecoderOptions options, CancellationToken cancellationToken)
        {
            switch (photometricInterpretation)
            {
                case TiffPhotometricInterpretation.RGB:
                    if (bitsPerSample.Count == 3)
                    {
                        return ResolveRgbAsync(bitsPerSample, tagReader, cancellationToken);
                    }
                    if (bitsPerSample.Count == 4)
                    {
                        return ResolveRgbaAsync(bitsPerSample, tagReader, options, cancellationToken);
                    }
                    break;
                case TiffPhotometricInterpretation.Seperated:
                    if (bitsPerSample.Count == 4)
                    {
                        return ResolveCmykAsync(bitsPerSample, tagReader, cancellationToken);
                    }
                    break;
                case TiffPhotometricInterpretation.YCbCr:
                    if (bitsPerSample.Count == 3)
                    {
                        return ResolveYCbCrAsync(bitsPerSample, tagReader, cancellationToken);
                    }
                    break;
            }

            throw new NotSupportedException("Photometric interpretation not supported.");

            static async ValueTask<ITiffImageDecoderMiddleware> ResolveRgbAsync(TiffValueCollection<ushort> bitsPerSample, TiffTagReader tagReader, CancellationToken cancellationToken)
            {
                Debug.Assert(bitsPerSample.Count == 3);
                TiffFillOrder fillOrder = await tagReader.ReadFillOrderAsync(cancellationToken).ConfigureAwait(false);

                if (fillOrder == 0 || fillOrder == TiffFillOrder.HigherOrderBitsFirst)
                {
                    if (bitsPerSample[0] == 8 && bitsPerSample[1] == 8 && bitsPerSample[2] == 8)
                    {
                        return TiffPlanarRgb888Interpreter.Instance;
                    }
                    if (bitsPerSample[0] == 16 && bitsPerSample[1] == 16 && bitsPerSample[2] == 16)
                    {
                        return TiffPlanarRgb161616Interpreter.Instance;
                    }
                }

                if (bitsPerSample[0] <= 8 && bitsPerSample[1] <= 8 && bitsPerSample[2] <= 8)
                {
                    return new TiffPlanarRgbAny888Interpreter(bitsPerSample, fillOrder);
                }
                if (bitsPerSample[0] <= 16 && bitsPerSample[1] <= 16 && bitsPerSample[2] <= 16)
                {
                    return new TiffPlanarRgbAny161616Interpreter(bitsPerSample, fillOrder);
                }
                if (bitsPerSample[0] <= 32 && bitsPerSample[1] <= 32 && bitsPerSample[2] <= 32)
                {
                    return new TiffPlanarRgbAny323232Interpreter(bitsPerSample, fillOrder);
                }

                throw new NotSupportedException("Photometric interpretation not supported.");
            }

            static async ValueTask<ITiffImageDecoderMiddleware> ResolveRgbaAsync(TiffValueCollection<ushort> bitsPerSample, TiffTagReader tagReader, TiffImageDecoderOptions options, CancellationToken cancellationToken)
            {
                Debug.Assert(bitsPerSample.Count == 4);
                TiffValueCollection<TiffExtraSample> extraSamples = await tagReader.ReadExtraSamplesAsync(cancellationToken).ConfigureAwait(false);
                TiffFillOrder fillOrder = await tagReader.ReadFillOrderAsync(cancellationToken).ConfigureAwait(false);

                if (bitsPerSample[0] <= 16 && bitsPerSample[1] <= 16 && bitsPerSample[2] <= 16 && bitsPerSample[3] <= 16)
                {
                    return new TiffPlanarRgbaAny16161616Interpreter(extraSamples[0] == TiffExtraSample.AssociatedAlphaData, options.UndoColorPreMultiplying, bitsPerSample, fillOrder);
                }
                if (bitsPerSample[0] <= 32 && bitsPerSample[1] <= 32 && bitsPerSample[2] <= 32 && bitsPerSample[3] <= 32)
                {
                    return new TiffPlanarRgbaAny32323232Interpreter(extraSamples[0] == TiffExtraSample.AssociatedAlphaData, options.UndoColorPreMultiplying, bitsPerSample, fillOrder);
                }

                throw new NotSupportedException("Photometric interpretation not supported.");
            }
            static async ValueTask<ITiffImageDecoderMiddleware> ResolveCmykAsync(TiffValueCollection<ushort> bitsPerSample, TiffTagReader tagReader, CancellationToken cancellationToken)
            {
                Debug.Assert(bitsPerSample.Count == 4);
                TiffInkSet inkSet = await tagReader.ReadInkSetAsync(cancellationToken).ConfigureAwait(false);
                if (inkSet == TiffInkSet.CMYK)
                {
                    if (bitsPerSample[0] == 8 && bitsPerSample[1] == 8 && bitsPerSample[2] == 8 && bitsPerSample[3] == 8)
                    {
                        return TiffPlanarCmyk8888Interpreter.Instance;
                    }
                    if (bitsPerSample[0] == 16 && bitsPerSample[1] == 16 && bitsPerSample[2] == 16 && bitsPerSample[3] == 16)
                    {
                        return TiffPlanarCmyk16161616Interpreter.Instance;
                    }
                }
                throw new NotSupportedException("Photometric interpretation not supported.");
            }
            static async ValueTask<ITiffImageDecoderMiddleware> ResolveYCbCrAsync(TiffValueCollection<ushort> bitsPerSample, TiffTagReader tagReader, CancellationToken cancellationToken)
            {
                Debug.Assert(bitsPerSample.Count == 3);
                if (bitsPerSample[0] == 8 && bitsPerSample[1] == 8 && bitsPerSample[2] == 8)
                {
                    TiffRational[] coefficients = await tagReader.ReadYCbCrCoefficientsAsync(cancellationToken).ConfigureAwait(false);
                    TiffRational[] referenceBlackWhite = await tagReader.ReadReferenceBlackWhiteAsync(cancellationToken).ConfigureAwait(false);
                    return new TiffPlanarYCbCr888Interpreter(TiffValueCollection.UnsafeWrap(coefficients), TiffValueCollection.UnsafeWrap(referenceBlackWhite));
                }
                throw new NotSupportedException("Photometric interpretation not supported.");
            }
        }
    }
}
