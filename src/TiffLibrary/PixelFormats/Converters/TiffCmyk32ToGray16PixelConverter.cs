using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.PixelFormats.Converters
{

    internal class TiffCmyk32ToGray16PixelConverter : TiffPixelConverter<TiffCmyk32, TiffGray16>
    {
        public TiffCmyk32ToGray16PixelConverter(ITiffPixelBufferWriter<TiffGray16> writer) : base(writer) { }

        public override void Convert(ReadOnlySpan<TiffCmyk32> source, Span<TiffGray16> destination)
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
                => typeof(TSource) == typeof(TiffCmyk32) && typeof(TDestination) == typeof(TiffGray16);
            public ITiffPixelBufferWriter<TSource> CreateConverter<TSource, TDestination>(ITiffPixelBufferWriter<TDestination> buffer)
                where TSource : unmanaged
                where TDestination : unmanaged
            {
                if (typeof(TSource) != typeof(TiffCmyk32) || typeof(TDestination) != typeof(TiffGray16))
                {
                    ThrowHelper.ThrowInvalidOperationException();
                }
                return Unsafe.As<ITiffPixelBufferWriter<TSource>>(new TiffCmyk32ToGray16PixelConverter(Unsafe.As<ITiffPixelBufferWriter<TiffGray16>>(buffer)));
            }
        }

        internal class Converter : ITiffPixelSpanConverter<TiffCmyk32, TiffGray16>
        {
            private const int Scale = 14;
            private const int CR = (int)(0.299 * (1 << Scale) + 0.5);
            private const int CG = (int)(0.587 * (1 << Scale) + 0.5);
            private const int CB = ((1 << Scale) - CR - CG);

            public void Convert(ReadOnlySpan<TiffCmyk32> source, Span<TiffGray16> destination)
            {
                int length = source.Length;
                ref TiffCmyk32 sourceRef = ref MemoryMarshal.GetReference(source);
                ref ushort destinationRef = ref Unsafe.As<TiffGray16, ushort>(ref MemoryMarshal.GetReference(destination));

                for (int i = 0; i < length; i++)
                {
                    TiffCmyk32 cmyk = Unsafe.Add(ref sourceRef, i);
                    int k = cmyk.K;
                    int c = cmyk.K - ((255 - cmyk.C) * k >> 8);
                    int m = cmyk.K - ((255 - cmyk.M) * k >> 8);
                    int y = cmyk.K - ((255 - cmyk.Y) * k >> 8);
                    int t = Descale(y * CB + m * CG + c * CR, Scale);
                    Unsafe.Add(ref destinationRef, i) = (ushort)(t << 8 | t);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static int Descale(int x, int n)
            {
                return (((x) + (1 << ((n) - 1))) >> (n));
            }
        }
    }
}
