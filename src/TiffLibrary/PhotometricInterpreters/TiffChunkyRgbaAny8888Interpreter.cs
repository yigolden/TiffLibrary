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
    /// A middleware to read any bits (less than 8 bits) RGBA pixels from uncompressed data to destination buffer writer.
    /// </summary>
    public sealed class TiffChunkyRgbaAny8888Interpreter : ITiffImageDecoderMiddleware
    {
        private readonly TiffValueCollection<ushort> _bitsPerSample;
        private readonly TiffFillOrder _fillOrder;

        private readonly bool _isAlphaAssociated;
        private readonly bool _undoColorPreMultiplying;

        /// <summary>
        /// Initialize the middleware.
        /// </summary>
        /// <param name="isAlphaAssociated">Whether the alpha channel is associated.</param>
        /// <param name="undoColorPreMultiplying">Whether to undo color pre-multiplying.</param>
        /// <param name="bitsPerSample">The BitsPerSample flags.</param>
        /// <param name="fillOrder">The FillOrder tag.</param>
        [CLSCompliant(false)]
        public TiffChunkyRgbaAny8888Interpreter(bool isAlphaAssociated, bool undoColorPreMultiplying, TiffValueCollection<ushort> bitsPerSample, TiffFillOrder fillOrder = TiffFillOrder.HigherOrderBitsFirst)
        {
            _isAlphaAssociated = isAlphaAssociated;
            _undoColorPreMultiplying = undoColorPreMultiplying;
            if (bitsPerSample.Count != 4)
            {
                throw new ArgumentOutOfRangeException(nameof(bitsPerSample));
            }
            if ((uint)bitsPerSample[0] > 8 || (uint)bitsPerSample[1] > 8 || (uint)bitsPerSample[2] > 8 || (uint)bitsPerSample[3] > 8)
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

            Span<ushort> bitsPerSample = stackalloc ushort[4];
            _bitsPerSample.CopyTo(bitsPerSample);
            bool isHigherOrderBitsFirst = _fillOrder != TiffFillOrder.LowerOrderBitsFirst;
            bool canDoFastPath = bitsPerSample[0] >= 4 && bitsPerSample[1] >= 4 && bitsPerSample[2] >= 4 && bitsPerSample[3] >= 4;
            int totalBitsPerSample = bitsPerSample[0] + bitsPerSample[1] + bitsPerSample[2] + bitsPerSample[3];

            int bytesPerScanline = (context.SourceImageSize.Width * totalBitsPerSample + 7) / 8;
            Memory<byte> source = context.UncompressedData.Slice(context.SourceReadOffset.Y * bytesPerScanline);
            ReadOnlySpan<byte> sourceSpan = source.Span;

            using TiffPixelBufferWriter<TiffRgba32> writer = context.GetWriter<TiffRgba32>();

            int rows = context.ReadSize.Height;
            int cols = context.ReadSize.Width;

            TiffRgba32 pixel = default;
            pixel.A = byte.MaxValue;

            for (int row = 0; row < rows; row++)
            {
                using TiffPixelSpanHandle<TiffRgba32> pixelSpanHandle = writer.GetRowSpan(row);
                Span<TiffRgba32> pixelSpan = pixelSpanHandle.GetSpan();
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
                        pixel.A = (byte)FastExpandBits(bitReader.Read(bitsPerSample[3]), bitsPerSample[3], 8);
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
                        pixel.A = (byte)ExpandBits(bitReader.Read(bitsPerSample[3]), bitsPerSample[3], 8);
                        pixelSpan[col] = pixel;
                    }
                }

                if (_isAlphaAssociated)
                {
                    if (_undoColorPreMultiplying)
                    {
                        UndoColorPreMultiplying(pixelSpan);
                    }
                    else
                    {
                        WipeAlphaChanel(pixelSpan);
                    }
                }

                sourceSpan = sourceSpan.Slice(bytesPerScanline);
            }

            return next.RunAsync(context);
        }

        private static void WipeAlphaChanel(Span<TiffRgba32> pixels)
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i].A = byte.MaxValue;
            }
        }

        private static void UndoColorPreMultiplying(Span<TiffRgba32> pixels)
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                TiffRgba32 pixel = pixels[i];
                int a = pixel.A;
                if (a == 0)
                {
                    pixels[i] = default;
                }
                else
                {
                    pixel.R = (byte)(pixel.R * 0xff / a);
                    pixel.G = (byte)(pixel.G * 0xff / a);
                    pixel.B = (byte)(pixel.B * 0xff / a);
                    pixels[i] = pixel;
                }
            }
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
