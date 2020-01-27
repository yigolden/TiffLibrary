#nullable enable

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace JpegLibrary.ScanDecoder
{
    internal abstract class JpegScanDecoder : IDisposable
    {
        private readonly JpegDecoder _decoder;

        public JpegScanDecoder(JpegDecoder decoder)
        {
            _decoder = decoder;
        }

        public abstract void ProcessScan(ref JpegReader reader, JpegScanHeader scanHeader);

        public abstract void Dispose();

        public static JpegScanDecoder? Create(JpegMarker sofMarker, JpegDecoder decoder, JpegFrameHeader header)
        {
            switch (sofMarker)
            {
                case JpegMarker.StartOfFrame0:
                    return new JpegHuffmanBaselineScanDecoder(decoder, header);
                case JpegMarker.StartOfFrame1:
                    return new JpegHuffmanBaselineScanDecoder(decoder, header);
                case JpegMarker.StartOfFrame2:
                    return new JpegHuffmanProgressiveScanDecoder(decoder, header);
                default:
                    return null;
            }
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
                    throw new InvalidDataException();
                }
                JpegDecodeComponent component = components[i];
                if (component is null)
                {
                    components[i] = component = new JpegDecodeComponent();
                }
                component.ComponentIndex = componentIndex;
                component.HorizontalSamplingFactor = frameComponent.GetValueOrDefault().HorizontalSamplingFactor;
                component.VerticalSamplingFactor = frameComponent.GetValueOrDefault().VerticalSamplingFactor;
                component.DcTable = _decoder.GetHuffmanTable(true, scanComponenet.DcEntropyCodingTableSelector);
                component.AcTable = _decoder.GetHuffmanTable(false, scanComponenet.AcEntropyCodingTableSelector);
                component.QuantizationTable = _decoder.GetQuantizationTable(frameComponent.GetValueOrDefault().QuantizationTableSelector);
                component.HorizontalSubsamplingFactor = maxHorizontalSampling / component.HorizontalSamplingFactor;
                component.VerticalSubsamplingFactor = maxVerticalSampling / component.VerticalSamplingFactor;
            }

            return scanHeader.NumberOfComponents;
        }

        protected JpegDecodeComponent[] InitDecodeComponents(JpegFrameHeader frameHeader, JpegScanHeader scanHeader)
        {
            JpegDecodeComponent[] components = new JpegDecodeComponent[scanHeader.NumberOfComponents];
            InitDecodeComponents(frameHeader, scanHeader, components);
            return components;
        }

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
    }
}
