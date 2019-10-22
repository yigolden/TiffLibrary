using System;

namespace TiffLibrary.PixelBuffer
{
    internal sealed class TiffEmptyPixelBuffer<TPixel> : ITiffPixelBuffer<TPixel> where TPixel : unmanaged
    {
        public static TiffEmptyPixelBuffer<TPixel> Default { get; } = new TiffEmptyPixelBuffer<TPixel>();

        public int Width => 0;

        public int Height => 0;

        public Span<TPixel> GetSpan()
        {
            return Span<TPixel>.Empty;
        }
    }
}
