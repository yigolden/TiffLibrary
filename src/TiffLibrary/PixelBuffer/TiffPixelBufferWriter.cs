using TiffLibrary.PixelBuffer;

namespace TiffLibrary
{
#pragma warning disable CA1815 // CA1815: Override equals and operator equals on value types
    /// <summary>
    /// Represents a write-only 2-dimensional region or sub-region of pixel buffer.
    /// </summary>
    /// <typeparam name="TPixel">The pixel type.</typeparam>
    public readonly struct TiffPixelBufferWriter<TPixel> : ITiffPixelBufferWriter<TPixel> where TPixel : unmanaged
#pragma warning restore CA1815 // CA1815: Override equals and operator equals on value types
    {
        internal readonly ITiffPixelBufferWriter<TPixel> _writer;
        internal readonly TiffPoint _offset;
        internal readonly TiffSize _size;

        internal TiffPixelBufferWriter(ITiffPixelBufferWriter<TPixel> writer, TiffPoint offset, TiffSize size)
        {
            _writer = writer;
            _offset = offset;
            _size = size;
        }

        /// <summary>
        /// Initialize <see cref="TiffPixelBufferWriter{TPixel}"/> to wrap <paramref name="writer"/> and represents the same region as <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The pixel buffer writer. If <paramref name="writer"/> is null, <see cref="TiffPixelBufferWriter{TPixel}"/> will be empty and represents an emtry 2-dimensional region.</param>
        public TiffPixelBufferWriter(ITiffPixelBufferWriter<TPixel> writer)
        {
            _writer = writer ?? TiffEmptyPixelBufferWriter<TPixel>.Default;
            if (_writer is TiffPixelBufferWriter<TPixel> structWriter)
            {
                _writer = structWriter._writer;
                _offset = structWriter._offset;
                _size = structWriter._size;
            }
            else
            {
                _offset = default;
                _size = new TiffSize(_writer.Width, _writer.Height);
            }
        }

        /// <inheritdoc />
        public int Width => _size.Width;

        /// <inheritdoc />
        public int Height => _size.Height;

        /// <summary>
        /// Gets whether this region is empty, or the area of the region is zero.
        /// </summary>
        public bool IsEmpty => _writer is null || _size.IsAreaEmpty;


        /// <summary>
        /// Gets a <see cref="TiffPixelSpanHandle{TPixel}"/> representing a write-only span of the <paramref name="rowIndex"/> row of the region. <see cref="TiffPixelSpanHandle{TPixel}"/> should be disposed after use to flush the pixels into the underlying storage.
        /// </summary>
        /// <param name="rowIndex">The row index.</param>
        /// <returns>A <see cref="TiffPixelSpanHandle{TPixel}"/> representing the temporary buffer consumer can write to. When disposed, it will flush the temporary pixels into the actual storage.</returns>
        public TiffPixelSpanHandle<TPixel> GetRowSpan(int rowIndex)
        {
            if ((uint)rowIndex >= (uint)_size.Height)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(rowIndex));
            }
            if (_writer is null)
            {
                return TiffEmptyRowSpanHandle<TPixel>.Default;
            }

            return _writer.GetRowSpan(rowIndex + _offset.Y, _offset.X, _size.Width);
        }

        /// <summary>
        /// Gets a <see cref="TiffPixelSpanHandle{TPixel}"/> representing a write-only span of the <paramref name="rowIndex"/> row of the region, while skipping <paramref name="start"/> pixels and limiting the length of the span to <paramref name="length"/>. <see cref="TiffPixelSpanHandle{TPixel}"/> should be disposed after use to flush the pixels into the underlying storage.
        /// </summary>
        /// <param name="rowIndex">The row index.</param>
        /// <param name="start">Number of pixels to skip in this row.</param>
        /// <param name="length">Number of pixels to write.</param>
        /// <returns>A <see cref="TiffPixelSpanHandle{TPixel}"/> representing the temporary buffer consumer can write to. When disposed, it will flush the temporary pixels into the actual storage.</returns>
        public TiffPixelSpanHandle<TPixel> GetRowSpan(int rowIndex, int start, int length)
        {
            if ((uint)rowIndex >= (uint)_size.Height)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(rowIndex));
            }
            if ((uint)start > (uint)_size.Width)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(start));
            }
            if ((uint)(start + length) > (uint)_size.Width)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(length));
            }

            if (_writer is null)
            {
                return TiffEmptyRowSpanHandle<TPixel>.Default;
            }

            return _writer.GetRowSpan(rowIndex + _offset.Y, _offset.X + start, length);
        }


        /// <summary>
        /// Gets a <see cref="TiffPixelSpanHandle{TPixel}"/> representing a write-only span of the <paramref name="colIndex"/> column of the region, while skipping <paramref name="start"/> pixels and limiting the length of the span to <paramref name="length"/>. <see cref="TiffPixelSpanHandle{TPixel}"/> should be disposed after use to flush the pixels into the underlying storage.
        /// </summary>
        /// <param name="colIndex">The column index.</param>
        /// <param name="start">Number of pixels to skip in this column.</param>
        /// <param name="length">Number of pixels to write.</param>
        /// <returns>A <see cref="TiffPixelSpanHandle{TPixel}"/> representing the temporary buffer consumer can write to. When disposed, it will flush the temporary pixels into the actual storage.</returns>
        public TiffPixelSpanHandle<TPixel> GetColumnSpan(int colIndex, int start, int length)
        {
            if ((uint)colIndex >= (uint)_size.Width)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(colIndex));
            }
            if ((uint)start > (uint)_size.Height)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(start));
            }
            if ((uint)(start + length) > (uint)_size.Height)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(length));
            }

            if (_writer is null)
            {
                return TiffEmptyRowSpanHandle<TPixel>.Default;
            }

            return _writer.GetColumnSpan(colIndex + _offset.X, _offset.Y + start, length);
        }

        /// <summary>
        /// Dispose the underlying writer.
        /// </summary>
        public void Dispose()
        {
            _writer?.Dispose();
        }
    }
}
