using System;

namespace TiffLibrary
{
    /// <summary>
    /// Represents a 2-dimensional region of pixels in a contiguous memory buffer in row-major order.
    /// </summary>
    /// <typeparam name="TPixel">The pixel type.</typeparam>
    public interface ITiffPixelBuffer<TPixel> where TPixel : unmanaged
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
        /// Gets a <see cref="Span{TPixel}"/> representing memory of the entire region in row-major order.
        /// </summary>
        /// <returns>A <see cref="Span{TPixel}"/> representing memory of the entire region in row-major order.</returns>
        Span<TPixel> GetSpan();

        /// <summary>
        /// Gets a <see cref="ReadOnlySpan{TPixel}"/> representing memory of the entire region in row-major order.
        /// </summary>
        /// <returns>A <see cref="ReadOnlySpan{TPixel}"/> representing memory of the entire region in row-major order.</returns>
        ReadOnlySpan<TPixel> GetReadOnlySpan();
    }
}
