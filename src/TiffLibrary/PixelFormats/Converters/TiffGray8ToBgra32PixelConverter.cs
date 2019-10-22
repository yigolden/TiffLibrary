using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.PixelFormats.Converters
{

    internal class TiffGray8ToBgra32PixelConverter : TiffPixelConverter<TiffGray8, TiffBgra32>
    {
        public TiffGray8ToBgra32PixelConverter(ITiffPixelBufferWriter<TiffBgra32> writer) : base(writer) { }

        public override void Convert(ReadOnlySpan<TiffGray8> source, Span<TiffBgra32> destination)
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
                => typeof(TSource) == typeof(TiffGray8) && typeof(TDestination) == typeof(TiffBgra32);
            public ITiffPixelBufferWriter<TSource> CreateConverter<TSource, TDestination>(ITiffPixelBufferWriter<TDestination> buffer)
                where TSource : unmanaged
                where TDestination : unmanaged
            {
                if (typeof(TSource) != typeof(TiffGray8) || typeof(TDestination) != typeof(TiffBgra32))
                {
                    throw new InvalidOperationException();
                }
                return Unsafe.As<ITiffPixelBufferWriter<TSource>>(new TiffGray8ToBgra32PixelConverter(Unsafe.As<ITiffPixelBufferWriter<TiffBgra32>>(buffer)));
            }
        }

        internal class Converter : ITiffPixelSpanConverter<TiffGray8, TiffBgra32>
        {
            public void Convert(ReadOnlySpan<TiffGray8> source, Span<TiffBgra32> destination)
            {
                int length = source.Length;
                ref byte sourceRef = ref Unsafe.As<TiffGray8, byte>(ref MemoryMarshal.GetReference(source));
                ref uint destinationRef = ref Unsafe.As<TiffBgra32, uint>(ref MemoryMarshal.GetReference(destination));

                if (BitConverter.IsLittleEndian)
                {
                    for (int i = 0; i < length; i++)
                    {
                        uint intensity = Unsafe.Add(ref sourceRef, i);
                        intensity = intensity << 8 | intensity;
                        intensity = intensity << 16 | intensity;
                        intensity = 0xff000000 | intensity;
                        Unsafe.Add(ref destinationRef, i) = intensity;
                    }
                }
                else
                {
                    for (int i = 0; i < length; i++)
                    {
                        uint intensity = Unsafe.Add(ref sourceRef, i);
                        intensity = intensity << 8 | intensity;
                        intensity = intensity << 16 | intensity;
                        intensity = 0xff | intensity;
                        Unsafe.Add(ref destinationRef, i) = intensity;
                    }
                }
            }
        }
    }
}
