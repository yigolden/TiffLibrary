using System;

namespace TiffLibrary
{
    /// <summary>
    /// Contains extension methods to manipulate <see cref="TiffPixelBuffer{TPixel}"/> structure.
    /// </summary>
    public static class TiffPixelBufferExtensions
    {
        /// <summary>
        /// Wraps <paramref name="buffer"/> in <see cref="TiffPixelBuffer{TPixel}"/> structure.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="buffer">The pixel buffer.</param>
        /// <returns>A <see cref="TiffPixelBuffer{TPixel}"/> wrapping <paramref name="buffer"/>.</returns>
        public static TiffPixelBuffer<TPixel> AsPixelBuffer<TPixel>(this ITiffPixelBuffer<TPixel> buffer) where TPixel : unmanaged
        {
            return new TiffPixelBuffer<TPixel>(buffer);
        }

        /// <summary>
        /// Crop a sub region from <paramref name="buffer"/>.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="buffer">The original pixel buffer.</param>
        /// <param name="offset">The number of columns and rows to skip.</param>
        /// <returns>A <see cref="TiffPixelBuffer{TPixel}"/> representing the cropped region.</returns>
        public static TiffPixelBuffer<TPixel> Crop<TPixel>(this ITiffPixelBuffer<TPixel> buffer, TiffPoint offset) where TPixel : unmanaged
        {
            return buffer.AsPixelBuffer().Crop(offset);
        }

        /// <summary>
        /// Crop a sub region from <paramref name="buffer"/>.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="buffer">The original pixel buffer.</param>
        /// <param name="offset">The number of columns and rows to skip.</param>
        /// <param name="size">The number of columns and rows to take.</param>
        /// <returns>A <see cref="TiffPixelBuffer{TPixel}"/> representing the cropped region.</returns>
        public static TiffPixelBuffer<TPixel> Crop<TPixel>(this ITiffPixelBuffer<TPixel> buffer, TiffPoint offset, TiffSize size) where TPixel : unmanaged
        {
            return buffer.AsPixelBuffer().Crop(offset, size);
        }

        /// <summary>
        /// Crop a sub region from <paramref name="buffer"/>.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="buffer">The original pixel buffer.</param>
        /// <param name="offset">The number of columns and rows to skip.</param>
        /// <returns>A <see cref="TiffPixelBuffer{TPixel}"/> representing the cropped region.</returns>
        public static TiffPixelBuffer<TPixel> Crop<TPixel>(this TiffPixelBuffer<TPixel> buffer, TiffPoint offset) where TPixel : unmanaged
        {
            if ((uint)offset.X > (uint)buffer._size.Width)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(offset));
            }
            if ((uint)offset.Y > (uint)buffer._size.Height)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(offset));
            }
            int offsetX = buffer._offset.X + offset.X;
            int offsetY = buffer._offset.Y + offset.Y;
            if ((uint)offsetX > (uint)buffer._size.Width)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(offset));
            }
            if ((uint)offsetY > (uint)buffer._size.Height)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(offset));
            }
            int sizeWidth = buffer._size.Width - offset.X;
            int sizeHeight = buffer._size.Height - offset.Y;
            return new TiffPixelBuffer<TPixel>(buffer._buffer, new TiffPoint(offsetX, offsetY), new TiffSize(sizeWidth, sizeHeight));
        }

        /// <summary>
        /// Crop a sub region from <paramref name="buffer"/>.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="buffer">The original pixel buffer.</param>
        /// <param name="offset">The number of columns and rows to skip.</param>
        /// <param name="size">The number of columns and rows to take.</param>
        /// <returns>A <see cref="TiffPixelBuffer{TPixel}"/> representing the cropped region.</returns>
        public static TiffPixelBuffer<TPixel> Crop<TPixel>(this TiffPixelBuffer<TPixel> buffer, TiffPoint offset, TiffSize size) where TPixel : unmanaged
        {
            if ((uint)offset.X > (uint)buffer._size.Width)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(offset));
            }
            if ((uint)offset.Y > (uint)buffer._size.Height)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(offset));
            }
            int offsetX = buffer._offset.X + offset.X;
            int offsetY = buffer._offset.Y + offset.Y;
            if ((uint)offsetX > (uint)buffer._size.Width)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(offset));
            }
            if ((uint)offsetY > (uint)buffer._size.Height)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(offset));
            }
            int sizeWidth = buffer._size.Width - offset.X;
            int sizeHeight = buffer._size.Height - offset.Y;
            if ((uint)size.Width > (uint)sizeWidth)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(size));
            }
            if ((uint)size.Height > (uint)sizeHeight)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(size));
            }
            return new TiffPixelBuffer<TPixel>(buffer._buffer, new TiffPoint(offsetX, offsetY), size);
        }
    }
}
