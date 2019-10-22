using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.PixelFormats.Converters
{

    internal class TiffBgra64ToRgba32PixelConverter : TiffPixelConverter<TiffBgra64, TiffRgba32>
    {
        public TiffBgra64ToRgba32PixelConverter(ITiffPixelBufferWriter<TiffRgba32> writer) : base(writer) { }

        public override void Convert(ReadOnlySpan<TiffBgra64> source, Span<TiffRgba32> destination)
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
                => typeof(TSource) == typeof(TiffBgra64) && typeof(TDestination) == typeof(TiffRgba32);
            public ITiffPixelBufferWriter<TSource> CreateConverter<TSource, TDestination>(ITiffPixelBufferWriter<TDestination> buffer)
                where TSource : unmanaged
                where TDestination : unmanaged
            {
                if (typeof(TSource) != typeof(TiffBgra64) || typeof(TDestination) != typeof(TiffRgba32))
                {
                    throw new InvalidOperationException();
                }
                return Unsafe.As<ITiffPixelBufferWriter<TSource>>(new TiffBgra64ToRgba32PixelConverter(Unsafe.As<ITiffPixelBufferWriter<TiffRgba32>>(buffer)));
            }
        }

        internal class Converter : ITiffPixelSpanConverter<TiffBgra64, TiffRgba32>
        {
            public void Convert(ReadOnlySpan<TiffBgra64> source, Span<TiffRgba32> destination)
            {
                int length = source.Length;
                ref TiffBgra64 sourceRef = ref MemoryMarshal.GetReference(source);
                ref TiffRgba32 destinationRef = ref MemoryMarshal.GetReference(destination);

                TiffRgba32 pixel32 = default;
                for (int i = 0; i < length; i++)
                {
                    TiffBgra64 pixel64 = Unsafe.Add(ref sourceRef, i);
                    pixel32.B = (byte)(pixel64.B >> 8);
                    pixel32.G = (byte)(pixel64.G >> 8);
                    pixel32.R = (byte)(pixel64.R >> 8);
                    pixel32.A = (byte)(pixel64.A >> 8);
                    Unsafe.Add(ref destinationRef, i) = pixel32;
                }
            }
        }
    }
}
