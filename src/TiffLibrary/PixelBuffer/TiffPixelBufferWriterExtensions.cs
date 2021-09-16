namespace TiffLibrary
{
    /// <summary>
    /// Contains extension methods to manipulate <see cref="TiffPixelBufferWriter{TPixel}"/> structure.
    /// </summary>
    public static class TiffPixelBufferWriterExtensions
    {
        /// <summary>
        /// Wraps <paramref name="writer"/> in <see cref="TiffPixelBufferWriter{TPixel}"/> structure.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="writer">The pixel buffer writer.</param>
        /// <returns>A <see cref="TiffPixelBufferWriter{TPixel}"/> wrapping <paramref name="writer"/>.</returns>
        public static TiffPixelBufferWriter<TPixel> AsPixelBufferWriter<TPixel>(this ITiffPixelBufferWriter<TPixel> writer) where TPixel : unmanaged
        {
            if (writer is TiffPixelBufferWriter<TPixel> structWriter)
            {
                return structWriter;
            }
            return new TiffPixelBufferWriter<TPixel>(writer);
        }

        /// <summary>
        /// Crop a sub region from <paramref name="writer"/>.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="writer">The original pixel buffer.</param>
        /// <param name="offset">The number of columns and rows to skip.</param>
        /// <returns>A <see cref="TiffPixelBuffer{TPixel}"/> representing the cropped region.</returns>
        public static TiffPixelBufferWriter<TPixel> Crop<TPixel>(this ITiffPixelBufferWriter<TPixel> writer, TiffPoint offset) where TPixel : unmanaged
        {
            if (writer is TiffPixelBufferWriter<TPixel> structWriter)
            {
                return structWriter.Crop(offset);
            }
            return writer.AsPixelBufferWriter().Crop(offset);
        }


        /// <summary>
        /// Crop a sub region from <paramref name="writer"/>.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="writer">The original pixel buffer.</param>
        /// <param name="offset">The number of columns and rows to skip.</param>
        /// <param name="size">The number of columns and rows to take.</param>
        /// <returns>A <see cref="TiffPixelBuffer{TPixel}"/> representing the cropped region.</returns>
        public static TiffPixelBufferWriter<TPixel> Crop<TPixel>(this ITiffPixelBufferWriter<TPixel> writer, TiffPoint offset, TiffSize size) where TPixel : unmanaged
        {
            if (writer is TiffPixelBufferWriter<TPixel> structWriter)
            {
                return structWriter.Crop(offset, size);
            }
            return writer.AsPixelBufferWriter().Crop(offset, size);
        }

        /// <summary>
        /// Crop a sub region from <paramref name="writer"/>.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="writer">The original pixel buffer.</param>
        /// <param name="offset">The number of columns and rows to skip.</param>
        /// <returns>A <see cref="TiffPixelBuffer{TPixel}"/> representing the cropped region.</returns>
        public static TiffPixelBufferWriter<TPixel> Crop<TPixel>(this TiffPixelBufferWriter<TPixel> writer, TiffPoint offset) where TPixel : unmanaged
        {
            if ((uint)offset.X > (uint)writer._size.Width)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(offset));
            }
            if ((uint)offset.Y > (uint)writer._size.Height)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(offset));
            }
            int offsetX = writer._offset.X + offset.X;
            int offsetY = writer._offset.Y + offset.Y;
            if ((uint)offsetX > (uint)writer._size.Width)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(offset));
            }
            if ((uint)offsetY > (uint)writer._size.Height)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(offset));
            }
            int sizeWidth = writer._size.Width - offset.X;
            int sizeHeight = writer._size.Height - offset.Y;
            return new TiffPixelBufferWriter<TPixel>(writer._writer, new TiffPoint(offsetX, offsetY), new TiffSize(sizeWidth, sizeHeight));
        }

        /// <summary>
        /// Crop a sub region from <paramref name="writer"/>.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="writer">The original pixel buffer.</param>
        /// <param name="offset">The number of columns and rows to skip.</param>
        /// <param name="size">The number of columns and rows to take.</param>
        /// <returns>A <see cref="TiffPixelBuffer{TPixel}"/> representing the cropped region.</returns>
        public static TiffPixelBufferWriter<TPixel> Crop<TPixel>(this TiffPixelBufferWriter<TPixel> writer, TiffPoint offset, TiffSize size) where TPixel : unmanaged
        {
            if ((uint)offset.X > (uint)writer._size.Width)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(offset));
            }
            if ((uint)offset.Y > (uint)writer._size.Height)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(offset));
            }
            int offsetX = writer._offset.X + offset.X;
            int offsetY = writer._offset.Y + offset.Y;
            if ((uint)offsetX > (uint)writer._size.Width)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(offset));
            }
            if ((uint)offsetY > (uint)writer._size.Height)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(offset));
            }
            int sizeWidth = writer._size.Width - offset.X;
            int sizeHeight = writer._size.Height - offset.Y;
            if ((uint)size.Width > (uint)sizeWidth)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(size));
            }
            if ((uint)size.Height > (uint)sizeHeight)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(size));
            }
            return new TiffPixelBufferWriter<TPixel>(writer._writer, new TiffPoint(offsetX, offsetY), size);
        }
    }
}
