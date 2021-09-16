using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.PixelFormats.Converters
{

    internal class TiffCmyk64ToCmyk32PixelConverter : TiffPixelConverter<TiffCmyk64, TiffCmyk32>
    {
        public TiffCmyk64ToCmyk32PixelConverter(ITiffPixelBufferWriter<TiffCmyk32> writer) : base(writer) { }

        public override void Convert(ReadOnlySpan<TiffCmyk64> source, Span<TiffCmyk32> destination)
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
                => typeof(TSource) == typeof(TiffCmyk64) && typeof(TDestination) == typeof(TiffCmyk32);
            public ITiffPixelBufferWriter<TSource> CreateConverter<TSource, TDestination>(ITiffPixelBufferWriter<TDestination> buffer)
                where TSource : unmanaged
                where TDestination : unmanaged
            {
                if (typeof(TSource) != typeof(TiffCmyk64) || typeof(TDestination) != typeof(TiffCmyk32))
                {
                    ThrowHelper.ThrowInvalidOperationException();
                }
                return Unsafe.As<ITiffPixelBufferWriter<TSource>>(new TiffCmyk64ToCmyk32PixelConverter(Unsafe.As<ITiffPixelBufferWriter<TiffCmyk32>>(buffer)));
            }
        }

        internal class Converter : ITiffPixelSpanConverter<TiffCmyk64, TiffCmyk32>
        {
            public void Convert(ReadOnlySpan<TiffCmyk64> source, Span<TiffCmyk32> destination)
            {
                int length = source.Length;
                ref TiffCmyk64 sourceRef = ref MemoryMarshal.GetReference(source);
                ref TiffCmyk32 destinationRef = ref MemoryMarshal.GetReference(destination);

                TiffCmyk32 pixel32 = default;
                for (int i = 0; i < length; i++)
                {
                    TiffCmyk64 pixel64 = Unsafe.Add(ref sourceRef, i);
                    pixel32.C = (byte)(pixel64.C >> 8);
                    pixel32.M = (byte)(pixel64.M >> 8);
                    pixel32.Y = (byte)(pixel64.Y >> 8);
                    pixel32.K = (byte)(pixel64.K >> 8);
                    Unsafe.Add(ref destinationRef, i) = pixel32;
                }
            }
        }
    }
}
