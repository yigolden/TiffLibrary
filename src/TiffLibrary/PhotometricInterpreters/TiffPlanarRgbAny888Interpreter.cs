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
    /// A middleware to read any bits (less than 8 bits) RGB planar pixels from uncompressed data to destination buffer writer.
    /// </summary>
    public sealed class TiffPlanarRgbAny888Interpreter : ITiffImageDecoderMiddleware
    {
        private readonly TiffValueCollection<ushort> _bitsPerSample;
        private readonly TiffFillOrder _fillOrder;

        /// <summary>
        /// Initialize the middleware with the specified bits per sample and fill order.
        /// </summary>
        /// <param name="bitsPerSample">The BitsPerSample flags.</param>
        /// <param name="fillOrder">The FillOrder tag.</param>
        public TiffPlanarRgbAny888Interpreter(TiffValueCollection<ushort> bitsPerSample, TiffFillOrder fillOrder = TiffFillOrder.HigherOrderBitsFirst)
        {
            if (bitsPerSample.Count != 3)
            {
                throw new ArgumentOutOfRangeException(nameof(bitsPerSample));
            }
            if ((uint)bitsPerSample[0] > 8 || (uint)bitsPerSample[1] > 8 || (uint)bitsPerSample[2] > 8)
            {
                throw new ArgumentOutOfRangeException(nameof(bitsPerSample));
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
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (next is null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            Span<ushort> bitsPerSample = stackalloc ushort[3];
            _bitsPerSample.CopyTo(bitsPerSample);
            bool isHigherOrderBitsFirst = _fillOrder != TiffFillOrder.LowerOrderBitsFirst;
            bool canDoFastPath = bitsPerSample[0] >= 4 && bitsPerSample[1] >= 4 && bitsPerSample[2] >= 4;

            int bytesPerScanlineR = (context.SourceImageSize.Width * bitsPerSample[0] + 7) / 8;
            int bytesPerScanlineG = (context.SourceImageSize.Width * bitsPerSample[1] + 7) / 8;
            int bytesPerScanlineB = (context.SourceImageSize.Width * bitsPerSample[2] + 7) / 8;

            ReadOnlySpan<byte> sourceSpan = context.UncompressedData.Span;
            ReadOnlySpan<byte> sourceR = sourceSpan.Slice(0, context.SourceImageSize.Height * bytesPerScanlineR);
            ReadOnlySpan<byte> sourceG = sourceSpan.Slice(sourceR.Length, context.SourceImageSize.Height * bytesPerScanlineG);
            ReadOnlySpan<byte> sourceB = sourceSpan.Slice(sourceR.Length + sourceG.Length, context.SourceImageSize.Height * bytesPerScanlineB);

            sourceR = sourceR.Slice(context.SourceReadOffset.Y * bytesPerScanlineR);
            sourceG = sourceG.Slice(context.SourceReadOffset.Y * bytesPerScanlineG);
            sourceB = sourceB.Slice(context.SourceReadOffset.Y * bytesPerScanlineB);

            using TiffPixelBufferWriter<TiffRgba32> writer = context.GetWriter<TiffRgba32>();

            int rows = context.ReadSize.Height;
            int cols = context.ReadSize.Width;

            TiffRgba32 pixel = default;
            pixel.A = byte.MaxValue;

            for (int row = 0; row < rows; row++)
            {
                using TiffPixelSpanHandle<TiffRgba32> pixelSpanHandle = writer.GetRowSpan(row);
                Span<TiffRgba32> pixelSpan = pixelSpanHandle.GetSpan();
                var bitReaderR = new BitReader(sourceR.Slice(0, bytesPerScanlineR), isHigherOrderBitsFirst);
                var bitReaderG = new BitReader(sourceG.Slice(0, bytesPerScanlineG), isHigherOrderBitsFirst);
                var bitReaderB = new BitReader(sourceB.Slice(0, bytesPerScanlineB), isHigherOrderBitsFirst);
                bitReaderR.Advance(context.SourceReadOffset.X * bitsPerSample[0]);
                bitReaderG.Advance(context.SourceReadOffset.X * bitsPerSample[1]);
                bitReaderB.Advance(context.SourceReadOffset.X * bitsPerSample[2]);

                if (canDoFastPath)
                {
                    // Fast path for bits >= 8
                    for (int col = 0; col < cols; col++)
                    {
                        pixel.R = (byte)FastExpandBits(bitReaderR.Read(bitsPerSample[0]), bitsPerSample[0], 8);
                        pixel.G = (byte)FastExpandBits(bitReaderG.Read(bitsPerSample[1]), bitsPerSample[1], 8);
                        pixel.B = (byte)FastExpandBits(bitReaderB.Read(bitsPerSample[2]), bitsPerSample[2], 8);
                        pixelSpan[col] = pixel;
                    }
                }
                else
                {
                    // Slow path
                    for (int col = 0; col < cols; col++)
                    {
                        pixel.R = (byte)ExpandBits(bitReaderR.Read(bitsPerSample[0]), bitsPerSample[0], 8);
                        pixel.G = (byte)ExpandBits(bitReaderG.Read(bitsPerSample[1]), bitsPerSample[1], 8);
                        pixel.B = (byte)ExpandBits(bitReaderB.Read(bitsPerSample[2]), bitsPerSample[2], 8);
                        pixelSpan[col] = pixel;
                    }
                }

                sourceR = sourceR.Slice(bytesPerScanlineR);
                sourceG = sourceG.Slice(bytesPerScanlineG);
                sourceB = sourceB.Slice(bytesPerScanlineB);
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
