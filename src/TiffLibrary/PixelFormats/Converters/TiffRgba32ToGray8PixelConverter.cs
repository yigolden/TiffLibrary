using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.PixelFormats.Converters
{

    internal class TiffRgba32ToGray8PixelConverter : TiffPixelConverter<TiffRgba32, TiffGray8>
    {
        public TiffRgba32ToGray8PixelConverter(ITiffPixelBufferWriter<TiffGray8> writer) : base(writer) { }

        public override void Convert(ReadOnlySpan<TiffRgba32> source, Span<TiffGray8> destination)
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
                => typeof(TSource) == typeof(TiffRgba32) && typeof(TDestination) == typeof(TiffGray8);
            public ITiffPixelBufferWriter<TSource> CreateConverter<TSource, TDestination>(ITiffPixelBufferWriter<TDestination> buffer)
                where TSource : unmanaged
                where TDestination : unmanaged
            {
                if (typeof(TSource) != typeof(TiffRgba32) || typeof(TDestination) != typeof(TiffGray8))
                {
                    throw new InvalidOperationException();
                }
                return Unsafe.As<ITiffPixelBufferWriter<TSource>>(new TiffRgba32ToGray8PixelConverter(Unsafe.As<ITiffPixelBufferWriter<TiffGray8>>(buffer)));
            }
        }

        internal class Converter : ITiffPixelSpanConverter<TiffRgba32, TiffGray8>
        {
            public void Convert(ReadOnlySpan<TiffRgba32> source, Span<TiffGray8> destination)
            {
                int length = source.Length;
                ref TiffRgba32 sourceRef = ref MemoryMarshal.GetReference(source);
                ref byte destinationRef = ref Unsafe.As<TiffGray8, byte>(ref MemoryMarshal.GetReference(destination));

                for (int i = 0; i < length; i++)
                {
                    TiffRgba32 pixel = Unsafe.Add(ref sourceRef, i);

                    byte a = pixel.A;
                    pixel.R = (byte)(pixel.R * a / 255);
                    pixel.G = (byte)(pixel.G * a / 255);
                    pixel.B = (byte)(pixel.B * a / 255);

                    byte gray = (byte)((pixel.R * 38 + pixel.G * 75 + pixel.B * 15) >> 7);
                    Unsafe.Add(ref destinationRef, i) = gray;
                }
            }
        }
    }
}
