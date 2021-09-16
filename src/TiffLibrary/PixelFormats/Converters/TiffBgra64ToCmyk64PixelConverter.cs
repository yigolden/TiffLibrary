using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.PixelFormats.Converters
{

    internal class TiffBgra64ToCmyk64PixelConverter : TiffPixelConverter<TiffBgra64, TiffCmyk64>
    {
        public TiffBgra64ToCmyk64PixelConverter(ITiffPixelBufferWriter<TiffCmyk64> writer) : base(writer) { }

        public override void Convert(ReadOnlySpan<TiffBgra64> source, Span<TiffCmyk64> destination)
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
                => typeof(TSource) == typeof(TiffBgra64) && typeof(TDestination) == typeof(TiffCmyk64);
            public ITiffPixelBufferWriter<TSource> CreateConverter<TSource, TDestination>(ITiffPixelBufferWriter<TDestination> buffer)
                where TSource : unmanaged
                where TDestination : unmanaged
            {
                if (typeof(TSource) != typeof(TiffBgra64) || typeof(TDestination) != typeof(TiffCmyk64))
                {
                    ThrowHelper.ThrowInvalidOperationException();
                }
                return Unsafe.As<ITiffPixelBufferWriter<TSource>>(new TiffBgra64ToCmyk64PixelConverter(Unsafe.As<ITiffPixelBufferWriter<TiffCmyk64>>(buffer)));
            }
        }

        internal class Converter : ITiffPixelSpanConverter<TiffBgra64, TiffCmyk64>
        {
            public void Convert(ReadOnlySpan<TiffBgra64> source, Span<TiffCmyk64> destination)
            {
                int length = source.Length;
                ref TiffBgra64 sourceRef = ref MemoryMarshal.GetReference(source);
                ref TiffCmyk64 destinationRef = ref MemoryMarshal.GetReference(destination);

                for (int i = 0; i < length; i++)
                {
                    TiffBgra64 pixel = Unsafe.Add(ref sourceRef, i);

                    ushort a = pixel.A;
                    pixel.B = (ushort)(pixel.B * a / ushort.MaxValue);
                    pixel.G = (ushort)(pixel.G * a / ushort.MaxValue);
                    pixel.R = (ushort)(pixel.R * a / ushort.MaxValue);

                    TiffBgra64 cmy = pixel.Inverse();
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
