using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace TiffLibrary.ImageDecoder
{
    internal sealed class Jpeg16BitSampleExpansionMiddleware : ITiffImageDecoderMiddleware
    {
        private readonly int _bitCount;

        public Jpeg16BitSampleExpansionMiddleware(int bitCount)
        {
            Debug.Assert(bitCount > 8 && bitCount <= 16);
            _bitCount = bitCount;
        }

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

            int bitCount = _bitCount;
            Span<ushort> uncompressedData = MemoryMarshal.Cast<byte, ushort>(context.UncompressedData.Span);

            for (int i = 0; i < uncompressedData.Length; i++)
            {
                uncompressedData[i] = (ushort)FastExpandBits(uncompressedData[i], bitCount);
            }

            return next.RunAsync(new JpegDataEndianContextWrapper(context));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint FastExpandBits(uint bits, int bitCount)
        {
            const int TargetBitCount = 16;
            Debug.Assert(bitCount * 2 >= TargetBitCount);
            int remainingBits = TargetBitCount - bitCount;
            return (bits << remainingBits) | (bits & ((uint)(1 << remainingBits) - 1));
        }
    }
}
