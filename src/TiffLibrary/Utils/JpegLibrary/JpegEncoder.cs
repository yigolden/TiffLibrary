#nullable enable

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace JpegLibrary
{
    internal class JpegEncoder
    {
        private int _minimumBufferSegmentSize;

        private JpegBlockInputReader? _input;
        private IBufferWriter<byte>? _output;

        private List<JpegQuantizationTable>? _quantizationTables;
        private JpegHuffmanEncodingTableCollection _huffmanTables;
        private List<JpegHuffmanEncodingComponent>? _encodeComponents;

        public JpegEncoder() : this(4096) { }

        public JpegEncoder(int minimumBufferSegmentSize)
        {
            _minimumBufferSegmentSize = minimumBufferSegmentSize;
        }

        protected int MinimumBufferSegmentSize => _minimumBufferSegmentSize;

        protected T CloneParameters<T>() where T : JpegEncoder, new()
        {
            bool optimizeCoding = _huffmanTables.ContainsTableBuilder();
            var cloned = new T()
            {
                _minimumBufferSegmentSize = _minimumBufferSegmentSize,
                _quantizationTables = _quantizationTables,
                _huffmanTables = optimizeCoding ? _huffmanTables.DeepClone() : _huffmanTables
            };
            List<JpegHuffmanEncodingComponent>? components = _encodeComponents;
            if (!(components is null))
            {
                foreach (JpegHuffmanEncodingComponent item in components)
                {
                    cloned.AddComponent(item.QuantizationTable.Identifier, item.DcTableIdentifier, item.AcTableIdentifier, item.HorizontalSamplingFactor, item.VerticalSamplingFactor);
                }
            }
            return cloned;
        }

        public MemoryPool<byte>? MemoryPool { get; set; }

        public void SetInputReader(JpegBlockInputReader input)
        {
            _input = input ?? throw new ArgumentNullException(nameof(input));
        }

        public void SetOutput(IBufferWriter<byte> output)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
        }

        public void SetQuantizationTable(JpegQuantizationTable quantizationTable)
        {
            if (quantizationTable.IsEmpty)
            {
                throw new ArgumentException("Quantization table is not initialized.", nameof(quantizationTable));
            }
            if (quantizationTable.ElementPrecision != 0)
            {
                throw new InvalidOperationException("Only baseline JPEG is supported.");
            }

            List<JpegQuantizationTable>? tables = _quantizationTables;
            if (tables is null)
            {
                _quantizationTables = tables = new List<JpegQuantizationTable>(2);
            }

            for (int i = 0; i < tables.Count; i++)
            {
                if (tables[i].Identifier == quantizationTable.Identifier)
                {
                    tables[i] = quantizationTable;
                    return;
                }
            }

            tables.Add(quantizationTable);
        }

        public void SetHuffmanTable(bool isDcTable, byte identifier, JpegHuffmanEncodingTable? table)
        {
            _huffmanTables.AddTable(isDcTable ? (byte)0 : (byte)1, identifier, table);
        }

        public void SetHuffmanTable(bool isDcTable, byte identifier)
            => SetHuffmanTable(isDcTable, identifier, null);

        private JpegQuantizationTable GetQuantizationTable(byte identifier)
        {
            if (_quantizationTables is null)
            {
                return default;
            }
            foreach (JpegQuantizationTable item in _quantizationTables)
            {
                if (item.Identifier == identifier)
                {
                    return item;
                }
            }
            return default;
        }

        public void AddComponent(byte quantizationTableIdentifier, byte huffmanDcTableIdentifier, byte huffmanAcTableIdentifier, byte horizontalSubsampling, byte verticalSubsampling)
        {
            if (horizontalSubsampling != 1 && horizontalSubsampling != 2 && horizontalSubsampling != 4)
            {
                throw new ArgumentOutOfRangeException(nameof(horizontalSubsampling), "Subsampling factor can only be 1, 2 or 4.");
            }
            if (verticalSubsampling != 1 && verticalSubsampling != 2 && verticalSubsampling != 4)
            {
                throw new ArgumentOutOfRangeException(nameof(verticalSubsampling), "Subsampling factor can only be 1, 2 or 4.");
            }

            List<JpegHuffmanEncodingComponent>? components = _encodeComponents;
            if (components is null)
            {
                _encodeComponents = components = new List<JpegHuffmanEncodingComponent>(4);
            }

            JpegQuantizationTable quantizationTable = GetQuantizationTable(quantizationTableIdentifier);
            if (quantizationTable.IsEmpty)
            {
                throw new ArgumentException("Quantization table is not defined.", nameof(quantizationTableIdentifier));
            }
            JpegHuffmanEncodingTable? dcTable = _huffmanTables.GetTable(true, huffmanDcTableIdentifier);
            JpegHuffmanEncodingTableBuilder? dcTableBuilder = null;
            if (dcTable is null)
            {
                dcTableBuilder = _huffmanTables.GetTableBuilder(true, huffmanDcTableIdentifier);
                if (dcTableBuilder is null)
                {
                    throw new ArgumentException("Huffman table is not defined.", nameof(huffmanDcTableIdentifier));
                }
            }
            JpegHuffmanEncodingTable? acTable = _huffmanTables.GetTable(false, huffmanAcTableIdentifier);
            JpegHuffmanEncodingTableBuilder? acTableBuilder = null;
            if (acTable is null)
            {
                acTableBuilder = _huffmanTables.GetTableBuilder(false, huffmanAcTableIdentifier);
                if (acTableBuilder is null)
                {
                    throw new ArgumentException("Huffman table is not defined.", nameof(huffmanAcTableIdentifier));
                }
            }

            var component = new JpegHuffmanEncodingComponent
            {
                ComponentIndex = components.Count,
                HorizontalSamplingFactor = horizontalSubsampling,
                VerticalSamplingFactor = verticalSubsampling,
                DcTableIdentifier = huffmanDcTableIdentifier,
                AcTableIdentifier = huffmanAcTableIdentifier,
                DcTable = dcTable,
                AcTable = acTable,
                DcTableBuilder = dcTableBuilder,
                AcTableBuilder = acTableBuilder,
                QuantizationTable = quantizationTable
            };
            components.Add(component);
        }

        protected JpegWriter CreateJpegWriter()
        {
            IBufferWriter<byte> output = _output ?? throw new InvalidOperationException("Output is not specified.");
            return new JpegWriter(output, _minimumBufferSegmentSize);
        }

        public virtual void Encode()
        {
            bool optimizeCoding = _huffmanTables.ContainsTableBuilder();

            JpegWriter writer = CreateJpegWriter();

            WriteStartOfImage(ref writer);
            WriteQuantizationTables(ref writer);
            JpegFrameHeader frameHeader = WriteStartOfFrame(ref writer);
            JpegBlockAllocator? allocator = optimizeCoding ? new JpegBlockAllocator(MemoryPool) : null;
            try
            {
                if (!(allocator is null))
                {
                    allocator.Allocate(frameHeader);
                    TransformBlocks(allocator);
                    BuildHuffmanTables(frameHeader, allocator, optimal: false);
                    WriteHuffmanTables(ref writer);
                    WriteStartOfScan(ref writer);
                    WritePreparedScanData(frameHeader, allocator, ref writer);
                }
                else
                {
                    WriteHuffmanTables(ref writer);
                    WriteStartOfScan(ref writer);
                    WriteScanData(ref writer);
                }
            }
            finally
            {
                allocator?.Dispose();
            }
            WriteEndOfImage(ref writer);

            writer.Flush();
        }

        protected static void WriteStartOfImage(ref JpegWriter writer)
        {
            writer.WriteMarker(JpegMarker.StartOfImage);
        }

        protected void WriteQuantizationTables(ref JpegWriter writer)
        {
            List<JpegQuantizationTable>? quantizationTables = _quantizationTables;
            if (quantizationTables is null)
            {
                throw new InvalidOperationException();
            }


            writer.WriteMarker(JpegMarker.DefineQuantizationTable);

            ushort totalByteCount = 0;
            foreach (JpegQuantizationTable table in quantizationTables)
            {
                totalByteCount += table.BytesRequired;
            }

            writer.WriteLength(totalByteCount);

            foreach (JpegQuantizationTable table in quantizationTables)
            {
                Span<byte> buffer = writer.GetSpan(table.BytesRequired);
                table.TryWrite(buffer, out int bytesWritten);
                writer.Advance(bytesWritten);
            }
        }

        protected void WriteHuffmanTables(ref JpegWriter writer)
        {
            if (_huffmanTables.IsEmpty)
            {
                throw new InvalidOperationException();
            }

            writer.WriteMarker(JpegMarker.DefineHuffmanTable);
            ushort totalByteCoubt = _huffmanTables.GetTotalBytesRequired();
            writer.WriteLength(totalByteCoubt);
            _huffmanTables.Write(ref writer);
        }

        protected JpegFrameHeader WriteStartOfFrame(ref JpegWriter writer)
        {
            JpegBlockInputReader? input = _input;
            if (input is null)
            {
                throw new InvalidOperationException("Input is not specified.");
            }
            List<JpegHuffmanEncodingComponent>? encodeComponents = _encodeComponents;
            if (encodeComponents is null || encodeComponents.Count == 0)
            {
                throw new InvalidOperationException("No component is specified.");
            }
            JpegFrameComponentSpecificationParameters[] components = new JpegFrameComponentSpecificationParameters[encodeComponents.Count];
            for (int i = 0; i < encodeComponents.Count; i++)
            {
                JpegHuffmanEncodingComponent thisComponent = encodeComponents[i];
                components[i] = new JpegFrameComponentSpecificationParameters((byte)(i + 1), thisComponent.HorizontalSamplingFactor, thisComponent.VerticalSamplingFactor, thisComponent.QuantizationTable.Identifier);
            }
            JpegFrameHeader frameHeader = new JpegFrameHeader(8, (ushort)input.Height, (ushort)input.Width, (byte)components.Length, components);

            writer.WriteMarker(JpegMarker.StartOfFrame0);
            byte bytesCount = frameHeader.BytesRequired;
            writer.WriteLength(bytesCount);
            Span<byte> buffer = writer.GetSpan(bytesCount);
            frameHeader.TryWrite(buffer, out _);
            writer.Advance(bytesCount);

            return frameHeader;
        }

        protected void WriteStartOfScan(ref JpegWriter writer)
        {
            List<JpegHuffmanEncodingComponent>? encodeComponents = _encodeComponents;
            if (encodeComponents is null || encodeComponents.Count == 0)
            {
                throw new InvalidOperationException("No component is specified.");
            }
            JpegScanComponentSpecificationParameters[] components = new JpegScanComponentSpecificationParameters[encodeComponents.Count];
            for (int i = 0; i < encodeComponents.Count; i++)
            {
                JpegHuffmanEncodingComponent thisComponent = encodeComponents[i];
                components[i] = new JpegScanComponentSpecificationParameters((byte)(i + 1), thisComponent.DcTableIdentifier, thisComponent.AcTableIdentifier);
            }
            var scanHeader = new JpegScanHeader((byte)components.Length, components, 0, 63, 0, 0);

            writer.WriteMarker(JpegMarker.StartOfScan);
            byte bytesCount = scanHeader.BytesRequired;
            writer.WriteLength(bytesCount);
            Span<byte> buffer = writer.GetSpan(bytesCount);
            scanHeader.TryWrite(buffer, out _);
            writer.Advance(bytesCount);
        }

        protected void TransformBlocks(JpegBlockAllocator allocator)
        {
            JpegBlockInputReader inputReader = _input ?? throw new InvalidOperationException("Input is not specified.");
            List<JpegHuffmanEncodingComponent>? components = _encodeComponents;
            if (components is null || components.Count == 0)
            {
                throw new InvalidOperationException("No component is specified.");
            }

            // Compute maximum sampling factor and reset DC predictor
            int maxHorizontalSampling = 1;
            int maxVerticalSampling = 1;
            foreach (JpegHuffmanEncodingComponent currentComponent in components)
            {
                currentComponent.DcPredictor = 0;
                maxHorizontalSampling = Math.Max(maxHorizontalSampling, currentComponent.HorizontalSamplingFactor);
                maxVerticalSampling = Math.Max(maxVerticalSampling, currentComponent.VerticalSamplingFactor);
            }
            foreach (JpegHuffmanEncodingComponent currentComponent in components)
            {
                currentComponent.HorizontalSubsamplingFactor = maxHorizontalSampling / currentComponent.HorizontalSamplingFactor;
                currentComponent.VerticalSubsamplingFactor = maxVerticalSampling / currentComponent.VerticalSamplingFactor;
            }

            int mcusPerLine = (inputReader.Width + 8 * maxHorizontalSampling - 1) / (8 * maxHorizontalSampling);
            int mcusPerColumn = (inputReader.Height + 8 * maxVerticalSampling - 1) / (8 * maxVerticalSampling);
            const int levelShift = 1 << (8 - 1);

            JpegBlock8x8F inputFBuffer = default;
            JpegBlock8x8F outputFBuffer = default;
            JpegBlock8x8F tempFBuffer = default;

            for (int rowMcu = 0; rowMcu < mcusPerColumn; rowMcu++)
            {
                for (int colMcu = 0; colMcu < mcusPerLine; colMcu++)
                {
                    foreach (JpegHuffmanEncodingComponent component in components)
                    {
                        int index = component.ComponentIndex;
                        int h = component.HorizontalSamplingFactor;
                        int v = component.VerticalSamplingFactor;
                        int hs = component.HorizontalSubsamplingFactor;
                        int vs = component.VerticalSubsamplingFactor;
                        int offsetX = colMcu * h;
                        int offsetY = rowMcu * v;

                        for (int y = 0; y < v; y++)
                        {
                            int blockOffsetY = offsetY + y;
                            for (int x = 0; x < h; x++)
                            {
                                ref JpegBlock8x8 blockRef = ref allocator.GetBlockReference(index, offsetX + x, blockOffsetY);

                                // Read Block
                                ReadBlock(inputReader, out blockRef, component.ComponentIndex, (offsetX + x) * 8 * hs, blockOffsetY * 8 * vs, hs, vs);

                                // Level shift
                                ShiftDataLevel(ref blockRef, ref inputFBuffer, levelShift);

                                // FDCT
                                FastFloatingPointDCT.TransformFDCT(ref inputFBuffer, ref outputFBuffer, ref tempFBuffer);

                                // ZigZagAndQuantize
                                ZigZagAndQuantizeBlock(component.QuantizationTable, ref outputFBuffer, ref blockRef);
                            }
                        }
                    }
                }
            }
        }

        protected void BuildHuffmanTables(JpegFrameHeader frameHeader, JpegBlockAllocator allocator, bool optimal = false)
        {
            List<JpegHuffmanEncodingComponent>? components = _encodeComponents;
            if (components is null || components.Count == 0)
            {
                throw new InvalidOperationException("No component is specified.");
            }

            // Compute maximum sampling factor and reset DC predictor
            int maxHorizontalSampling = 1;
            int maxVerticalSampling = 1;
            foreach (JpegHuffmanEncodingComponent currentComponent in components)
            {
                currentComponent.DcPredictor = 0;
                maxHorizontalSampling = Math.Max(maxHorizontalSampling, currentComponent.HorizontalSamplingFactor);
                maxVerticalSampling = Math.Max(maxVerticalSampling, currentComponent.VerticalSamplingFactor);
            }

            int mcusPerLine = (frameHeader.SamplesPerLine + 8 * maxHorizontalSampling - 1) / (8 * maxHorizontalSampling);
            int mcusPerColumn = (frameHeader.NumberOfLines + 8 * maxVerticalSampling - 1) / (8 * maxVerticalSampling);

            for (int rowMcu = 0; rowMcu < mcusPerColumn; rowMcu++)
            {
                for (int colMcu = 0; colMcu < mcusPerLine; colMcu++)
                {
                    foreach (JpegHuffmanEncodingComponent component in components)
                    {
                        int index = component.ComponentIndex;
                        int h = component.HorizontalSamplingFactor;
                        int v = component.VerticalSamplingFactor;
                        int offsetX = colMcu * h;
                        int offsetY = rowMcu * v;

                        for (int y = 0; y < v; y++)
                        {
                            int blockOffsetY = offsetY + y;
                            for (int x = 0; x < h; x++)
                            {
                                ref JpegBlock8x8 blockRef = ref allocator.GetBlockReference(index, offsetX + x, blockOffsetY);

                                GatherBlockStatistics(component, ref blockRef);
                            }
                        }
                    }
                }
            }

            // Build huffman table
            _huffmanTables.BuildTables(optimal);

            // Reset huffman table
            foreach (JpegHuffmanEncodingComponent component in components)
            {
                component.DcTable = _huffmanTables.GetTable(true, component.DcTableIdentifier);
                component.AcTable = _huffmanTables.GetTable(false, component.AcTableIdentifier);
                component.DcTableBuilder = null;
                component.DcTableBuilder = null;
            }
        }

        private static void GatherBlockStatistics(JpegHuffmanEncodingComponent component, ref JpegBlock8x8 block)
        {
            ref short blockRef = ref Unsafe.As<JpegBlock8x8, short>(ref block);

            // DC
            int blockValue = blockRef;
            int t = blockValue - component.DcPredictor;
            component.DcPredictor = blockValue;
            if (!(component.DcTableBuilder is null))
            {
                GatherRunLengthCodeStatistics(component.DcTableBuilder, 0, t);
            }

            // AC
            JpegHuffmanEncodingTableBuilder? acTableBuilder = component.AcTableBuilder;
            if (acTableBuilder is null)
            {
                return;
            }
            int runLength = 0;
            for (int i = 1; i < 64; i++)
            {
                t = Unsafe.Add(ref blockRef, i);

                if (t == 0)
                {
                    runLength++;
                }
                else
                {
                    while (runLength > 15)
                    {
                        acTableBuilder.IncrementCodeCount(0xf0);
                        runLength -= 16;
                    }

                    GatherRunLengthCodeStatistics(acTableBuilder, runLength, t);
                    runLength = 0;
                }
            }

            if (runLength > 0)
            {
                // EOB
                acTableBuilder.IncrementCodeCount(0);
            }
        }

        protected void WritePreparedScanData(JpegFrameHeader frameHeader, JpegBlockAllocator allocator, ref JpegWriter writer)
        {
            List<JpegHuffmanEncodingComponent>? components = _encodeComponents;
            if (components is null || components.Count == 0)
            {
                throw new InvalidOperationException("No component is specified.");
            }

            // Compute maximum sampling factor and reset DC predictor
            int maxHorizontalSampling = 1;
            int maxVerticalSampling = 1;
            foreach (JpegHuffmanEncodingComponent currentComponent in components)
            {
                currentComponent.DcPredictor = 0;
                maxHorizontalSampling = Math.Max(maxHorizontalSampling, currentComponent.HorizontalSamplingFactor);
                maxVerticalSampling = Math.Max(maxVerticalSampling, currentComponent.VerticalSamplingFactor);
            }

            int mcusPerLine = (frameHeader.SamplesPerLine + 8 * maxHorizontalSampling - 1) / (8 * maxHorizontalSampling);
            int mcusPerColumn = (frameHeader.NumberOfLines + 8 * maxVerticalSampling - 1) / (8 * maxVerticalSampling);

            writer.EnterBitMode();

            for (int rowMcu = 0; rowMcu < mcusPerColumn; rowMcu++)
            {
                for (int colMcu = 0; colMcu < mcusPerLine; colMcu++)
                {
                    foreach (JpegHuffmanEncodingComponent component in components)
                    {
                        int index = component.ComponentIndex;
                        int h = component.HorizontalSamplingFactor;
                        int v = component.VerticalSamplingFactor;
                        int offsetX = colMcu * h;
                        int offsetY = rowMcu * v;

                        for (int y = 0; y < v; y++)
                        {
                            int blockOffsetY = offsetY + y;
                            for (int x = 0; x < h; x++)
                            {
                                ref JpegBlock8x8 blockRef = ref allocator.GetBlockReference(index, offsetX + x, blockOffsetY);

                                EncodeBlock(ref writer, component, ref blockRef);
                            }
                        }
                    }
                }
            }

            // Padding
            writer.ExitBitMode();
        }

        protected void WriteScanData(ref JpegWriter writer)
        {
            JpegBlockInputReader inputReader = _input ?? throw new InvalidOperationException("Input is not specified.");
            List<JpegHuffmanEncodingComponent>? components = _encodeComponents;
            if (components is null || components.Count == 0)
            {
                throw new InvalidOperationException("No component is specified.");
            }

            // Compute maximum sampling factor and reset DC predictor
            int maxHorizontalSampling = 1;
            int maxVerticalSampling = 1;
            foreach (JpegHuffmanEncodingComponent currentComponent in components)
            {
                currentComponent.DcPredictor = 0;
                maxHorizontalSampling = Math.Max(maxHorizontalSampling, currentComponent.HorizontalSamplingFactor);
                maxVerticalSampling = Math.Max(maxVerticalSampling, currentComponent.VerticalSamplingFactor);
            }
            foreach (JpegHuffmanEncodingComponent currentComponent in components)
            {
                currentComponent.HorizontalSubsamplingFactor = maxHorizontalSampling / currentComponent.HorizontalSamplingFactor;
                currentComponent.VerticalSubsamplingFactor = maxVerticalSampling / currentComponent.VerticalSamplingFactor;
            }

            // Prepare
            int mcusPerLine = (inputReader.Width + 8 * maxHorizontalSampling - 1) / (8 * maxHorizontalSampling);
            int mcusPerColumn = (inputReader.Height + 8 * maxVerticalSampling - 1) / (8 * maxVerticalSampling);

            writer.EnterBitMode();

            const int levelShift = 1 << (8 - 1);

            JpegBlock8x8F inputFBuffer = default;
            JpegBlock8x8F outputFBuffer = default;
            JpegBlock8x8F tempFBuffer = default;

            JpegBlock8x8 inputBuffer;

            for (int rowMcu = 0; rowMcu < mcusPerColumn; rowMcu++)
            {
                int offsetY = rowMcu * maxVerticalSampling;
                for (int colMcu = 0; colMcu < mcusPerLine; colMcu++)
                {
                    int offsetX = colMcu * maxHorizontalSampling;

                    // Scan an interleaved mcu... process components in order
                    foreach (JpegHuffmanEncodingComponent component in components)
                    {
                        int h = component.HorizontalSamplingFactor;
                        int v = component.VerticalSamplingFactor;
                        int hs = component.HorizontalSubsamplingFactor;
                        int vs = component.VerticalSubsamplingFactor;

                        for (int y = 0; y < v; y++)
                        {
                            int blockOffsetY = (offsetY + y) * 8;
                            for (int x = 0; x < h; x++)
                            {
                                // Read Block
                                ReadBlock(inputReader, out inputBuffer, component.ComponentIndex, (offsetX + x) * 8, blockOffsetY, hs, vs);

                                // Level shift
                                ShiftDataLevel(ref inputBuffer, ref inputFBuffer, levelShift);

                                // FDCT
                                FastFloatingPointDCT.TransformFDCT(ref inputFBuffer, ref outputFBuffer, ref tempFBuffer);

                                // ZigZagAndQuantize
                                ZigZagAndQuantizeBlock(component.QuantizationTable, ref outputFBuffer, ref inputBuffer);

                                // Write to bit stream
                                EncodeBlock(ref writer, component, ref inputBuffer);
                            }
                        }
                    }
                }
            }

            // Padding
            writer.ExitBitMode();
        }

        private static void ReadBlock(JpegBlockInputReader inputReader, out JpegBlock8x8 block, int componentIndex, int x, int y, int h, int v)
        {
            block = default;
            ref short blockRef = ref Unsafe.As<JpegBlock8x8, short>(ref block);

            if (h == 1 && v == 1)
            {
                inputReader.ReadBlock(ref blockRef, componentIndex, x, y);
                return;
            }

            ReadBlockWithSubsample(inputReader, ref blockRef, componentIndex, x, y, h, v);
        }

        private static void ReadBlockWithSubsample(JpegBlockInputReader inputReader, ref short blockRef, int componentIndex, int x, int y, int horizontalSubsampling, int verticalSubsampling)
        {
            JpegBlock8x8 temp = default;

            ref short tempRef = ref Unsafe.As<JpegBlock8x8, short>(ref temp);

            int hShift = JpegMathHelper.Log2((uint)horizontalSubsampling);
            int vShift = JpegMathHelper.Log2((uint)verticalSubsampling);
            int hBlockShift = 3 - hShift;
            int vBlockShift = 3 - vShift;

            for (int v = 0; v < verticalSubsampling; v++)
            {
                for (int h = 0; h < horizontalSubsampling; h++)
                {
                    inputReader.ReadBlock(ref tempRef, componentIndex, x + 8 * h, y + 8 * v);

                    CopySubsampleBlock(ref tempRef, ref blockRef, h << hBlockShift, v << vBlockShift, hShift, vShift);
                }
            }

            int totalShift = hShift + vShift;
            if (totalShift > 0)
            {
                int delta = 1 << (totalShift - 1);
                for (int i = 0; i < 64; i++)
                {
                    Unsafe.Add(ref blockRef, i) = (short)((Unsafe.Add(ref blockRef, i) + delta) >> totalShift);
                }
            }
        }

        private static void CopySubsampleBlock(ref short sourceRef, ref short destinationRef, int blockOffsetX, int blockOffsetY, int hShift, int vShift)
        {
            for (int y = 0; y < 8; y++)
            {
                ref short sourceRowRef = ref Unsafe.Add(ref sourceRef, y * 8);
                ref short destinationRowRef = ref Unsafe.Add(ref destinationRef, (blockOffsetY + (y >> vShift)) * 8 + blockOffsetX);
                for (int x = 0; x < 8; x++)
                {
                    Unsafe.Add(ref destinationRowRef, x >> hShift) += Unsafe.Add(ref sourceRowRef, x);
                }
            }
        }

        private static void ShiftDataLevel(ref JpegBlock8x8 source, ref JpegBlock8x8F destination, int levelShift)
        {
            ref short sourceRef = ref Unsafe.As<JpegBlock8x8, short>(ref source);
            ref float destinationRef = ref Unsafe.As<JpegBlock8x8F, float>(ref destination);

            for (int i = 0; i < 64; i++)
            {
                Unsafe.Add(ref destinationRef, i) = Unsafe.Add(ref sourceRef, i) - levelShift;
            }
        }

        private static void ZigZagAndQuantizeBlock(JpegQuantizationTable quantizationTable, ref JpegBlock8x8F input, ref JpegBlock8x8 output)
        {
            Debug.Assert(!quantizationTable.IsEmpty);

            ref ushort elementRef = ref MemoryMarshal.GetReference(quantizationTable.Elements);
            ref float sourceRef = ref Unsafe.As<JpegBlock8x8F, float>(ref input);
            ref short destinationRef = ref Unsafe.As<JpegBlock8x8, short>(ref output);

            for (int i = 0; i < 64; i++)
            {
                float coefficient = Unsafe.Add(ref sourceRef, JpegZigZag.BufferIndexToBlock(i));
                ushort element = Unsafe.Add(ref elementRef, i);
                Unsafe.Add(ref destinationRef, i) = JpegMathHelper.RoundToInt16(coefficient / element);
            }
        }

        private static void EncodeBlock(ref JpegWriter writer, JpegHuffmanEncodingComponent component, ref JpegBlock8x8 block)
        {
            ref short blockRef = ref Unsafe.As<JpegBlock8x8, short>(ref block);

            Debug.Assert(!(component.DcTable is null));
            Debug.Assert(!(component.AcTable is null));

            // DC
            int blockValue = blockRef;
            int t = blockValue - component.DcPredictor;
            component.DcPredictor = blockValue;
            EncodeRunLength(ref writer, component.DcTable!, 0, t);

            // AC
            JpegHuffmanEncodingTable acTable = component.AcTable!;
            int runLength = 0;
            for (int i = 1; i < 64; i++)
            {
                t = Unsafe.Add(ref blockRef, i);

                if (t == 0)
                {
                    runLength++;
                }
                else
                {
                    while (runLength > 15)
                    {
                        EncodeHuffmanSymbol(ref writer, acTable, 0xf0);
                        runLength -= 16;
                    }

                    EncodeRunLength(ref writer, acTable, runLength, t);
                    runLength = 0;
                }
            }

            if (runLength > 0)
            {
                // EOB
                EncodeHuffmanSymbol(ref writer, acTable, 0);
            }
        }

        private static void GatherRunLengthCodeStatistics(JpegHuffmanEncodingTableBuilder tableBuilder, int zeroRunLength, int value)
        {
            int a = value;
            if (a < 0)
            {
                a = -value;
            }

            int bitCount;
            if (a < 0x100)
            {
                bitCount = BitCountTable[a];
            }
            else
            {
                bitCount = 8 + BitCountTable[a >> 8];
            }

            tableBuilder.IncrementCodeCount(zeroRunLength << 4 | bitCount);
        }

        private static void EncodeRunLength(ref JpegWriter writer, JpegHuffmanEncodingTable encodingTable, int zeroRunLength, int value)
        {
            int a = value;
            int b = value;
            if (a < 0)
            {
                a = -value;
                b = value - 1;
            }

            int bitCount;
            if (a < 0x100)
            {
                bitCount = BitCountTable[a];
            }
            else
            {
                bitCount = 8 + BitCountTable[a >> 8];
            }

            EncodeHuffmanSymbol(ref writer, encodingTable, zeroRunLength << 4 | bitCount);
            if (bitCount > 0)
            {
                writer.WriteBits((uint)b & (uint)((1 << bitCount) - 1), bitCount);
            }
        }

        private static void EncodeHuffmanSymbol(ref JpegWriter writer, JpegHuffmanEncodingTable encodingTable, int symbol)
        {
            encodingTable.GetCode(symbol, out ushort code, out int codeLength);
            writer.WriteBits(code, codeLength);
        }

        protected static void WriteEndOfImage(ref JpegWriter writer)
        {
            writer.WriteMarker(JpegMarker.EndOfImage);
        }

        /// <summary>
        /// Gets the counts the number of bits needed to hold an integer.
        /// </summary>
        private static ReadOnlySpan<byte> BitCountTable => new byte[]
        {
            0, 1, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4, 5, 5, 5, 5, 5, 5,
            5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
            6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
            7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
            7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
            7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
            7, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
            8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
            8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
            8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
            8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
            8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
            8, 8, 8,
        };

        public void ResetInputReader()
        {
            _input = null;
        }

        public void ResetTables()
        {
            _quantizationTables = default;
            _huffmanTables = default;
        }

        public void ResetComponents()
        {
            _encodeComponents = default;
        }

        public void ResetOutput()
        {
            _output = null;
        }

        public void Reset()
        {
            ResetInputReader();
            ResetTables();
            ResetComponents();
            ResetOutput();
        }
    }
}
