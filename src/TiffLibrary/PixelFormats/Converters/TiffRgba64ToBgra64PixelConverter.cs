using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.PixelFormats.Converters
{
    internal class TiffRgba64ToBgra64PixelConverter : TiffPixelConverter<TiffRgba64, TiffBgra64>
    {
        public TiffRgba64ToBgra64PixelConverter(ITiffPixelBufferWriter<TiffBgra64> writer) : base(writer) { }

        public override void Convert(ReadOnlySpan<TiffRgba64> source, Span<TiffBgra64> destination)
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
                => typeof(TSource) == typeof(TiffRgba64) && typeof(TDestination) == typeof(TiffBgra64);
            public ITiffPixelBufferWriter<TSource> CreateConverter<TSource, TDestination>(ITiffPixelBufferWriter<TDestination> buffer)
                where TSource : unmanaged
                where TDestination : unmanaged
            {
                if (typeof(TSource) != typeof(TiffRgba64) || typeof(TDestination) != typeof(TiffBgra64))
                {
                    ThrowHelper.ThrowInvalidOperationException();
                }
                return Unsafe.As<ITiffPixelBufferWriter<TSource>>(new TiffRgba64ToBgra64PixelConverter(Unsafe.As<ITiffPixelBufferWriter<TiffBgra64>>(buffer)));
            }
        }

        internal class Converter : ITiffPixelSpanConverter<TiffRgba64, TiffBgra64>
        {
            public void Convert(ReadOnlySpan<TiffRgba64> source, Span<TiffBgra64> destination)
            {
                int length = source.Length;
                ref TiffRgba64 sourceRef = ref MemoryMarshal.GetReference(source);
                ref TiffBgra64 destinationRef = ref MemoryMarshal.GetReference(destination);

                TiffBgra64 destinationPixel = default;
                for (int i = 0; i < length; i++)
                {
                    TiffRgba64 sourcePixel = Unsafe.Add(ref sourceRef, i);
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
