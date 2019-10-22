namespace TiffLibrary.PixelConverter
{
    /// <summary>
    /// Represents an object capable of creating <see cref="TiffPixelConverter{TSource, TDestination}"/> object to convert from one pixel format to another.
    /// </summary>
    public interface ITiffPixelConverterFactory
    {
        /// <summary>
        /// Checks if this factory is able to create <see cref="TiffPixelConverter{TSource, TDestination}"/>.
        /// </summary>
        /// <typeparam name="TSource">The pixel format to convert from.</typeparam>
        /// <typeparam name="TDestination">The pixel format to convert to.</typeparam>
        /// <returns>True if this object is able to create the converter.</returns>
        bool IsConvertible<TSource, TDestination>() where TSource : unmanaged where TDestination : unmanaged;

        /// <summary>
        /// Create <see cref="TiffPixelConverter{TSource, TDestination}"/> and wraps <paramref name="writer"/>.
        /// </summary>
        /// <typeparam name="TSource">The pixel format to convert from.</typeparam>
        /// <typeparam name="TDestination">The pixel format to convert to.</typeparam>
        /// <param name="writer">The writer to be wrapped.</param>
        /// <returns>The created <see cref="TiffPixelConverter{TSource, TDestination}"/>.</returns>
        ITiffPixelBufferWriter<TSource> CreateConverter<TSource, TDestination>(ITiffPixelBufferWriter<TDestination> writer) where TSource : unmanaged where TDestination : unmanaged;
    }
}
