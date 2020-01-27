#nullable enable

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace JpegLibrary.ScanDecoder
{
    internal abstract class JpegHuffmanScanDecoder : JpegScanDecoder
    {
        public JpegHuffmanScanDecoder(JpegDecoder decoder) : base(decoder) { }

        protected static int DecodeHuffmanCode(ref JpegBitReader reader, JpegHuffmanDecodingTable table)
        {
            int bits = reader.PeekBits(16, out int bitsRead);
            JpegHuffmanDecodingTable.Entry entry = table.Lookup(bits);
            bitsRead = Math.Min(entry.CodeSize, bitsRead);
            _ = reader.TryAdvanceBits(bitsRead, out _);
            return entry.CodeValue;
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
                Unsafe.Add(ref destinationRef, i) = (short)(JpegMathHelper.RoundToInt32(Unsafe.Add(ref sourceRef, i)) + levelShift);
            }
        }

        protected static JpegHuffmanDecodingTable.Entry DecodeHuffmanCode(ref JpegBitReader reader, JpegHuffmanDecodingTable table, out int code, out int bitsRead)
        {
            int bits = reader.PeekBits(16, out bitsRead);
            JpegHuffmanDecodingTable.Entry entry = table.Lookup(bits);
            bitsRead = Math.Min(entry.CodeSize, bitsRead);
            _ = reader.TryAdvanceBits(bitsRead, out _);
            code = bits >> (16 - bitsRead);
            return entry;
        }

        protected static int Receive(ref JpegBitReader reader, int length)
        {
            Debug.Assert(length > 0);
            if (!reader.TryReadBits(length, out int value, out bool isMarkerEncountered))
            {
                if (isMarkerEncountered)
                {
                    ThrowInvalidDataException("Expect raw data from bit stream. Yet a marker is encountered.");
                }
                ThrowInvalidDataException("The bit stream ended prematurely.");
            }

            return Extend(value, length);

            static int Extend(int v, int nbits) => v - ((((v + v) >> nbits) - 1) & ((1 << nbits) - 1));
        }
    }
}
