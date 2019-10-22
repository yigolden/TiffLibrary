using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.PixelFormats.Converters
{
    internal class TiffGray8ToRgb24PixelConverter : TiffPixelConverter<TiffGray8, TiffRgb24>
    {
        public TiffGray8ToRgb24PixelConverter(ITiffPixelBufferWriter<TiffRgb24> writer) : base(writer) { }

        public override void Convert(ReadOnlySpan<TiffGray8> source, Span<TiffRgb24> destination)
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
                => typeof(TSource) == typeof(TiffGray8) && typeof(TDestination) == typeof(TiffRgb24);
            public ITiffPixelBufferWriter<TSource> CreateConverter<TSource, TDestination>(ITiffPixelBufferWriter<TDestination> buffer)
                where TSource : unmanaged
                where TDestination : unmanaged
            {
                if (typeof(TSource) != typeof(TiffGray8) || typeof(TDestination) != typeof(TiffRgb24))
                {
                    throw new InvalidOperationException();
                }
                return Unsafe.As<ITiffPixelBufferWriter<TSource>>(new TiffGray8ToRgb24PixelConverter(Unsafe.As<ITiffPixelBufferWriter<TiffRgb24>>(buffer)));
            }
        }

        internal class Converter : ITiffPixelSpanConverter<TiffGray8, TiffRgb24>
        {
            public void Convert(ReadOnlySpan<TiffGray8> source, Span<TiffRgb24> destination)
            {
                int length = source.Length;
                ref byte sourceRef = ref Unsafe.As<TiffGray8, byte>(ref MemoryMarshal.GetReference(source));
                ref TiffRgb24 destinationRef = ref MemoryMarshal.GetReference(destination);

                TiffRgb24 pixel = default;
                for (int i = 0; i < length; i++)
                {
                    byte intensity = Unsafe.Add(ref sourceRef, i);
                    pixel.R = intensity;
                    pixel.G = intensity;
                    pixel.B = intensity;
                    Unsafe.Add(ref destinationRef, i) = pixel;
                }
            }
        }
    }
}
