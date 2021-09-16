using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.PixelFormats.Converters
{

    internal class TiffGray16ToBgra64PixelConverter : TiffPixelConverter<TiffGray16, TiffBgra64>
    {
        public TiffGray16ToBgra64PixelConverter(ITiffPixelBufferWriter<TiffBgra64> writer) : base(writer) { }

        public override void Convert(ReadOnlySpan<TiffGray16> source, Span<TiffBgra64> destination)
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
                => typeof(TSource) == typeof(TiffGray16) && typeof(TDestination) == typeof(TiffBgra64);
            public ITiffPixelBufferWriter<TSource> CreateConverter<TSource, TDestination>(ITiffPixelBufferWriter<TDestination> buffer)
                where TSource : unmanaged
                where TDestination : unmanaged
            {
                if (typeof(TSource) != typeof(TiffGray16) || typeof(TDestination) != typeof(TiffBgra64))
                {
                    ThrowHelper.ThrowInvalidOperationException();
                }
                return Unsafe.As<ITiffPixelBufferWriter<TSource>>(new TiffGray16ToBgra64PixelConverter(Unsafe.As<ITiffPixelBufferWriter<TiffBgra64>>(buffer)));
            }
        }

        internal class Converter : ITiffPixelSpanConverter<TiffGray16, TiffBgra64>
        {
            public void Convert(ReadOnlySpan<TiffGray16> source, Span<TiffBgra64> destination)
            {
                int length = source.Length;
                ref ushort sourceRef = ref Unsafe.As<TiffGray16, ushort>(ref MemoryMarshal.GetReference(source));
                ref ulong destinationRef = ref Unsafe.As<TiffBgra64, ulong>(ref MemoryMarshal.GetReference(destination));

                if (BitConverter.IsLittleEndian)
                {
                    for (int i = 0; i < length; i++)
                    {
                        ulong intensity = Unsafe.Add(ref sourceRef, i);
                        intensity = intensity << 16 | intensity;
                        intensity = intensity << 32 | intensity;
                        intensity = 0xffff000000000000 | intensity;
                        Unsafe.Add(ref destinationRef, i) = intensity;
                    }
                }
                else
                {
                    for (int i = 0; i < length; i++)
                    {
                        ulong intensity = Unsafe.Add(ref sourceRef, i);
                        intensity = intensity << 16 | intensity;
                        intensity = intensity << 32 | intensity;
                        intensity = 0xffff | intensity;
                        Unsafe.Add(ref destinationRef, i) = intensity;
                    }
                }
            }
        }
    }
}
