using System;

namespace TiffLibrary.ImageSharpAdapter
{
    internal static class MathHelper
    {
        internal static int Clamp(int value, int min, int max) => Math.Min(Math.Max(value, min), max);
    }
}
