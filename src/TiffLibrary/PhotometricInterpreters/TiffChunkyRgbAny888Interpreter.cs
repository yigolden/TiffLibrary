using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TiffLibrary.ImageDecoder;
using TiffLibrary.PixelBuffer;
using TiffLibrary.PixelFormats;

namespace TiffLibrary.PhotometricInterpreters
{
    /// <summary>
    /// A middleware to read any bits (less than 8 bits) RGB pixels from uncompressed data to destination buffer writer.
    /// </summary>
    public sealed class TiffChunkyRgbAny888Interpreter : ITiffImageDecoderMiddleware
    {
        private readonly TiffValueCollection<ushort> _bitsPerSample;
        private readonly TiffFillOrder _fillOrder;

        /// <summary>
        /// Initialize the middleware with the specified bits per sample and fill order.
        /// </summary>
        /// <param name="bitsPerSample">The BitsPerSample flags.</param>
        /// <param name="fillOrder">The FillOrder tag.</param>
        [CLSCompliant(false)]
        public TiffChunkyRgbAny888Interpreter(TiffValueCollection<ushort> bitsPerSample, TiffFillOrder fillOrder = TiffFillOrder.HigherOrderBitsFirst)
        {
            if (bitsPerSample.Count != 3)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(bitsPerSample));
            }
            if ((uint)bitsPerSample[0] > 8 || (uint)bitsPerSample[1] > 8 || (uint)bitsPerSample[2] > 8)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(bitsPerSample));
            }
            if (fillOrder == 0)
            {
                fillOrder = TiffFillOrder.HigherOrderBitsFirst;
            }
            _bitsPerSample = bitsPerSample;
            _fillOrder = fillOrder;
        }

        /// <inheritdoc />
        public ValueTask InvokeAsync(TiffImageDecoderContext context, ITiffImageDecoderPipelineNode next)
        {
            ThrowHelper.ThrowIfNull(context);
            ThrowHelper.ThrowIfNull(next);

            Span<ushort> bitsPerSample = stackalloc ushort[3];
            _bitsPerSample.CopyTo(bitsPerSample);
            bool isHigherOrderBitsFirst = _fillOrder != TiffFillOrder.LowerOrderBitsFirst;
            bool canDoFastPath = bitsPerSample[0] >= 4 && bitsPerSample[1] >= 4 && bitsPerSample[2] >= 4;
            int totalBitsPerSample = bitsPerSample[0] + bitsPerSample[1] + bitsPerSample[2];

            int bytesPerScanline = (context.SourceImageSize.Width * totalBitsPerSample + 7) / 8;
            Memory<byte> source = context.UncompressedData.Slice(context.SourceReadOffset.Y * bytesPerScanline);
            ReadOnlySpan<byte> sourceSpan = source.Span;

            using TiffPixelBufferWriter<TiffRgb24> writer = context.GetWriter<TiffRgb24>();

            int rows = context.ReadSize.Height;
            int cols = context.ReadSize.Width;

            TiffRgb24 pixel = default;

            for (int row = 0; row < rows; row++)
            {
                using TiffPixelSpanHandle<TiffRgb24> pixelSpanHandle = writer.GetRowSpan(row);
                Span<TiffRgb24> pixelSpan = pixelSpanHandle.GetSpan();
                var bitReader = new BitReader(sourceSpan.Slice(0, bytesPerScanline), isHigherOrderBitsFirst);
                bitReader.Advance(context.SourceReadOffset.X * totalBitsPerSample);

                if (canDoFastPath)
                {
                    // Fast path for bits >= 8
                    for (int col = 0; col < cols; col++)
                    {
                        pixel.R = (byte)FastExpandBits(bitReader.Read(bitsPerSample[0]), bitsPerSample[0], 8);
                        pixel.G = (byte)FastExpandBits(bitReader.Read(bitsPerSample[1]), bitsPerSample[1], 8);
                        pixel.B = (byte)FastExpandBits(bitReader.Read(bitsPerSample[2]), bitsPerSample[2], 8);
                        pixelSpan[col] = pixel;
                    }
                }
                else
                {
                    // Slow path
                    for (int col = 0; col < cols; col++)
                    {
                        pixel.R = (byte)ExpandBits(bitReader.Read(bitsPerSample[0]), bitsPerSample[0], 8);
                        pixel.G = (byte)ExpandBits(bitReader.Read(bitsPerSample[1]), bitsPerSample[1], 8);
                        pixel.B = (byte)ExpandBits(bitReader.Read(bitsPerSample[2]), bitsPerSample[2], 8);
                        pixelSpan[col] = pixel;
                    }
                }

                sourceSpan = sourceSpan.Slice(bytesPerScanline);
            }

            return next.RunAsync(context);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint FastExpandBits(uint bits, int bitCount, int targetBitCount)
        {
            Debug.Assert(bitCount * 2 >= targetBitCount);
            int remainingBits = targetBitCount - bitCount;
            return (bits << remainingBits) | (bits & ((uint)(1 << remainingBits) - 1));
        }

        private static uint ExpandBits(uint bits, int bitCount, int targetBitCount)
        {
            int currentBitCount = bitCount;
            while (currentBitCount < targetBitCount)
            {
                bits = (bits << bitCount) | bits;
                currentBitCount += bitCount;
            }

            if (currentBitCount > targetBitCount)
            {
                bits = bits >> bitCount;
                currentBitCount -= bitCount;
                return FastExpandBits(bits, currentBitCount, targetBitCount);
            }

            return bits;
        }
    }
}
