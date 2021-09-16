using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.PixelFormats.Converters
{
    internal class TiffRgba32ToRgb24PixelConverter : TiffPixelConverter<TiffRgba32, TiffRgb24>
    {
        public TiffRgba32ToRgb24PixelConverter(ITiffPixelBufferWriter<TiffRgb24> writer) : base(writer) { }

        public override void Convert(ReadOnlySpan<TiffRgba32> source, Span<TiffRgb24> destination)
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
                => typeof(TSource) == typeof(TiffRgba32) && typeof(TDestination) == typeof(TiffRgb24);
            public ITiffPixelBufferWriter<TSource> CreateConverter<TSource, TDestination>(ITiffPixelBufferWriter<TDestination> buffer)
                where TSource : unmanaged
                where TDestination : unmanaged
            {
                if (typeof(TSource) != typeof(TiffRgba32) || typeof(TDestination) != typeof(TiffRgb24))
                {
                    ThrowHelper.ThrowInvalidOperationException();
                }
                return Unsafe.As<ITiffPixelBufferWriter<TSource>>(new TiffRgba32ToRgb24PixelConverter(Unsafe.As<ITiffPixelBufferWriter<TiffRgb24>>(buffer)));
            }
        }

        internal class Converter : ITiffPixelSpanConverter<TiffRgba32, TiffRgb24>
        {
            public void Convert(ReadOnlySpan<TiffRgba32> source, Span<TiffRgb24> destination)
            {
                int length = source.Length;
                ref TiffRgba32 sourceRef = ref MemoryMarshal.GetReference(source);
                ref TiffRgb24 destinationRef = ref MemoryMarshal.GetReference(destination);

                TiffRgba32 sourcePixel;
                TiffRgb24 destinationPixel = default;
                for (int i = 0; i < length; i++)
                {
                    sourcePixel = Unsafe.Add(ref sourceRef, i);
                    byte a = sourcePixel.A;
                    destinationPixel.R = (byte)(sourcePixel.R * a / 255);
                    destinationPixel.G = (byte)(sourcePixel.G * a / 255);
                    destinationPixel.B = (byte)(sourcePixel.B * a / 255);
                    Unsafe.Add(ref destinationRef, i) = destinationPixel;
                }
            }
        }
    }
}
