using System;
using System.Threading.Tasks;
using TiffLibrary.ImageDecoder;
using TiffLibrary.PixelBuffer;
using TiffLibrary.PixelFormats;

namespace TiffLibrary.PhotometricInterpreters
{
    /// <summary>
    /// A middleware to read 4-bit PaletteColor pixels from uncompressed data to destination buffer writer.
    /// </summary>
    public sealed class TiffPaletteColor4Interpreter : ITiffImageDecoderMiddleware
    {
        private const int SingleColorCount = 1 << 4;
        private readonly ushort[] _colorMap;

        /// <summary>
        /// Initialize the middleware with the specified color map.
        /// </summary>
        /// <param name="colorMap">The color map.</param>
        [CLSCompliant(false)]
        public TiffPaletteColor4Interpreter(ushort[] colorMap)
        {
            _colorMap = colorMap ?? throw new ArgumentNullException(nameof(colorMap));
            if (_colorMap.Length < 3 * SingleColorCount)
            {
                throw new ArgumentException($"Color map requires {3 * SingleColorCount} elements.", nameof(colorMap));
            }
        }

        /// <inheritdoc />
        public ValueTask InvokeAsync(TiffImageDecoderContext context, ITiffImageDecoderPipelineNode next)
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

            int bytesPerScanline = (context.SourceImageSize.Width + 1) / 2;
            Memory<byte> source = context.UncompressedData.Slice(context.SourceReadOffset.Y * bytesPerScanline);
            ReadOnlySpan<byte> sourceSpan = source.Span;

            using TiffPixelBufferWriter<TiffRgba64> writer = context.GetWriter<TiffRgba64>();

            int xOffset = context.SourceReadOffset.X;
            int rows = context.ReadSize.Height;
            for (int row = 0; row < rows; row++)
            {
                using TiffPixelSpanHandle<TiffRgba64> pixelSpanHandle = writer.GetRowSpan(row);
                ReadOnlySpan<byte> bitsSpan = sourceSpan.Slice(xOffset >> 1); // xOffset / 2
                Span<TiffRgba64> rowDestinationSpan = pixelSpanHandle.GetSpan();
                int bitsOffset = xOffset & 1; // xOffset % 2
                int sourceIndex = 0;
                int destinationIndex = 0;
                int remainingWidth = context.ReadSize.Width;
                int offset;
                TiffRgba64 pixel;
                pixel.A = ushort.MaxValue;

                if (bitsOffset == 1)
                {
                    remainingWidth--;
                    offset = bitsSpan[sourceIndex++] & 0xf;

                    pixel.R = colorMap[offset];
                    pixel.G = colorMap[SingleColorCount + offset];
                    pixel.B = colorMap[2 * SingleColorCount + offset];

                    rowDestinationSpan[destinationIndex++] = pixel;
                }

                for (; (remainingWidth >> 1) > 0; remainingWidth -= 2) // for (; remainingWidth >= 2; remainingWidth -= 2)
                {
                    int bitsValue = bitsSpan[sourceIndex++];

                    offset = bitsValue >> 4;
                    pixel.R = colorMap[offset];
                    pixel.G = colorMap[SingleColorCount + offset];
                    pixel.B = colorMap[2 * SingleColorCount + offset];
                    rowDestinationSpan[destinationIndex++] = pixel;

                    offset = bitsValue & 0xf;
                    pixel.R = colorMap[offset];
                    pixel.G = colorMap[SingleColorCount + offset];
                    pixel.B = colorMap[2 * SingleColorCount + offset];
                    rowDestinationSpan[destinationIndex++] = pixel;
                }

                if (remainingWidth != 0)
                {
                    offset = bitsSpan[sourceIndex++] >> 4;

                    pixel.R = colorMap[offset];
                    pixel.G = colorMap[SingleColorCount + offset];
                    pixel.B = colorMap[2 * SingleColorCount + offset];

                    rowDestinationSpan[destinationIndex++] = pixel;
                }

                sourceSpan = sourceSpan.Slice(bytesPerScanline);
            }

            return next.RunAsync(context);
        }
    }
}
