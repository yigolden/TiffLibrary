using System;

namespace TiffLibrary
{
    /// <summary>
    /// Contains extension methods to manipulate <see cref="TiffPixelBufferReader{TPixel}"/> structure.
    /// </summary>
    public static class TiffPixelBufferReaderExtensions
    {
        /// <summary>
        /// Wraps <paramref name="reader"/> in <see cref="TiffPixelBufferReader{TPixel}"/> structure.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="reader">The pixel buffer reader.</param>
        /// <returns>A <see cref="TiffPixelBufferReader{TPixel}"/> wrapping <paramref name="reader"/>.</returns>
        public static TiffPixelBufferReader<TPixel> AsPixelBufferReader<TPixel>(this ITiffPixelBufferReader<TPixel> reader) where TPixel : unmanaged
        {
            if (reader is TiffPixelBufferReader<TPixel> structReader)
            {
                return structReader;
            }
            return new TiffPixelBufferReader<TPixel>(reader);
        }

        /// <summary>
        /// Crop a sub region from <paramref name="reader"/>.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="reader">The original pixel buffer.</param>
        /// <param name="offset">The number of columns and rows to skip.</param>
        /// <returns>A <see cref="TiffPixelBuffer{TPixel}"/> representing the cropped region.</returns>
        public static TiffPixelBufferReader<TPixel> Crop<TPixel>(this ITiffPixelBufferReader<TPixel> reader, TiffPoint offset) where TPixel : unmanaged
        {
            if (reader is TiffPixelBufferReader<TPixel> structReader)
            {
                return structReader.Crop(offset);
            }
            return reader.AsPixelBufferReader().Crop(offset);
        }

        /// <summary>
        /// Crop a sub region from <paramref name="reader"/>.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="reader">The original pixel buffer.</param>
        /// <param name="offset">The number of columns and rows to skip.</param>
        /// <param name="size">The number of columns and rows to take.</param>
        /// <returns>A <see cref="TiffPixelBuffer{TPixel}"/> representing the cropped region.</returns>
        public static TiffPixelBufferReader<TPixel> Crop<TPixel>(this ITiffPixelBufferReader<TPixel> reader, TiffPoint offset, TiffSize size) where TPixel : unmanaged
        {
            if (reader is TiffPixelBufferReader<TPixel> structReader)
            {
                return structReader.Crop(offset, size);
            }
            return reader.AsPixelBufferReader().Crop(offset, size);
        }

        /// <summary>
        /// Crop a sub region from <paramref name="reader"/>.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="reader">The original pixel buffer.</param>
        /// <param name="offset">The number of columns and rows to skip.</param>
        /// <returns>A <see cref="TiffPixelBuffer{TPixel}"/> representing the cropped region.</returns>
        public static TiffPixelBufferReader<TPixel> Crop<TPixel>(this TiffPixelBufferReader<TPixel> reader, TiffPoint offset) where TPixel : unmanaged
        {
            if ((uint)offset.X > (uint)reader._size.Width)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
            if ((uint)offset.Y > (uint)reader._size.Height)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
            int offsetX = reader._offset.X + offset.X;
            int offsetY = reader._offset.Y + offset.Y;
            if ((uint)offsetX > (uint)reader._size.Width)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
            if ((uint)offsetY > (uint)reader._size.Height)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
            int sizeWidth = reader._size.Width - offset.X;
            int sizeHeight = reader._size.Height - offset.Y;
            return new TiffPixelBufferReader<TPixel>(reader._reader, new TiffPoint(offsetX, offsetY), new TiffSize(sizeWidth, sizeHeight));
        }

        /// <summary>
        /// Crop a sub region from <paramref name="reader"/>.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="reader">The original pixel buffer.</param>
        /// <param name="offset">The number of columns and rows to skip.</param>
        /// <param name="size">The number of columns and rows to take.</param>
        /// <returns>A <see cref="TiffPixelBuffer{TPixel}"/> representing the cropped region.</returns>
        public static TiffPixelBufferReader<TPixel> Crop<TPixel>(this TiffPixelBufferReader<TPixel> reader, TiffPoint offset, TiffSize size) where TPixel : unmanaged
        {
            if ((uint)offset.X > (uint)reader._size.Width)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
            if ((uint)offset.Y > (uint)reader._size.Height)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
            int offsetX = reader._offset.X + offset.X;
            int offsetY = reader._offset.Y + offset.Y;
            if ((uint)offsetX > (uint)reader._size.Width)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
            if ((uint)offsetY > (uint)reader._size.Height)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
            int sizeWidth = reader._size.Width - offset.X;
            int sizeHeight = reader._size.Height - offset.Y;
            if ((uint)size.Width > (uint)sizeWidth)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }
            if ((uint)size.Height > (uint)sizeHeight)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }
            return new TiffPixelBufferReader<TPixel>(reader._reader, new TiffPoint(offsetX, offsetY), size);
        }
    }
}
