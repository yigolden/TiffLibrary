using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TiffLibrary.Utils
{
    internal static class TiffMathHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int DivRem(int a, int b, out int result)
        {
#if NO_MATH_DIVREM
            int div = a / b;
            result = a - (div * b);
            return div;
#else
            return Math.DivRem(a, b, out result);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ClampTo8Bit(short input)
        {
#if NO_MATH_CLAMP
            return (byte)Math.Min(Math.Max(input, (short)0), (short)255);
#else
            return (byte)Math.Clamp(input, (short)0, (short)255);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ClampTo12Bit(short input)
        {
#if NO_MATH_CLAMP
            return (ushort)Math.Min(Math.Max(input, (short)0), (short)0b1111_1111_1111);
#else
            return (ushort)Math.Clamp(input, (short)0, (short)0b1111_1111_1111);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Clamp8Bit(short input, ushort maxValue)
        {
#if NO_MATH_CLAMP
            return (byte)Math.Min(Math.Max(input, (short)0), (short)maxValue);
#else
            return (byte)Math.Clamp(input, (short)0, (short)maxValue);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort Clamp16Bit(ushort input, ushort maxValue)
        {
#if NO_MATH_CLAMP
            return Math.Min(Math.Max(input, (ushort)0), maxValue);
#else
            return Math.Clamp(input, (ushort)0, maxValue);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ClampTo8Bit(long value)
        {
#if NO_MATH_CLAMP
            return (byte)Math.Min(Math.Max(value, 0), 255);
#else
            return (byte)Math.Clamp(value, 0, 255);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte RoundAndClampTo8Bit(float value)
        {
#if NO_MATHF_ROUND
            int input = (int)Math.Round(value);
#else
            int input = (int)MathF.Round(value);
#endif

#if NO_MATH_CLAMP
            return (byte)Math.Min(Math.Max(input, 0), 255);
#else
            return (byte)Math.Clamp(input, 0, 255);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort RoundAndClampTo16Bit(float value)
        {
#if NO_MATHF_ROUND
            int input = (int)Math.Round(value);
#else
            int input = (int)MathF.Round(value);
#endif

#if NO_MATH_CLAMP
            return (ushort)Math.Min(Math.Max(input, 0), ushort.MaxValue);
#else
            return (ushort)Math.Clamp(input, 0, ushort.MaxValue);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RoundToInt32(float value)
        {
#if NO_MATHF_ROUND
            return (int)Math.Round(value);
#else
            return (int)MathF.Round(value);
#endif
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long RoundToInt64(float value)
        {
#if NO_MATHF_ROUND
            return (long)Math.Round(value);
#else
            return (long)MathF.Round(value);
#endif
        }

        private static ReadOnlySpan<byte> Log2DeBruijn => new byte[32]
        {
            00, 09, 01, 10, 13, 21, 02, 29,
            11, 14, 16, 18, 22, 25, 03, 30,
            08, 12, 20, 28, 15, 17, 24, 07,
            19, 27, 23, 06, 26, 05, 04, 31
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Log2(uint value)
        {
#if NO_BIT_OPERATIONS
            return Log2SoftwareFallback(value);
#else
            return BitOperations.Log2(value);
#endif
        }

        /// <summary>
        /// Returns the integer (floor) log of the specified value, base 2.
        /// Note that by convention, input value 0 returns 0 since Log(0) is undefined.
        /// Does not directly use any hardware intrinsics, nor does it incur branching.
        /// </summary>
        /// <param name="value">The value.</param>
        private static int Log2SoftwareFallback(uint value)
        {
            // No AggressiveInlining due to large method size
            // Has conventional contract 0->0 (Log(0) is undefined)

            // Fill trailing zeros with ones, eg 00010010 becomes 00011111
            value |= value >> 01;
            value |= value >> 02;
            value |= value >> 04;
            value |= value >> 08;
            value |= value >> 16;

            // uint.MaxValue >> 27 is always in range [0 - 31] so we use Unsafe.AddByteOffset to avoid bounds check
            return Unsafe.AddByteOffset(
                // Using deBruijn sequence, k=2, n=5 (2^5=32) : 0b_0000_0111_1100_0100_1010_1100_1101_1101u
                ref MemoryMarshal.GetReference(Log2DeBruijn),
                // uint|long -> IntPtr cast on 32-bit platforms does expensive overflow checks not needed here
                (IntPtr)(int)((value * 0x07C4ACDDu) >> 27));
        }
    }
}
