using System;
using System.Threading.Tasks;
using TiffLibrary.ImageDecoder;
using TiffLibrary.PixelBuffer;
using TiffLibrary.PixelFormats;

namespace TiffLibrary.PhotometricInterpreters
{
    /// <summary>
    /// A middleware to read any bits (less than 8 bits) PaletteColor pixels from uncompressed data to destination buffer writer.
    /// </summary>
    public sealed class TiffPaletteColorAny8Interpreter : ITiffImageDecoderMiddleware
    {
        private readonly ushort[] _colorMap;
        private readonly int _bitCount;
        private readonly TiffFillOrder _fillOrder;

        private int SingleColorCount => 1 << _bitCount;

        /// <summary>
        /// Initialize the middleware with the specified bit count and fill order.
        /// </summary>
        /// <param name="colorMap">The color map.</param>
        /// <param name="bitCount">The bit count.</param>
        /// <param name="fillOrder">The FillOrder tag.</param>
        public TiffPaletteColorAny8Interpreter(ushort[] colorMap, int bitCount, TiffFillOrder fillOrder = TiffFillOrder.HigherOrderBitsFirst)
        {
            if ((uint)bitCount > 32)
            {
                throw new ArgumentOutOfRangeException(nameof(bitCount));
            }
            if (fillOrder == 0)
            {
                fillOrder = TiffFillOrder.HigherOrderBitsFirst;
            }
            _bitCount = bitCount;
            _fillOrder = fillOrder;
            _colorMap = colorMap ?? throw new ArgumentNullException(nameof(colorMap));
            if (_colorMap.Length < 3 * SingleColorCount)
            {
                throw new ArgumentException($"Color map requires {3 * SingleColorCount} elements.", nameof(colorMap));
            }
        }

        /// <summary>
        /// Run this middleware.
        /// </summary>
        /// <param name="context">Information of the current decoding process.</param>
        /// <param name="next">The next middleware in the decoder pipeline.</param>
        /// <returns>A <see cref="Task"/> that completes when this middleware completes running.</returns>
        public Task InvokeAsync(TiffImageDecoderContext context, ITiffImageDecoderPipelineNode next)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (next is null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            // Colormap array is read using TiffTagReader, its elements should be in machine-endian.
            ushort[] colorMap = _colorMap;

            int bitCount = _bitCount;
            bool isHigherOrderBitsFirst = _fillOrder != TiffFillOrder.LowerOrderBitsFirst;

            int bytesPerScanline = (context.SourceImageSize.Width * bitCount + 7) / 8;
            Memory<byte> source = context.UncompressedData.Slice(context.SourceReadOffset.Y * bytesPerScanline);
            ReadOnlySpan<byte> sourceSpan = source.Span;

            using TiffPixelBufferWriter<TiffRgba64> writer = context.GetWriter<TiffRgba64>();

            int rows = context.ReadSize.Height;
            int cols = context.ReadSize.Width;

            TiffRgba64 pixel = default;
            pixel.A = ushort.MaxValue;

            for (int row = 0; row < rows; row++)
            {
                using TiffPixelSpanHandle<TiffRgba64> pixelSpanHandle = writer.GetRowSpan(row);
                Span<TiffRgba64> pixelSpan = pixelSpanHandle.GetSpan();
                var bitReader = new BitReader(sourceSpan.Slice(0, bytesPerScanline), isHigherOrderBitsFirst);
                bitReader.Advance(context.SourceReadOffset.X * bitCount);

                for (int col = 0; col < cols; col++)
                {
                    uint offset = bitReader.Read(bitCount);
                    pixel.R = colorMap[offset];
                    pixel.G = colorMap[SingleColorCount + offset];
                    pixel.B = colorMap[2 * SingleColorCount + offset];

                    pixelSpan[col] = pixel;
                }

                sourceSpan = sourceSpan.Slice(bytesPerScanline);
            }

            return next.RunAsync(context);
        }

    }
}
