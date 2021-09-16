using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.PixelFormats.Converters
{
    internal class TiffRgb24ToBgr24PixelConverter : TiffPixelConverter<TiffRgb24, TiffBgr24>
    {
        public TiffRgb24ToBgr24PixelConverter(ITiffPixelBufferWriter<TiffBgr24> writer) : base(writer) { }

        public override void Convert(ReadOnlySpan<TiffRgb24> source, Span<TiffBgr24> destination)
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
                => typeof(TSource) == typeof(TiffRgb24) && typeof(TDestination) == typeof(TiffBgr24);
            public ITiffPixelBufferWriter<TSource> CreateConverter<TSource, TDestination>(ITiffPixelBufferWriter<TDestination> buffer)
                where TSource : unmanaged
                where TDestination : unmanaged
            {
                if (typeof(TSource) != typeof(TiffRgb24) || typeof(TDestination) != typeof(TiffBgr24))
                {
                    ThrowHelper.ThrowInvalidOperationException();
                }
                return Unsafe.As<ITiffPixelBufferWriter<TSource>>(new TiffRgb24ToBgr24PixelConverter(Unsafe.As<ITiffPixelBufferWriter<TiffBgr24>>(buffer)));
            }
        }

        internal class Converter : ITiffPixelSpanConverter<TiffRgb24, TiffBgr24>
        {
            public void Convert(ReadOnlySpan<TiffRgb24> source, Span<TiffBgr24> destination)
            {
                int length = source.Length;
                ref TiffRgb24 sourceRef = ref MemoryMarshal.GetReference(source);
                ref TiffBgr24 destinationRef = ref MemoryMarshal.GetReference(destination);

                TiffRgb24 sourcePixel;
                TiffBgr24 destinationPixel = default;
                for (int i = 0; i < length; i++)
                {
                    sourcePixel = Unsafe.Add(ref sourceRef, i);
                    destinationPixel.R = sourcePixel.R;
                    destinationPixel.G = sourcePixel.G;
                    destinationPixel.B = sourcePixel.B;
                    Unsafe.Add(ref destinationRef, i) = destinationPixel;
                }
            }
        }
    }
}
