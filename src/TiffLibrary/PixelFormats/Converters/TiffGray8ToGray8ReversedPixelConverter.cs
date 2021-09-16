using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.PixelFormats.Converters
{
    internal class TiffGray8ToGray8ReversedPixelConverter : TiffPixelConverter<TiffGray8, TiffGray8Reversed>
    {
        public TiffGray8ToGray8ReversedPixelConverter(ITiffPixelBufferWriter<TiffGray8Reversed> writer) : base(writer) { }

        public override void Convert(ReadOnlySpan<TiffGray8> source, Span<TiffGray8Reversed> destination)
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
                => typeof(TSource) == typeof(TiffGray8) && typeof(TDestination) == typeof(TiffGray8Reversed);
            public ITiffPixelBufferWriter<TSource> CreateConverter<TSource, TDestination>(ITiffPixelBufferWriter<TDestination> buffer)
                where TSource : unmanaged
                where TDestination : unmanaged
            {
                if (typeof(TSource) != typeof(TiffGray8) || typeof(TDestination) != typeof(TiffGray8Reversed))
                {
                    ThrowHelper.ThrowInvalidOperationException();
                }
                return Unsafe.As<ITiffPixelBufferWriter<TSource>>(new TiffGray8ToGray8ReversedPixelConverter(Unsafe.As<ITiffPixelBufferWriter<TiffGray8Reversed>>(buffer)));
            }
        }

        internal class Converter : ITiffPixelSpanConverter<TiffGray8, TiffGray8Reversed>
        {
            public void Convert(ReadOnlySpan<TiffGray8> source, Span<TiffGray8Reversed> destination)
            {
                ReadOnlySpan<byte> sourceSpan = MemoryMarshal.AsBytes(source);
                Span<byte> destinationSpan = MemoryMarshal.AsBytes(destination);

                for (int i = 0; i < sourceSpan.Length; i++)
                {
                    destinationSpan[i] = (byte)~sourceSpan[i];
                }
            }
        }
    }
}
