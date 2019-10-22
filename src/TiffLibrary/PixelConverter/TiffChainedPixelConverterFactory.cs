using System;
using System.Runtime.CompilerServices;

namespace TiffLibrary.PixelConverter
{
    internal static class TiffChainedPixelConverterFactory
    {
        public static ITiffPixelConverterFactory Create<TSource, TIntermediate, TDestination>(ITiffPixelConverterFactory factory1, ITiffPixelConverterFactory factory2) where TSource : unmanaged where TIntermediate : unmanaged where TDestination : unmanaged
        {
            return new TiffChainedPixelConverterFactory<TSource, TIntermediate, TDestination>(factory1, factory2);
        }

        public static ITiffPixelConverterFactory Create<TSource, TIntermediate1, TIntermediate2, TDestination>(ITiffPixelConverterFactory factory1, ITiffPixelConverterFactory factory2, ITiffPixelConverterFactory factory3) where TSource : unmanaged where TIntermediate1 : unmanaged where TIntermediate2 : unmanaged where TDestination : unmanaged
        {
            var intermediateFactory = new TiffChainedPixelConverterFactory<TSource, TIntermediate1, TIntermediate2>(factory1, factory2);
            return new TiffChainedPixelConverterFactory<TSource, TIntermediate2, TDestination>(intermediateFactory, factory3);
        }
    }

    internal class TiffChainedPixelConverterFactory<TSource, TIntermediate, TDestination> : ITiffPixelConverterFactory
            where TSource : unmanaged where TIntermediate : unmanaged where TDestination : unmanaged

    {
        private readonly ITiffPixelConverterFactory _factory1;
        private readonly ITiffPixelConverterFactory _factory2;

        public TiffChainedPixelConverterFactory(ITiffPixelConverterFactory factory1, ITiffPixelConverterFactory factory2)
        {
            _factory1 = factory1;
            _factory2 = factory2;
        }

        public bool IsConvertible<TSource1, TDestination1>()
                    where TSource1 : unmanaged
                    where TDestination1 : unmanaged
            => typeof(TSource1) == typeof(TSource) && typeof(TDestination1) == typeof(TDestination) && _factory1.IsConvertible<TSource, TIntermediate>() && _factory2.IsConvertible<TIntermediate, TDestination>();

        public ITiffPixelBufferWriter<TSource1> CreateConverter<TSource1, TDestination1>(ITiffPixelBufferWriter<TDestination1> buffer)
            where TSource1 : unmanaged
            where TDestination1 : unmanaged
        {
            if (typeof(TSource) != typeof(TSource1) || typeof(TDestination) != typeof(TDestination1))
            {
                throw new InvalidOperationException();
            }

            return Unsafe.As<ITiffPixelBufferWriter<TSource1>>(_factory1.CreateConverter<TSource, TIntermediate>(_factory2.CreateConverter<TIntermediate, TDestination>(Unsafe.As<ITiffPixelBufferWriter<TDestination>>(buffer))));
        }
    }
}
