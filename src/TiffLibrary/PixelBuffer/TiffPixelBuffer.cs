using TiffLibrary.PixelBuffer;

namespace TiffLibrary
{
#pragma warning disable CA1815 // CA1815: Override equals and operator equals on value types
    /// <summary>
    /// Represents a 2-dimensional region or sub-region of pixels in a contiguous memory buffer in row-major order.
    /// </summary>
    /// <typeparam name="TPixel">The pixel type.</typeparam>
    public readonly struct TiffPixelBuffer<TPixel> where TPixel : unmanaged
#pragma warning restore CA1815 // CA1815: Override equals and operator equals on value types
    {
        internal readonly ITiffPixelBuffer<TPixel> _buffer;
        internal readonly TiffPoint _offset;
        internal readonly TiffSize _size;

        internal TiffPixelBuffer(ITiffPixelBuffer<TPixel> buffer, TiffPoint offset, TiffSize size)
        {
            _buffer = buffer;
            _offset = offset;
            _size = size;
        }

        /// <summary>
        /// Initialize <see cref="TiffPixelBuffer{TPixel}"/> to wrap <paramref name="buffer"/> and represents the same region as <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The pixel buffer. If <paramref name="buffer"/> is null, <see cref="TiffPixelBuffer{TPixel}"/> will be empty and represents an emtry 2-dimensional region.</param>
        public TiffPixelBuffer(ITiffPixelBuffer<TPixel> buffer)
        {
            _buffer = buffer ?? TiffEmptyPixelBuffer<TPixel>.Default;
            _offset = default;
            _size = new TiffSize(_buffer.Width, _buffer.Height);
        }

        /// <summary>
        /// The number of columns in the region.
        /// </summary>
        public int Width => _size.Width;

        /// <summary>
        /// The number of rows in the region.
        /// </summary>
        public int Height => _size.Height;

        /// <summary>
        /// Gets whether this region is empty, or the area of the region is zero.
        /// </summary>
        public bool IsEmpty => _buffer is null || _size.IsAreaEmpty;

        /// <summary>
        /// Creates a <see cref="TiffPixelBufferWriter{TPixel}"/> adapter to write pixels into the current pixel buffer.
        /// </summary>
        /// <returns>The <see cref="TiffPixelBufferWriter{TPixel}"/> adapter.</returns>
        public TiffPixelBufferWriter<TPixel> CreateWriter()
        {
            return new TiffPixelBufferWriter<TPixel>(_buffer is null ? TiffEmptyPixelBufferWriter<TPixel>.Default : new TiffPixelBufferWriterAdapter<TPixel>(_buffer), _offset, _size);
        }

        /// <summary>
        /// Creates a <see cref="TiffPixelBufferReader{TPixel}"/> adapter to reader pixels from the current pixel buffer.
        /// </summary>
        /// <returns>The <see cref="TiffPixelBufferReader{TPixel}"/> adapter.</returns>
        public TiffPixelBufferReader<TPixel> CreateReader()
        {
            return new TiffPixelBufferReader<TPixel>(_buffer is null ? TiffEmptyPixelBufferReader<TPixel>.Default : new TiffPixelBufferReaderAdapter<TPixel>(_buffer), _offset, _size);
        }
    }
}
