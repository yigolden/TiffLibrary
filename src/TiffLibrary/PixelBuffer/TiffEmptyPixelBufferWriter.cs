using System;

namespace TiffLibrary.PixelBuffer
{
    internal sealed class TiffEmptyPixelBufferWriter<TPixel> : ITiffPixelBufferWriter<TPixel> where TPixel : unmanaged
    {
        public static ITiffPixelBufferWriter<TPixel> Default { get; } = new TiffEmptyPixelBufferWriter<TPixel>();

        public int Width => 0;

        public int Height => 0;

        public void Dispose() { }

        public TiffPixelSpanHandle<TPixel> GetRowSpan(int rowIndex, int start, int length)
        {
            throw new ArgumentOutOfRangeException(nameof(rowIndex));
        }

        public TiffPixelSpanHandle<TPixel> GetColumnSpan(int colIndex, int start, int length)
        {
            throw new ArgumentOutOfRangeException(nameof(colIndex));
        }
    }

}
