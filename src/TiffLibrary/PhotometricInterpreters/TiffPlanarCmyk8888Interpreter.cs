using System;
using System.Threading.Tasks;
using TiffLibrary.ImageDecoder;
using TiffLibrary.PixelBuffer;
using TiffLibrary.PixelFormats;

namespace TiffLibrary.PhotometricInterpreters
{
    /// <summary>
    /// A middleware to read 8-bit CMYK planar pixels from uncompressed data to destination buffer writer.
    /// </summary>
    public sealed class TiffPlanarCmyk8888Interpreter : ITiffImageDecoderMiddleware
    {
        /// <summary>
        /// A shared instance of <see cref="TiffPlanarCmyk8888Interpreter"/>.
        /// </summary>
        public static TiffPlanarCmyk8888Interpreter Instance { get; } = new TiffPlanarCmyk8888Interpreter();

        /// <inheritdoc />
        public ValueTask InvokeAsync(TiffImageDecoderContext context, ITiffImageDecoderPipelineNode next)
        {
            ThrowHelper.ThrowIfNull(context);
            ThrowHelper.ThrowIfNull(next);

            int skippedRowOffset = context.SourceImageSize.Width * context.SourceReadOffset.Y;
            int planarByteCount = context.SourceImageSize.Width * context.SourceImageSize.Height;

            ReadOnlySpan<byte> sourceSpan = context.UncompressedData.Span;
            ReadOnlySpan<byte> sourceC = sourceSpan.Slice(0, planarByteCount);
            ReadOnlySpan<byte> sourceM = sourceSpan.Slice(planarByteCount, planarByteCount);
            ReadOnlySpan<byte> sourceY = sourceSpan.Slice(2 * planarByteCount, planarByteCount);
            ReadOnlySpan<byte> sourceK = sourceSpan.Slice(3 * planarByteCount, planarByteCount);

            using TiffPixelBufferWriter<TiffCmyk32> writer = context.GetWriter<TiffCmyk32>();

            int rows = context.ReadSize.Height;
            int cols = context.ReadSize.Width;
            for (int row = 0; row < rows; row++)
            {
                using TiffPixelSpanHandle<TiffCmyk32> pixelSpanHandle = writer.GetRowSpan(row);
                Span<TiffCmyk32> rowDestinationSpan = pixelSpanHandle.GetSpan();
                int rowOffset = skippedRowOffset + row * context.SourceImageSize.Width + context.SourceReadOffset.X;
                for (int col = 0; col < cols; col++)
                {
                    int componentOffset = rowOffset + col;

                    rowDestinationSpan[col] = new TiffCmyk32(sourceC[componentOffset], sourceM[componentOffset], sourceY[componentOffset], sourceK[componentOffset]);
                }
            }

            return next.RunAsync(context);
        }
    }
}
