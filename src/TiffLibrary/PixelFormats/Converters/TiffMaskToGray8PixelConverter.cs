using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelBuffer;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.PixelFormats.Converters
{
    internal class TiffMaskToGray8PixelConverter : TiffPixelConverter<TiffMask, TiffGray8>
    {
        public TiffMaskToGray8PixelConverter(ITiffPixelBufferWriter<TiffGray8> writer) : base(writer) { }

        public override void Convert(ReadOnlySpan<TiffMask> source, Span<TiffGray8> destination)
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
                => typeof(TSource) == typeof(TiffMask) && typeof(TDestination) == typeof(TiffGray8);
            public ITiffPixelBufferWriter<TSource> CreateConverter<TSource, TDestination>(ITiffPixelBufferWriter<TDestination> buffer)
                where TSource : unmanaged
                where TDestination : unmanaged
            {
                if (typeof(TSource) != typeof(TiffMask) || typeof(TDestination) != typeof(TiffGray8))
                {
                    ThrowHelper.ThrowInvalidOperationException();
                }
                return Unsafe.As<ITiffPixelBufferWriter<TSource>>(new TiffPassthroughPixelBufferWriter<TiffMask, TiffGray8>(Unsafe.As<ITiffPixelBufferWriter<TiffGray8>>(buffer)));
            }
        }

        internal class Converter : ITiffPixelSpanConverter<TiffMask, TiffGray8>
        {
            public void Convert(ReadOnlySpan<TiffMask> source, Span<TiffGray8> destination)
            {
                ReadOnlySpan<byte> sourceSpan = MemoryMarshal.AsBytes(source);
                Span<byte> destinationSpan = MemoryMarshal.AsBytes(destination);

                if (!Unsafe.AreSame(ref MemoryMarshal.GetReference(sourceSpan), ref MemoryMarshal.GetReference(destinationSpan)))
                {
                    sourceSpan.CopyTo(destinationSpan);
                }

            }
        }
    }
}
