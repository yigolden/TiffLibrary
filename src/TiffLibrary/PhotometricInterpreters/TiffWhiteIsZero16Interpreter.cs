using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TiffLibrary.ImageDecoder;
using TiffLibrary.PixelBuffer;
using TiffLibrary.PixelFormats;
using TiffLibrary.Utils;

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

        /// <inheritdoc />
        public ValueTask InvokeAsync(TiffImageDecoderContext context, ITiffImageDecoderPipelineNode next)
        {
            ThrowHelper.ThrowIfNull(context);
            ThrowHelper.ThrowIfNull(next);

            int bytesPerScanline = context.SourceImageSize.Width * 2;
            Memory<byte> source = context.UncompressedData.Slice(context.SourceReadOffset.Y * bytesPerScanline);
            ReadOnlySpan<byte> sourceSpan = source.Span;

            using TiffPixelBufferWriter<TiffGray16> writer = context.GetWriter<TiffGray16>();

            bool reverseEndiannessNeeded = context.IsLittleEndian != BitConverter.IsLittleEndian;
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
                    TiffMathHelper.InvertCopy(sourceSpan.Slice(sizeof(ushort) * context.SourceReadOffset.X, sizeof(ushort) * context.ReadSize.Width), rowDestinationSpan);
                    sourceSpan = sourceSpan.Slice(bytesPerScanline);
                }
            }

            return next.RunAsync(context);
        }
    }
}
