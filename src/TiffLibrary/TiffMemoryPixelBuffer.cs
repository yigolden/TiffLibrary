using System;

namespace TiffLibrary
{
    /// <summary>
    /// A 2-dimensional region of pixels in a contiguous memory buffer in row-major order.
    /// </summary>
    /// <typeparam name="TPixel"></typeparam>
    public sealed class TiffMemoryPixelBuffer<TPixel> : ITiffPixelBuffer<TPixel> where TPixel : unmanaged
    {
        private readonly Memory<TPixel> _buffer;
        private readonly int _width;
        private readonly int _height;

        /// <summary>
        /// Initialize the region with the specified <see cref="Memory{TPixel}"/>.
        /// </summary>
        /// <param name="buffer">The memory buffer.</param>
        /// <param name="width">The width of the region.</param>
        /// <param name="height">The height of the region.</param>
        public TiffMemoryPixelBuffer(Memory<TPixel> buffer, int width, int height)
        {
            if (buffer.Length < width * height)
            {
                throw new ArgumentException("buffer is too small.");
            }
            if (width < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }
            if (height < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }
            _buffer = buffer.Slice(0, width * height);
            _width = width;
            _height = height;
        }

        /// <summary>
        /// The number of columns in the region.
        /// </summary>
        public int Width => _width;

        /// <summary>
        /// The number of rows in the region.
        /// </summary>
        public int Height => _height;

        /// <summary>
        /// Gets a <see cref="Span{TPixel}"/> representing memory of the entire region in row-major order.
        /// </summary>
        /// <returns>A <see cref="Span{TPixel}"/> representing memory of the entire region in row-major order.</returns>
        public Span<TPixel> GetSpan()
        {
            return _buffer.Span.Slice(0, _width * _height);
        }
    }
}
