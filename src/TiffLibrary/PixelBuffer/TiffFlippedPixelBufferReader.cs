using System.Threading;
using System.Threading.Tasks;

namespace TiffLibrary.PixelBuffer
{
    internal sealed class TiffFlippedPixelBufferReader<TPixel> : ITiffPixelBufferReader<TPixel> where TPixel : unmanaged
    {
        private readonly TiffPixelBufferReader<TPixel> _reader;
        private readonly bool _flipLeftRight;
        private readonly bool _flipTopBottom;

        public TiffFlippedPixelBufferReader(TiffPixelBufferReader<TPixel> reader, bool flipLeftRight, bool flipTopBottom)
        {
            _reader = reader;
            _flipLeftRight = flipLeftRight;
            _flipTopBottom = flipTopBottom;
        }

        public int Width => _reader.Width;

        public int Height => _reader.Height;

        public ValueTask ReadAsync(TiffPoint offset, TiffPixelBufferWriter<TPixel> destination, CancellationToken cancellationToken)
        {
            if (_flipLeftRight)
            {
                offset = new TiffPoint(Width - offset.X - destination.Width, offset.Y);
            }
            if (_flipTopBottom)
            {
                offset = new TiffPoint(offset.X, Height - offset.Y - destination.Height);
            }
            destination = new TiffFlippedPixelBufferWriter<TPixel>(destination, _flipLeftRight, _flipTopBottom).AsPixelBufferWriter();
            return _reader.ReadAsync(offset, destination, cancellationToken);
        }
    }
}
