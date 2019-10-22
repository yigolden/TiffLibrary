using System;
using TiffLibrary.PixelBuffer;

namespace TiffLibrary
{
    /// <summary>
    /// Represents a write-only 2-dimensional region of pixel buffer.
    /// </summary>
    /// <typeparam name="TPixel">The pixel type.</typeparam>
    public interface ITiffPixelBufferWriter<TPixel> : IDisposable where TPixel : unmanaged
    {
        /// <summary>
        /// The number of columns in the region.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// The number of rows in the region.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Gets a <see cref="TiffPixelSpanHandle{TPixel}"/> representing a write-only span of the <paramref name="rowIndex"/> row of the region, while skipping <paramref name="start"/> pixels and limiting the length of the span to <paramref name="length"/>. <see cref="TiffPixelSpanHandle{TPixel}"/> should be disposed after use to flush the pixels into the underlying storage.
        /// </summary>
        /// <param name="rowIndex">The row index.</param>
        /// <param name="start">Number of pixels to skip in this row.</param>
        /// <param name="length">Number of pixels to write.</param>
        /// <returns>A <see cref="TiffPixelSpanHandle{TPixel}"/> representing the temporary buffer consumer can write to. When disposed, it will flush the temporary pixels into the actual storage.</returns>
        TiffPixelSpanHandle<TPixel> GetRowSpan(int rowIndex, int start, int length);

        /// <summary>
        /// Gets a <see cref="TiffPixelSpanHandle{TPixel}"/> representing a write-only span of the <paramref name="colIndex"/> column of the region, while skipping <paramref name="start"/> pixels and limiting the length of the span to <paramref name="length"/>. <see cref="TiffPixelSpanHandle{TPixel}"/> should be disposed after use to flush the pixels into the underlying storage.
        /// </summary>
        /// <param name="colIndex">The column index.</param>
        /// <param name="start">Number of pixels to skip in this column.</param>
        /// <param name="length">Number of pixels to write.</param>
        /// <returns>A <see cref="TiffPixelSpanHandle{TPixel}"/> representing the temporary buffer consumer can write to. When disposed, it will flush the temporary pixels into the actual storage.</returns>
        TiffPixelSpanHandle<TPixel> GetColumnSpan(int colIndex, int start, int length);
    }
}
