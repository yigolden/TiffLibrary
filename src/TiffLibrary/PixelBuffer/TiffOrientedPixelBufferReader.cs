using System.Threading.Tasks;

namespace TiffLibrary.PixelBuffer
{
    internal sealed class TiffOrientedPixelBufferReader<TPixel> : ITiffPixelBufferReader<TPixel> where TPixel : unmanaged
    {
        private readonly TiffPixelBufferReader<TPixel> _reader;
        private readonly bool _flipLeftRight;
        private readonly bool _flipTopBottom;

        public TiffOrientedPixelBufferReader(TiffPixelBufferReader<TPixel> reader, bool flipLeftRight, bool flipTopBottom)
        {
            _reader = reader;
            _flipLeftRight = flipLeftRight;
            _flipTopBottom = flipTopBottom;
        }

        public int Width => _reader.Height;

        public int Height => _reader.Width;

        public ValueTask ReadAsync(TiffPoint offset, TiffPixelBufferWriter<TPixel> destination)
        {
            if (_flipLeftRight)
            {
                offset = new TiffPoint(offset.X, Height - offset.Y - destination.Height);
            }
            if (_flipTopBottom)
            {
                offset = new TiffPoint(Width - offset.X - destination.Width, offset.Y);
            }
            offset = new TiffPoint(offset.Y, offset.X);
            destination = new TiffOrientedPixelBufferWriter<TPixel>(destination, _flipTopBottom, _flipLeftRight).AsPixelBufferWriter();
            return _reader.ReadAsync(offset, destination);
        }
    }
}
