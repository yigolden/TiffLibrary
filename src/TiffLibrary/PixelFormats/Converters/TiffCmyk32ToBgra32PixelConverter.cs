using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.PixelFormats.Converters
{

    internal class TiffCmyk32ToBgra32PixelConverter : TiffPixelConverter<TiffCmyk32, TiffBgra32>
    {
        public TiffCmyk32ToBgra32PixelConverter(ITiffPixelBufferWriter<TiffBgra32> writer) : base(writer) { }

        public override void Convert(ReadOnlySpan<TiffCmyk32> source, Span<TiffBgra32> destination)
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
                => typeof(TSource) == typeof(TiffCmyk32) && typeof(TDestination) == typeof(TiffBgra32);
            public ITiffPixelBufferWriter<TSource> CreateConverter<TSource, TDestination>(ITiffPixelBufferWriter<TDestination> buffer)
                where TSource : unmanaged
                where TDestination : unmanaged
            {
                if (typeof(TSource) != typeof(TiffCmyk32) || typeof(TDestination) != typeof(TiffBgra32))
                {
                    ThrowHelper.ThrowInvalidOperationException();
                }
                return Unsafe.As<ITiffPixelBufferWriter<TSource>>(new TiffCmyk32ToBgra32PixelConverter(Unsafe.As<ITiffPixelBufferWriter<TiffBgra32>>(buffer)));
            }
        }

        internal class Converter : ITiffPixelSpanConverter<TiffCmyk32, TiffBgra32>
        {
            public void Convert(ReadOnlySpan<TiffCmyk32> source, Span<TiffBgra32> destination)
            {
                int length = source.Length;
                ref TiffCmyk32 sourceRef = ref MemoryMarshal.GetReference(source);
                ref TiffBgra32 destinationRef = ref MemoryMarshal.GetReference(destination);

                TiffBgra32 bgra = default;
                bgra.A = 255;
                for (int i = 0; i < length; i++)
                {
                    TiffCmyk32 cmyk = Unsafe.Add(ref sourceRef, i);
                    bgra.B = (byte)((255 - cmyk.Y) * (255 - cmyk.K) >> 8);
                    bgra.G = (byte)((255 - cmyk.M) * (255 - cmyk.K) >> 8);
                    bgra.R = (byte)((255 - cmyk.C) * (255 - cmyk.K) >> 8);
                    Unsafe.Add(ref destinationRef, i) = bgra;
                }
            }
        }
    }
}
