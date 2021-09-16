using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.PixelFormats.Converters
{

    internal class TiffGray8ToCmyk64PixelConverter : TiffPixelConverter<TiffGray8, TiffCmyk64>
    {
        public TiffGray8ToCmyk64PixelConverter(ITiffPixelBufferWriter<TiffCmyk64> writer) : base(writer) { }

        public override void Convert(ReadOnlySpan<TiffGray8> source, Span<TiffCmyk64> destination)
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
                => typeof(TSource) == typeof(TiffGray8) && typeof(TDestination) == typeof(TiffCmyk64);
            public ITiffPixelBufferWriter<TSource> CreateConverter<TSource, TDestination>(ITiffPixelBufferWriter<TDestination> buffer)
                where TSource : unmanaged
                where TDestination : unmanaged
            {
                if (typeof(TSource) != typeof(TiffGray8) || typeof(TDestination) != typeof(TiffCmyk64))
                {
                    ThrowHelper.ThrowInvalidOperationException();
                }
                return Unsafe.As<ITiffPixelBufferWriter<TSource>>(new TiffGray8ToCmyk64PixelConverter(Unsafe.As<ITiffPixelBufferWriter<TiffCmyk64>>(buffer)));
            }
        }

        internal class Converter : ITiffPixelSpanConverter<TiffGray8, TiffCmyk64>
        {
            public void Convert(ReadOnlySpan<TiffGray8> source, Span<TiffCmyk64> destination)
            {
                int length = source.Length;
                ref TiffGray8 sourceRef = ref MemoryMarshal.GetReference(source);
                ref TiffCmyk64 destinationRef = ref MemoryMarshal.GetReference(destination);

                TiffCmyk64 cmyk = default;
                for (int i = 0; i < length; i++)
                {
                    int intensity = Unsafe.As<TiffGray8, byte>(ref Unsafe.Add(ref sourceRef, i));
                    cmyk.K = (ushort)~(intensity << 8 | intensity);
                    Unsafe.Add(ref destinationRef, i) = cmyk;
                }
            }
        }
    }
}
