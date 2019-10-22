using System;
using System.Runtime.CompilerServices;

namespace TiffLibrary.PixelConverter
{
    internal static class TiffCombinedPixelConverterFactory
    {
        public static TiffCombinedPixelConverterFactory<TSource, TIntermediate, TDestination> Create<TSource, TIntermediate, TDestination>(ITiffPixelSpanConverter<TSource, TIntermediate> converter1, ITiffPixelSpanConverter<TIntermediate, TDestination> converter2) where TSource : unmanaged where TIntermediate : unmanaged where TDestination : unmanaged
        {
            return new TiffCombinedPixelConverterFactory<TSource, TIntermediate, TDestination>(converter1, converter2);
        }
    }

    internal class TiffCombinedPixelConverterFactory<TSource, TIntermediate, TDestination> : ITiffPixelConverterFactory
        where TSource : unmanaged where TIntermediate : unmanaged where TDestination : unmanaged

    {
        private readonly ITiffPixelSpanConverter<TSource, TIntermediate> _converter1;
        private readonly ITiffPixelSpanConverter<TIntermediate, TDestination> _converter2;
        private readonly bool _allowInPlaceConvert;

        public TiffCombinedPixelConverterFactory(ITiffPixelSpanConverter<TSource, TIntermediate> converter1, ITiffPixelSpanConverter<TIntermediate, TDestination> converter2, bool allowInPlaceConvert = true)
        {
            _converter1 = converter1 ?? throw new ArgumentNullException(nameof(converter1));
            _converter2 = converter2 ?? throw new ArgumentNullException(nameof(converter2));
            _allowInPlaceConvert = allowInPlaceConvert;
        }

        public bool IsConvertible<TSource1, TDestination1>()
                    where TSource1 : unmanaged
                    where TDestination1 : unmanaged
            => typeof(TSource1) == typeof(TSource) && typeof(TDestination1) == typeof(TDestination);

        public ITiffPixelBufferWriter<TSource1> CreateConverter<TSource1, TDestination1>(ITiffPixelBufferWriter<TDestination1> buffer)
            where TSource1 : unmanaged
            where TDestination1 : unmanaged
        {
            if (typeof(TSource) != typeof(TSource1) || typeof(TDestination) != typeof(TDestination1))
            {
                throw new InvalidOperationException();
            }

            return Unsafe.As<ITiffPixelBufferWriter<TSource1>>(new TiffCombinedPixelConverter<TSource, TIntermediate, TDestination>(Unsafe.As<ITiffPixelBufferWriter<TDestination>>(buffer), _converter1, _converter2, _allowInPlaceConvert));
        }
    }
}
