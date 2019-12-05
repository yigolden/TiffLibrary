using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TiffLibrary.ImageDecoder;
using TiffLibrary.PixelBuffer;
using TiffLibrary.PixelFormats;

namespace TiffLibrary.PhotometricInterpreters
{
    /// <summary>
    /// A middleware to read any bits (less than 16 bits) RGBA planar pixels from uncompressed data to destination buffer writer.
    /// </summary>
    public sealed class TiffPlanarRgbaAny16161616Interpreter : ITiffImageDecoderMiddleware
    {
        private readonly TiffValueCollection<ushort> _bitsPerSample;
        private readonly TiffFillOrder _fillOrder;

        private readonly bool _isAlphaAssociated;
        private readonly bool _undoColorPreMultiplying;

        /// <summary>
        /// Initialize the middleware with the specified bits per sample and fill order.
        /// </summary>
        /// <param name="isAlphaAssociated">Whether the alpha channel is associated.</param>
        /// <param name="undoColorPreMultiplying">Whether to undo color pre-multiplying.</param>
        /// <param name="bitsPerSample">The BitsPerSample flags.</param>
        /// <param name="fillOrder">The FillOrder tag.</param>
        public TiffPlanarRgbaAny16161616Interpreter(bool isAlphaAssociated, bool undoColorPreMultiplying, TiffValueCollection<ushort> bitsPerSample, TiffFillOrder fillOrder = TiffFillOrder.HigherOrderBitsFirst)
        {
            _isAlphaAssociated = isAlphaAssociated;
            _undoColorPreMultiplying = undoColorPreMultiplying;
            if (bitsPerSample.Count != 4)
            {
                throw new ArgumentOutOfRangeException(nameof(bitsPerSample));
            }
            if ((uint)bitsPerSample[0] > 16 || (uint)bitsPerSample[1] > 16 || (uint)bitsPerSample[2] > 16 || (uint)bitsPerSample[3] > 16)
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

        /// <summary>
        /// Run this middleware.
        /// </summary>
        /// <param name="context">Information of the current decoding process.</param>
        /// <param name="next">The next middleware in the decoder pipeline.</param>
        /// <returns>A <see cref="Task"/> that completes when this middleware completes running.</returns>
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

            Span<ushort> bitsPerSample = stackalloc ushort[4];
            _bitsPerSample.CopyTo(bitsPerSample);
            bool isHigherOrderBitsFirst = _fillOrder != TiffFillOrder.LowerOrderBitsFirst;

            int bytesPerScanlineR = (context.SourceImageSize.Width * bitsPerSample[0] + 7) / 8;
            int bytesPerScanlineG = (context.SourceImageSize.Width * bitsPerSample[1] + 7) / 8;
            int bytesPerScanlineB = (context.SourceImageSize.Width * bitsPerSample[2] + 7) / 8;
            int bytesPerScanlineA = (context.SourceImageSize.Width * bitsPerSample[3] + 7) / 8;

            ReadOnlySpan<byte> sourceSpan = context.UncompressedData.Span;
            ReadOnlySpan<byte> sourceR = sourceSpan.Slice(0, context.SourceImageSize.Height * bytesPerScanlineR);
            ReadOnlySpan<byte> sourceG = sourceSpan.Slice(sourceR.Length, context.SourceImageSize.Height * bytesPerScanlineG);
            ReadOnlySpan<byte> sourceB = sourceSpan.Slice(sourceR.Length + sourceG.Length, context.SourceImageSize.Height * bytesPerScanlineB);
            ReadOnlySpan<byte> sourceA = sourceSpan.Slice(sourceR.Length + sourceG.Length + sourceB.Length, context.SourceImageSize.Height * bytesPerScanlineA);

            sourceR = sourceR.Slice(context.SourceReadOffset.Y * bytesPerScanlineR);
            sourceG = sourceG.Slice(context.SourceReadOffset.Y * bytesPerScanlineG);
            sourceB = sourceB.Slice(context.SourceReadOffset.Y * bytesPerScanlineB);
            sourceA = sourceB.Slice(context.SourceReadOffset.Y * bytesPerScanlineA);

            using TiffPixelBufferWriter<TiffRgba64> writer = context.GetWriter<TiffRgba64>();

            int rows = context.ReadSize.Height;
            int cols = context.ReadSize.Width;

            // BitReader.Read reads bytes in big-endian way, we only need to reverse the endianness if the source is little-endian.
            bool reverseEndiannessR = context.OperationContext.IsLittleEndian && bitsPerSample[0] % 8 == 0;
            bool reverseEndiannessG = context.OperationContext.IsLittleEndian && bitsPerSample[1] % 8 == 0;
            bool reverseEndiannessB = context.OperationContext.IsLittleEndian && bitsPerSample[2] % 8 == 0;
            bool reverseEndiannessA = context.OperationContext.IsLittleEndian && bitsPerSample[3] % 8 == 0;
            bool canDoFastPath = bitsPerSample[0] >= 8 && bitsPerSample[1] >= 8 && bitsPerSample[2] >= 8 && bitsPerSample[3] >= 8
                                 && !reverseEndiannessR & !reverseEndiannessG & !reverseEndiannessB & !reverseEndiannessA;

            TiffRgba64 pixel = default;

            for (int row = 0; row < rows; row++)
            {
                using TiffPixelSpanHandle<TiffRgba64> pixelSpanHandle = writer.GetRowSpan(row);
                Span<TiffRgba64> pixelSpan = pixelSpanHandle.GetSpan();
                var bitReaderR = new BitReader(sourceR.Slice(0, bytesPerScanlineR), isHigherOrderBitsFirst);
                var bitReaderG = new BitReader(sourceG.Slice(0, bytesPerScanlineG), isHigherOrderBitsFirst);
                var bitReaderB = new BitReader(sourceB.Slice(0, bytesPerScanlineB), isHigherOrderBitsFirst);
                var bitReaderA = new BitReader(sourceA.Slice(0, bytesPerScanlineA), isHigherOrderBitsFirst);
                bitReaderR.Advance(context.SourceReadOffset.X * bitsPerSample[0]);
                bitReaderG.Advance(context.SourceReadOffset.X * bitsPerSample[1]);
                bitReaderB.Advance(context.SourceReadOffset.X * bitsPerSample[2]);
                bitReaderA.Advance(context.SourceReadOffset.X * bitsPerSample[3]);

                if (canDoFastPath)
                {
                    // Fast path for bits >= 8
                    for (int col = 0; col < cols; col++)
                    {
                        pixel.R = (ushort)FastExpandBits(bitReaderR.Read(bitsPerSample[0]), bitsPerSample[0], 16);
                        pixel.G = (ushort)FastExpandBits(bitReaderG.Read(bitsPerSample[1]), bitsPerSample[1], 16);
                        pixel.B = (ushort)FastExpandBits(bitReaderB.Read(bitsPerSample[2]), bitsPerSample[2], 16);
                        pixel.A = (ushort)FastExpandBits(bitReaderA.Read(bitsPerSample[3]), bitsPerSample[3], 16);
                        pixelSpan[col] = pixel;
                    }
                }
                else
                {
                    // Slow path
                    for (int col = 0; col < cols; col++)
                    {
                        pixel.R = (ushort)ExpandBits(bitReaderR.Read(bitsPerSample[0]), bitsPerSample[0], 16, reverseEndiannessR);
                        pixel.G = (ushort)ExpandBits(bitReaderG.Read(bitsPerSample[1]), bitsPerSample[1], 16, reverseEndiannessG);
                        pixel.B = (ushort)ExpandBits(bitReaderB.Read(bitsPerSample[2]), bitsPerSample[2], 16, reverseEndiannessB);
                        pixel.A = (ushort)ExpandBits(bitReaderA.Read(bitsPerSample[3]), bitsPerSample[3], 16, reverseEndiannessA);
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

                sourceR = sourceR.Slice(bytesPerScanlineR);
                sourceG = sourceG.Slice(bytesPerScanlineG);
                sourceB = sourceB.Slice(bytesPerScanlineB);
                sourceA = sourceA.Slice(bytesPerScanlineA);
            }

            return next.RunAsync(context);
        }

        private static void WipeAlphaChanel(Span<TiffRgba64> pixels)
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i].A = ushort.MaxValue;
            }
        }

        private static void UndoColorPreMultiplying(Span<TiffRgba64> pixels)
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                TiffRgba64 pixel = pixels[i];
                int a = pixel.A;
                if (a == 0)
                {
                    pixels[i] = default;
                }
                else
                {
                    pixel.R = (ushort)(pixel.R * 0xffff / a);
                    pixel.G = (ushort)(pixel.G * 0xffff / a);
                    pixel.B = (ushort)(pixel.B * 0xffff / a);
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

        private static uint ExpandBits(uint bits, int bitCount, int targetBitCount, bool reverseEndianness)
        {
            if (reverseEndianness)
            {
                Debug.Assert(bitCount % 8 == 0);
                // Left-align
                bits = bits << (32 - bitCount);
                bits = BinaryPrimitives.ReverseEndianness(bits);
            }

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
