using System;

namespace TiffLibrary.PixelConverter
{
    /// <summary>
    /// Represents an object capable of converting one pixel format in the buffer to another pixel format.
    /// </summary>
    /// <typeparam name="TSource">The pixel format to convert from.</typeparam>
    /// <typeparam name="TDestination">The pixel format to convert to.</typeparam>
    public interface ITiffPixelSpanConverter<TSource, TDestination> where TSource : unmanaged where TDestination : unmanaged
    {
        /// <summary>
        /// Method to convert from one pixel format to another.
        /// </summary>
        /// <param name="source">The source buffer.</param>
        /// <param name="destination">The destination buffer.</param>
        void Convert(ReadOnlySpan<TSource> source, Span<TDestination> destination);
    }
}
