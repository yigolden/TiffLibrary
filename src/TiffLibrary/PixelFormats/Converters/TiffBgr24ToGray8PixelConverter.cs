using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.PixelFormats.Converters
{

    internal class TiffBgr24ToGray8PixelConverter : TiffPixelConverter<TiffBgr24, TiffGray8>
    {
        public TiffBgr24ToGray8PixelConverter(ITiffPixelBufferWriter<TiffGray8> writer) : base(writer) { }

        public override void Convert(ReadOnlySpan<TiffBgr24> source, Span<TiffGray8> destination)
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
                => typeof(TSource) == typeof(TiffBgr24) && typeof(TDestination) == typeof(TiffGray8);
            public ITiffPixelBufferWriter<TSource> CreateConverter<TSource, TDestination>(ITiffPixelBufferWriter<TDestination> buffer)
                where TSource : unmanaged
                where TDestination : unmanaged
            {
                if (typeof(TSource) != typeof(TiffBgr24) || typeof(TDestination) != typeof(TiffGray8))
                {
                    ThrowHelper.ThrowInvalidOperationException();
                }
                return Unsafe.As<ITiffPixelBufferWriter<TSource>>(new TiffBgr24ToGray8PixelConverter(Unsafe.As<ITiffPixelBufferWriter<TiffGray8>>(buffer)));
            }
        }

        internal class Converter : ITiffPixelSpanConverter<TiffBgr24, TiffGray8>
        {
            public void Convert(ReadOnlySpan<TiffBgr24> source, Span<TiffGray8> destination)
            {
                // not improved
#if !NO_VECTOR_SPAN
                if (Vector.IsHardwareAccelerated)
                {
                    ConvertVector(source, destination);
                }
                else
#endif
                {
                    ConvertNormal(source, destination);
                }
            }

            private static void ConvertNormal(ReadOnlySpan<TiffBgr24> source, Span<TiffGray8> destination)
            {
                int length = source.Length;
                ref TiffBgr24 sourceRef = ref MemoryMarshal.GetReference(source);
                ref byte destinationRef = ref Unsafe.As<TiffGray8, byte>(ref MemoryMarshal.GetReference(destination));

                for (int i = 0; i < length; i++)
                {
                    TiffBgr24 pixel = Unsafe.Add(ref sourceRef, i);
                    byte gray = (byte)((pixel.R * 38 + pixel.G * 75 + pixel.B * 15) >> 7);
                    Unsafe.Add(ref destinationRef, i) = gray;
                }
            }

#if !NO_VECTOR_SPAN
            private static void ConvertVector(ReadOnlySpan<TiffBgr24> source, Span<TiffGray8> destination)
            {
                int length = source.Length;
                int batchSize = Vector<int>.Count / 3;
                ref TiffBgr24 sourceRef = ref MemoryMarshal.GetReference(source);
                ref byte destinationRef = ref Unsafe.As<TiffGray8, byte>(ref MemoryMarshal.GetReference(destination));

                Span<int> sourceBuffer = stackalloc int[Vector<int>.Count];
                Span<int> conversationBuffer = stackalloc int[Vector<int>.Count];

                for (int i = 0; i < batchSize; i++)
                {
                    conversationBuffer[i * 3 + 0] = 15;
                    conversationBuffer[i * 3 + 1] = 75;
                    conversationBuffer[i * 3 + 2] = 38;
                }
                var conversationVector = new Vector<int>(conversationBuffer);

                while (length >= Vector<int>.Count)
                {
                    for (int i = 0; i < batchSize; i++)
                    {
                        ref TiffBgr24 pixel = ref Unsafe.Add(ref sourceRef, i);
                        sourceBuffer[i * 3 + 0] = pixel.B;
                        sourceBuffer[i * 3 + 1] = pixel.G;
                        sourceBuffer[i * 3 + 2] = pixel.R;
                    }

                    var pixelVector = new Vector<int>(sourceBuffer);
                    pixelVector = Vector.Multiply(pixelVector, conversationVector);
                    pixelVector.CopyTo(sourceBuffer);

                    for (int i = 0; i < batchSize; i++)
                    {
                        int intensity = sourceBuffer[i * 3 + 0] + sourceBuffer[i * 3 + 1] + sourceBuffer[i * 3 + 2];
                        Unsafe.Add(ref destinationRef, i) = (byte)(intensity >> 7);
                    }

                    sourceRef = ref Unsafe.Add(ref sourceRef, batchSize);
                    destinationRef = ref Unsafe.Add(ref destinationRef, batchSize);
                    length -= batchSize;
                }

                for (int i = 0; i < length; i++)
                {
                    TiffBgr24 pixel = Unsafe.Add(ref sourceRef, i);
                    byte gray = (byte)((pixel.R * 38 + pixel.G * 75 + pixel.B * 15) >> 7);
                    Unsafe.Add(ref destinationRef, i) = gray;
                }
            }
#endif
        }
    }
}
