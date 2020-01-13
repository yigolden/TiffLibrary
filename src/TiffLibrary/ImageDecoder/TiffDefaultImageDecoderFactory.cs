using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TiffLibrary.Compression;
using TiffLibrary.PhotometricInterpreters;

namespace TiffLibrary.ImageDecoder
{
    internal static class TiffDefaultImageDecoderFactory
    {

        public static Task<TiffImageDecoder> CreateImageDecoderAsync(TiffOperationContext operationContext, ITiffFileContentSource contentSource, TiffImageFileDirectory ifd, TiffImageDecoderOptions? options, CancellationToken cancellationToken)
        {
            if (!ifd.Contains(TiffTag.PhotometricInterpretation))
            {
                throw new InvalidDataException("PhotometricInterpretation tag is missing.");
            }
            if (ifd.Contains(TiffTag.TileWidth) && ifd.Contains(TiffTag.TileLength))
            {
                return CreateTiledImageDecoderAsync(operationContext, contentSource, ifd, options ?? TiffImageDecoderOptions.Default, cancellationToken);
            }
            if (ifd.Contains(TiffTag.StripOffsets))
            {
                return CreateStrippedImageDecoderAsync(operationContext, contentSource, ifd, options ?? TiffImageDecoderOptions.Default, cancellationToken);
            }
            throw new InvalidDataException("Failed to determine offsets to the image data.");
        }

