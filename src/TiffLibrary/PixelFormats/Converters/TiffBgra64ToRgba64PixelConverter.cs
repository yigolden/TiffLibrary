using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.PixelFormats.Converters
{
    internal class TiffBgra64ToRgba64PixelConverter : TiffPixelConverter<TiffBgra64, TiffRgba64>
    {
        public TiffBgra64ToRgba64PixelConverter(ITiffPixelBufferWriter<TiffRgba64> writer) : base(writer) { }

        public override void Convert(ReadOnlySpan<TiffBgra64> source, Span<TiffRgba64> destination)
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
                => typeof(TSource) == typeof(TiffBgra64) && typeof(TDestination) == typeof(TiffRgba64);
            public ITiffPixelBufferWriter<TSource> CreateConverter<TSource, TDestination>(ITiffPixelBufferWriter<TDestination> buffer)
                where TSource : unmanaged
                where TDestination : unmanaged
            {
                if (typeof(TSource) != typeof(TiffBgra64) || typeof(TDestination) != typeof(TiffRgba64))
                {
                    throw new InvalidOperationException();
                }
                return Unsafe.As<ITiffPixelBufferWriter<TSource>>(new TiffBgra64ToRgba64PixelConverter(Unsafe.As<ITiffPixelBufferWriter<TiffRgba64>>(buffer)));
            }
        }

        internal class Converter : ITiffPixelSpanConverter<TiffBgra64, TiffRgba64>
        {
            public void Convert(ReadOnlySpan<TiffBgra64> source, Span<TiffRgba64> destination)
            {
                int length = source.Length;
                ref TiffBgra64 sourceRef = ref MemoryMarshal.GetReference(source);
                ref TiffRgba64 destinationRef = ref MemoryMarshal.GetReference(destination);

                TiffRgba64 destinationPixel = default;
                for (int i = 0; i < length; i++)
                {
                    TiffBgra64 sourcePixel = Unsafe.Add(ref sourceRef, i);
                    destinationPixel.B = sourcePixel.B;
                    destinationPixel.G = sourcePixel.G;
                    destinationPixel.R = sourcePixel.R;
                    destinationPixel.A = sourcePixel.A;
                    Unsafe.Add(ref destinationRef, i) = destinationPixel;
                }
            }

        }
    }
}
