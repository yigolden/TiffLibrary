using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.PixelFormats.Converters
{

    internal class TiffRgba32ToRgba64PixelConverter : TiffPixelConverter<TiffRgba32, TiffRgba64>
    {
        public TiffRgba32ToRgba64PixelConverter(ITiffPixelBufferWriter<TiffRgba64> writer) : base(writer) { }

        public override void Convert(ReadOnlySpan<TiffRgba32> source, Span<TiffRgba64> destination)
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
                => typeof(TSource) == typeof(TiffRgba32) && typeof(TDestination) == typeof(TiffRgba64);
            public ITiffPixelBufferWriter<TSource> CreateConverter<TSource, TDestination>(ITiffPixelBufferWriter<TDestination> buffer)
                where TSource : unmanaged
                where TDestination : unmanaged
            {
                if (typeof(TSource) != typeof(TiffRgba32) || typeof(TDestination) != typeof(TiffRgba64))
                {
                    ThrowHelper.ThrowInvalidOperationException();
                }
                return Unsafe.As<ITiffPixelBufferWriter<TSource>>(new TiffRgba32ToRgba64PixelConverter(Unsafe.As<ITiffPixelBufferWriter<TiffRgba64>>(buffer)));
            }
        }

        internal class Converter : ITiffPixelSpanConverter<TiffRgba32, TiffRgba64>
        {
            public void Convert(ReadOnlySpan<TiffRgba32> source, Span<TiffRgba64> destination)
            {
                int length = source.Length;
                ref uint sourceRef = ref Unsafe.As<TiffRgba32, uint>(ref MemoryMarshal.GetReference(source));
                ref ulong destinationRef = ref Unsafe.As<TiffRgba64, ulong>(ref MemoryMarshal.GetReference(destination));

                for (int i = 0; i < length; i++)
                {
                    // start:     [rr gg bb aa]
                    // tmp1:      [00 00 00 00 rr 00 gg 00]
                    // tmp2:      [00 00 00 00 00 bb 00 aa]
                    // tmp3:      [00 rr 00 gg 00 bb 00 aa]
                    // tmp3 << 8: [rr 00 gg 00 bb 00 aa 00]
                    // result:    [rr rr gg gg bb bb aa aa]
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
