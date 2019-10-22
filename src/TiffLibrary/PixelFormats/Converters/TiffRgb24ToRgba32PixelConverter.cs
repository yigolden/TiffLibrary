using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.PixelFormats.Converters
{
    internal class TiffRgb24ToRgba32PixelConverter : TiffPixelConverter<TiffRgb24, TiffRgba32>
    {
        public TiffRgb24ToRgba32PixelConverter(ITiffPixelBufferWriter<TiffRgba32> writer) : base(writer) { }

        public override void Convert(ReadOnlySpan<TiffRgb24> source, Span<TiffRgba32> destination)
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
                => typeof(TSource) == typeof(TiffRgb24) && typeof(TDestination) == typeof(TiffRgba32);
            public ITiffPixelBufferWriter<TSource> CreateConverter<TSource, TDestination>(ITiffPixelBufferWriter<TDestination> buffer)
                where TSource : unmanaged
                where TDestination : unmanaged
            {
                if (typeof(TSource) != typeof(TiffRgb24) || typeof(TDestination) != typeof(TiffRgba32))
                {
                    throw new InvalidOperationException();
                }
                return Unsafe.As<ITiffPixelBufferWriter<TSource>>(new TiffRgb24ToRgba32PixelConverter(Unsafe.As<ITiffPixelBufferWriter<TiffRgba32>>(buffer)));
            }
        }

        internal class Converter : ITiffPixelSpanConverter<TiffRgb24, TiffRgba32>
        {
            public void Convert(ReadOnlySpan<TiffRgb24> source, Span<TiffRgba32> destination)
            {
                int length = source.Length;
                ref TiffRgb24 sourceRef = ref MemoryMarshal.GetReference(source);
                ref TiffRgba32 destinationRef = ref MemoryMarshal.GetReference(destination);

                TiffRgb24 sourcePixel;
                TiffRgba32 destinationPixel = default;
                destinationPixel.A = byte.MaxValue;
                for (int i = 0; i < length; i++)
                {
                    sourcePixel = Unsafe.Add(ref sourceRef, i);
                    destinationPixel.R = sourcePixel.R;
                    destinationPixel.G = sourcePixel.G;
                    destinationPixel.B = sourcePixel.B;
                    Unsafe.Add(ref destinationRef, i) = destinationPixel;
                }
            }
        }
    }
}
