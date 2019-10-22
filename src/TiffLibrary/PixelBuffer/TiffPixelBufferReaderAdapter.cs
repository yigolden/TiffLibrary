using System;
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
            _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
            _size = new TiffSize(buffer.Width, buffer.Height);
        }

        /// <summary>
        /// The number of columns in the region the reader object provides.
        /// </summary>
        public int Width => _size.Width;

        /// <summary>
        /// The number of rows in the region the reader object provides.
        /// </summary>
        public int Height => _size.Height;

        /// <summary>
        /// Copy the 2-dimensional pixel data into <paramref name="destination"/>, after skipping some rows and columns specified in <paramref name="offset"/>.
        /// </summary>
        /// <param name="offset">The number rows and columns to skip. X represents the number of columns to skip; Y represents the number of rows to skip.</param>
        /// <param name="destination">The destination writer. It also limits the number of rows and columns to copy.</param>
        /// <returns>A <see cref="ValueTask"/> that completes when all the requested pixels are copied.</returns>
        public ValueTask ReadAsync(TiffPoint offset, TiffPixelBufferWriter<TPixel> destination)
        {
            if (offset.X >= (uint)_size.Width || offset.Y >= (uint)_size.Height)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            int width = Math.Min(_size.Width - offset.X, destination.Width);
            int height = Math.Min(_size.Height - offset.Y, destination.Height);

            Span<TPixel> buffer = _buffer.GetSpan();
            int bufferWidth = _size.Width;

            for (int row = 0; row < height; row++)
            {
                Span<TPixel> sourceSpan = buffer.Slice(bufferWidth * (offset.Y + row) + offset.X, width);
                using TiffPixelSpanHandle<TPixel> destinationHandle = destination.GetRowSpan(row);
                sourceSpan.CopyTo(destinationHandle.GetSpan());
            }

            return default;
        }
    }
}
