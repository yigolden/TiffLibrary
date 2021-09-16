using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelBuffer;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.PixelFormats.Converters
{
    internal class TiffGray8ToMaskPixelConverter : TiffPixelConverter<TiffGray8, TiffMask>
    {
        public TiffGray8ToMaskPixelConverter(ITiffPixelBufferWriter<TiffMask> writer) : base(writer) { }

        public override void Convert(ReadOnlySpan<TiffGray8> source, Span<TiffMask> destination)
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
                => typeof(TSource) == typeof(TiffGray8) && typeof(TDestination) == typeof(TiffMask);
            public ITiffPixelBufferWriter<TSource> CreateConverter<TSource, TDestination>(ITiffPixelBufferWriter<TDestination> buffer)
                where TSource : unmanaged
                where TDestination : unmanaged
            {
                if (typeof(TSource) != typeof(TiffGray8) || typeof(TDestination) != typeof(TiffMask))
                {
                    ThrowHelper.ThrowInvalidOperationException();
                }
                return Unsafe.As<ITiffPixelBufferWriter<TSource>>(new TiffPassthroughPixelBufferWriter<TiffGray8, TiffMask>(Unsafe.As<ITiffPixelBufferWriter<TiffMask>>(buffer)));
            }
        }

        internal class Converter : ITiffPixelSpanConverter<TiffGray8, TiffMask>
        {
            public void Convert(ReadOnlySpan<TiffGray8> source, Span<TiffMask> destination)
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
