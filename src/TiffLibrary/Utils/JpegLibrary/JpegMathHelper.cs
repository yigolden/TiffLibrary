#nullable enable

using System;
using System.Runtime.CompilerServices;

namespace JpegLibrary
{
    internal static class JpegMathHelper
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RoundToInt32(float value)
        {
#if NO_MATHF
            return (int)Math.Round(value);
#else
            return (int)MathF.Round(value);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short RoundToInt16(float value)
        {
#if NO_MATHF
            return (short)Math.Round(value);
#else
            return (short)MathF.Round(value);
#endif
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Clamp(int value, int min, int max)
        {
#if NO_MATH_CLAMP
            return Math.Min(Math.Max(value, min), max);
#else
            return Math.Clamp(value, min, max);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp(float value, float min, float max)
        {
#if NO_MATH_CLAMP
            return Math.Min(Math.Max(value, min), max);
#else
            return Math.Clamp(value, min, max);
#endif
        }

        public static int CalculateShiftFactor(int value)
        {
            int shift = 0;
            while ((value = value / 2) != 0) shift++;
            return shift;
        }
    }
}
