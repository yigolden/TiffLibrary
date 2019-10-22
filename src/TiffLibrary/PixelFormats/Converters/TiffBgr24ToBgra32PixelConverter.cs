using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.PixelFormats.Converters
{
    internal class TiffBgr24ToBgra32PixelConverter : TiffPixelConverter<TiffBgr24, TiffBgra32>
    {
        public TiffBgr24ToBgra32PixelConverter(ITiffPixelBufferWriter<TiffBgra32> writer) : base(writer) { }

        public override void Convert(ReadOnlySpan<TiffBgr24> source, Span<TiffBgra32> destination)
        {
            SpanConverter.Convert(source, destination);
        }

        public static Factory FactoryInstance { get; } = new Factory();
        public static Converter SpanConverter { get; } = new Converter();

        internal class Factory : ITiffPixelConverterFactory
        {
            public bool IsConvertible<TSource, TDestination>()
                where TSource : unmanaged
                where TDestination : unmanaged
                => typeof(TSource) == typeof(TiffBgr24) && typeof(TDestination) == typeof(TiffBgra32);
            public ITiffPixelBufferWriter<TSource> CreateConverter<TSource, TDestination>(ITiffPixelBufferWriter<TDestination> buffer)
                where TSource : unmanaged
                where TDestination : unmanaged
            {
                if (typeof(TSource) != typeof(TiffBgr24) || typeof(TDestination) != typeof(TiffBgra32))
                {
                    throw new InvalidOperationException();
                }
                return Unsafe.As<ITiffPixelBufferWriter<TSource>>(new TiffBgr24ToBgra32PixelConverter(Unsafe.As<ITiffPixelBufferWriter<TiffBgra32>>(buffer)));
            }
        }

        internal class Converter : ITiffPixelSpanConverter<TiffBgr24, TiffBgra32>
        {
            public void Convert(ReadOnlySpan<TiffBgr24> source, Span<TiffBgra32> destination)
            {
                int length = source.Length;
                ref TiffBgr24 sourceRef = ref MemoryMarshal.GetReference(source);
                ref TiffBgra32 destinationRef = ref MemoryMarshal.GetReference(destination);

                TiffBgr24 sourcePixel;
                TiffBgra32 destinationPixel = default;
                destinationPixel.A = byte.MaxValue;
                for (int i = 0; i < length; i++)
                {
                    sourcePixel = Unsafe.Add(ref sourceRef, i);
                    destinationPixel.B = sourcePixel.B;
                    destinationPixel.G = sourcePixel.G;
                    destinationPixel.R = sourcePixel.R;
                    Unsafe.Add(ref destinationRef, i) = destinationPixel;
                }
            }
        }
    }
}
