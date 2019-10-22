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
    /// A middleware to read 16-bit WhiteIsZero pixels from uncompressed data to destination buffer writer.
    /// </summary>
    public sealed class TiffWhiteIsZero16Interpreter : ITiffImageDecoderMiddleware
    {
        /// <summary>
        /// A shared instance of <see cref="TiffWhiteIsZero16Interpreter"/>.
        /// </summary>
        public static TiffWhiteIsZero16Interpreter Instance { get; } = new TiffWhiteIsZero16Interpreter();

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

            int bytesPerScanline = context.SourceImageSize.Width * 2;
            Memory<byte> source = context.UncompressedData.Slice(context.SourceReadOffset.Y * bytesPerScanline);
            ReadOnlySpan<byte> sourceSpan = source.Span;

            using TiffPixelBufferWriter<TiffGray16> writer = context.GetWriter<TiffGray16>();

            TiffOperationContext operationContext = context.OperationContext;
            bool reverseEndiannessNeeded = operationContext.IsLittleEndian != BitConverter.IsLittleEndian;
            int rows = context.ReadSize.Height;

            if (reverseEndiannessNeeded)
            {
                for (int row = 0; row < rows; row++)
                {
                    using TiffPixelSpanHandle<TiffGray16> pixelSpanHandle = writer.GetRowSpan(row);
                    ReadOnlySpan<ushort> scanline = MemoryMarshal.Cast<byte, ushort>(sourceSpan).Slice(context.SourceReadOffset.X, context.ReadSize.Width);
                    Span<ushort> destination16 = MemoryMarshal.Cast<TiffGray16, ushort>(pixelSpanHandle.GetSpan());
                    for (int col = 0; col < scanline.Length; row++)
                    {
                        destination16[col] = (ushort)~BinaryPrimitives.ReverseEndianness(scanline[col]);
                    }
                    sourceSpan = sourceSpan.Slice(bytesPerScanline);
                }
            }
            else
            {
                for (int row = 0; row < rows; row++)
                {
                    using TiffPixelSpanHandle<TiffGray16> pixelSpanHandle = writer.GetRowSpan(row);
                    Span<byte> rowDestinationSpan = MemoryMarshal.AsBytes(pixelSpanHandle.GetSpan());
                    InvertCopy(sourceSpan.Slice(sizeof(ushort) * context.SourceReadOffset.X, sizeof(ushort) * context.ReadSize.Width), rowDestinationSpan);
                    sourceSpan = sourceSpan.Slice(bytesPerScanline);
                }
            }

            return next.RunAsync(context);
        }

        private static unsafe void InvertCopy(ReadOnlySpan<byte> source, Span<byte> destination)
        {
            if (destination.Length < source.Length)
            {
                throw new InvalidOperationException("destination too short.");
            }

            int count8 = source.Length / 8;
            ReadOnlySpan<ulong> source8 = MemoryMarshal.Cast<byte, ulong>(source.Slice(0, 8 * count8));
            Span<ulong> destination8 = MemoryMarshal.Cast<byte, ulong>(destination.Slice(0, 8 * count8));
            for (int i = 0; i < source8.Length; i++)
            {
                destination8[i] = ~source8[i];
            }

            for (int i = 8 * count8; i < source.Length; i++)
            {
                destination[i] = (byte)~source[i];
            }
        }
    }
}
