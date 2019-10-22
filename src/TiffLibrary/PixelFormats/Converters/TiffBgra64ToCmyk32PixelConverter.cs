using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.PixelFormats.Converters
{

    internal class TiffBgra64ToCmyk32PixelConverter : TiffPixelConverter<TiffBgra64, TiffCmyk32>
    {
        public TiffBgra64ToCmyk32PixelConverter(ITiffPixelBufferWriter<TiffCmyk32> writer) : base(writer) { }

        public override void Convert(ReadOnlySpan<TiffBgra64> source, Span<TiffCmyk32> destination)
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
                => typeof(TSource) == typeof(TiffBgra64) && typeof(TDestination) == typeof(TiffCmyk32);
            public ITiffPixelBufferWriter<TSource> CreateConverter<TSource, TDestination>(ITiffPixelBufferWriter<TDestination> buffer)
                where TSource : unmanaged
                where TDestination : unmanaged
            {
                if (typeof(TSource) != typeof(TiffBgra64) || typeof(TDestination) != typeof(TiffCmyk32))
                {
                    throw new InvalidOperationException();
                }
                return Unsafe.As<ITiffPixelBufferWriter<TSource>>(new TiffBgra64ToCmyk32PixelConverter(Unsafe.As<ITiffPixelBufferWriter<TiffCmyk32>>(buffer)));
            }
        }

        internal class Converter : ITiffPixelSpanConverter<TiffBgra64, TiffCmyk32>
        {
            public void Convert(ReadOnlySpan<TiffBgra64> source, Span<TiffCmyk32> destination)
            {
                int length = source.Length;
                ref TiffBgra64 sourceRef = ref MemoryMarshal.GetReference(source);
                ref TiffCmyk32 destinationRef = ref MemoryMarshal.GetReference(destination);

                for (int i = 0; i < length; i++)
                {
                    TiffBgra64 pixel = Unsafe.Add(ref sourceRef, i);

                    ushort a = pixel.A;
                    pixel.B = (ushort)(pixel.B * a / ushort.MaxValue);
                    pixel.G = (ushort)(pixel.G * a / ushort.MaxValue);
                    pixel.R = (ushort)(pixel.R * a / ushort.MaxValue);

                    TiffBgra64 cmy = pixel.Inverse();
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
