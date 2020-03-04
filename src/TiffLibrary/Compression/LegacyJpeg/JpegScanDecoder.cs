#nullable enable

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.Utils;

namespace JpegLibrary.ScanDecoder
{
    internal abstract class JpegScanDecoder : IDisposable
    {
        public abstract void ProcessScan(ref JpegReader reader, JpegScanHeader scanHeader);

        public abstract void Dispose();

        [DoesNotReturn]
        protected static void ThrowInvalidDataException(string? message = null)
        {
            if (message is null)
            {
                throw new InvalidDataException();
            }
            else
            {
                throw new InvalidDataException(message);
            }
        }

        [DoesNotReturn]
        protected static void ThrowInvalidDataException(int offset, string message)
        {
            throw new InvalidDataException($"Failed to decode JPEG data at offset {offset}. {message}");
        }

        protected static void DequantizeBlockAndUnZigZag(JpegQuantizationTable quantizationTable, ref JpegBlock8x8 input, ref JpegBlock8x8F output)
        {
            Debug.Assert(!quantizationTable.IsEmpty);

            ref ushort elementRef = ref MemoryMarshal.GetReference(quantizationTable.Elements);
            ref short sourceRef = ref Unsafe.As<JpegBlock8x8, short>(ref input);
            ref float destinationRef = ref Unsafe.As<JpegBlock8x8F, float>(ref output);

            for (int i = 0; i < 64; i++)
            {
                Unsafe.Add(ref destinationRef, JpegZigZag.BufferIndexToBlock(i)) = Unsafe.Add(ref elementRef, i) * Unsafe.Add(ref sourceRef, i);
            }
        }

        protected static void ShiftDataLevel(ref JpegBlock8x8F source, ref JpegBlock8x8 destination, int levelShift)
        {
            ref float sourceRef = ref Unsafe.As<JpegBlock8x8F, float>(ref source);
            ref short destinationRef = ref Unsafe.As<JpegBlock8x8, short>(ref destination);

            for (int i = 0; i < 64; i++)
            {
                Unsafe.Add(ref destinationRef, i) = (short)(TiffMathHelper.RoundToInt32(Unsafe.Add(ref sourceRef, i)) + levelShift);
            }
        }
    }
}
