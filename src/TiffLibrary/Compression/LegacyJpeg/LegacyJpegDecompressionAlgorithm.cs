using System;
using System.IO;
using JpegLibrary;

namespace TiffLibrary.Compression
{
    internal sealed class LegacyJpegDecompressionAlgorithm : ITiffDecompressionAlgorithm
    {
        private readonly ComponentInfo[] _components;
        private ushort _restartInterval;

        private JpegFrameHeader _frameHeader;
        private JpegScanHeader _scanHeader;


        public LegacyJpegDecompressionAlgorithm(int componentCount)
        {
            _components = new ComponentInfo[componentCount];
        }

        public void SetComponent(int componentIndex, ReadOnlySpan<byte> quantizationTableBytes, ReadOnlySpan<byte> dcTableBytes, ReadOnlySpan<byte> acTableBytes)
        {
            // Build quantization table.
            ushort[] quantizationTableElements = new ushort[64];
            for (int i = 0; i < quantizationTableElements.Length; i++)
            {
                quantizationTableElements[i] = quantizationTableBytes[i];
            }
            var quantizationTable = new JpegQuantizationTable(0, (byte)componentIndex, quantizationTableElements);

            // Build DC Table
            int bytesConsumed = 0;
            if (!JpegHuffmanDecodingTable.TryParse(0, (byte)componentIndex, dcTableBytes, out JpegHuffmanDecodingTable? dcTable, ref bytesConsumed))
            {
                throw new InvalidDataException("Corrupted DC table is encountered.");
            }

            // Build AC Table
            if (!JpegHuffmanDecodingTable.TryParse(1, (byte)componentIndex, acTableBytes, out JpegHuffmanDecodingTable? acTable, ref bytesConsumed))
            {
                throw new InvalidDataException("Corrupted DC table is encountered.");
            }

            _components[componentIndex] = new ComponentInfo(quantizationTable, dcTable, acTable);
        }

        public void Initialize(ushort restartInterval, ushort[] subsamplingFactors)
        {
            _restartInterval = restartInterval;

            // Make sure everything is initialized
            if (_components.Length == 0)
            {
                throw new InvalidDataException();
            }
            foreach (ComponentInfo component in _components)
            {
                if (!component.IsInitialized)
                {
                    throw new InvalidDataException();
                }
            }

            var frameComponents = new JpegFrameComponentSpecificationParameters[_components.Length];
            var scanComponents = new JpegScanComponentSpecificationParameters[_components.Length];
            // Special case for YCbCr.
            if (subsamplingFactors.Length == 2)
            {
                if (frameComponents.Length != 3)
                {
                    // YCbCr image must have 3 components.
                    throw new InvalidDataException();
                }
                if (subsamplingFactors[0] != 1 && subsamplingFactors[0] != 2 && subsamplingFactors[0] != 4)
                {
                    throw new InvalidDataException("Subsampling factor other than 1,2,4 is not supported.");
                }
                if (subsamplingFactors[1] != 1 && subsamplingFactors[1] != 2 && subsamplingFactors[1] != 4)
                {
                    throw new InvalidDataException("Subsampling factor other than 1,2,4 is not supported.");
                }
                ushort maxFactor = Math.Max(subsamplingFactors[0], subsamplingFactors[1]);
                frameComponents[0] = new JpegFrameComponentSpecificationParameters(0, (byte)maxFactor, (byte)maxFactor, 0);
                frameComponents[1] = new JpegFrameComponentSpecificationParameters(1, (byte)(maxFactor / subsamplingFactors[0]), (byte)(maxFactor / subsamplingFactors[1]), 1);
                frameComponents[2] = new JpegFrameComponentSpecificationParameters(2, (byte)(maxFactor / subsamplingFactors[0]), (byte)(maxFactor / subsamplingFactors[1]), 2);
            }
            else
            {
                for (int i = 0; i < frameComponents.Length; i++)
                {
                    frameComponents[i] = new JpegFrameComponentSpecificationParameters((byte)i, 1, 1, (byte)i);
                }
            }

            for (int i = 0; i < scanComponents.Length; i++)
            {
                scanComponents[i] = new JpegScanComponentSpecificationParameters((byte)i, (byte)i, (byte)i);
            }

            _frameHeader = new JpegFrameHeader(8, 0, 0, (byte)frameComponents.Length, frameComponents);
            _scanHeader = new JpegScanHeader((byte)scanComponents.Length, scanComponents, 0, 0, 0, 0);
        }

        public int Decompress(TiffDecompressionContext context, ReadOnlyMemory<byte> input, Memory<byte> output)
        {
            // Copy frame header
            JpegFrameHeader frameHeader = _frameHeader;
            frameHeader = new JpegFrameHeader(frameHeader.SamplePrecision, (ushort)context.ImageSize.Height, (ushort)context.ImageSize.Width, frameHeader.NumberOfComponents, frameHeader.Components);

            var decoder = new JpegDecoder();
            decoder.StartOfFrame = JpegMarker.StartOfFrame0;
            decoder.MemoryPool = context.MemoryPool;
            decoder.SetFrameHeader(frameHeader);
            decoder.SetRestartInterval(_restartInterval);

            foreach (ComponentInfo componentInfo in _components)
            {
                decoder.SetQuantizationTable(componentInfo.QuantizationTable);
                decoder.SetHuffmanTable(componentInfo.DcTable);
                decoder.SetHuffmanTable(componentInfo.AcTable);
            }

            var outputWriter = new JpegBuffer8BitOutputWriter(context.ImageSize.Width, context.SkippedScanlines, context.SkippedScanlines + context.RequestedScanlines, decoder.NumberOfComponents, output);
            decoder.SetOutputWriter(outputWriter);

            var reader = new JpegReader(input);
            decoder.ProcessScan(ref reader, _scanHeader);

            return context.BytesPerScanline * context.ImageSize.Height;
        }

        readonly struct ComponentInfo
        {
            public ComponentInfo(JpegQuantizationTable quantizationTable, JpegHuffmanDecodingTable dcTable, JpegHuffmanDecodingTable acTable)
            {
                IsInitialized = true;
                QuantizationTable = quantizationTable;
                DcTable = dcTable;
                AcTable = acTable;
            }

            public bool IsInitialized { get; }
            public JpegQuantizationTable QuantizationTable { get; }
            public JpegHuffmanDecodingTable DcTable { get; }
            public JpegHuffmanDecodingTable AcTable { get; }
        }
    }
}
