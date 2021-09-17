using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.PixelFormats.Converters
{

    internal class TiffGray16ToCmyk64PixelConverter : TiffPixelConverter<TiffGray16, TiffCmyk64>
    {
        public TiffGray16ToCmyk64PixelConverter(ITiffPixelBufferWriter<TiffCmyk64> writer) : base(writer) { }

        public override void Convert(ReadOnlySpan<TiffGray16> source, Span<TiffCmyk64> destination)
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
                => typeof(TSource) == typeof(TiffGray16) && typeof(TDestination) == typeof(TiffCmyk64);
            public ITiffPixelBufferWriter<TSource> CreateConverter<TSource, TDestination>(ITiffPixelBufferWriter<TDestination> buffer)
                where TSource : unmanaged
                where TDestination : unmanaged
            {
                if (typeof(TSource) != typeof(TiffGray16) || typeof(TDestination) != typeof(TiffCmyk64))
                {
                    ThrowHelper.ThrowInvalidOperationException();
                }
                return Unsafe.As<ITiffPixelBufferWriter<TSource>>(new TiffGray16ToCmyk64PixelConverter(Unsafe.As<ITiffPixelBufferWriter<TiffCmyk64>>(buffer)));
            }
        }

        internal class Converter : ITiffPixelSpanConverter<TiffGray16, TiffCmyk64>
        {
            public void Convert(ReadOnlySpan<TiffGray16> source, Span<TiffCmyk64> destination)
            {
                // not improved
#if !NO_VECTOR_SPAN
                if (Vector.IsHardwareAccelerated && source.Length >= Vector<ushort>.Count)
                {
                    ConvertVector(MemoryMarshal.Cast<TiffGray16, ushort>(source), destination);
                }
                else
#endif
                {
                    ConvertNormal(source, destination);
                }
            }

            private void ConvertNormal(ReadOnlySpan<TiffGray16> source, Span<TiffCmyk64> destination)
            {
                int length = source.Length;
                ref TiffGray16 sourceRef = ref MemoryMarshal.GetReference(source);
                ref TiffCmyk64 destinationRef = ref MemoryMarshal.GetReference(destination);

                TiffCmyk64 cmyk = default;
                for (int i = 0; i < length; i++)
                {
                    ushort intensity = Unsafe.As<TiffGray16, ushort>(ref Unsafe.Add(ref sourceRef, i));
                    cmyk.K = (ushort)~intensity;
                    Unsafe.Add(ref destinationRef, i) = cmyk;
                }
            }

#if !NO_VECTOR_SPAN
            private static void ConvertVector(ReadOnlySpan<ushort> source, Span<TiffCmyk64> destination)
            {
                Span<ushort> buffer = stackalloc ushort[Vector<ushort>.Count];

                TiffCmyk64 cmyk = default;
                var oneVector = Vector.Negate(Vector<ushort>.One);
                var shiftVector = Vector.Multiply(0x1000000000000ul, Vector<ulong>.One);
                while (source.Length >= Vector<ushort>.Count)
                {
                    var sourceVector = new Vector<ushort>(source);
                    sourceVector = Vector.Xor(sourceVector, oneVector);
                    sourceVector.CopyTo(buffer);
                    Vector.Widen(sourceVector, out Vector<uint> v12, out Vector<uint> v34);
                    Vector.Widen(v12, out Vector<ulong> v1, out Vector<ulong> v2);
                    Vector.Widen(v34, out Vector<ulong> v3, out Vector<ulong> v4);
                    if (BitConverter.IsLittleEndian)
                    {
                        v1 = Vector.Multiply(v1, shiftVector);
                        v2 = Vector.Multiply(v2, shiftVector);
                        v3 = Vector.Multiply(v3, shiftVector);
                        v4 = Vector.Multiply(v4, shiftVector);
                    }

                    v1.CopyTo(MemoryMarshal.Cast<TiffCmyk64, ulong>(destination));
                    destination = destination.Slice(Vector<ulong>.Count);
                    v2.CopyTo(MemoryMarshal.Cast<TiffCmyk64, ulong>(destination));
                    destination = destination.Slice(Vector<ulong>.Count);
                    v3.CopyTo(MemoryMarshal.Cast<TiffCmyk64, ulong>(destination));
                    destination = destination.Slice(Vector<ulong>.Count);
                    v4.CopyTo(MemoryMarshal.Cast<TiffCmyk64, ulong>(destination));
                    destination = destination.Slice(Vector<ulong>.Count);

                    source = source.Slice(Vector<ushort>.Count);
                }

                ref TiffCmyk64 destinationRef = ref MemoryMarshal.GetReference(destination);
                for (int i = 0; i < source.Length; i++)
                {
                    cmyk.K = (ushort)~source[i];
                    Unsafe.Add(ref destinationRef, i) = cmyk;
                }
            }
#endif
        }
    }
}
