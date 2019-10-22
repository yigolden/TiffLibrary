using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.PixelFormats.Converters
{

    internal class TiffGray16ToCmyk64PixelConverter : TiffPixelConverter<TiffGray16, TiffCmyk64>
    {
        public TiffGray16ToCmyk64PixelConverter(ITiffPixelBufferWriter<TiffCmyk64> writer) : base(writer) { }

        public override void Convert(ReadOnlySpan<TiffGray16> source, Span<TiffCmyk64> destination)
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
                => typeof(TSource) == typeof(TiffGray16) && typeof(TDestination) == typeof(TiffCmyk64);
            public ITiffPixelBufferWriter<TSource> CreateConverter<TSource, TDestination>(ITiffPixelBufferWriter<TDestination> buffer)
                where TSource : unmanaged
                where TDestination : unmanaged
            {
                if (typeof(TSource) != typeof(TiffGray16) || typeof(TDestination) != typeof(TiffCmyk64))
                {
                    throw new InvalidOperationException();
                }
                return Unsafe.As<ITiffPixelBufferWriter<TSource>>(new TiffGray16ToCmyk64PixelConverter(Unsafe.As<ITiffPixelBufferWriter<TiffCmyk64>>(buffer)));
            }
        }

        internal class Converter : ITiffPixelSpanConverter<TiffGray16, TiffCmyk64>
        {
            public void Convert(ReadOnlySpan<TiffGray16> source, Span<TiffCmyk64> destination)
            {
                int length = source.Length;
                ref TiffGray16 sourceRef = ref MemoryMarshal.GetReference(source);
                ref TiffCmyk64 destinationRef = ref MemoryMarshal.GetReference(destination);

                TiffCmyk64 cmyk = default;
                for (int i = 0; i < length; i++)
                {
                    ushort intensity = Unsafe.As<TiffGray16, ushort>(ref Unsafe.Add(ref sourceRef, i));
                    cmyk.K = (ushort)~intensity;
                    Unsafe.Add(ref destinationRef, i) = cmyk;
                }
            }
        }
    }
}
