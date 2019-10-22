using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.PixelFormats.Converters
{

    internal class TiffBgra32ToBgra64PixelConverter : TiffPixelConverter<TiffBgra32, TiffBgra64>
    {
        public TiffBgra32ToBgra64PixelConverter(ITiffPixelBufferWriter<TiffBgra64> writer) : base(writer) { }

        public override void Convert(ReadOnlySpan<TiffBgra32> source, Span<TiffBgra64> destination)
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
                => typeof(TSource) == typeof(TiffBgra32) && typeof(TDestination) == typeof(TiffBgra64);
            public ITiffPixelBufferWriter<TSource> CreateConverter<TSource, TDestination>(ITiffPixelBufferWriter<TDestination> buffer)
                where TSource : unmanaged
                where TDestination : unmanaged
            {
                if (typeof(TSource) != typeof(TiffBgra32) || typeof(TDestination) != typeof(TiffBgra64))
                {
                    throw new InvalidOperationException();
                }
                return Unsafe.As<ITiffPixelBufferWriter<TSource>>(new TiffBgra32ToBgra64PixelConverter(Unsafe.As<ITiffPixelBufferWriter<TiffBgra64>>(buffer)));
            }
        }

        internal class Converter : ITiffPixelSpanConverter<TiffBgra32, TiffBgra64>
        {
            public void Convert(ReadOnlySpan<TiffBgra32> source, Span<TiffBgra64> destination)
            {
                int length = source.Length;
                ref uint sourceRef = ref Unsafe.As<TiffBgra32, uint>(ref MemoryMarshal.GetReference(source));
                ref ulong destinationRef = ref Unsafe.As<TiffBgra64, ulong>(ref MemoryMarshal.GetReference(destination));

                for (int i = 0; i < length; i++)
                {
                    // start:     [bb gg rr aa]
                    // tmp1:      [00 00 00 00 bb 00 gg 00]
                    // tmp2:      [00 00 00 00 00 rr 00 aa]
                    // tmp3:      [00 bb 00 gg 00 rr 00 aa]
                    // tmp3 << 8: [bb 00 gg 00 rr 00 aa 00]
                    // result:    [bb bb gg gg rr rr aa aa]
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
