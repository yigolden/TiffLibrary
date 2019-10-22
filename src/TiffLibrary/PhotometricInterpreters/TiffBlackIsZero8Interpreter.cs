using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TiffLibrary.ImageDecoder;
using TiffLibrary.PixelBuffer;
using TiffLibrary.PixelFormats;

namespace TiffLibrary.PhotometricInterpreters
{
    /// <summary>
    /// A middleware to read 8-bit BlackIsZero pixels from uncompressed data to destination buffer writer.
    /// </summary>
    public sealed class TiffBlackIsZero8Interpreter : ITiffImageDecoderMiddleware
    {
        /// <summary>
        /// A shared instance of <see cref="TiffBlackIsZero8Interpreter"/>.
        /// </summary>
        public static TiffBlackIsZero8Interpreter Instance { get; } = new TiffBlackIsZero8Interpreter();

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

            int bytesPerScanline = context.SourceImageSize.Width;
            Memory<byte> source = context.UncompressedData.Slice(context.SourceReadOffset.Y * bytesPerScanline);
            ReadOnlySpan<byte> sourceSpan = source.Span;

            using TiffPixelBufferWriter<TiffGray8> writer = context.GetWriter<TiffGray8>();

            int rows = context.ReadSize.Height;
            for (int row = 0; row < rows; row++)
            {
                using TiffPixelSpanHandle<TiffGray8> pixelSpanHandle = writer.GetRowSpan(row);
                ReadOnlySpan<byte> rowSourceSpan = sourceSpan.Slice(context.SourceReadOffset.X, context.ReadSize.Width);
                Span<byte> rowDestinationSpan = MemoryMarshal.AsBytes(pixelSpanHandle.GetSpan());
                rowSourceSpan.CopyTo(rowDestinationSpan);
                sourceSpan = sourceSpan.Slice(bytesPerScanline);
            }

            return next.RunAsync(context);
        }
    }
}
