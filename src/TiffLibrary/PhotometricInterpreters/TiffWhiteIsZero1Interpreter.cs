﻿using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TiffLibrary.ImageDecoder;
using TiffLibrary.PixelBuffer;
using TiffLibrary.PixelFormats;

namespace TiffLibrary.PhotometricInterpreters
{
    /// <summary>
    /// A middleware to read 1-bit WhiteIsZero pixels from uncompressed data to destination buffer writer.
    /// </summary>
    public sealed class TiffWhiteIsZero1Interpreter : ITiffImageDecoderMiddleware
    {
        private bool _shouldReverseBits;

        /// <summary>
        /// Initialize the middleware with the specified fill order.
        /// </summary>
        /// <param name="fillOrder">The FillOrder tag.</param>
        [CLSCompliant(false)]
        public TiffWhiteIsZero1Interpreter(TiffFillOrder fillOrder)
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
            ThrowHelper.ThrowIfNull(context);
            ThrowHelper.ThrowIfNull(next);

            int bytesPerScanline = (context.SourceImageSize.Width + 7) / 8;
            Memory<byte> source = context.UncompressedData.Slice(context.SourceReadOffset.Y * bytesPerScanline);
            ReadOnlySpan<byte> sourceSpan = source.Span;

            using TiffPixelBufferWriter<TiffGray8> writer = context.GetWriter<TiffGray8>();

            int xOffset = context.SourceReadOffset.X;
            int rows = context.ReadSize.Height;

            for (int row = 0; row < rows; row++)
            {
                using TiffPixelSpanHandle<TiffGray8> pixelSpanHandle = writer.GetRowSpan(row);
                ReadOnlySpan<byte> bitsSpan = sourceSpan.Slice(xOffset >> 3); // xOffset / 8
                Span<byte> rowDestinationSpan = MemoryMarshal.AsBytes(pixelSpanHandle.GetSpan());
                int bitsOffset = xOffset & 7; // xOffset % 8
                int sourceIndex = 0;
                int destinationIndex = 0;
                int remainingWidth = context.ReadSize.Width;
                byte bits;

                if (bitsOffset > 0)
                {
                    int count = Math.Min(8, remainingWidth + bitsOffset);
                    remainingWidth -= 8 - bitsOffset;
                    bits = MaybeReverseBits(bitsSpan[sourceIndex++]);
                    for (; bitsOffset < count; bitsOffset++)
                    {
                        bool isSet = (bits >> (7 - bitsOffset) & 1) != 0;
                        rowDestinationSpan[destinationIndex++] = isSet ? (byte)0 : (byte)255;
                    }
                }

                while (remainingWidth >= 8)
                {
                    int bits32 = MaybeReverseBits(bitsSpan[sourceIndex++]);

                    rowDestinationSpan[destinationIndex++] = (byte)~((bits32 << 24) >> 31);
                    rowDestinationSpan[destinationIndex++] = (byte)~((bits32 << 25) >> 31);
                    rowDestinationSpan[destinationIndex++] = (byte)~((bits32 << 26) >> 31);
                    rowDestinationSpan[destinationIndex++] = (byte)~((bits32 << 27) >> 31);
                    rowDestinationSpan[destinationIndex++] = (byte)~((bits32 << 28) >> 31);
                    rowDestinationSpan[destinationIndex++] = (byte)~((bits32 << 29) >> 31);
                    rowDestinationSpan[destinationIndex++] = (byte)~((bits32 << 30) >> 31);
                    rowDestinationSpan[destinationIndex++] = (byte)~((bits32 << 31) >> 31);

                    remainingWidth -= 8;
                }

                if (remainingWidth > 0)
                {
                    bits = MaybeReverseBits(bitsSpan[sourceIndex++]);
                    for (; remainingWidth > 0; remainingWidth--)
                    {
                        bool isSet = (bits & 0b10000000) != 0;
                        rowDestinationSpan[destinationIndex++] = isSet ? (byte)0 : (byte)255;
                        bits = (byte)(bits << 1);
                    }
                }

                sourceSpan = sourceSpan.Slice(bytesPerScanline);
            }

            return next.RunAsync(context);
        }
    }
}
