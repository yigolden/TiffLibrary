using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.PixelFormats.Converters
{

    internal class TiffCmyk32ToCmyk64PixelConverter : TiffPixelConverter<TiffCmyk32, TiffCmyk64>
    {
        public TiffCmyk32ToCmyk64PixelConverter(ITiffPixelBufferWriter<TiffCmyk64> writer) : base(writer) { }

        public override void Convert(ReadOnlySpan<TiffCmyk32> source, Span<TiffCmyk64> destination)
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
                => typeof(TSource) == typeof(TiffCmyk32) && typeof(TDestination) == typeof(TiffCmyk64);
            public ITiffPixelBufferWriter<TSource> CreateConverter<TSource, TDestination>(ITiffPixelBufferWriter<TDestination> buffer)
                where TSource : unmanaged
                where TDestination : unmanaged
            {
                if (typeof(TSource) != typeof(TiffCmyk32) || typeof(TDestination) != typeof(TiffCmyk64))
                {
                    throw new InvalidOperationException();
                }
                return Unsafe.As<ITiffPixelBufferWriter<TSource>>(new TiffCmyk32ToCmyk64PixelConverter(Unsafe.As<ITiffPixelBufferWriter<TiffCmyk64>>(buffer)));
            }
        }

        internal class Converter : ITiffPixelSpanConverter<TiffCmyk32, TiffCmyk64>
        {
            public void Convert(ReadOnlySpan<TiffCmyk32> source, Span<TiffCmyk64> destination)
            {
                int length = source.Length;
                ref uint sourceRef = ref Unsafe.As<TiffCmyk32, uint>(ref MemoryMarshal.GetReference(source));
                ref ulong destinationRef = ref Unsafe.As<TiffCmyk64, ulong>(ref MemoryMarshal.GetReference(destination));

                for (int i = 0; i < length; i++)
                {
                    // start:     [cc mm yy kk]
                    // tmp1:      [00 00 00 00 cc 00 mm 00]
                    // tmp2:      [00 00 00 00 00 yy 00 kk]
                    // tmp3:      [00 cc 00 mm 00 yy 00 kk]
                    // tmp3 << 8: [cc 00 mm 00 yy 00 kk 00]
                    // result:    [cc cc mm mm yy yy kk kk]
                    uint start = Unsafe.Add(ref sourceRef, i);
                    ulong tmp1 = (start & 0xff000000) | ((start & 0x00ff0000) >> 8);
                    ulong tmp2 = ((start & 0x0000ff00) << 8) | (byte)start;
                    ulong tmp3 = (tmp1 << 24) | tmp2;
                    Unsafe.Add(ref destinationRef, i) = (tmp3 << 8) | tmp3;
                }
            }
        }
    }
}
