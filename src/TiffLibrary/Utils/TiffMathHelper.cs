using System;
using System.Runtime.CompilerServices;

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
        public static long RoundToInt64(float value)
        {
#if NO_MATHF_ROUND
            return (long)Math.Round(value);
#else
            return (long)MathF.Round(value);
#endif
        }

    }
}
