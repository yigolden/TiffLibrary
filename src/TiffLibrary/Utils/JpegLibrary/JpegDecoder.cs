#nullable enable

using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace JpegLibrary
{
    internal sealed partial class JpegDecoder
    {
        private ReadOnlySequence<byte> _inputBuffer;

        private JpegFrameHeader? _frameHeader;
        private ushort _restartInterval;
        private bool _extended;
        private bool _progressive;

        private JpegBlockOutputWriter? _outputWriter;

        private List<JpegQuantizationTable>? _quantizationTables;
        private List<JpegHuffmanDecodingTable>? _huffmanTables;

        public bool IsExtendedJpeg => _extended;
        public bool IsProgressiveJpeg => _progressive;

        public void SetInput(ReadOnlyMemory<byte> inputBuffer)
            => SetInput(new ReadOnlySequence<byte>(inputBuffer));

        public void SetInput(ReadOnlySequence<byte> inputBuffer)
        {
            _inputBuffer = inputBuffer;

            _frameHeader = null;
            _restartInterval = 0;
        }


        public void Identify() { Identify(false); }

        public void Identify(bool loadQuantizationTables)
        {
            if (_inputBuffer.IsEmpty)
            {
                throw new InvalidOperationException("Input buffer is not specified.");
            }

            JpegReader reader = new JpegReader(_inputBuffer);

            // Reset frame header
            _frameHeader = default;

            bool endOfImageReached = false;
            while (!endOfImageReached && !reader.IsEmpty)
            {
                // Read next marker
                if (!reader.TryReadMarker(out JpegMarker marker))
                {
                    ThrowInvalidDataException(reader.ConsumedBytes, "No marker found.");
                    return;
                }

                switch (marker)
                {
                    case JpegMarker.StartOfImage:
                        break;
                    case JpegMarker.StartOfFrame0:
                        _extended = false;
                        _progressive = false;
                        ProcessFrameHeader(ref reader, false, false);
                        break;
                    case JpegMarker.StartOfFrame1:
                        _extended = true;
                        _progressive = false;
                        ProcessFrameHeader(ref reader, false, false);
                        break;
                    case JpegMarker.StartOfFrame2:
                        _extended = false;
                        _progressive = true;
                        ProcessFrameHeader(ref reader, false, false);
                        break;
                    case JpegMarker.StartOfFrame3:
                    case JpegMarker.StartOfFrame5:
                    case JpegMarker.StartOfFrame6:
                    case JpegMarker.StartOfFrame7:
                    case JpegMarker.StartOfFrame9:
                    case JpegMarker.StartOfFrame10:
                    case JpegMarker.StartOfFrame11:
                    case JpegMarker.StartOfFrame13:
                    case JpegMarker.StartOfFrame14:
                    case JpegMarker.StartOfFrame15:
                        ThrowInvalidDataException(reader.ConsumedBytes, $"This type of JPEG stream is not supported ({marker}).");
                        return;
                    case JpegMarker.StartOfScan:
                        ProcessScanHeader(ref reader, true);
                        break;
                    case JpegMarker.DefineRestartInterval:
                        ProcessDefineRestartInterval(ref reader);
                        break;
                    case JpegMarker.DefineQuantizationTable:
                        if (loadQuantizationTables)
                        {
                            ProcessDefineQuantizationTable(ref reader);
                        }
                        else
                        {
                            ProcessOtherMarker(ref reader);
                        }
                        break;
                    case JpegMarker.DefineRestart0:
                    case JpegMarker.DefineRestart1:
                    case JpegMarker.DefineRestart2:
                    case JpegMarker.DefineRestart3:
                    case JpegMarker.DefineRestart4:
                    case JpegMarker.DefineRestart5:
                    case JpegMarker.DefineRestart6:
                    case JpegMarker.DefineRestart7:
                        break;
                    case JpegMarker.EndOfImage:
                        endOfImageReached = true;
                        break;
                    default:
                        ProcessOtherMarker(ref reader);
                        break;
                }

            }

            if (!endOfImageReached)
            {
                ThrowInvalidDataException(reader.ConsumedBytes, "End of image marker was not found.");
                return;
            }

            if (_frameHeader is null)
            {
                throw new InvalidOperationException("Frame header was not found.");
            }
        }

        public bool TryEstimateQuanlity(out float quality)
        {
            if (_quantizationTables is null)
            {
                throw new InvalidOperationException("Quantization tables must be loaded before this operation.");
            }

            // Luminance
            JpegQuantizationTable quantizationTable = GetQuantizationTable(0);
            if (quantizationTable.IsEmpty)
            {
                quality = 0;
                return false;
            }
            quality = EstimateQuality(quantizationTable, JpegStandardQuantizationTable.GetLuminanceTable(0, 0), out _);

            // Chrominance
            quantizationTable = GetQuantizationTable(1);
            if (!quantizationTable.IsEmpty)
            {
                float quality2 = EstimateQuality(quantizationTable, JpegStandardQuantizationTable.GetChrominanceTable(0, 0), out _);
                quality = Math.Min(quality, quality2);
            }

            quality = JpegMathHelper.Clamp(quality, 0f, 100f);
            return true;
        }

        private static float EstimateQuality(JpegQuantizationTable quantizationTable, JpegQuantizationTable standardTable, out float dVariance)
        {
            Debug.Assert(!quantizationTable.IsEmpty);
            Debug.Assert(!standardTable.IsEmpty);

            bool allOnes = true;
            double dSumPercent = 0;
            double dSumPercentSqr = 0;

            ref ushort elementRef = ref MemoryMarshal.GetReference(quantizationTable.Elements);
            ref ushort standardRef = ref MemoryMarshal.GetReference(standardTable.Elements);

            for (int i = 0; i < 64; i++)
            {
                double dComparePercent;

                ushort element = Unsafe.Add(ref elementRef, i);
                if (element == 0)
                {
                    dComparePercent = 999.99;
                }
                else
                {
                    ushort standard = Unsafe.Add(ref standardRef, i);
                    dComparePercent = 100.0 * element / standard;
                }

                dSumPercent += dComparePercent;
                dSumPercentSqr += dComparePercent * dComparePercent;

                if (element != 1)
                {
                    allOnes = false;
                }
            }

            // Perform some statistical analysis of the quality factor
            // to determine the likelihood of the current quantization
            // table being a scaled version of the "standard" tables.
            // If the variance is high, it is unlikely to be the case.
            dSumPercent /= 64.0;    /* mean scale factor */
            dSumPercentSqr /= 64.0;
            dVariance = (float)(dSumPercentSqr - (dSumPercent * dSumPercent)); /* variance */

            // Generate the equivalent IJQ "quality" factor
            if (allOnes)      /* special case for all-ones table */
                return 100.0f;
            else if (dSumPercent <= 100.0)
                return (float)((200.0 - dSumPercent) / 2.0);
            else
                return (float)(5000.0 / dSumPercent);
        }

        private static void ProcessOtherMarker(ref JpegReader reader)
        {
            if (!reader.TryReadLength(out ushort length))
            {
                ThrowInvalidDataException(reader.ConsumedBytes, "Unexpected end of input data when reading segment length.");
                return;
            }
            if (!reader.TryAdvance(length))
            {
                ThrowInvalidDataException(reader.ConsumedBytes, "Unexpected end of input data reached.");
                return;
            }
        }

        private void ProcessFrameHeader(ref JpegReader reader, bool metadataOnly, bool overrideAllowed)
        {
            // Read length field
            if (!reader.TryReadLength(out ushort length))
            {
                ThrowInvalidDataException(reader.ConsumedBytes, "Unexpected end of input data when reading segment length.");
                return;
            }
            if (!reader.TryReadBytes(length, out ReadOnlySequence<byte> buffer))
            {
                ThrowInvalidDataException(reader.ConsumedBytes, "Unexpected end of input data when reading segment content.");
                return;
            }
            if (!JpegFrameHeader.TryParse(buffer, metadataOnly, out JpegFrameHeader frameHeader, out int bytesConsumed))
            {
                ThrowInvalidDataException(reader.ConsumedBytes - length + bytesConsumed, "Failed to parse frame header.");
                return;
            }
            if (!overrideAllowed && _frameHeader.HasValue)
            {
                ThrowInvalidDataException(reader.ConsumedBytes, "Multiple frame is not supported.");
                return;
            }
            _frameHeader = frameHeader;
        }

        private static JpegScanHeader ProcessScanHeader(ref JpegReader reader, bool metadataOnly)
        {

            if (!reader.TryReadLength(out ushort length))
            {
                ThrowInvalidDataException(reader.ConsumedBytes, "Unexpected end of input data when reading segment length.");
            }
            if (!reader.TryReadBytes(length, out ReadOnlySequence<byte> buffer))
            {
                ThrowInvalidDataException(reader.ConsumedBytes, "Unexpected end of input data when reading segment content.");
            }
            if (!JpegScanHeader.TryParse(buffer, metadataOnly, out JpegScanHeader scanHeader, out int bytesConsumed))
            {
                ThrowInvalidDataException(reader.ConsumedBytes - length + bytesConsumed, "Failed to parse scan header.");
            }
            return scanHeader;
        }

        public void LoadTables(Memory<byte> content) => LoadTables(new ReadOnlySequence<byte>(content));

        public void LoadTables(ReadOnlySequence<byte> content)
        {
            JpegReader reader = new JpegReader(content);

            while (!reader.IsEmpty)
            {
                // Read next marker
                if (!reader.TryReadMarker(out JpegMarker marker))
                {
                    return;
                }

                switch (marker)
                {
                    case JpegMarker.StartOfImage:
                        break;
                    case JpegMarker.DefineRestart0:
                    case JpegMarker.DefineRestart1:
                    case JpegMarker.DefineRestart2:
                    case JpegMarker.DefineRestart3:
                    case JpegMarker.DefineRestart4:
                    case JpegMarker.DefineRestart5:
                    case JpegMarker.DefineRestart6:
                    case JpegMarker.DefineRestart7:
                        break;
                    case JpegMarker.DefineHuffmanTable:
                        ProcessDefineHuffmanTable(ref reader);
                        break;
                    case JpegMarker.DefineQuantizationTable:
                        ProcessDefineQuantizationTable(ref reader);
                        break;
                    case JpegMarker.DefineRestartInterval:
                        ProcessDefineRestartInterval(ref reader);
                        break;
                    case JpegMarker.EndOfImage:
                        return;
                    default:
                        ProcessOtherMarker(ref reader);
                        break;
                }
            }

        }

        private static void ThrowInvalidDataException(string message)
        {
            throw new InvalidDataException(message);
        }
        private static void ThrowInvalidDataException(int offset, string message)
        {
            throw new InvalidDataException($"Failed to decode JPEG data at offset {offset}. {message}");
        }


        private JpegFrameHeader GetFrameHeader() => _frameHeader.HasValue ? _frameHeader.GetValueOrDefault() : throw new InvalidOperationException("Call Identify() before this operation.");

        public int Width => GetFrameHeader().SamplesPerLine;
        public int Height => GetFrameHeader().NumberOfLines;
        public int Precision => GetFrameHeader().SamplePrecision;
        public int NumberOfComponents => GetFrameHeader().NumberOfComponents;

        public byte GetMaximumHorizontalSampling()
        {
            JpegFrameHeader frameHeader = GetFrameHeader();
            if (frameHeader.Components is null)
            {
                throw new InvalidOperationException();
            }
            byte maxHorizontalSampling = 1;
            foreach (JpegFrameComponentSpecificationParameters currentFrameComponent in frameHeader.Components)
            {
                maxHorizontalSampling = Math.Max(maxHorizontalSampling, currentFrameComponent.HorizontalSamplingFactor);
            }
            return maxHorizontalSampling;
        }

        public byte GetMaximumVerticalSampling()
        {
            JpegFrameHeader frameHeader = GetFrameHeader();
            if (frameHeader.Components is null)
            {
                throw new InvalidOperationException();
            }
            byte maxVerticalSampling = 1;
            foreach (JpegFrameComponentSpecificationParameters currentFrameComponent in frameHeader.Components)
            {
                maxVerticalSampling = Math.Max(maxVerticalSampling, currentFrameComponent.VerticalSamplingFactor);
            }
            return maxVerticalSampling;
        }

        public byte GetHorizontalSampling(int componentIndex)
        {
            JpegFrameHeader frameHeader = GetFrameHeader();
            JpegFrameComponentSpecificationParameters[]? components = frameHeader.Components;
            if (components is null)
            {
                throw new InvalidOperationException();
            }
            if ((uint)componentIndex >= (uint)components.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(componentIndex));
            }
            return components[componentIndex].HorizontalSamplingFactor;
        }

        public byte GetVerticalSampling(int componentIndex)
        {
            JpegFrameHeader frameHeader = GetFrameHeader();
            JpegFrameComponentSpecificationParameters[]? components = frameHeader.Components;
            if (components is null)
            {
                throw new InvalidOperationException();
            }
            if ((uint)componentIndex >= (uint)components.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(componentIndex));
            }
            return components[componentIndex].VerticalSamplingFactor;
        }

        public void SetOutputWriter(JpegBlockOutputWriter outputWriter)
        {
            _outputWriter = outputWriter ?? throw new ArgumentNullException(nameof(outputWriter));
        }

        public void Decode()
        {
            if (_inputBuffer.IsEmpty)
            {
                throw new InvalidOperationException("Input buffer is not specified.");
            }
            if (_outputWriter is null)
            {
                throw new InvalidOperationException("The output buffer is not specified.");
            }

            JpegReader reader = new JpegReader(_inputBuffer);

            // SOI marker
            if (!reader.TryReadStartOfImageMarker())
            {
                ThrowInvalidDataException(reader.ConsumedBytes, "Marker StartOfImage not found.");
                return;
            }

            bool scanRead = false;
            bool endOfImageReached = false;
            while (!endOfImageReached && !reader.IsEmpty)
            {
                // Read next marker
                if (!reader.TryReadMarker(out JpegMarker marker))
                {
                    ThrowInvalidDataException(reader.ConsumedBytes, "No marker found.");
                    return;
                }

                switch (marker)
                {
                    case JpegMarker.StartOfFrame0:
                        _extended = false;
                        _progressive = false;
                        ProcessFrameHeader(ref reader, false, true);
                        break;
                    case JpegMarker.StartOfFrame1:
                        _extended = true;
                        _progressive = false;
                        ProcessFrameHeader(ref reader, false, true);
                        break;
                    case JpegMarker.StartOfFrame2:
                        _extended = false;
                        _progressive = true;
                        ProcessFrameHeader(ref reader, false, true);
                        throw new InvalidDataException("Progressive JPEG is not supported currently.");
                    case JpegMarker.StartOfFrame3:
                    case JpegMarker.StartOfFrame5:
                    case JpegMarker.StartOfFrame6:
                    case JpegMarker.StartOfFrame7:
                    case JpegMarker.StartOfFrame9:
                    case JpegMarker.StartOfFrame10:
                    case JpegMarker.StartOfFrame11:
                    case JpegMarker.StartOfFrame13:
                    case JpegMarker.StartOfFrame14:
                    case JpegMarker.StartOfFrame15:
                        ThrowInvalidDataException(reader.ConsumedBytes, $"This type of JPEG stream is not supported ({marker}).");
                        return;
                    case JpegMarker.DefineHuffmanTable:
                        ProcessDefineHuffmanTable(ref reader);
                        break;
                    case JpegMarker.DefineQuantizationTable:
                        ProcessDefineQuantizationTable(ref reader);
                        break;
                    case JpegMarker.DefineRestartInterval:
                        ProcessDefineRestartInterval(ref reader);
                        break;
                    case JpegMarker.StartOfScan:
                        JpegScanHeader scanHeader = ProcessScanHeader(ref reader, false);
                        // Decode
                        DecodeScanBaseline(ref reader, scanHeader);
                        scanRead = true;
                        break;
                    case JpegMarker.DefineRestart0:
                    case JpegMarker.DefineRestart1:
                    case JpegMarker.DefineRestart2:
                    case JpegMarker.DefineRestart3:
                    case JpegMarker.DefineRestart4:
                    case JpegMarker.DefineRestart5:
                    case JpegMarker.DefineRestart6:
                    case JpegMarker.DefineRestart7:
                        break;
                    case JpegMarker.EndOfImage:
                        endOfImageReached = true;
                        break;
                    default:
                        ProcessOtherMarker(ref reader);
                        break;
                }

            }

            if (!scanRead)
            {
                ThrowInvalidDataException(reader.ConsumedBytes, "No image data is read.");
                return;
            }

        }

        private void ProcessDefineRestartInterval(ref JpegReader reader)
        {
            if (!reader.TryReadLength(out ushort length))
            {
                ThrowInvalidDataException(reader.ConsumedBytes, "Unexpected end of input data when reading segment length.");
                return;
            }
            if (!reader.TryReadBytes(length, out ReadOnlySequence<byte> buffer) || buffer.Length < 2)
            {
                ThrowInvalidDataException(reader.ConsumedBytes, "Unexpected end of input data when reading segment content.");
                return;
            }
            Span<byte> local = stackalloc byte[2];
            buffer.Slice(0, 2).CopyTo(local);
            _restartInterval = BinaryPrimitives.ReadUInt16BigEndian(local);
        }

        public void SetRestartInterval(int restartInterval)
        {
            if ((uint)restartInterval > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(restartInterval));
            }

            _restartInterval = (ushort)restartInterval;
        }

        private void ProcessDefineHuffmanTable(ref JpegReader reader)
        {
            if (!reader.TryReadLength(out ushort length))
            {
                ThrowInvalidDataException(reader.ConsumedBytes, "Unexpected end of input data when reading segment length.");
                return;
            }
            if (!reader.TryReadBytes(length, out ReadOnlySequence<byte> buffer))
            {
                ThrowInvalidDataException(reader.ConsumedBytes, "Unexpected end of input data when reading segment content.");
                return;
            }
            ProcessDefineHuffmanTable(buffer, reader.ConsumedBytes - length);
        }

        private void ProcessDefineHuffmanTable(ReadOnlySequence<byte> segment, int currentOffset)
        {
            while (!segment.IsEmpty)
            {
                if (!JpegHuffmanDecodingTable.TryParse(segment, out JpegHuffmanDecodingTable? huffmanTable, out int bytesConsumed))
                {
                    ThrowInvalidDataException(currentOffset, "Failed to parse Huffman table.");
                    return;
                }
                segment = segment.Slice(bytesConsumed);
                currentOffset += bytesConsumed;
                SetHuffmanTable(huffmanTable);
            }
        }

        private void ProcessDefineQuantizationTable(ref JpegReader reader)
        {
            if (!reader.TryReadLength(out ushort length))
            {
                ThrowInvalidDataException(reader.ConsumedBytes, "Unexpected end of input data when reading segment length.");
                return;
            }
            if (!reader.TryReadBytes(length, out ReadOnlySequence<byte> buffer))
            {
                ThrowInvalidDataException(reader.ConsumedBytes, "Unexpected end of input data when reading segment content.");
                return;
            }
            ProcessDefineQuantizationTable(buffer, reader.ConsumedBytes - length);
        }

        private void ProcessDefineQuantizationTable(ReadOnlySequence<byte> segment, int currentOffset)
        {
            while (!segment.IsEmpty)
            {
                if (!JpegQuantizationTable.TryParse(segment, out JpegQuantizationTable quantizationTable, out int bytesConsumed))
                {
                    ThrowInvalidDataException(currentOffset, "Failed to parse quantization table.");
                    return;
                }
                segment = segment.Slice(bytesConsumed);
                currentOffset += bytesConsumed;
                SetQuantizationTable(quantizationTable);
            }
        }

        public void ClearHuffmanTable()
        {
            List<JpegHuffmanDecodingTable>? list = _huffmanTables;
            if (list is null)
            {
                list = _huffmanTables = new List<JpegHuffmanDecodingTable>(4);
            }
            list.Clear();
        }

        public void ClearQuantizationTable()
        {
            List<JpegQuantizationTable>? list = _quantizationTables;
            if (list is null)
            {
                list = _quantizationTables = new List<JpegQuantizationTable>(2);
            }
            list.Clear();
        }

        internal void SetHuffmanTable(JpegHuffmanDecodingTable table)
        {
            List<JpegHuffmanDecodingTable>? list = _huffmanTables;
            if (list is null)
            {
                list = _huffmanTables = new List<JpegHuffmanDecodingTable>(4);
            }
            for (int i = 0; i < list.Count; i++)
            {
                JpegHuffmanDecodingTable item = list[i];
                if (item.TableClass == table.TableClass && item.Identifier == table.Identifier)
                {
                    list[i] = table;
                    return;
                }
            }
            list.Add(table);
        }

        internal void SetQuantizationTable(JpegQuantizationTable table)
        {
            List<JpegQuantizationTable>? list = _quantizationTables;
            if (list is null)
            {
                list = _quantizationTables = new List<JpegQuantizationTable>(2);
            }
            for (int i = 0; i < list.Count; i++)
            {
                JpegQuantizationTable item = list[i];
                if (item.Identifier == table.Identifier)
                {
                    list[i] = table;
                    return;
                }
            }
            list.Add(table);
        }

        private JpegHuffmanDecodingTable? GetHuffmanTable(bool isDcTable, byte identifier)
        {
            List<JpegHuffmanDecodingTable>? huffmanTables = _huffmanTables;
            if (huffmanTables is null)
            {
                return null;
            }
            int tableClass = isDcTable ? 0 : 1;
            foreach (JpegHuffmanDecodingTable item in huffmanTables)
            {
                if (item.TableClass == tableClass && item.Identifier == identifier)
                {
                    return item;
                }
            }
            return null;
        }

        private JpegQuantizationTable GetQuantizationTable(byte identifier)
        {
            List<JpegQuantizationTable>? quantizationTables = _quantizationTables;
            if (quantizationTables is null)
            {
                return default;
            }
            foreach (JpegQuantizationTable item in quantizationTables)
            {
                if (item.Identifier == identifier)
                {
                    return item;
                }
            }
            return default;
        }

        internal void DecodeScanBaseline(ref JpegReader reader, JpegScanHeader scanHeader)
        {
            JpegFrameHeader frameHeader = _frameHeader.GetValueOrDefault();

            if (frameHeader.Components is null)
            {
                throw new InvalidOperationException();
            }

            // Compute maximum sampling factor
            byte maxHorizontalSampling = 1;
            byte maxVerticalSampling = 1;
            foreach (JpegFrameComponentSpecificationParameters currentFrameComponent in frameHeader.Components)
            {
                maxHorizontalSampling = Math.Max(maxHorizontalSampling, currentFrameComponent.HorizontalSamplingFactor);
                maxVerticalSampling = Math.Max(maxVerticalSampling, currentFrameComponent.VerticalSamplingFactor);
            }

            if (scanHeader.Components is null)
            {
                throw new InvalidOperationException();
            }
            if (_outputWriter is null)
            {
                throw new InvalidOperationException();
            }

            // Resolve each component
            JpegDecodeComponent[] components = new JpegDecodeComponent[scanHeader.NumberOfComponents];
            for (int i = 0; i < scanHeader.NumberOfComponents; i++)
            {
                JpegScanComponentSpecificationParameters scanComponenet = scanHeader.Components[i];
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
                ref JpegDecodeComponent componentRef = ref components[i];
                componentRef = new JpegDecodeComponent
                {
                    ComponentIndex = componentIndex,
                    HorizontalSamplingFactor = frameComponent.GetValueOrDefault().HorizontalSamplingFactor,
                    VerticalSamplingFactor = frameComponent.GetValueOrDefault().VerticalSamplingFactor,
                    DcTable = GetHuffmanTable(true, scanComponenet.DcEntropyCodingTableSelector),
                    AcTable = GetHuffmanTable(false, scanComponenet.AcEntropyCodingTableSelector),
                    QuantizationTable = GetQuantizationTable(frameComponent.GetValueOrDefault().QuantizationTableSelector),
                };
                componentRef.HorizontalSubsamplingFactor = maxHorizontalSampling / componentRef.HorizontalSamplingFactor;
                componentRef.VerticalSubsamplingFactor = maxVerticalSampling / componentRef.VerticalSamplingFactor;
            }

            // Prepare
            int mcusPerLine = (frameHeader.SamplesPerLine + 8 * maxHorizontalSampling - 1) / (8 * maxHorizontalSampling);
            int mcusPerColumn = (frameHeader.NumberOfLines + 8 * maxVerticalSampling - 1) / (8 * maxVerticalSampling);
            JpegBitReader bitReader = new JpegBitReader(reader.RemainingBytes);
            int mcusBeforeRestart = _restartInterval;

            int levelShift = 1 << (frameHeader.SamplePrecision - 1);

            // DCT Block
            JpegBlock8x8F blockFBuffer = default;
            JpegBlock8x8F tempFBuffer = default;
            JpegBlock8x8F outputFBuffer = default;

            JpegBlock8x8 outputBuffer;

            for (int rowMcu = 0; rowMcu < mcusPerColumn; rowMcu++)
            {
                int offsetY = rowMcu * maxVerticalSampling;
                for (int colMcu = 0; colMcu < mcusPerLine; colMcu++)
                {
                    int offsetX = colMcu * maxHorizontalSampling;

                    // Scan an interleaved mcu... process components in order
                    foreach (JpegDecodeComponent component in components)
                    {
                        int index = component.ComponentIndex;
                        int h = component.HorizontalSamplingFactor;
                        int v = component.VerticalSamplingFactor;
                        int hs = component.HorizontalSubsamplingFactor;
                        int vs = component.VerticalSubsamplingFactor;

                        for (int y = 0; y < v; y++)
                        {
                            int blockOffsetY = (offsetY + y) * 8;
                            for (int x = 0; x < h; x++)
                            {
                                // Read MCU
                                outputBuffer = default;
                                ReadBlockBaseline(ref bitReader, component, ref outputBuffer);

                                // Dequantization
                                DequantizeBlockAndUnZigZag(component.QuantizationTable, ref outputBuffer, ref blockFBuffer);

                                // IDCT
                                FastFloatingPointDCT.TransformIDCT(ref blockFBuffer, ref outputFBuffer, ref tempFBuffer);

                                // Level shift
                                ShiftDataLevel(ref outputFBuffer, ref outputBuffer, levelShift);

                                // CopyToOutput
                                WriteBlock(in outputBuffer, index, (offsetX + x) * 8, blockOffsetY, hs, vs);
                            }
                        }
                    }

                    if (_restartInterval > 0 && (--mcusBeforeRestart) == 0)
                    {
                        bitReader.AdvanceAlignByte();

                        JpegMarker marker = bitReader.TryReadMarker();
                        if (marker == JpegMarker.EndOfImage)
                        {
                            int bytesConsumedEoi = reader.RemainingByteCount - bitReader.RemainingBits / 8;
                            reader.TryAdvance(bytesConsumedEoi - 2);
                            return;
                        }
                        if (!marker.IsRestartMarker())
                        {
                            throw new InvalidOperationException("Expect restart marker.");
                        }

                        mcusBeforeRestart = _restartInterval;

                        foreach (JpegDecodeComponent component in components)
                        {
                            component.DcPredictor = 0;
                        }

                    }
                }
            }

            bitReader.AdvanceAlignByte();
            int bytesConsumed = reader.RemainingByteCount - bitReader.RemainingBits / 8;
            if (bitReader.TryPeekMarker() != 0)
            {
                if (!bitReader.TryPeekMarker().IsRestartMarker())
                {
                    bytesConsumed -= 2;
                }
            }
            reader.TryAdvance(bytesConsumed);
        }

        private void WriteBlock(in JpegBlock8x8 block, int componentIndex, int x, int y, int horizontalSamplingFactor, int verticalSamplingFactor)
        {
            JpegBlockOutputWriter? outputWriter = _outputWriter;
            Debug.Assert(!(outputWriter is null));

            if (horizontalSamplingFactor == 1 && verticalSamplingFactor == 1)
            {
                outputWriter!.WriteBlock(block, componentIndex, x, y);
            }
            else
            {
                JpegBlock8x8 tempBlock = default;

                int hShift = CalculateShiftFactor(horizontalSamplingFactor);
                int vShift = CalculateShiftFactor(verticalSamplingFactor);

                ref short blockRef = ref Unsafe.As<JpegBlock8x8, short>(ref Unsafe.AsRef(block));
                ref short tempRef = ref Unsafe.As<JpegBlock8x8, short>(ref Unsafe.AsRef(tempBlock));

                for (int v = 0; v < verticalSamplingFactor; v++)
                {
                    int yOffset = y + 8 * v;
                    for (int h = 0; h < horizontalSamplingFactor; h++)
                    {
                        // Fill tempBlock
                        for (int i = 0; i < 8; i++)
                        {
                            ref short tempRowRef = ref Unsafe.Add(ref tempRef, 8 * i);
                            ref short blockRowRef = ref Unsafe.Add(ref blockRef, 8 * i >> vShift);
                            for (int j = 0; j < 8; j++)
                            {
                                Unsafe.Add(ref tempRowRef, j) = Unsafe.Add(ref blockRowRef, j >> hShift);
                            }
                        }

                        // Write tempBlock to output
                        outputWriter!.WriteBlock(tempBlock, componentIndex, x + 8 * h, yOffset);
                    }
                }
            }
        }

        private static int CalculateShiftFactor(int value)
        {
            int shift = 0;
            while ((value = value / 2) != 0) shift++;
            return shift;
        }

        private static void ReadBlockBaseline(ref JpegBitReader reader, JpegDecodeComponent component, ref JpegBlock8x8 destinationBlock)
        {
            ref short destinationRef = ref Unsafe.As<JpegBlock8x8, short>(ref destinationBlock);

            Debug.Assert(!(component.DcTable is null));
            Debug.Assert(!(component.AcTable is null));

            // DC
            int t = DecodeHuffmanCode(ref reader, component.DcTable!);
            if (t != 0)
            {
                t = Receive(ref reader, t);
            }

            t += component.DcPredictor;
            component.DcPredictor = t;
            destinationRef = (short)t;

            // AC
            JpegHuffmanDecodingTable acTable = component.AcTable!;
            for (int i = 1; i < 64;)
            {
                int s = DecodeHuffmanCode(ref reader, acTable);

                int r = s >> 4;
                s &= 15;

                if (s != 0)
                {
                    i += r;
                    s = Receive(ref reader, s);
                    Unsafe.Add(ref destinationRef, Math.Min(i++, 63)) = (short)s;
                }
                else
                {
                    if (r == 0)
                    {
                        break;
                    }

                    i += 16;
                }
            }
        }

        private static byte DecodeHuffmanCode(ref JpegBitReader reader, JpegHuffmanDecodingTable table)
        {
            int bits = reader.PeekBits(16, out byte bitsRead);
            JpegHuffmanDecodingTable.Entry entry = table.Lookup(bits);
            if (entry.CodeSize == 0)
            {
                ThrowInvalidDataException("Invalid Huffman code encountered.");
            }
            bitsRead = Math.Min(entry.CodeSize, bitsRead);
            _ = reader.TryAdvanceBits(bitsRead, out _);
            return entry.CodeValue;
        }

        private static JpegHuffmanDecodingTable.Entry DecodeHuffmanCode(ref JpegBitReader reader, JpegHuffmanDecodingTable table, out int code, out byte bitsRead)
        {
            int bits = reader.PeekBits(16, out bitsRead);
            JpegHuffmanDecodingTable.Entry entry = table.Lookup(bits);
            if (entry.CodeSize == 0)
            {
                ThrowInvalidDataException("Invalid Huffman code encountered.");
            }
            bitsRead = Math.Min(entry.CodeSize, bitsRead);
            _ = reader.TryAdvanceBits(bitsRead, out _);
            code = bits >> (16 - bitsRead);
            return entry;
        }

        private static int Receive(ref JpegBitReader reader, int length)
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

        private static void DequantizeBlockAndUnZigZag(JpegQuantizationTable quantizationTable, ref JpegBlock8x8 input, ref JpegBlock8x8F output)
        {
            Debug.Assert(!quantizationTable.IsEmpty);

            ref ushort elementRef = ref MemoryMarshal.GetReference(quantizationTable.Elements);
            ref short sourceRef = ref Unsafe.As<JpegBlock8x8, short>(ref input);
            ref float destinationRef = ref Unsafe.As<JpegBlock8x8F, float>(ref output);
            for (int i = 0; i < 64; i++)
            {
                ushort element = Unsafe.Add(ref elementRef, i);
                Unsafe.Add(ref destinationRef, JpegZigZag.BufferIndexToBlock(i)) = (element * Unsafe.Add(ref sourceRef, i));
            }
        }

        private static void ShiftDataLevel(ref JpegBlock8x8F source, ref JpegBlock8x8 destination, int levelShift)
        {
            ref float sourceRef = ref Unsafe.As<JpegBlock8x8F, float>(ref source);
            ref short destinationRef = ref Unsafe.As<JpegBlock8x8, short>(ref destination);

            for (int i = 0; i < 64; i++)
            {
                Unsafe.Add(ref destinationRef, i) = (short)(JpegMathHelper.RoundToInt32(Unsafe.Add(ref sourceRef, i)) + levelShift);
            }
        }

        public void Reset()
        {
            ResetInput();
            ResetHeader();
            ResetTables();
            ResetOutputWriter();
        }

        public void ResetInput()
        {
            _inputBuffer = default;
        }

        public void ResetHeader()
        {
            _frameHeader = null;
            _restartInterval = 0;
        }

        public void ResetTables()
        {
            if (!(_huffmanTables is null))
            {
                _huffmanTables.Clear();
            }
            if (!(_quantizationTables is null))
            {
                _quantizationTables.Clear();
            }
        }

        [Obsolete("Use ResetOutputWriter instead.")]
        public void ResetOutput()
        {
            _outputWriter = null;
        }

        public void ResetOutputWriter()
        {
            _outputWriter = null;
        }
    }
}
