using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.PixelFormats.Converters
{

    internal class TiffGray8ToCmyk32PixelConverter : TiffPixelConverter<TiffGray8, TiffCmyk32>
    {
        public TiffGray8ToCmyk32PixelConverter(ITiffPixelBufferWriter<TiffCmyk32> writer) : base(writer) { }

        public override void Convert(ReadOnlySpan<TiffGray8> source, Span<TiffCmyk32> destination)
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
                => typeof(TSource) == typeof(TiffGray8) && typeof(TDestination) == typeof(TiffCmyk32);
            public ITiffPixelBufferWriter<TSource> CreateConverter<TSource, TDestination>(ITiffPixelBufferWriter<TDestination> buffer)
                where TSource : unmanaged
                where TDestination : unmanaged
            {
                if (typeof(TSource) != typeof(TiffGray8) || typeof(TDestination) != typeof(TiffCmyk32))
                {
                    throw new InvalidOperationException();
                }
                return Unsafe.As<ITiffPixelBufferWriter<TSource>>(new TiffGray8ToCmyk32PixelConverter(Unsafe.As<ITiffPixelBufferWriter<TiffCmyk32>>(buffer)));
            }
        }

        internal class Converter : ITiffPixelSpanConverter<TiffGray8, TiffCmyk32>
        {
            public void Convert(ReadOnlySpan<TiffGray8> source, Span<TiffCmyk32> destination)
            {
                int length = source.Length;
                ref TiffGray8 sourceRef = ref MemoryMarshal.GetReference(source);
                ref TiffCmyk32 destinationRef = ref MemoryMarshal.GetReference(destination);

                TiffCmyk32 cmyk = default;
                for (int i = 0; i < length; i++)
                {
                    byte intensity = Unsafe.As<TiffGray8, byte>(ref Unsafe.Add(ref sourceRef, i));
                    cmyk.K = (byte)~intensity;
                    Unsafe.Add(ref destinationRef, i) = cmyk;
                }
            }
        }
    }
}
