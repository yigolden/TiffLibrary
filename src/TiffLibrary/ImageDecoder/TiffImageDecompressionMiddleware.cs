using System;
using System.Buffers;
using System.IO;
using System.Threading.Tasks;
using TiffLibrary.Compression;
using TiffLibrary.PixelFormats;

namespace TiffLibrary.ImageDecoder
{
    /// <summary>
    /// A middleware that handles decompression of the input image.
    /// </summary>
    public sealed class TiffImageDecompressionMiddleware : ITiffImageDecoderMiddleware
    {
        private readonly TiffPhotometricInterpretation _photometricInterpretation;
        private readonly TiffValueCollection<ushort> _bitsPerSample;
        private readonly TiffValueCollection<int> _bytesPerScanlines;
        private readonly ITiffDecompressionAlgorithm _decompressionAlgorithm;

        /// <summary>
        /// Initialize the middleware.
        /// </summary>
        /// <param name="photometricInterpretation">The photometric interpretation of the image.</param>
        /// <param name="bitsPerSample">Bits per sample.</param>
        /// <param name="bytesPerScanlines">Byte count per scanline.</param>
        /// <param name="decompressionAlgorithm">The decompression algorithm.</param>
        [CLSCompliant(false)]
        public TiffImageDecompressionMiddleware(TiffPhotometricInterpretation photometricInterpretation, TiffValueCollection<ushort> bitsPerSample, TiffValueCollection<int> bytesPerScanlines, ITiffDecompressionAlgorithm decompressionAlgorithm)
        {
            if (bytesPerScanlines.IsEmpty)
            {
                throw new ArgumentException("BytesPerScanlines not specified.");
            }

            _photometricInterpretation = photometricInterpretation;
            _bitsPerSample = bitsPerSample;
            _bytesPerScanlines = bytesPerScanlines;
            _decompressionAlgorithm = decompressionAlgorithm;
        }

        /// <inheritdoc />
        public async ValueTask InvokeAsync(TiffImageDecoderContext context, ITiffImageDecoderPipelineNode next)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (next is null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (_bytesPerScanlines.Count != context.PlanarRegions.Count)
            {
                throw new InvalidOperationException();
            }

            int planeCount = _bytesPerScanlines.Count;

            // calculate the uncompressed data buffer length
            int uncompressedDataLength = 0;
            int imageHeight = context.SourceImageSize.Height;
            foreach (int bytesPerScanline in _bytesPerScanlines)
            {
                uncompressedDataLength += bytesPerScanline * imageHeight;
            }

            TiffEmptyStrileWriter? emptyWriter = null;

            // calculate the maximum buffer needed to read from stream
            int readCount = 0;
            foreach (TiffStreamRegion planarRegion in context.PlanarRegions)
            {
                int length = planarRegion.Length;
                if (length == 0)
                {
                    emptyWriter = new TiffEmptyStrileWriter(new TiffRgba32(255, 255, 255, 0));
                    break;
                }
                if (length > readCount)
                {
                    readCount = planarRegion.Length;
                }
            }

            if (!(emptyWriter is null))
            {
                emptyWriter.Write(context);
                return;
            }

            // allocate the raw data buffer and the uncompressed data buffer
            MemoryPool<byte> memoryPool = context.MemoryPool ?? MemoryPool<byte>.Shared;
            using IMemoryOwner<byte> bufferMemory = memoryPool.Rent(uncompressedDataLength);
            int planarUncompressedByteCount = 0;
            TiffFileContentReader? reader = context.ContentReader;
            if (reader is null)
            {
                throw new InvalidOperationException("Failed to acquire ContentReader.");
            }

            using (IMemoryOwner<byte> rawBuffer = memoryPool.Rent(readCount))
            {
                TiffDecompressionContext decompressionContext = new TiffDecompressionContext();

                // decompress each plane
                for (int i = 0; i < planeCount; i++)
                {
                    TiffStreamRegion region = context.PlanarRegions[i];

                    // Read from stream
                    readCount = await reader.ReadAsync(region.Offset, rawBuffer.Memory.Slice(0, region.Length), context.CancellationToken).ConfigureAwait(false);
                    if (readCount != region.Length)
                    {
                        throw new InvalidDataException();
                    }

                    // Decompress this plane
                    int bytesPerScanline = _bytesPerScanlines[i];
                    decompressionContext.MemoryPool = memoryPool;
                    decompressionContext.PhotometricInterpretation = _photometricInterpretation;
                    decompressionContext.BitsPerSample = _bitsPerSample;
                    decompressionContext.ImageSize = context.SourceImageSize;
                    decompressionContext.BytesPerScanline = bytesPerScanline;
                    decompressionContext.SkippedScanlines = context.SourceReadOffset.Y;
                    decompressionContext.RequestedScanlines = context.ReadSize.Height;
                    _decompressionAlgorithm.Decompress(decompressionContext, rawBuffer.Memory.Slice(0, readCount), bufferMemory.Memory.Slice(planarUncompressedByteCount, bytesPerScanline * imageHeight));
                    planarUncompressedByteCount += bytesPerScanline * imageHeight;
                }
            }

            // Pass down the data
            context.UncompressedData = bufferMemory.Memory.Slice(0, uncompressedDataLength);
            await next.RunAsync(context).ConfigureAwait(false);
            context.UncompressedData = default;

        }
    }
}
