using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.PixelFormats.Converters
{

    internal class TiffRgba64ToCmyk32PixelConverter : TiffPixelConverter<TiffRgba64, TiffCmyk32>
    {
        public TiffRgba64ToCmyk32PixelConverter(ITiffPixelBufferWriter<TiffCmyk32> writer) : base(writer) { }

        public override void Convert(ReadOnlySpan<TiffRgba64> source, Span<TiffCmyk32> destination)
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
                => typeof(TSource) == typeof(TiffRgba64) && typeof(TDestination) == typeof(TiffCmyk32);
            public ITiffPixelBufferWriter<TSource> CreateConverter<TSource, TDestination>(ITiffPixelBufferWriter<TDestination> buffer)
                where TSource : unmanaged
                where TDestination : unmanaged
            {
                if (typeof(TSource) != typeof(TiffRgba64) || typeof(TDestination) != typeof(TiffCmyk32))
                {
                    throw new InvalidOperationException();
                }
                return Unsafe.As<ITiffPixelBufferWriter<TSource>>(new TiffRgba64ToCmyk32PixelConverter(Unsafe.As<ITiffPixelBufferWriter<TiffCmyk32>>(buffer)));
            }
        }

        internal class Converter : ITiffPixelSpanConverter<TiffRgba64, TiffCmyk32>
        {
            public void Convert(ReadOnlySpan<TiffRgba64> source, Span<TiffCmyk32> destination)
            {
                int length = source.Length;
                ref TiffRgba64 sourceRef = ref MemoryMarshal.GetReference(source);
                ref TiffCmyk32 destinationRef = ref MemoryMarshal.GetReference(destination);

                for (int i = 0; i < length; i++)
                {
                    TiffRgba64 pixel = Unsafe.Add(ref sourceRef, i);

                    ushort a = pixel.A;
                    pixel.R = (ushort)(pixel.R * a / ushort.MaxValue);
                    pixel.G = (ushort)(pixel.G * a / ushort.MaxValue);
                    pixel.B = (ushort)(pixel.B * a / ushort.MaxValue);

                    TiffRgba64 cmy = pixel.Inverse();
                    TiffCmyk32 cmyk = default;

                    ushort K = Math.Min(Math.Min(cmy.B, cmy.G), cmy.R);
                    cmyk.K = (byte)(K >> 8);

                    if (K != ushort.MaxValue)
                    {
                        cmyk.C = (byte)(((cmy.R - K) * ushort.MaxValue / (ushort.MaxValue - K)) >> 8);
                        cmyk.M = (byte)(((cmy.G - K) * ushort.MaxValue / (ushort.MaxValue - K)) >> 8);
                        cmyk.Y = (byte)(((cmy.B - K) * ushort.MaxValue / (ushort.MaxValue - K)) >> 8);
                    }

                    Unsafe.Add(ref destinationRef, i) = cmyk;
                }
            }
        }
    }
}
