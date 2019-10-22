using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.PixelFormats.Converters
{

    internal class TiffRgba64ToCmyk64PixelConverter : TiffPixelConverter<TiffRgba64, TiffCmyk64>
    {
        public TiffRgba64ToCmyk64PixelConverter(ITiffPixelBufferWriter<TiffCmyk64> writer) : base(writer) { }

        public override void Convert(ReadOnlySpan<TiffRgba64> source, Span<TiffCmyk64> destination)
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
                => typeof(TSource) == typeof(TiffRgba64) && typeof(TDestination) == typeof(TiffCmyk64);
            public ITiffPixelBufferWriter<TSource> CreateConverter<TSource, TDestination>(ITiffPixelBufferWriter<TDestination> buffer)
                where TSource : unmanaged
                where TDestination : unmanaged
            {
                if (typeof(TSource) != typeof(TiffRgba64) || typeof(TDestination) != typeof(TiffCmyk64))
                {
                    throw new InvalidOperationException();
                }
                return Unsafe.As<ITiffPixelBufferWriter<TSource>>(new TiffRgba64ToCmyk64PixelConverter(Unsafe.As<ITiffPixelBufferWriter<TiffCmyk64>>(buffer)));
            }
        }

        internal class Converter : ITiffPixelSpanConverter<TiffRgba64, TiffCmyk64>
        {
            public void Convert(ReadOnlySpan<TiffRgba64> source, Span<TiffCmyk64> destination)
            {
                int length = source.Length;
                ref TiffRgba64 sourceRef = ref MemoryMarshal.GetReference(source);
                ref TiffCmyk64 destinationRef = ref MemoryMarshal.GetReference(destination);

                for (int i = 0; i < length; i++)
                {
                    TiffRgba64 pixel = Unsafe.Add(ref sourceRef, i);

                    ushort a = pixel.A;
                    pixel.R = (ushort)(pixel.R * a / ushort.MaxValue);
                    pixel.G = (ushort)(pixel.G * a / ushort.MaxValue);
                    pixel.B = (ushort)(pixel.B * a / ushort.MaxValue);

                    TiffRgba64 cmy = pixel.Inverse();
                    TiffCmyk64 cmyk = default;

                    ushort K = cmyk.K = Math.Min(Math.Min(cmy.B, cmy.G), cmy.R);

                    if (K != ushort.MaxValue)
                    {
                        cmyk.C = (ushort)((cmy.R - K) * ushort.MaxValue / (ushort.MaxValue - K));
                        cmyk.M = (ushort)((cmy.G - K) * ushort.MaxValue / (ushort.MaxValue - K));
                        cmyk.Y = (ushort)((cmy.B - K) * ushort.MaxValue / (ushort.MaxValue - K));
                    }

                    Unsafe.Add(ref destinationRef, i) = cmyk;
                }
            }
        }
    }
}
