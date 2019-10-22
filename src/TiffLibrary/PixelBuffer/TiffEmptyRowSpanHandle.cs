using System;

namespace TiffLibrary.PixelBuffer
{
    internal sealed class TiffEmptyRowSpanHandle<TPixel> : TiffPixelSpanHandle<TPixel> where TPixel : unmanaged
    {
        public static TiffEmptyRowSpanHandle<TPixel> Default { get; } = new TiffEmptyRowSpanHandle<TPixel>();

        public override int Length => 0;
        public override Span<TPixel> GetSpan() => Span<TPixel>.Empty;
        public override void Dispose() { }
    }
}
