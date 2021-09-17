using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.PixelFormats.Converters
{

    internal class TiffCmyk32ToRgba32PixelConverter : TiffPixelConverter<TiffCmyk32, TiffRgba32>
    {
        public TiffCmyk32ToRgba32PixelConverter(ITiffPixelBufferWriter<TiffRgba32> writer) : base(writer) { }

        public override void Convert(ReadOnlySpan<TiffCmyk32> source, Span<TiffRgba32> destination)
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
                => typeof(TSource) == typeof(TiffCmyk32) && typeof(TDestination) == typeof(TiffRgba32);
            public ITiffPixelBufferWriter<TSource> CreateConverter<TSource, TDestination>(ITiffPixelBufferWriter<TDestination> buffer)
                where TSource : unmanaged
                where TDestination : unmanaged
            {
                if (typeof(TSource) != typeof(TiffCmyk32) || typeof(TDestination) != typeof(TiffRgba32))
                {
                    ThrowHelper.ThrowInvalidOperationException();
                }
                return Unsafe.As<ITiffPixelBufferWriter<TSource>>(new TiffCmyk32ToRgba32PixelConverter(Unsafe.As<ITiffPixelBufferWriter<TiffRgba32>>(buffer)));
            }
        }

        internal class Converter : ITiffPixelSpanConverter<TiffCmyk32, TiffRgba32>
        {
            public void Convert(ReadOnlySpan<TiffCmyk32> source, Span<TiffRgba32> destination)
            {
                // not improved
#if !NO_VECTOR_SPAN
                if (Vector.IsHardwareAccelerated )
                {
                    ConvertVector(source, destination);
                }
                else
#endif
                {
                    ConvertNormal(source, destination);
                }
            }

            public static void ConvertNormal(ReadOnlySpan<TiffCmyk32> source, Span<TiffRgba32> destination)
            {
                int length = source.Length;
                ref TiffCmyk32 sourceRef = ref MemoryMarshal.GetReference(source);
                ref TiffRgba32 destinationRef = ref MemoryMarshal.GetReference(destination);

                TiffRgba32 bgra = default;
                bgra.A = 255;
                for (int i = 0; i < length; i++)
                {
                    TiffCmyk32 cmyk = Unsafe.Add(ref sourceRef, i);
                    bgra.B = (byte)((255 - cmyk.Y) * (255 - cmyk.K) >> 8);
                    bgra.G = (byte)((255 - cmyk.M) * (255 - cmyk.K) >> 8);
                    bgra.R = (byte)((255 - cmyk.C) * (255 - cmyk.K) >> 8);
                    Unsafe.Add(ref destinationRef, i) = bgra;
                }
            }

            private static void ConvertVector(ReadOnlySpan<TiffCmyk32> source, Span<TiffRgba32> destination)
            {
                while (source.Length >= Vector<byte>.Count)
                {
                    var vector = new Vector<uint>(MemoryMarshal.Cast<TiffCmyk32, uint>(source));
                    Vector<byte> vectorPixel = Vector.AsVectorByte(vector);
                    if (BitConverter.IsLittleEndian)
                    {
                        vector = Vector.BitwiseAnd(vector, new Vector<uint>(0xFF000000));
                        vector = Vector.BitwiseOr(vector, Vector.Divide(vector, new Vector<uint>(0x10000)));
                        vector = Vector.BitwiseOr(vector, Vector.Divide(vector, new Vector<uint>(0x100)));
                    }
                    else
                    {
                        vector = Vector.BitwiseAnd(vector, new Vector<uint>(0xFF));
                        vector = Vector.BitwiseOr(vector, Vector.Multiply(vector, new Vector<uint>(0x10000)));
                        vector = Vector.BitwiseOr(vector, Vector.Multiply(vector, new Vector<uint>(0x100)));
                    }

                    Vector<byte> vectorKBytes = Vector.AsVectorByte(vector);
                    vectorPixel = Vector.Subtract(new Vector<byte>(255), vectorPixel);
                    vectorKBytes = Vector.Subtract(new Vector<byte>(255), vectorKBytes);

                    Vector.Widen(vectorPixel, out Vector<ushort> vectorPixel1, out Vector<ushort> vectorPixel2);
                    Vector.Widen(vectorKBytes, out Vector<ushort> vectorK1, out Vector<ushort> vectorK2);

                    vectorPixel1 = Vector.Multiply(vectorPixel1, vectorK1);
                    vectorPixel2 = Vector.Multiply(vectorPixel2, vectorK2);

                    vectorPixel1 = Vector.Divide(vectorPixel1, new Vector<ushort>(0x100));
                    vectorPixel2 = Vector.Divide(vectorPixel2, new Vector<ushort>(0x100));

                    vectorPixel = Vector.Narrow(vectorPixel1, vectorPixel2);
                    vector = Vector.AsVectorUInt32(vectorPixel);
                    if (BitConverter.IsLittleEndian)
                    {
                        vector = Vector.BitwiseAnd(vector, new Vector<uint>(0xFF000000));
                    }
                    else
                    {
                        vector = Vector.BitwiseAnd(vector, new Vector<uint>(0xFF));
                    }

                    vector.CopyTo(MemoryMarshal.Cast<TiffRgba32, uint>(destination));

                    source = source.Slice(Vector<uint>.Count);
                    destination = destination.Slice(Vector<uint>.Count);
                }

                ConvertNormal(source, destination);
            }
        }
    }
}
