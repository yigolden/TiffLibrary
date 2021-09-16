using System;
using System.Threading.Tasks;
using TiffLibrary.ImageDecoder;
using TiffLibrary.PixelBuffer;
using TiffLibrary.PixelFormats;

namespace TiffLibrary.PhotometricInterpreters
{
    /// <summary>
    /// A middleware to read 8-bit RGB planar pixels from uncompressed data to destination buffer writer.
    /// </summary>
    public sealed class TiffPlanarRgb888Interpreter : ITiffImageDecoderMiddleware
    {
        /// <summary>
        /// A shared instance of <see cref="TiffPlanarRgb888Interpreter"/>.
        /// </summary>
        public static TiffPlanarRgb888Interpreter Instance { get; } = new TiffPlanarRgb888Interpreter();

        /// <inheritdoc />
        public ValueTask InvokeAsync(TiffImageDecoderContext context, ITiffImageDecoderPipelineNode next)
        {
            ThrowHelper.ThrowIfNull(context);
            ThrowHelper.ThrowIfNull(next);

            int skippedRowOffset = context.SourceImageSize.Width * context.SourceReadOffset.Y;
            int planarByteCount = context.SourceImageSize.Width * context.SourceImageSize.Height;

            ReadOnlySpan<byte> sourceSpan = context.UncompressedData.Span;
            ReadOnlySpan<byte> sourceR = sourceSpan.Slice(0, planarByteCount);
            ReadOnlySpan<byte> sourceG = sourceSpan.Slice(planarByteCount, planarByteCount);
            ReadOnlySpan<byte> sourceB = sourceSpan.Slice(2 * planarByteCount, planarByteCount);

            using TiffPixelBufferWriter<TiffRgb24> writer = context.GetWriter<TiffRgb24>();

            int rows = context.ReadSize.Height;
            int cols = context.ReadSize.Width;
            for (int row = 0; row < rows; row++)
            {
                using TiffPixelSpanHandle<TiffRgb24> pixelSpanHandle = writer.GetRowSpan(row);
                Span<TiffRgb24> rowDestinationSpan = pixelSpanHandle.GetSpan();
                int rowOffset = skippedRowOffset + row * context.SourceImageSize.Width + context.SourceReadOffset.X;
                for (int col = 0; col < cols; col++)
                {
                    int componentOffset = rowOffset + col;
                    rowDestinationSpan[col] = new TiffRgb24(sourceR[componentOffset], sourceG[componentOffset], sourceB[componentOffset]);
                }
            }

            return next.RunAsync(context);
        }
    }
}
