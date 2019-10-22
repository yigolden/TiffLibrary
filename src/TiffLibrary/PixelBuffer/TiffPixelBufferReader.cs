using System;
using System.Threading.Tasks;
using TiffLibrary.PixelBuffer;

namespace TiffLibrary
{
#pragma warning disable CA1815 // CA1815: Override equals and operator equals on value types
    /// <summary>
    /// Represents a reader object capable of copying 2-dimensional pixel data from its storage into a specified <see cref="TiffPixelBufferWriter{TPixel}"/>.
    /// </summary>
    /// <typeparam name="TPixel">The pixel type.</typeparam>
    public readonly struct TiffPixelBufferReader<TPixel> : ITiffPixelBufferReader<TPixel> where TPixel : unmanaged
#pragma warning restore CA1815 // CA1815: Override equals and operator equals on value types
    {
        internal readonly ITiffPixelBufferReader<TPixel> _reader;
        internal readonly TiffPoint _offset;
        internal readonly TiffSize _size;

        internal TiffPixelBufferReader(ITiffPixelBufferReader<TPixel> reader, TiffPoint offset, TiffSize size)
        {
            _reader = reader;
            if (_reader is TiffPixelBufferReader<TPixel> structReader)
            {
                _reader = structReader._reader;
                _offset = structReader._offset;
                _size = structReader._size;
            }
            else
            {
                _offset = offset;
                _size = size;
            }
        }

        /// <summary>
        /// Initialize <see cref="TiffPixelBufferReader{TPixel}"/> to wrap <paramref name="reader"/>.
        /// </summary>
        /// <param name="reader">The reader to wrap.</param>
        public TiffPixelBufferReader(ITiffPixelBufferReader<TPixel> reader)
        {
            _reader = reader ?? TiffEmptyPixelBufferReader<TPixel>.Default;
            _offset = default;
            _size = new TiffSize(reader.Width, reader.Height);
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
        /// Gets whether this region is empty, or the area of the region is zero.
        /// </summary>
        public bool IsEmpty => _reader is null || _size.IsAreaEmpty;

        /// <summary>
        /// Copy the 2-dimensional pixel data into <paramref name="destination"/>, after skipping some rows and columns specified in <paramref name="offset"/>.
        /// </summary>
        /// <param name="offset">The number rows and columns to skip. X represents the number of columns to skip; Y represents the number of rows to skip.</param>
        /// <param name="destination">The destination writer. It also limits the number of rows and columns to copy.</param>
        /// <returns>A <see cref="ValueTask"/> that completes when all the requested pixels are copied.</returns>
        public ValueTask ReadAsync(TiffPoint offset, TiffPixelBufferWriter<TPixel> destination)
        {
            offset = new TiffPoint(offset.X + _offset.X, offset.Y + _offset.Y);
            var readSize = new TiffSize(Math.Min(_size.Width, destination.Width), Math.Min(_size.Height, destination.Height));
            if (readSize.IsAreaEmpty)
            {
                return default;
            }
            return _reader.ReadAsync(offset, destination.Crop(default, readSize));
        }

        /// <summary>
        /// Copy the 2-dimensional pixel data into <paramref name="destination"/>.
        /// </summary>
        /// <param name="destination">The destination writer. It limits the number of rows and columns to copy.</param>
        /// <returns>A <see cref="ValueTask"/> that completes when all the requested pixels are copied.</returns>
        public ValueTask ReadAsync(TiffPixelBufferWriter<TPixel> destination)
        {
            var readSize = new TiffSize(Math.Min(_size.Width, destination.Width), Math.Min(_size.Height, destination.Height));
            if (readSize.IsAreaEmpty)
            {
                return default;
            }
            return _reader.ReadAsync(_offset, destination.Crop(default, readSize));
        }
    }
}
