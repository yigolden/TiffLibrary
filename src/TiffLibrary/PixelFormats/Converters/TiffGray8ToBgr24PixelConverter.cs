using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.PixelFormats.Converters
{
    internal class TiffGray8ToBgr24PixelConverter : TiffPixelConverter<TiffGray8, TiffBgr24>
    {
        public TiffGray8ToBgr24PixelConverter(ITiffPixelBufferWriter<TiffBgr24> writer) : base(writer) { }

        public override void Convert(ReadOnlySpan<TiffGray8> source, Span<TiffBgr24> destination)
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
                => typeof(TSource) == typeof(TiffGray8) && typeof(TDestination) == typeof(TiffBgr24);
            public ITiffPixelBufferWriter<TSource> CreateConverter<TSource, TDestination>(ITiffPixelBufferWriter<TDestination> buffer)
                where TSource : unmanaged
                where TDestination : unmanaged
            {
                if (typeof(TSource) != typeof(TiffGray8) || typeof(TDestination) != typeof(TiffBgr24))
                {
                    ThrowHelper.ThrowInvalidOperationException();
                }
                return Unsafe.As<ITiffPixelBufferWriter<TSource>>(new TiffGray8ToBgr24PixelConverter(Unsafe.As<ITiffPixelBufferWriter<TiffBgr24>>(buffer)));
            }
        }

        internal class Converter : ITiffPixelSpanConverter<TiffGray8, TiffBgr24>
        {
            public void Convert(ReadOnlySpan<TiffGray8> source, Span<TiffBgr24> destination)
            {
                int length = source.Length;
                ref byte sourceRef = ref Unsafe.As<TiffGray8, byte>(ref MemoryMarshal.GetReference(source));
                ref TiffBgr24 destinationRef = ref MemoryMarshal.GetReference(destination);

                TiffBgr24 pixel = default;
                for (int i = 0; i < length; i++)
                {
                    byte intensity = Unsafe.Add(ref sourceRef, i);
                    pixel.B = intensity;
                    pixel.G = intensity;
                    pixel.R = intensity;
                    Unsafe.Add(ref destinationRef, i) = pixel;
                }
            }
        }
    }
}
