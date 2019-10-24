using System;
using System.Threading.Tasks;
using TiffLibrary.ImageDecoder;
using TiffLibrary.PixelBuffer;
using TiffLibrary.PixelFormats;

namespace TiffLibrary.PhotometricInterpreters
{
    /// <summary>
    /// A middleware to read 8-bit PaletteColor pixels from uncompressed data to destination buffer writer.
    /// </summary>
    public sealed class TiffPaletteColor8Interpreter : ITiffImageDecoderMiddleware
    {
        private const int SingleColorCount = 1 << 8;
        private readonly ushort[] _colorMap;

        /// <summary>
        /// Initialize the middleware with the specified color map.
        /// </summary>
        /// <param name="colorMap">The color map.</param>
        public TiffPaletteColor8Interpreter(ushort[] colorMap)
        {
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

            int bytesPerScanline = context.SourceImageSize.Width;
            Memory<byte> source = context.UncompressedData.Slice(context.SourceReadOffset.Y * bytesPerScanline);
            ReadOnlySpan<byte> sourceSpan = source.Span;

            using TiffPixelBufferWriter<TiffRgba64> writer = context.GetWriter<TiffRgba64>();

            int rows = context.ReadSize.Height;

            TiffRgba64 pixel = default;
            pixel.A = ushort.MaxValue;

            for (int row = 0; row < rows; row++)
            {
                using TiffPixelSpanHandle<TiffRgba64> pixelSpanHandle = writer.GetRowSpan(row);
                ReadOnlySpan<byte> rowSourceSpan = sourceSpan.Slice(context.SourceReadOffset.X, context.ReadSize.Width);
                Span<TiffRgba64> rowDestinationSpan = pixelSpanHandle.GetSpan();

                for (int i = 0; i < rowSourceSpan.Length; i++)
                {
                    int offset = rowSourceSpan[i];
                    pixel.R = colorMap[offset];
                    pixel.G = colorMap[SingleColorCount + offset];
                    pixel.B = colorMap[2 * SingleColorCount + offset];

                    rowDestinationSpan[i] = pixel;
                }

                sourceSpan = sourceSpan.Slice(bytesPerScanline);
            }

            return next.RunAsync(context);
        }
    }
}
