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
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(rowIndex));
            return null!;
        }

        public TiffPixelSpanHandle<TPixel> GetColumnSpan(int colIndex, int start, int length)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(colIndex));
            return null!;
        }
    }

}
