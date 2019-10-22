using System;
using System.Runtime.CompilerServices;

namespace TiffLibrary.PixelConverter
{
    /// <summary>
    /// A default implementation of <see cref="ITiffPixelConverterFactory"/>, providing supports for built-in pixel formats.
    /// </summary>
    public class TiffDefaultPixelConverterFactory : ITiffPixelConverterFactory
    {
        /// <summary>
        /// A shared instance that should be used across the application.
        /// </summary>
        public static ITiffPixelConverterFactory Instance { get; } = new TiffDefaultPixelConverterFactory();

        /// <summary>
        /// Create <see cref="TiffPixelConverter{TSource, TDestination}"/> and wraps <paramref name="writer"/>.
        /// </summary>
        /// <typeparam name="TSource">The pixel format to convert from.</typeparam>
        /// <typeparam name="TDestination">The pixel format to convert to.</typeparam>
        /// <param name="writer">The writer to be wrapped.</param>
        /// <returns>The created <see cref="TiffPixelConverter{TSource, TDestination}"/>.</returns>
        public ITiffPixelBufferWriter<TSource> CreateConverter<TSource, TDestination>(ITiffPixelBufferWriter<TDestination> writer)
                    where TSource : unmanaged
                    where TDestination : unmanaged
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (typeof(TSource) == typeof(TDestination))
            {
                return Unsafe.As<ITiffPixelBufferWriter<TSource>>(writer);
            }

            ITiffPixelConverterFactory factory = DefaultPixelConverterFactoryCache<TSource, TDestination>.Factory;
            if (factory != null)
            {
                return factory.CreateConverter<TSource, TDestination>(writer);
            }

            foreach (ConverterFactoryDescriptor item in DefaultPixelConverterFactoryList.Factories)
            {
                if (item.SourceType == typeof(TSource) && item.DestinationType == typeof(TDestination))
                {
                    factory = item.ConverterFactory;
                }
            }

            if (factory is null)
            {
                throw new InvalidOperationException($"No converter from {typeof(TSource).FullName} to {typeof(TDestination).FullName} is found.");
            }

            ITiffPixelBufferWriter<TSource> converter = factory.CreateConverter<TSource, TDestination>(writer);
            if (converter != null)
            {
                DefaultPixelConverterFactoryCache<TSource, TDestination>.Factory = factory;
                return converter;
            }

            throw new InvalidOperationException($"No converter from {typeof(TSource).FullName} to {typeof(TDestination).FullName} is found.");
        }

        /// <summary>
        /// Checks if this factory is able to create <see cref="TiffPixelConverter{TSource, TDestination}"/>.
        /// </summary>
        /// <typeparam name="TSource">The pixel format to convert from.</typeparam>
        /// <typeparam name="TDestination">The pixel format to convert to.</typeparam>
        /// <returns>True if this object is able to create the converter.</returns>
        public bool IsConvertible<TSource, TDestination>()
            where TSource : unmanaged
            where TDestination : unmanaged
        {
            if (typeof(TSource) == typeof(TDestination))
            {
                return true;
            }

            ITiffPixelConverterFactory factory = DefaultPixelConverterFactoryCache<TSource, TDestination>.Factory;
            if (factory != null)
            {
                return true;
            }

            foreach (ConverterFactoryDescriptor item in DefaultPixelConverterFactoryList.Factories)
            {
                if (item.SourceType == typeof(TSource) && item.DestinationType == typeof(TDestination))
                {
                    if (item.ConverterFactory.IsConvertible<TSource, TDestination>())
                    {
                        DefaultPixelConverterFactoryCache<TSource, TDestination>.Factory = item.ConverterFactory;
                        return true;
                    }
                }
            }

            return false;
        }
    }

}
