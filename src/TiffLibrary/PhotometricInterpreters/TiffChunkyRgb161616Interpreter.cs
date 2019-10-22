using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TiffLibrary.ImageDecoder;
using TiffLibrary.PixelBuffer;
using TiffLibrary.PixelFormats;

namespace TiffLibrary.PhotometricInterpreters
{
    /// <summary>
    /// A middleware to read 16-bit RGB pixels from uncompressed data to destination buffer writer.
    /// </summary>
    public sealed class TiffChunkyRgb161616Interpreter : ITiffImageDecoderMiddleware
    {
        /// <summary>
        /// A shared instance of <see cref="TiffChunkyRgb161616Interpreter"/>.
        /// </summary>
        public static TiffChunkyRgb161616Interpreter Instance { get; } = new TiffChunkyRgb161616Interpreter();

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

            int bytesPerScanline = 6 * context.SourceImageSize.Width;
            Memory<byte> source = context.UncompressedData.Slice(context.SourceReadOffset.Y * bytesPerScanline);
            ReadOnlySpan<byte> sourceSpan = source.Span;

            using TiffPixelBufferWriter<TiffBgra64> writer = context.GetWriter<TiffBgra64>();

            TiffOperationContext operationContext = context.OperationContext;

            int rows = context.ReadSize.Height;
            for (int row = 0; row < rows; row++)
            {
                using TiffPixelSpanHandle<TiffBgra64> pixelSpanHandle = writer.GetRowSpan(row);
                Span<byte> rowDestinationSpan = MemoryMarshal.Cast<TiffBgra64, byte>(pixelSpanHandle.GetSpan());
                CopyScanlineRgbToBgra(sourceSpan.Slice(6 * context.SourceReadOffset.X, 6 * context.ReadSize.Width), rowDestinationSpan.Slice(0, 8 * context.ReadSize.Width), context.ReadSize.Width, operationContext.IsLittleEndian == BitConverter.IsLittleEndian);
                sourceSpan = sourceSpan.Slice(bytesPerScanline);
            }

            return next.RunAsync(context);
        }

        private static void CopyScanlineRgbToBgra(ReadOnlySpan<byte> source, Span<byte> destination, int count, bool endiannessMatches)
        {
            if (source.Length < 6 * count)
            {
                throw new ArgumentException("source too short.", nameof(source));
            }
            if (destination.Length < 8 * count)
            {
                throw new ArgumentException("destination too short.", nameof(destination));
            }

            if (endiannessMatches)
            {
                for (int i = 0; i < count; i++)
                {
                    ulong value = 0xffff; // a
                    value = (value << 16) | (uint)(source[1] << 8) | source[0]; // r
                    value = (value << 16) | (uint)(source[3] << 8) | source[2]; // g
                    value = (value << 16) | (uint)(source[5] << 8) | source[4]; // b

                    BinaryPrimitives.WriteUInt64LittleEndian(destination, value);

                    source = source.Slice(6);
                    destination = destination.Slice(8);
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    ulong value = 0xffff; // a
                    value = (value << 16) | (uint)(source[0] << 8) | source[1]; // r
                    value = (value << 16) | (uint)(source[2] << 8) | source[3]; // g
                    value = (value << 16) | (uint)(source[4] << 8) | source[5]; // b

                    BinaryPrimitives.WriteUInt64LittleEndian(destination, value);

                    source = source.Slice(6);
                    destination = destination.Slice(8);
                }
            }
        }

    }
}