        public static async Task<TiffImageDecoder> CreateStrippedImageDecoderAsync(TiffOperationContext operationContext, ITiffFileContentSource contentSource, TiffImageFileDirectory ifd, TiffImageDecoderOptions options, CancellationToken cancellationToken)
        {
            TiffSize size;
            TiffValueCollection<int> bytesPerScanline;
            TiffOrientation orientation = default;
            var builder = new TiffImageDecoderPipelineBuilder();

            TiffFileContentReader reader = await contentSource.OpenReaderAsync().ConfigureAwait(false);
            try
            {
                var fieldReader = new TiffFieldReader(reader, operationContext, cancellationToken);
                try
                {
                    var tagReader = new TiffTagReader(fieldReader, ifd);

                    // Basic Informations
                    TiffPhotometricInterpretation photometricInterpretation = (await tagReader.ReadPhotometricInterpretationAsync().ConfigureAwait(false)).GetValueOrDefault();
                    TiffCompression compression = await tagReader.ReadCompressionAsync().ConfigureAwait(false);
                    int width = (int)await tagReader.ReadImageWidthAsync().ConfigureAwait(false);
                    int height = (int)await tagReader.ReadImageLengthAsync().ConfigureAwait(false);
                    TiffPlanarConfiguration planarConfiguration = await tagReader.ReadPlanarConfigurationAsync().ConfigureAwait(false);
                    TiffValueCollection<ushort> bitsPerSample = await tagReader.ReadBitsPerSampleAsync().ConfigureAwait(false);

                    size = new TiffSize(width, height);

                    // Special case for 12-bit JPEG
                    bool is12BitJpeg = false;
                    if (compression == TiffCompression.Jpeg)
                    {
                        // The JpegDecompressionAlgorithm class will output 16-bit pixels for such images
                        // Therefore, we treat it as if it is a 16-bit image in the following steps.

                        // BlackIsZero or WhiteIsZero
                        if ((photometricInterpretation == TiffPhotometricInterpretation.BlackIsZero || photometricInterpretation == TiffPhotometricInterpretation.WhiteIsZero) && bitsPerSample.Count == 1 && bitsPerSample[0] == 12)
                        {
                            bitsPerSample = TiffValueCollection.Single((ushort)16);
                            is12BitJpeg = true;
                        }
                        // YCbCr or RGB
                        else if ((photometricInterpretation == TiffPhotometricInterpretation.RGB || photometricInterpretation == TiffPhotometricInterpretation.YCbCr) && bitsPerSample.Count == 3 && bitsPerSample[0] == 12 && bitsPerSample[1] == 12 && bitsPerSample[2] == 12)
                        {
                            bitsPerSample = TiffValueCollection.UnsafeWrap(new ushort[] { 16, 16, 16 });
                            is12BitJpeg = true;
                        }
                        // CMYK
                        else if (photometricInterpretation == TiffPhotometricInterpretation.Seperated && bitsPerSample.Count == 4 && bitsPerSample[0] == 12 && bitsPerSample[1] == 12 && bitsPerSample[2] == 12 && bitsPerSample[3] == 12)
                        {
                            bitsPerSample = TiffValueCollection.UnsafeWrap(new ushort[] { 16, 16, 16, 16 });
                            is12BitJpeg = true;
                        }
                    }

                    // Calculate BytesPerScanline
                    bytesPerScanline = planarConfiguration == TiffPlanarConfiguration.Planar ? CalculatePlanarBytesPerScanline(photometricInterpretation, bitsPerSample, width) : CalculateChunkyBytesPerScanline(photometricInterpretation, compression, bitsPerSample, width);

                    // Middleware: ApplyOrientation
                    if (!options.IgnoreOrientation)
                    {
                        orientation = await tagReader.ReadOrientationAsync().ConfigureAwait(false);
                        BuildApplyOrientationMiddleware(builder, orientation);
                    }

                    // Middleware: ImageEnumerator
                    await BuildStrippedImageEnumerator(builder, tagReader, compression, height, bytesPerScanline).ConfigureAwait(false);

                    // Middleware: Decompression
                    builder.Add(new TiffImageDecompressionMiddleware(photometricInterpretation, bitsPerSample, bytesPerScanline, await ResolveDecompressionAlgorithmAsync(compression, tagReader).ConfigureAwait(false)));

                    // Middleware: ReverseYCbCrSubsampling
                    if (photometricInterpretation == TiffPhotometricInterpretation.YCbCr && compression != TiffCompression.OldJpeg && compression != TiffCompression.Jpeg)
                    {
                        await BuildReverseYCbCrSubsamlpingMiddleware(builder, planarConfiguration, bitsPerSample, tagReader).ConfigureAwait(false);
                    }

                    // Special case for 12-bit JPEG
                    if (is12BitJpeg)
                    {
                        builder.Add(new Jpeg16BitEndianessCorrectionMiddleware());
                    }

                    // Middleware: ReversePredictor
                    TiffPredictor prediction = await tagReader.ReadPredictorAsync();
                    if (prediction != TiffPredictor.None)
                    {
                        builder.Add(new TiffReversePredictorMiddleware(bytesPerScanline, bitsPerSample, prediction));
                    }

                    // Middleware: Photometric Interpretation
                    ITiffImageDecoderMiddleware photometricInterpretationMiddleware = planarConfiguration == TiffPlanarConfiguration.Planar ?
                        await ResolvePlanarPhotometricInterpretationMiddlewareAsync(photometricInterpretation, bitsPerSample, tagReader, options).ConfigureAwait(false) :
                        await ResolveChunkyPhotometricInterpretationMiddlewareAsync(photometricInterpretation, compression, bitsPerSample, tagReader, options).ConfigureAwait(false);
                    builder.Add(photometricInterpretationMiddleware);
                }
                finally
                {
                    await fieldReader.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await reader.DisposeAsync().ConfigureAwait(false);
            }

            var parameters = new TiffImageDecoderParameters()
            {
                MemoryPool = options.MemoryPool ?? MemoryPool<byte>.Shared,
                OperationContext = operationContext,
                ContentSource = contentSource,
                ImageFileDirectory = ifd,
                PixelConverterFactory = options.PixelConverterFactory,
                BytesPerScanline = bytesPerScanline,
                ImageSize = size,
                Orientation = orientation
            };

            return new TiffImageDecoderPipelineAdapter(parameters, builder.Build());
        }

        public static async Task<TiffImageDecoder> CreateTiledImageDecoderAsync(TiffOperationContext operationContext, ITiffFileContentSource contentSource, TiffImageFileDirectory ifd, TiffImageDecoderOptions options, CancellationToken cancellationToken)
        {
            TiffSize size;
            TiffValueCollection<int> bytesPerScanline;
            TiffOrientation orientation = default;
            var builder = new TiffImageDecoderPipelineBuilder();

            TiffFileContentReader reader = await contentSource.OpenReaderAsync().ConfigureAwait(false);
            try
            {
                var fieldReader = new TiffFieldReader(reader, operationContext, cancellationToken);
                try
                {
                    var tagReader = new TiffTagReader(fieldReader, ifd);

                    // Basic Informations
                    TiffPhotometricInterpretation photometricInterpretation = (await tagReader.ReadPhotometricInterpretationAsync().ConfigureAwait(false)).GetValueOrDefault();
                    TiffCompression compression = await tagReader.ReadCompressionAsync().ConfigureAwait(false);
                    int width = (int)await tagReader.ReadImageWidthAsync().ConfigureAwait(false);
                    int height = (int)await tagReader.ReadImageLengthAsync().ConfigureAwait(false);
                    TiffPlanarConfiguration planarConfiguration = await tagReader.ReadPlanarConfigurationAsync().ConfigureAwait(false);
                    TiffValueCollection<ushort> bitsPerSample = await tagReader.ReadBitsPerSampleAsync().ConfigureAwait(false);
                    uint? tileWidth = await tagReader.ReadTileWidthAsync().ConfigureAwait(false);
                    Debug.Assert(tileWidth.HasValue);

                    size = new TiffSize(width, height);

                    // Special case for 12-bit JPEG
                    bool is12BitJpeg = false;
                    if (compression == TiffCompression.Jpeg)
                    {
                        // The JpegDecompressionAlgorithm class will output 16-bit pixels for such images
                        // Therefore, we treat it as if it is a 16-bit image in the following steps.

                        // BlackIsZero or WhiteIsZero
                        if ((photometricInterpretation == TiffPhotometricInterpretation.BlackIsZero || photometricInterpretation == TiffPhotometricInterpretation.WhiteIsZero) && bitsPerSample.Count == 1 && bitsPerSample[0] == 12)
                        {
                            bitsPerSample = TiffValueCollection.Single((ushort)16);
                            is12BitJpeg = true;
                        }
                        // YCbCr or RGB
                        else if ((photometricInterpretation == TiffPhotometricInterpretation.RGB || photometricInterpretation == TiffPhotometricInterpretation.YCbCr) && bitsPerSample.Count == 3 && bitsPerSample[0] == 12 && bitsPerSample[1] == 12 && bitsPerSample[2] == 12)
                        {
                            bitsPerSample = TiffValueCollection.UnsafeWrap(new ushort[] { 16, 16, 16 });
                            is12BitJpeg = true;
                        }
                        // CMYK
                        else if (photometricInterpretation == TiffPhotometricInterpretation.Seperated && bitsPerSample.Count == 4 && bitsPerSample[0] == 12 && bitsPerSample[1] == 12 && bitsPerSample[2] == 12 && bitsPerSample[3] == 12)
                        {
                            bitsPerSample = TiffValueCollection.UnsafeWrap(new ushort[] { 16, 16, 16, 16 });
                            is12BitJpeg = true;
                        }
                    }

                    // Calculate BytesPerScanline
                    bytesPerScanline = planarConfiguration == TiffPlanarConfiguration.Planar ? CalculatePlanarBytesPerScanline(photometricInterpretation, bitsPerSample, (int)tileWidth.GetValueOrDefault()) : CalculateChunkyBytesPerScanline(photometricInterpretation, compression, bitsPerSample, (int)tileWidth.GetValueOrDefault());

                    // Middleware: ApplyOrientation
                    if (!options.IgnoreOrientation)
                    {
                        orientation = await tagReader.ReadOrientationAsync().ConfigureAwait(false);
                        BuildApplyOrientationMiddleware(builder, orientation);
                    }

                    // Middleware: ImageEnumerator
                    await BuildTiledImageEnumerator(builder, tagReader, bytesPerScanline.Count).ConfigureAwait(false);

                    // Middleware: Decompression
                    builder.Add(new TiffImageDecompressionMiddleware(photometricInterpretation, bitsPerSample, bytesPerScanline, await ResolveDecompressionAlgorithmAsync(compression, tagReader).ConfigureAwait(false)));

                    // Middleware: ReverseYCbCrSubsampling
                    if (photometricInterpretation == TiffPhotometricInterpretation.YCbCr && compression != TiffCompression.OldJpeg && compression != TiffCompression.Jpeg)
                    {
                        await BuildReverseYCbCrSubsamlpingMiddleware(builder, planarConfiguration, bitsPerSample, tagReader).ConfigureAwait(false);
                    }

                    // Special case for 12-bit JPEG
                    if (is12BitJpeg)
                    {
                        builder.Add(new Jpeg16BitEndianessCorrectionMiddleware());
                    }

                    // Middleware: ReversePredictor
                    TiffPredictor prediction = await tagReader.ReadPredictorAsync();
                    if (prediction != TiffPredictor.None)
                    {
                        builder.Add(new TiffReversePredictorMiddleware(bytesPerScanline, bitsPerSample, prediction));
                    }

                    // Middleware: Photometric Interpretation
                    ITiffImageDecoderMiddleware photometricInterpretationMiddleware = planarConfiguration == TiffPlanarConfiguration.Planar ?
                        await ResolvePlanarPhotometricInterpretationMiddlewareAsync(photometricInterpretation, bitsPerSample, tagReader, options).ConfigureAwait(false) :
                        await ResolveChunkyPhotometricInterpretationMiddlewareAsync(photometricInterpretation, compression, bitsPerSample, tagReader, options).ConfigureAwait(false);
                    builder.Add(photometricInterpretationMiddleware);
                }
                finally
                {
                    await fieldReader.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await reader.DisposeAsync().ConfigureAwait(false);
            }

            var parameters = new TiffImageDecoderParameters()
            {
                OperationContext = operationContext,
                ContentSource = contentSource,
                ImageFileDirectory = ifd,
                PixelConverterFactory = options.PixelConverterFactory,
                BytesPerScanline = bytesPerScanline,
                ImageSize = size,
                Orientation = orientation
            };

            return new TiffImageDecoderPipelineAdapter(parameters, builder.Build());
        }

        private static async Task BuildStrippedImageEnumerator(TiffImageDecoderPipelineBuilder builder, TiffTagReader tagReader, TiffCompression compression, int height, TiffValueCollection<int> bytesPerScanline)
        {
            // Strip data
            int rowsPerStrip = (int)(await tagReader.ReadRowsPerStripAsync().ConfigureAwait(false));
            TiffValueCollection<ulong> stripOffsets = await tagReader.ReadStripOffsetsAsync().ConfigureAwait(false);
            TiffValueCollection<ulong> stripsByteCount = await tagReader.ReadStripByteCountsAsync().ConfigureAwait(false);
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

        private static async Task BuildTiledImageEnumerator(TiffImageDecoderPipelineBuilder builder, TiffTagReader tagReader, int planeCount)
        {
            // Read Tile Size 
            uint? tileWidth = await tagReader.ReadTileWidthAsync().ConfigureAwait(false);
            uint? tileHeight = await tagReader.ReadTileLengthAsync().ConfigureAwait(false);
            // TileWidth and TileHeight can not be null because they were checked in TiffFileReader.InternalCreateImageDecoderInstance
            Debug.Assert(tileWidth != null && tileHeight != null);

            // Read Tile Offsets
            TiffValueCollection<ulong> tileOffsets = await tagReader.ReadTileOffsetsAsync().ConfigureAwait(false);
            TiffValueCollection<ulong> tileByteCounts = await tagReader.ReadTileByteCountsAsync().ConfigureAwait(false);
            if (tileOffsets.IsEmpty || tileByteCounts.IsEmpty)
            {
                // Fallback to using StripOffsets and StripByteCounts
                tileOffsets = await tagReader.ReadStripOffsetsAsync().ConfigureAwait(false);
                tileByteCounts = await tagReader.ReadStripByteCountsAsync().ConfigureAwait(false);
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

        private static async Task BuildReverseYCbCrSubsamlpingMiddleware(TiffImageDecoderPipelineBuilder builder, TiffPlanarConfiguration planarConfiguration, TiffValueCollection<ushort> bitsPerSample, TiffTagReader tagReader)
        {
            ushort[] subsampling = await tagReader.ReadYCbCrSubSamplingAsync().ConfigureAwait(false);
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

        private static ValueTask<ITiffDecompressionAlgorithm> ResolveDecompressionAlgorithmAsync(TiffCompression compression, TiffTagReader tagReader)
        {
            ITiffDecompressionAlgorithm decompressionAlgorithm;

            // Select a decompression algorithm
            switch (compression)
            {
                case TiffCompression.NoCompression:
                    decompressionAlgorithm = NoneCompressionAlgorithm.Instance;
                    break;
                case TiffCompression.ModifiedHuffmanCompression:
                    return ResolveModifiedHuffmanDecompressionAlgorithmAsync(tagReader);
                case TiffCompression.T4Encoding:
                    return ResolveT4DecompressionAlgorithmAsync(tagReader);
                case TiffCompression.T6Encoding:
                    return ResolveT6DecompressionAlgorithmAsync(tagReader);
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
                    return ResolveJpegDecompressionAlgorithmAsync(tagReader);
                default:
                    throw new NotSupportedException("Compression algorithm not supported.");
            }

            return new ValueTask<ITiffDecompressionAlgorithm>(decompressionAlgorithm);

            async ValueTask<ITiffDecompressionAlgorithm> ResolveModifiedHuffmanDecompressionAlgorithmAsync(TiffTagReader tagReader)
            {
                TiffFillOrder fillOrder = await tagReader.ReadFillOrderAsync().ConfigureAwait(false);
                return ModifiedHuffmanCompressionAlgorithm.GetSharedInstance(fillOrder);
            }

            static async ValueTask<ITiffDecompressionAlgorithm> ResolveT4DecompressionAlgorithmAsync(TiffTagReader tagReader)
            {
                TiffFillOrder fillOrder = await tagReader.ReadFillOrderAsync().ConfigureAwait(false);
                TiffT4Options t4Options = await tagReader.ReadT4OptionsAsync().ConfigureAwait(false);
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

            static async ValueTask<ITiffDecompressionAlgorithm> ResolveT6DecompressionAlgorithmAsync(TiffTagReader tagReader)
            {
                TiffFillOrder fillOrder = await tagReader.ReadFillOrderAsync().ConfigureAwait(false);
                TiffT6Options t6Options = await tagReader.ReadT6OptionsAsync().ConfigureAwait(false);
                if (t6Options.HasFlag(TiffT6Options.AllowUncompressedMode))
                {
                    throw new NotSupportedException("Uncompressed mode is not supported.");
                }
                return CcittGroup4Compression.GetSharedInstance(fillOrder);
            }

            static async ValueTask<ITiffDecompressionAlgorithm> ResolveJpegDecompressionAlgorithmAsync(TiffTagReader tagReader)
            {
                byte[] jpegTables = await tagReader.ReadJPEGTablesAsync().ConfigureAwait(false);
                return new JpegDecompressionAlgorithm(jpegTables, 3);
            }
        }


        private static ValueTask<ITiffImageDecoderMiddleware> ResolveChunkyPhotometricInterpretationMiddlewareAsync(TiffPhotometricInterpretation photometricInterpretation, TiffCompression compression, TiffValueCollection<ushort> bitsPerSample, TiffTagReader tagReader, TiffImageDecoderOptions options)
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
                        return ResolveWhiteIsZeroAsync(bitsPerSample.GetFirstOrDefault(), tagReader);
                    }
                    break;
                case TiffPhotometricInterpretation.BlackIsZero:
                    if (bitsPerSample.Count == 1)
                    {
                        return ResolveBlackIsZeroAsync(bitsPerSample.GetFirstOrDefault(), tagReader);
                    }
                    break;
                case TiffPhotometricInterpretation.RGB:
                    if (bitsPerSample.Count == 3)
                    {
                        return ResolveRgbAsync(bitsPerSample, tagReader);
                    }
                    if (bitsPerSample.Count == 4)
                    {
                        return ResolveRgbaAsync(bitsPerSample, tagReader, options);
                    }
                    break;
                case TiffPhotometricInterpretation.PaletteColor:
                    if (bitsPerSample.Count == 1)
                    {
                        return ResolvePaletteColorAsync(bitsPerSample.GetFirstOrDefault(), tagReader);
                    }
                    break;
                case TiffPhotometricInterpretation.TransparencyMask:
                    if (bitsPerSample.Count == 1 && bitsPerSample.GetFirstOrDefault() == 1)
                    {
                        return ResolveTransparencyMaskAsync(tagReader);
                    }
                    break;
                case TiffPhotometricInterpretation.Seperated:
                    if (bitsPerSample.Count == 4)
                    {
                        return ResolveCmykAsync(bitsPerSample, tagReader);
                    }
                    break;
                case TiffPhotometricInterpretation.YCbCr:
                    if (bitsPerSample.Count == 3)
                    {
                        return ResolveYCbCrAsync(bitsPerSample, tagReader);
                    }
                    break;
            }

            throw new NotSupportedException("Photometric interpretation not supported.");

            static async ValueTask<ITiffImageDecoderMiddleware> ResolveWhiteIsZeroAsync(int bitCount, TiffTagReader tagReader)
            {
                TiffFillOrder fillOrder = await tagReader.ReadFillOrderAsync().ConfigureAwait(false);
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
            static async ValueTask<ITiffImageDecoderMiddleware> ResolveBlackIsZeroAsync(int bitCount, TiffTagReader tagReader)
            {
                TiffFillOrder fillOrder = await tagReader.ReadFillOrderAsync().ConfigureAwait(false);
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

            static async ValueTask<ITiffImageDecoderMiddleware> ResolveRgbAsync(TiffValueCollection<ushort> bitsPerSample, TiffTagReader tagReader)
            {
                Debug.Assert(bitsPerSample.Count == 3);
                TiffFillOrder fillOrder = await tagReader.ReadFillOrderAsync().ConfigureAwait(false);

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
            static async ValueTask<ITiffImageDecoderMiddleware> ResolveRgbaAsync(TiffValueCollection<ushort> bitsPerSample, TiffTagReader tagReader, TiffImageDecoderOptions options)
            {
                Debug.Assert(bitsPerSample.Count == 4);
                TiffValueCollection<TiffExtraSample> extraSamples = await tagReader.ReadExtraSamplesAsync().ConfigureAwait(false);
                TiffFillOrder fillOrder = await tagReader.ReadFillOrderAsync().ConfigureAwait(false);
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
            static async ValueTask<ITiffImageDecoderMiddleware> ResolvePaletteColorAsync(int bitCount, TiffTagReader tagReader)
            {
                TiffFillOrder fillOrder = await tagReader.ReadFillOrderAsync().ConfigureAwait(false);
                if (fillOrder == 0 || fillOrder == TiffFillOrder.HigherOrderBitsFirst)
                {
                    switch (bitCount)
                    {
                        case 4:
                            return new TiffPaletteColor4Interpreter(await tagReader.ReadColorMapAsync().ConfigureAwait(false));
                        case 8:
                            return new TiffPaletteColor8Interpreter(await tagReader.ReadColorMapAsync().ConfigureAwait(false));
                    }
                }

                if (bitCount <= 8)
                {
                    return new TiffPaletteColorAny8Interpreter(await tagReader.ReadColorMapAsync().ConfigureAwait(false), bitCount, fillOrder);
                }

                throw new NotSupportedException("Photometric interpretation not supported.");
            }
            static async ValueTask<ITiffImageDecoderMiddleware> ResolveTransparencyMaskAsync(TiffTagReader tagReader)
            {
                TiffFillOrder fillOrder = await tagReader.ReadFillOrderAsync().ConfigureAwait(false);
                return new TiffTransparencyMask1Interpreter(fillOrder);
            }
            static async ValueTask<ITiffImageDecoderMiddleware> ResolveCmykAsync(TiffValueCollection<ushort> bitsPerSample, TiffTagReader tagReader)
            {
                Debug.Assert(bitsPerSample.Count == 4);
                TiffInkSet inkSet = await tagReader.ReadInkSetAsync().ConfigureAwait(false);
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
            static async ValueTask<ITiffImageDecoderMiddleware> ResolveYCbCrAsync(TiffValueCollection<ushort> bitsPerSample, TiffTagReader tagReader)
            {
                Debug.Assert(bitsPerSample.Count == 3);
                if (bitsPerSample[0] == 8 && bitsPerSample[1] == 8 && bitsPerSample[2] == 8)
                {
                    TiffRational[] coefficients = await tagReader.ReadYCbCrCoefficientsAsync().ConfigureAwait(false);
                    TiffRational[] referenceBlackWhite = await tagReader.ReadReferenceBlackWhiteAsync().ConfigureAwait(false);
                    return new TiffChunkyYCbCr888Interpreter(TiffValueCollection.UnsafeWrap(coefficients), TiffValueCollection.UnsafeWrap(referenceBlackWhite));
                }
                if (bitsPerSample[0] == 16 && bitsPerSample[1] == 16 && bitsPerSample[2] == 16)
                {
                    TiffRational[] coefficients = await tagReader.ReadYCbCrCoefficientsAsync().ConfigureAwait(false);
                    TiffRational[] referenceBlackWhite = await tagReader.ReadReferenceBlackWhiteAsync().ConfigureAwait(false);
                    return new TiffChunkyYCbCr161616Interpreter(TiffValueCollection.UnsafeWrap(coefficients), TiffValueCollection.UnsafeWrap(referenceBlackWhite));
                }
                throw new NotSupportedException("Photometric interpretation not supported.");
            }
        }

        private static ValueTask<ITiffImageDecoderMiddleware> ResolvePlanarPhotometricInterpretationMiddlewareAsync(TiffPhotometricInterpretation photometricInterpretation, TiffValueCollection<ushort> bitsPerSample, TiffTagReader tagReader, TiffImageDecoderOptions options)
        {
            switch (photometricInterpretation)
            {
                case TiffPhotometricInterpretation.RGB:
                    if (bitsPerSample.Count == 3)
                    {
                        return ResolveRgbAsync(bitsPerSample, tagReader);
                    }
                    if (bitsPerSample.Count == 4)
                    {
                        return ResolveRgbaAsync(bitsPerSample, tagReader, options);
                    }
                    break;
                case TiffPhotometricInterpretation.Seperated:
                    if (bitsPerSample.Count == 4)
                    {
                        return ResolveCmykAsync(bitsPerSample, tagReader);
                    }
                    break;
                case TiffPhotometricInterpretation.YCbCr:
                    if (bitsPerSample.Count == 3)
                    {
                        return ResolveYCbCrAsync(bitsPerSample, tagReader);
                    }
                    break;
            }

            throw new NotSupportedException("Photometric interpretation not supported.");

            static async ValueTask<ITiffImageDecoderMiddleware> ResolveRgbAsync(TiffValueCollection<ushort> bitsPerSample, TiffTagReader tagReader)
            {
                Debug.Assert(bitsPerSample.Count == 3);
                TiffFillOrder fillOrder = await tagReader.ReadFillOrderAsync().ConfigureAwait(false);

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

            static async ValueTask<ITiffImageDecoderMiddleware> ResolveRgbaAsync(TiffValueCollection<ushort> bitsPerSample, TiffTagReader tagReader, TiffImageDecoderOptions options)
            {
                Debug.Assert(bitsPerSample.Count == 4);
                TiffValueCollection<TiffExtraSample> extraSamples = await tagReader.ReadExtraSamplesAsync().ConfigureAwait(false);
                TiffFillOrder fillOrder = await tagReader.ReadFillOrderAsync().ConfigureAwait(false);

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
            static async ValueTask<ITiffImageDecoderMiddleware> ResolveCmykAsync(TiffValueCollection<ushort> bitsPerSample, TiffTagReader tagReader)
            {
                Debug.Assert(bitsPerSample.Count == 4);
                TiffInkSet inkSet = await tagReader.ReadInkSetAsync().ConfigureAwait(false);
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
            static async ValueTask<ITiffImageDecoderMiddleware> ResolveYCbCrAsync(TiffValueCollection<ushort> bitsPerSample, TiffTagReader tagReader)
            {
                Debug.Assert(bitsPerSample.Count == 3);
                if (bitsPerSample[0] == 8 && bitsPerSample[1] == 8 && bitsPerSample[2] == 8)
                {
                    TiffRational[] coefficients = await tagReader.ReadYCbCrCoefficientsAsync().ConfigureAwait(false);
                    TiffRational[] referenceBlackWhite = await tagReader.ReadReferenceBlackWhiteAsync().ConfigureAwait(false);
                    return new TiffPlanarYCbCr888Interpreter(TiffValueCollection.UnsafeWrap(coefficients), TiffValueCollection.UnsafeWrap(referenceBlackWhite));
                }
                throw new NotSupportedException("Photometric interpretation not supported.");
            }
        }
    }
}
