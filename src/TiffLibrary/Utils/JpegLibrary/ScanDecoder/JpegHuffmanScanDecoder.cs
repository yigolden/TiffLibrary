#nullable enable

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace JpegLibrary.ScanDecoder
{
    internal abstract class JpegHuffmanScanDecoder : JpegScanDecoder
    {
        protected JpegDecoder Decoder { get; private set; }

        public JpegHuffmanScanDecoder(JpegDecoder decoder)
        {
            Decoder = decoder;
        }

        protected int InitDecodeComponents(JpegFrameHeader frameHeader, JpegScanHeader scanHeader, Span<JpegDecodeComponent> components)
        {
            Debug.Assert(!(frameHeader.Components is null));
            Debug.Assert(!(scanHeader.Components is null));

            // Compute maximum sampling factor
            int maxHorizontalSampling = 1;
            int maxVerticalSampling = 1;
            foreach (JpegFrameComponentSpecificationParameters currentFrameComponent in frameHeader.Components!)
            {
                maxHorizontalSampling = Math.Max(maxHorizontalSampling, currentFrameComponent.HorizontalSamplingFactor);
                maxVerticalSampling = Math.Max(maxVerticalSampling, currentFrameComponent.VerticalSamplingFactor);
            }

            // Resolve each component
            if (components.Length < scanHeader.NumberOfComponents)
            {
                throw new InvalidOperationException();
            }
            for (int i = 0; i < scanHeader.NumberOfComponents; i++)
            {
                JpegScanComponentSpecificationParameters scanComponenet = scanHeader.Components![i];
                int componentIndex = 0;
                JpegFrameComponentSpecificationParameters? frameComponent = null;

                for (int j = 0; j < frameHeader.NumberOfComponents; j++)
                {
                    JpegFrameComponentSpecificationParameters currentFrameComponent = frameHeader.Components[j];
                    if (scanComponenet.ScanComponentSelector == currentFrameComponent.Identifier)
                    {
                        componentIndex = j;
                        frameComponent = currentFrameComponent;
                    }
                }
                if (frameComponent is null)
                {
                    ThrowInvalidDataException();
                }
                JpegDecodeComponent component = components[i];
                if (component is null)
                {
                    components[i] = component = new JpegDecodeComponent();
                }
                component.ComponentIndex = componentIndex;
                component.HorizontalSamplingFactor = frameComponent.GetValueOrDefault().HorizontalSamplingFactor;
                component.VerticalSamplingFactor = frameComponent.GetValueOrDefault().VerticalSamplingFactor;
                component.DcTable = Decoder.GetHuffmanTable(true, scanComponenet.DcEntropyCodingTableSelector);
                component.AcTable = Decoder.GetHuffmanTable(false, scanComponenet.AcEntropyCodingTableSelector);
                component.QuantizationTable = Decoder.GetQuantizationTable(frameComponent.GetValueOrDefault().QuantizationTableSelector);
                component.HorizontalSubsamplingFactor = maxHorizontalSampling / component.HorizontalSamplingFactor;
                component.VerticalSubsamplingFactor = maxVerticalSampling / component.VerticalSamplingFactor;
                component.DcPredictor = 0;
            }

            return scanHeader.NumberOfComponents;
        }

        protected JpegDecodeComponent[] InitDecodeComponents(JpegFrameHeader frameHeader, JpegScanHeader scanHeader)
        {
            JpegDecodeComponent[] components = new JpegDecodeComponent[scanHeader.NumberOfComponents];
            InitDecodeComponents(frameHeader, scanHeader, components);
            return components;
        }

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

        protected static int ReceiveAndExtend(ref JpegBitReader reader, int length)
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
