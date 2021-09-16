using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.PixelFormats.Converters
{

    internal class TiffCmyk64ToRgba64PixelConverter : TiffPixelConverter<TiffCmyk64, TiffRgba64>
    {
        public TiffCmyk64ToRgba64PixelConverter(ITiffPixelBufferWriter<TiffRgba64> writer) : base(writer) { }

        public override void Convert(ReadOnlySpan<TiffCmyk64> source, Span<TiffRgba64> destination)
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
                => typeof(TSource) == typeof(TiffCmyk64) && typeof(TDestination) == typeof(TiffRgba64);
            public ITiffPixelBufferWriter<TSource> CreateConverter<TSource, TDestination>(ITiffPixelBufferWriter<TDestination> buffer)
                where TSource : unmanaged
                where TDestination : unmanaged
            {
                if (typeof(TSource) != typeof(TiffCmyk64) || typeof(TDestination) != typeof(TiffRgba64))
                {
                    ThrowHelper.ThrowInvalidOperationException();
                }
                return Unsafe.As<ITiffPixelBufferWriter<TSource>>(new TiffCmyk64ToRgba64PixelConverter(Unsafe.As<ITiffPixelBufferWriter<TiffRgba64>>(buffer)));
            }
        }

        internal class Converter : ITiffPixelSpanConverter<TiffCmyk64, TiffRgba64>
        {
            public void Convert(ReadOnlySpan<TiffCmyk64> source, Span<TiffRgba64> destination)
            {
                int length = source.Length;
                ref TiffCmyk64 sourceRef = ref MemoryMarshal.GetReference(source);
                ref TiffRgba64 destinationRef = ref MemoryMarshal.GetReference(destination);

                TiffRgba64 bgra = default;
                bgra.A = ushort.MaxValue;
                for (int i = 0; i < length; i++)
                {
                    TiffCmyk64 cmyk = Unsafe.Add(ref sourceRef, i);
                    bgra.R = (ushort)((ushort.MaxValue - cmyk.C) * (ushort.MaxValue - cmyk.K) >> 16);
                    bgra.G = (ushort)((ushort.MaxValue - cmyk.M) * (ushort.MaxValue - cmyk.K) >> 16);
                    bgra.B = (ushort)((ushort.MaxValue - cmyk.Y) * (ushort.MaxValue - cmyk.K) >> 16);

                    Unsafe.Add(ref destinationRef, i) = bgra;
                }
            }
        }
    }
}
