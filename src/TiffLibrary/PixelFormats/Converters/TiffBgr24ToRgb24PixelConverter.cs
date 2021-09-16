using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.PixelFormats.Converters
{
    internal class TiffBgr24ToRgb24PixelConverter : TiffPixelConverter<TiffBgr24, TiffRgb24>
    {
        public TiffBgr24ToRgb24PixelConverter(ITiffPixelBufferWriter<TiffRgb24> writer) : base(writer) { }

        public override void Convert(ReadOnlySpan<TiffBgr24> source, Span<TiffRgb24> destination)
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
                => typeof(TSource) == typeof(TiffBgr24) && typeof(TDestination) == typeof(TiffRgb24);
            public ITiffPixelBufferWriter<TSource> CreateConverter<TSource, TDestination>(ITiffPixelBufferWriter<TDestination> buffer)
                where TSource : unmanaged
                where TDestination : unmanaged
            {
                if (typeof(TSource) != typeof(TiffBgr24) || typeof(TDestination) != typeof(TiffRgb24))
                {
                    ThrowHelper.ThrowInvalidOperationException();
                }
                return Unsafe.As<ITiffPixelBufferWriter<TSource>>(new TiffBgr24ToRgb24PixelConverter(Unsafe.As<ITiffPixelBufferWriter<TiffRgb24>>(buffer)));
            }
        }

        internal class Converter : ITiffPixelSpanConverter<TiffBgr24, TiffRgb24>
        {
            public void Convert(ReadOnlySpan<TiffBgr24> source, Span<TiffRgb24> destination)
            {
                int length = source.Length;
                ref TiffBgr24 sourceRef = ref MemoryMarshal.GetReference(source);
                ref TiffRgb24 destinationRef = ref MemoryMarshal.GetReference(destination);

                TiffRgb24 destinationPixel = default;
                for (int i = 0; i < length; i++)
                {
                    TiffBgr24 sourcePixel = Unsafe.Add(ref sourceRef, i);
                    destinationPixel.B = sourcePixel.B;
                    destinationPixel.G = sourcePixel.G;
                    destinationPixel.R = sourcePixel.R;
                    Unsafe.Add(ref destinationRef, i) = destinationPixel;
                }
            }
        }
    }
}
