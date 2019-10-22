using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.PixelFormats.Converters
{
    internal class TiffBgra32ToRgb24PixelConverter : TiffPixelConverter<TiffBgra32, TiffRgb24>
    {
        public TiffBgra32ToRgb24PixelConverter(ITiffPixelBufferWriter<TiffRgb24> writer) : base(writer) { }

        public override void Convert(ReadOnlySpan<TiffBgra32> source, Span<TiffRgb24> destination)
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
                => typeof(TSource) == typeof(TiffBgra32) && typeof(TDestination) == typeof(TiffRgb24);
            public ITiffPixelBufferWriter<TSource> CreateConverter<TSource, TDestination>(ITiffPixelBufferWriter<TDestination> buffer)
                where TSource : unmanaged
                where TDestination : unmanaged
            {
                if (typeof(TSource) != typeof(TiffBgra32) || typeof(TDestination) != typeof(TiffRgb24))
                {
                    throw new InvalidOperationException();
                }
                return Unsafe.As<ITiffPixelBufferWriter<TSource>>(new TiffBgra32ToRgb24PixelConverter(Unsafe.As<ITiffPixelBufferWriter<TiffRgb24>>(buffer)));
            }
        }

        internal class Converter : ITiffPixelSpanConverter<TiffBgra32, TiffRgb24>
        {
            public void Convert(ReadOnlySpan<TiffBgra32> source, Span<TiffRgb24> destination)
            {
                int length = source.Length;
                ref TiffBgra32 sourceRef = ref MemoryMarshal.GetReference(source);
                ref TiffRgb24 destinationRef = ref MemoryMarshal.GetReference(destination);

                TiffBgra32 sourcePixel;
                TiffRgb24 destinationPixel = default;
                for (int i = 0; i < length; i++)
                {
                    sourcePixel = Unsafe.Add(ref sourceRef, i);
                    byte a = sourcePixel.A;
                    destinationPixel.B = (byte)(sourcePixel.B * a / 255);
                    destinationPixel.G = (byte)(sourcePixel.G * a / 255);
                    destinationPixel.R = (byte)(sourcePixel.R * a / 255);
                    Unsafe.Add(ref destinationRef, i) = destinationPixel;
                }
            }
        }
    }
}
