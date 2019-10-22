using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.PixelFormats.Converters
{

    internal class TiffGray8ToGray16PixelConverter : TiffPixelConverter<TiffGray8, TiffGray16>
    {
        public TiffGray8ToGray16PixelConverter(ITiffPixelBufferWriter<TiffGray16> writer) : base(writer) { }

        public override void Convert(ReadOnlySpan<TiffGray8> source, Span<TiffGray16> destination)
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
                => typeof(TSource) == typeof(TiffGray8) && typeof(TDestination) == typeof(TiffGray16);
            public ITiffPixelBufferWriter<TSource> CreateConverter<TSource, TDestination>(ITiffPixelBufferWriter<TDestination> buffer)
                where TSource : unmanaged
                where TDestination : unmanaged
            {
                if (typeof(TSource) != typeof(TiffGray8) || typeof(TDestination) != typeof(TiffGray16))
                {
                    throw new InvalidOperationException();
                }
                return Unsafe.As<ITiffPixelBufferWriter<TSource>>(new TiffGray8ToGray16PixelConverter(Unsafe.As<ITiffPixelBufferWriter<TiffGray16>>(buffer)));
            }
        }

        internal class Converter : ITiffPixelSpanConverter<TiffGray8, TiffGray16>
        {
            public void Convert(ReadOnlySpan<TiffGray8> source, Span<TiffGray16> destination)
            {
                int length = source.Length;
                ref byte sourceRef = ref Unsafe.As<TiffGray8, byte>(ref MemoryMarshal.GetReference(source));
                ref ushort destinationRef = ref Unsafe.As<TiffGray16, ushort>(ref MemoryMarshal.GetReference(destination));

                for (int i = 0; i < length; i++)
                {
                    byte intensity = Unsafe.Add(ref sourceRef, i);
                    Unsafe.Add(ref destinationRef, i) = (ushort)(intensity << 8 | intensity);
                }
            }
        }
    }
}
