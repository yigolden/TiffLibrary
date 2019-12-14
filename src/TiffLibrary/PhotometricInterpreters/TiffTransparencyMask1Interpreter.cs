using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TiffLibrary.ImageDecoder;
using TiffLibrary.PixelBuffer;
using TiffLibrary.PixelFormats;

namespace TiffLibrary.PhotometricInterpreters
{
    /// <summary>
    /// A middleware to read 1-bit TransparencyMask pixels from uncompressed data to destination buffer writer.
    /// </summary>
    public sealed class TiffTransparencyMask1Interpreter : ITiffImageDecoderMiddleware
    {
        private bool _shouldReverseBits = false;

        /// <summary>
        /// Initialize the middleware with the specified fill order.
        /// </summary>
        /// <param name="fillOrder">The FillOrder tag.</param>
        public TiffTransparencyMask1Interpreter(TiffFillOrder fillOrder)
        {
            _shouldReverseBits = fillOrder == TiffFillOrder.LowerOrderBitsFirst;
        }

        private byte MaybeReverseBits(byte b)
        {
            if (_shouldReverseBits)
            {
                // http://graphics.stanford.edu/~seander/bithacks.html#ReverseByteWith64Bits
                return (byte)(((b * 0x80200802UL) & 0x0884422110UL) * 0x0101010101UL >> 32);
            }
            return b;
        }

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

            int bytesPerScanline = (context.SourceImageSize.Width + 7) / 8;
            Memory<byte> source = context.UncompressedData.Slice(context.SourceReadOffset.Y * bytesPerScanline);
            ReadOnlySpan<byte> sourceSpan = source.Span;

            using TiffPixelBufferWriter<TiffMask> writer = context.GetWriter<TiffMask>();

            int xOffset = context.SourceReadOffset.X;
            int rows = context.ReadSize.Height;

            for (int row = 0; row < rows; row++)
            {
                using TiffPixelSpanHandle<TiffMask> pixelSpanHandle = writer.GetRowSpan(row);
                ReadOnlySpan<byte> bitsSpan = sourceSpan.Slice(xOffset >> 3); // xOffset / 8
                Span<byte> rowDestinationSpan = MemoryMarshal.AsBytes(pixelSpanHandle.GetSpan());
                int bitsOffset = xOffset & 7; // xOffset % 8
                int sourceIndex = 0;
                int destinationIndex = 0;
                int remainingWidth = context.ReadSize.Width;
                byte bits;

                if (bitsOffset > 0)
                {
                    remainingWidth -= 8 - bitsOffset;
                    bits = MaybeReverseBits(bitsSpan[sourceIndex++]);
                    for (; bitsOffset < 8; bitsOffset++)
                    {
                        bool isSet = (bits >> (7 - bitsOffset) & 1) != 0;
                        rowDestinationSpan[destinationIndex++] = isSet ? (byte)255 : (byte)0;
                    }
                }

                while (remainingWidth >= 8)
                {
                    bits = (bitsSpan[sourceIndex++]);
                    bool bit0 = (bits >> 7 & 1) != 0;
                    bool bit1 = (bits >> 6 & 1) != 0;
                    bool bit2 = (bits >> 5 & 1) != 0;
                    bool bit3 = (bits >> 4 & 1) != 0;
                    bool bit4 = (bits >> 3 & 1) != 0;
                    bool bit5 = (bits >> 2 & 1) != 0;
                    bool bit6 = (bits >> 1 & 1) != 0;
                    bool bit7 = (bits & 1) != 0;
                    rowDestinationSpan[destinationIndex++] = bit0 ? (byte)255 : (byte)0;
                    rowDestinationSpan[destinationIndex++] = bit1 ? (byte)255 : (byte)0;
                    rowDestinationSpan[destinationIndex++] = bit2 ? (byte)255 : (byte)0;
                    rowDestinationSpan[destinationIndex++] = bit3 ? (byte)255 : (byte)0;
                    rowDestinationSpan[destinationIndex++] = bit4 ? (byte)255 : (byte)0;
                    rowDestinationSpan[destinationIndex++] = bit5 ? (byte)255 : (byte)0;
                    rowDestinationSpan[destinationIndex++] = bit6 ? (byte)255 : (byte)0;
                    rowDestinationSpan[destinationIndex++] = bit7 ? (byte)255 : (byte)0;
                    remainingWidth -= 8;
                }

                if (remainingWidth > 0)
                {
                    bits = MaybeReverseBits(bitsSpan[sourceIndex++]);
                    for (; remainingWidth > 0; remainingWidth--)
                    {
                        bool isSet = (bits & 0b10000000) != 0;
                        rowDestinationSpan[destinationIndex++] = isSet ? (byte)255 : (byte)0;
                        bits = (byte)(bits << 1);
                    }
                }

                sourceSpan = sourceSpan.Slice(bytesPerScanline);
            }

            return next.RunAsync(context);
        }

    }
}
