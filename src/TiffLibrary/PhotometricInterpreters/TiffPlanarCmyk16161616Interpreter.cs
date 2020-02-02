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
    /// A middleware to read 16-bit CMYK planar pixels from uncompressed data to destination buffer writer.
    /// </summary>
    public sealed class TiffPlanarCmyk16161616Interpreter : ITiffImageDecoderMiddleware
    {
        /// <summary>
        /// A shared instance of <see cref="TiffPlanarCmyk16161616Interpreter"/>.
        /// </summary>
        public static TiffPlanarCmyk16161616Interpreter Instance { get; } = new TiffPlanarCmyk16161616Interpreter();

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

            int skippedRowOffset = context.SourceImageSize.Width * context.SourceReadOffset.Y;
            int planarByteCount = sizeof(ushort) * context.SourceImageSize.Width * context.SourceImageSize.Height;

            ReadOnlySpan<byte> sourceSpan = context.UncompressedData.Span;
            ReadOnlySpan<byte> sourceC = sourceSpan.Slice(0, planarByteCount);
            ReadOnlySpan<byte> sourceM = sourceSpan.Slice(planarByteCount, planarByteCount);
            ReadOnlySpan<byte> sourceY = sourceSpan.Slice(2 * planarByteCount, planarByteCount);
            ReadOnlySpan<byte> sourceK = sourceSpan.Slice(3 * planarByteCount, planarByteCount);

            using TiffPixelBufferWriter<TiffCmyk64> writer = context.GetWriter<TiffCmyk64>();

            int rows = context.ReadSize.Height;
            int cols = context.ReadSize.Width;

            if (context.IsLittleEndian == BitConverter.IsLittleEndian)
            {
                for (int row = 0; row < rows; row++)
                {
                    using TiffPixelSpanHandle<TiffCmyk64> pixelSpanHandle = writer.GetRowSpan(row);
                    Span<byte> rowDestinationSpan = MemoryMarshal.AsBytes(pixelSpanHandle.GetSpan());
                    int rowOffset = sizeof(ushort) * (skippedRowOffset + row * context.SourceImageSize.Width + context.SourceReadOffset.X);
                    for (int col = 0; col < cols; col++)
                    {
                        int componentOffset = rowOffset + sizeof(ushort) * col;
                        ulong value = (uint)(sourceK[componentOffset + 1] << 8) | sourceK[componentOffset]; //k
                        value = (value << 16) | (uint)(sourceY[componentOffset + 1] << 8) | sourceY[componentOffset]; // y
                        value = (value << 16) | (uint)(sourceM[componentOffset + 1] << 8) | sourceM[componentOffset]; // m
                        value = (value << 16) | (uint)(sourceC[componentOffset + 1] << 8) | sourceC[componentOffset]; // c

                        BinaryPrimitives.WriteUInt64LittleEndian(rowDestinationSpan, value);

                        rowDestinationSpan = rowDestinationSpan.Slice(8);
                    }
                }
            }
            else
            {
                for (int row = 0; row < rows; row++)
                {
                    using TiffPixelSpanHandle<TiffCmyk64> pixelSpanHandle = writer.GetRowSpan(row);
                    Span<byte> rowDestinationSpan = MemoryMarshal.AsBytes(pixelSpanHandle.GetSpan());
                    int rowOffset = sizeof(ushort) * (skippedRowOffset + row * context.SourceImageSize.Width + context.SourceReadOffset.X);
                    for (int col = 0; col < cols; col++)
                    {
                        int componentOffset = rowOffset + sizeof(ushort) * col;
                        ulong value = (uint)(sourceK[componentOffset] << 8) | sourceK[componentOffset + 1]; // k
                        value = (value << 16) | (uint)(sourceY[componentOffset] << 8) | sourceY[componentOffset + 1]; // y
                        value = (value << 16) | (uint)(sourceM[componentOffset] << 8) | sourceM[componentOffset + 1]; // m
                        value = (value << 16) | (uint)(sourceC[componentOffset] << 8) | sourceC[componentOffset + 1]; // c

                        BinaryPrimitives.WriteUInt64LittleEndian(rowDestinationSpan, value);

                        rowDestinationSpan = rowDestinationSpan.Slice(8);
                    }
                }
            }

            return next.RunAsync(context);
        }
    }
}
