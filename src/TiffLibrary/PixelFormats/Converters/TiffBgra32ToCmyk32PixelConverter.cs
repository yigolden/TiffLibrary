using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.PixelFormats.Converters
{

    internal class TiffBgra32ToCmyk32PixelConverter : TiffPixelConverter<TiffBgra32, TiffCmyk32>
    {
        public TiffBgra32ToCmyk32PixelConverter(ITiffPixelBufferWriter<TiffCmyk32> writer) : base(writer) { }

        public override void Convert(ReadOnlySpan<TiffBgra32> source, Span<TiffCmyk32> destination)
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
                => typeof(TSource) == typeof(TiffBgra32) && typeof(TDestination) == typeof(TiffCmyk32);
            public ITiffPixelBufferWriter<TSource> CreateConverter<TSource, TDestination>(ITiffPixelBufferWriter<TDestination> buffer)
                where TSource : unmanaged
                where TDestination : unmanaged
            {
                if (typeof(TSource) != typeof(TiffBgra32) || typeof(TDestination) != typeof(TiffCmyk32))
                {
                    throw new InvalidOperationException();
                }
                return Unsafe.As<ITiffPixelBufferWriter<TSource>>(new TiffBgra32ToCmyk32PixelConverter(Unsafe.As<ITiffPixelBufferWriter<TiffCmyk32>>(buffer)));
            }
        }

        internal class Converter : ITiffPixelSpanConverter<TiffBgra32, TiffCmyk32>
        {
            public void Convert(ReadOnlySpan<TiffBgra32> source, Span<TiffCmyk32> destination)
            {
                int length = source.Length;
                ref TiffBgra32 sourceRef = ref MemoryMarshal.GetReference(source);
                ref TiffCmyk32 destinationRef = ref MemoryMarshal.GetReference(destination);

                for (int i = 0; i < length; i++)
                {
                    TiffBgra32 pixel = Unsafe.Add(ref sourceRef, i);

                    byte a = pixel.A;
                    pixel.B = (byte)(pixel.B * a / 255);
                    pixel.G = (byte)(pixel.G * a / 255);
                    pixel.R = (byte)(pixel.R * a / 255);

                    TiffBgra32 cmy = pixel.Inverse();
                    TiffCmyk32 cmyk = default;
                    byte K = cmyk.K = Math.Min(Math.Min(cmy.B, cmy.G), cmy.R);

                    if (K != 255)
                    {
                        cmyk.C = (byte)((cmy.R - K) * 255 / (255 - K));
                        cmyk.M = (byte)((cmy.G - K) * 255 / (255 - K));
                        cmyk.Y = (byte)((cmy.B - K) * 255 / (255 - K));
                    }

                    Unsafe.Add(ref destinationRef, i) = cmyk;
                }
            }
        }
    }
}
