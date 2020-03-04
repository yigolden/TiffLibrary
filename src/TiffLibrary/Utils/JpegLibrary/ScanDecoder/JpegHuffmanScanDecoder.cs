#nullable enable

using System;
using System.Diagnostics;

namespace JpegLibrary.ScanDecoder
{
    internal abstract class JpegHuffmanScanDecoder : JpegScanDecoder
    {
        protected JpegDecoder Decoder { get; private set; }

        public JpegHuffmanScanDecoder(JpegDecoder decoder)
        {
            Decoder = decoder;
        }

        protected int InitDecodeComponents(JpegFrameHeader frameHeader, JpegScanHeader scanHeader, Span<JpegHuffmanDecodingComponent> components)
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
                JpegHuffmanDecodingComponent component = components[i];
                if (component is null)
                {
                    components[i] = component = new JpegHuffmanDecodingComponent();
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

        protected JpegHuffmanDecodingComponent[] InitDecodeComponents(JpegFrameHeader frameHeader, JpegScanHeader scanHeader)
        {
            JpegHuffmanDecodingComponent[] components = new JpegHuffmanDecodingComponent[scanHeader.NumberOfComponents];
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
