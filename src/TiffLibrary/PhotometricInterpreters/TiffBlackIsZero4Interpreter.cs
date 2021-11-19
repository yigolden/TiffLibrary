using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TiffLibrary.ImageDecoder;
using TiffLibrary.PixelBuffer;
using TiffLibrary.PixelFormats;

namespace TiffLibrary.PhotometricInterpreters
{
    /// <summary>
    /// A middleware to read 4-bit BlackIsZero pixels from uncompressed data to destination buffer writer.
    /// </summary>
    public sealed class TiffBlackIsZero4Interpreter : ITiffImageDecoderMiddleware
    {
        /// <summary>
        /// A shared instance of <see cref="TiffBlackIsZero4Interpreter"/>.
        /// </summary>
        public static TiffBlackIsZero4Interpreter Instance { get; } = new TiffBlackIsZero4Interpreter();

        /// <inheritdoc />
        public ValueTask InvokeAsync(TiffImageDecoderContext context, ITiffImageDecoderPipelineNode next)
        {
            ThrowHelper.ThrowIfNull(context);
            ThrowHelper.ThrowIfNull(next);

            int bytesPerScanline = (context.SourceImageSize.Width + 1) / 2;
            Memory<byte> source = context.UncompressedData.Slice(context.SourceReadOffset.Y * bytesPerScanline);
            ReadOnlySpan<byte> sourceSpan = source.Span;

            using TiffPixelBufferWriter<TiffGray8> writer = context.GetWriter<TiffGray8>();

            int xOffset = context.SourceReadOffset.X;
            int rows = context.ReadSize.Height;
            for (int row = 0; row < rows; row++)
            {
                using TiffPixelSpanHandle<TiffGray8> pixelSpanHandle = writer.GetRowSpan(row);
                ReadOnlySpan<byte> bitsSpan = sourceSpan.Slice(xOffset >> 1); // xOffset / 2
                Span<byte> rowDestinationSpan = MemoryMarshal.AsBytes(pixelSpanHandle.GetSpan());
                int bitsOffset = xOffset & 1; // xOffset % 2
                int sourceIndex = 0;
                int destinationIndex = 0;
                int remainingWidth = context.ReadSize.Width;
                uint bits;

                if (bitsOffset != 0)
                {
                    remainingWidth--;
                    bits = bitsSpan[sourceIndex++];
                    bits = bits & 0xf;
                    rowDestinationSpan[destinationIndex++] = (byte)(bits << 4 | bits);
                }

                if (BitConverter.IsLittleEndian)
                {
                    for (; remainingWidth >= 8; remainingWidth -= 8)
                    {
                        uint nbits = MemoryMarshal.Read<uint>(bitsSpan.Slice(sourceIndex));
                        sourceIndex += 4;

                        uint b1 = nbits & 0xf0f0f0f0;
                        uint b2 = nbits & 0x0f0f0f0f;
                        b1 = b1 | (b1 >> 4);
                        b2 = b2 | (b2 << 4);

                        rowDestinationSpan[destinationIndex++] = (byte)b1;
                        rowDestinationSpan[destinationIndex++] = (byte)b2;
                        rowDestinationSpan[destinationIndex++] = (byte)(b1 >> 8);
                        rowDestinationSpan[destinationIndex++] = (byte)(b2 >> 8);
                        rowDestinationSpan[destinationIndex++] = (byte)(b1 >> 16);
                        rowDestinationSpan[destinationIndex++] = (byte)(b2 >> 16);
                        rowDestinationSpan[destinationIndex++] = (byte)(b1 >> 24);
                        rowDestinationSpan[destinationIndex++] = (byte)(b2 >> 24);
                    }
                }
                else
                {
                    for (; remainingWidth >= 8; remainingWidth -= 8)
                    {
                        uint nbits = MemoryMarshal.Read<uint>(bitsSpan.Slice(sourceIndex));
                        sourceIndex += 4;

                        uint b1 = nbits & 0xf0f0f0f0;
                        uint b2 = nbits & 0x0f0f0f0f;
                        b1 = b1 | (b1 >> 4);
                        b2 = b2 | (b2 << 4);

                        rowDestinationSpan[destinationIndex++] = (byte)(b1 >> 24);
                        rowDestinationSpan[destinationIndex++] = (byte)(b2 >> 24);
                        rowDestinationSpan[destinationIndex++] = (byte)(b1 >> 16);
                        rowDestinationSpan[destinationIndex++] = (byte)(b2 >> 16);
                        rowDestinationSpan[destinationIndex++] = (byte)(b1 >> 8);
                        rowDestinationSpan[destinationIndex++] = (byte)(b2 >> 8);
                        rowDestinationSpan[destinationIndex++] = (byte)b1;
                        rowDestinationSpan[destinationIndex++] = (byte)b2;
                    }
                }

                for (; remainingWidth >= 2; remainingWidth -= 2)
                {
                    bits = bitsSpan[sourceIndex++];
                    rowDestinationSpan[destinationIndex++] = (byte)(bits & 0xf0 | bits >> 4);
                    rowDestinationSpan[destinationIndex++] = (byte)(bits << 4 | bits & 0xf);
                }

                if (remainingWidth != 0)
                {
                    bits = bitsSpan[sourceIndex++];
                    rowDestinationSpan[destinationIndex++] = (byte)(bits & 0xf0 | bits >> 4);
                }

                sourceSpan = sourceSpan.Slice(bytesPerScanline);
            }

            return next.RunAsync(context);
        }
    }
}
