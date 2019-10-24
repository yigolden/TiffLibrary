using System;
using System.Buffers;
using System.IO;
using System.Threading.Tasks;
using TiffLibrary.Compression;

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

        /// <summary>
        /// Run this middleware.
        /// </summary>
        /// <param name="context">Information of the current decoding process.</param>
        /// <param name="next">The next middleware in the decoder pipeline.</param>
        /// <returns>A <see cref="Task"/> that completes when this middleware completes running.</returns>
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
            foreach (var bytesPerScanline in _bytesPerScanlines)
            {
                uncompressedDataLength += bytesPerScanline * imageHeight;
            }

            // calculate the maximum buffer needed to read from stream
            int readCount = context.PlanarRegions[0].Length;
            foreach (TiffStreamRegion planarRegion in context.PlanarRegions)
            {
                if (planarRegion.Length > readCount)
                {
                    readCount = planarRegion.Length;
                }
            }

            // allocate the raw data buffer and the uncompressed data buffer
            byte[] raw = ArrayPool<byte>.Shared.Rent(readCount);
            byte[] buffer = ArrayPool<byte>.Shared.Rent(uncompressedDataLength);
            try
            {
                int planarUncompressedByteCount = 0;
                TiffFileContentReader reader = context.ContentReader;

                // decompress each plane
                for (int i = 0; i < planeCount; i++)
                {
                    TiffStreamRegion region = context.PlanarRegions[i];

                    // Read from stream
                    readCount = await reader.ReadAsync(region.Offset, new ArraySegment<byte>(raw, 0, region.Length)).ConfigureAwait(false);
                    if (readCount != region.Length)
                    {
                        throw new InvalidDataException();
                    }

                    // Decompress this plane
                    int bytesPerScanline = _bytesPerScanlines[i];
                    var decompressionContext = new TiffDecompressionContext
                    {
                        PhotometricInterpretation = _photometricInterpretation,
                        BitsPerSample = _bitsPerSample,
                        ImageSize = context.SourceImageSize,
                        BytesPerScanline = bytesPerScanline,
                        SkippedScanlines = context.SourceReadOffset.Y,
                        RequestedScanlines = context.ReadSize.Height
                    };
                    _decompressionAlgorithm.Decompress(decompressionContext, raw.AsMemory(0, readCount), buffer.AsMemory(planarUncompressedByteCount, bytesPerScanline * imageHeight));
                    planarUncompressedByteCount += bytesPerScanline * imageHeight;
                }

                ArrayPool<byte>.Shared.Return(raw);
                raw = null;

                // Pass down the data
                context.UncompressedData = buffer.AsMemory(0, uncompressedDataLength);
                await next.RunAsync(context).ConfigureAwait(false);
                context.UncompressedData = default;
            }
            finally
            {
                if (!(raw is null))
                {
                    ArrayPool<byte>.Shared.Return(raw);
                }
                ArrayPool<byte>.Shared.Return(buffer);
            }

        }
    }
}
