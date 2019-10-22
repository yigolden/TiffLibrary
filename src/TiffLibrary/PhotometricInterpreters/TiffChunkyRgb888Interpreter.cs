using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TiffLibrary.ImageDecoder;
using TiffLibrary.PixelBuffer;
using TiffLibrary.PixelFormats;

namespace TiffLibrary.PhotometricInterpreters
{
    /// <summary>
    /// A middleware to read 8-bit RGB pixels from uncompressed data to destination buffer writer.
    /// </summary>
    public sealed class TiffChunkyRgb888Interpreter : ITiffImageDecoderMiddleware
    {
        /// <summary>
        /// A shared instance of <see cref="TiffChunkyRgb888Interpreter"/>.
        /// </summary>
        public static TiffChunkyRgb888Interpreter Instance { get; } = new TiffChunkyRgb888Interpreter();

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

            int bytesPerScanline = 3 * context.SourceImageSize.Width;
            Memory<byte> source = context.UncompressedData.Slice(context.SourceReadOffset.Y * bytesPerScanline);
            ReadOnlySpan<byte> sourceSpan = source.Span;

            using TiffPixelBufferWriter<TiffRgb24> writer = context.GetWriter<TiffRgb24>();

            int rows = context.ReadSize.Height;
            for (int row = 0; row < rows; row++)
            {
                using TiffPixelSpanHandle<TiffRgb24> pixelSpanHandle = writer.GetRowSpan(row);
                Span<byte> rowDestinationSpan = MemoryMarshal.Cast<TiffRgb24, byte>(pixelSpanHandle.GetSpan());
                sourceSpan.Slice(3 * context.SourceReadOffset.X, 3 * context.ReadSize.Width).CopyTo(rowDestinationSpan);
                sourceSpan = sourceSpan.Slice(bytesPerScanline);
            }

            return next.RunAsync(context);
        }

    }
}
