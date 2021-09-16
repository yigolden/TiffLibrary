using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace TiffLibrary.ImageDecoder
{
    internal sealed class Jpeg8BitSampleExpansionMiddleware : ITiffImageDecoderMiddleware
    {
        private readonly int _bitCount;

        public Jpeg8BitSampleExpansionMiddleware(int bitCount)
        {
            Debug.Assert(bitCount > 0 && bitCount <= 8);
            _bitCount = bitCount;
        }

        public ValueTask InvokeAsync(TiffImageDecoderContext context, ITiffImageDecoderPipelineNode next)
        {
            ThrowHelper.ThrowIfNull(context);
            ThrowHelper.ThrowIfNull(next);

            int bitCount = _bitCount;
            Span<byte> uncompressedData = context.UncompressedData.Span;

            for (int i = 0; i < uncompressedData.Length; i++)
            {
                uncompressedData[i] = (byte)FastExpandBits(uncompressedData[i], bitCount);
            }

            return next.RunAsync(new JpegDataEndianContextWrapper(context));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint FastExpandBits(uint bits, int bitCount)
        {
            const int TargetBitCount = 8;
            Debug.Assert(bitCount * 2 >= TargetBitCount);
            int remainingBits = TargetBitCount - bitCount;
            return (bits << remainingBits) | (bits & ((uint)(1 << remainingBits) - 1));
        }
    }
}
