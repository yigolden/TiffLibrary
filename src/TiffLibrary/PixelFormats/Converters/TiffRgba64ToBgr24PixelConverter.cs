using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.PixelFormats.Converters
{
    internal class TiffRgba64ToBgr24PixelConverter : TiffPixelConverter<TiffRgba64, TiffBgr24>
    {
        public TiffRgba64ToBgr24PixelConverter(ITiffPixelBufferWriter<TiffBgr24> writer) : base(writer) { }

        public override void Convert(ReadOnlySpan<TiffRgba64> source, Span<TiffBgr24> destination)
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
                => typeof(TSource) == typeof(TiffRgba64) && typeof(TDestination) == typeof(TiffBgr24);
            public ITiffPixelBufferWriter<TSource> CreateConverter<TSource, TDestination>(ITiffPixelBufferWriter<TDestination> buffer)
                where TSource : unmanaged
                where TDestination : unmanaged
            {
                if (typeof(TSource) != typeof(TiffRgba64) || typeof(TDestination) != typeof(TiffBgr24))
                {
                    throw new InvalidOperationException();
                }
                return Unsafe.As<ITiffPixelBufferWriter<TSource>>(new TiffRgba64ToBgr24PixelConverter(Unsafe.As<ITiffPixelBufferWriter<TiffBgr24>>(buffer)));
            }
        }

        internal class Converter : ITiffPixelSpanConverter<TiffRgba64, TiffBgr24>
        {
            public void Convert(ReadOnlySpan<TiffRgba64> source, Span<TiffBgr24> destination)
            {
                int length = source.Length;
                ref TiffRgba64 sourceRef = ref MemoryMarshal.GetReference(source);
                ref TiffBgr24 destinationRef = ref MemoryMarshal.GetReference(destination);

                TiffRgba64 sourcePixel;
                TiffBgr24 destinationPixel = default;
                for (int i = 0; i < length; i++)
                {
                    sourcePixel = Unsafe.Add(ref sourceRef, i);
                    ushort a = sourcePixel.A;
                    destinationPixel.B = (byte)((sourcePixel.B * a / ushort.MaxValue) >> 8);
                    destinationPixel.G = (byte)((sourcePixel.G * a / ushort.MaxValue) >> 8);
                    destinationPixel.R = (byte)((sourcePixel.R * a / ushort.MaxValue) >> 8);
                    Unsafe.Add(ref destinationRef, i) = destinationPixel;
                }
            }
        }
    }
}
