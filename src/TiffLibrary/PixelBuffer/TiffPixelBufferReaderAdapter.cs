using System;
using System.Threading;
using System.Threading.Tasks;

namespace TiffLibrary.PixelBuffer
{
    /// <summary>
    /// Uses <see cref="ITiffPixelBuffer{TPixel}"/> as the underlying storage. Provides <see cref="ITiffPixelBufferReader{TPixel}"/> API to read pixels from <see cref="ITiffPixelBuffer{TPixel}"/>.
    /// </summary>
    /// <typeparam name="TPixel"></typeparam>
    public sealed class TiffPixelBufferReaderAdapter<TPixel> : ITiffPixelBufferReader<TPixel> where TPixel : unmanaged
    {
        private readonly ITiffPixelBuffer<TPixel> _buffer;
        private readonly TiffSize _size;

        /// <summary>
        /// Initialize the object to wrap <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The pixel buffer to wrap.</param>
        public TiffPixelBufferReaderAdapter(ITiffPixelBuffer<TPixel> buffer)
        {
            ThrowHelper.ThrowIfNull(buffer);
            _buffer = buffer;
            _size = new TiffSize(buffer.Width, buffer.Height);
        }

        /// <inheritdoc />
        public int Width => _size.Width;

        /// <inheritdoc />
        public int Height => _size.Height;

        /// <inheritdoc />
        public ValueTask ReadAsync(TiffPoint offset, TiffPixelBufferWriter<TPixel> destination, CancellationToken cancellationToken)
        {
            if (offset.X >= (uint)_size.Width || offset.Y >= (uint)_size.Height)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(offset));
            }

            int width = Math.Min(_size.Width - offset.X, destination.Width);
            int height = Math.Min(_size.Height - offset.Y, destination.Height);

            ReadOnlySpan<TPixel> buffer = _buffer.GetReadOnlySpan();
            int bufferWidth = _size.Width;

            for (int row = 0; row < height; row++)
            {
                ReadOnlySpan<TPixel> sourceSpan = buffer.Slice(bufferWidth * (offset.Y + row) + offset.X, width);
                using TiffPixelSpanHandle<TPixel> destinationHandle = destination.GetRowSpan(row);
                sourceSpan.CopyTo(destinationHandle.GetSpan());
            }

            return default;
        }
    }
}
