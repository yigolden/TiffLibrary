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
    }
}
