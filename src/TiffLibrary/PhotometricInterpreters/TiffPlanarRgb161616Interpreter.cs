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
    /// A middleware to read 16-bit RGB planar pixels from uncompressed data to destination buffer writer.
    /// </summary>
    public sealed class TiffPlanarRgb161616Interpreter : ITiffImageDecoderMiddleware
    {
        /// <summary>
        /// A shared instance of <see cref="TiffPlanarRgb161616Interpreter"/>.
        /// </summary>
        public static TiffPlanarRgb161616Interpreter Instance { get; } = new TiffPlanarRgb161616Interpreter();

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
            if (context.OperationContext is null)
            {
                throw new InvalidOperationException("Failed to acquire OperationContext.");
            }

            int skippedRowOffset = context.SourceImageSize.Width * context.SourceReadOffset.Y;
            int planarByteCount = sizeof(ushort) * context.SourceImageSize.Width * context.SourceImageSize.Height;

            ReadOnlySpan<byte> sourceSpan = context.UncompressedData.Span;
            ReadOnlySpan<byte> sourceR = sourceSpan.Slice(0, planarByteCount);
            ReadOnlySpan<byte> sourceG = sourceSpan.Slice(planarByteCount, planarByteCount);
            ReadOnlySpan<byte> sourceB = sourceSpan.Slice(2 * planarByteCount, planarByteCount);

            using TiffPixelBufferWriter<TiffBgra64> writer = context.GetWriter<TiffBgra64>();

            TiffOperationContext operationContext = context.OperationContext;

            int rows = context.ReadSize.Height;
            int cols = context.ReadSize.Width;

            if (operationContext.IsLittleEndian == BitConverter.IsLittleEndian)
            {
                for (int row = 0; row < rows; row++)
                {
                    using TiffPixelSpanHandle<TiffBgra64> pixelSpanHandle = writer.GetRowSpan(row);
                    Span<byte> rowDestinationSpan = MemoryMarshal.AsBytes(pixelSpanHandle.GetSpan());
                    int rowOffset = sizeof(ushort) * (skippedRowOffset + row * context.SourceImageSize.Width + context.SourceReadOffset.X);
                    for (int col = 0; col < cols; col++)
                    {
                        int componentOffset = rowOffset + sizeof(ushort) * col;
                        ulong value = 0xffff;
                        value = (value << 16) | (uint)(sourceR[componentOffset + 1] << 8) | sourceR[componentOffset]; // r
                        value = (value << 16) | (uint)(sourceG[componentOffset + 1] << 8) | sourceG[componentOffset]; // g
                        value = (value << 16) | (uint)(sourceB[componentOffset + 1] << 8) | sourceB[componentOffset]; // b

                        BinaryPrimitives.WriteUInt64LittleEndian(rowDestinationSpan, value);

                        rowDestinationSpan = rowDestinationSpan.Slice(8);
                    }
                }
            }
            else
            {
                for (int row = 0; row < rows; row++)
                {
                    using TiffPixelSpanHandle<TiffBgra64> pixelSpanHandle = writer.GetRowSpan(row);
                    Span<byte> rowDestinationSpan = MemoryMarshal.AsBytes(pixelSpanHandle.GetSpan());
                    int rowOffset = sizeof(ushort) * (skippedRowOffset + row * context.SourceImageSize.Width + context.SourceReadOffset.X);
                    for (int col = 0; col < cols; col++)
                    {
                        int componentOffset = rowOffset + sizeof(ushort) * col;
                        ulong value = 0xffff;
                        value = (value << 16) | (uint)(sourceR[componentOffset] << 8) | sourceR[componentOffset + 1]; // r
                        value = (value << 16) | (uint)(sourceG[componentOffset] << 8) | sourceG[componentOffset + 1]; // g
                        value = (value << 16) | (uint)(sourceB[componentOffset] << 8) | sourceB[componentOffset + 1]; // b

                        BinaryPrimitives.WriteUInt64LittleEndian(rowDestinationSpan, value);

                        rowDestinationSpan = rowDestinationSpan.Slice(8);
                    }
                }
            }

            return next.RunAsync(context);
        }
    }
}
