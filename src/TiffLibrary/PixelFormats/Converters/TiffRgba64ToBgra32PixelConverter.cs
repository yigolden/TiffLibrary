using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.PixelFormats.Converters
{

    internal class TiffRgba64ToBgra32PixelConverter : TiffPixelConverter<TiffRgba64, TiffBgra32>
    {
        public TiffRgba64ToBgra32PixelConverter(ITiffPixelBufferWriter<TiffBgra32> writer) : base(writer) { }

        public override void Convert(ReadOnlySpan<TiffRgba64> source, Span<TiffBgra32> destination)
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
                => typeof(TSource) == typeof(TiffRgba64) && typeof(TDestination) == typeof(TiffBgra32);
            public ITiffPixelBufferWriter<TSource> CreateConverter<TSource, TDestination>(ITiffPixelBufferWriter<TDestination> buffer)
                where TSource : unmanaged
                where TDestination : unmanaged
            {
                if (typeof(TSource) != typeof(TiffRgba64) || typeof(TDestination) != typeof(TiffBgra32))
                {
                    ThrowHelper.ThrowInvalidOperationException();
                }
                return Unsafe.As<ITiffPixelBufferWriter<TSource>>(new TiffRgba64ToBgra32PixelConverter(Unsafe.As<ITiffPixelBufferWriter<TiffBgra32>>(buffer)));
            }
        }

        internal class Converter : ITiffPixelSpanConverter<TiffRgba64, TiffBgra32>
        {
            public void Convert(ReadOnlySpan<TiffRgba64> source, Span<TiffBgra32> destination)
            {
                int length = source.Length;
                ref TiffRgba64 sourceRef = ref MemoryMarshal.GetReference(source);
                ref TiffBgra32 destinationRef = ref MemoryMarshal.GetReference(destination);

                TiffBgra32 pixel32 = default;
                for (int i = 0; i < length; i++)
                {
                    TiffRgba64 pixel64 = Unsafe.Add(ref sourceRef, i);
                    pixel32.R = (byte)(pixel64.R >> 8);
                    pixel32.G = (byte)(pixel64.G >> 8);
                    pixel32.B = (byte)(pixel64.B >> 8);
                    pixel32.A = (byte)(pixel64.A >> 8);
                    Unsafe.Add(ref destinationRef, i) = pixel32;
                }
            }
        }
    }
}
