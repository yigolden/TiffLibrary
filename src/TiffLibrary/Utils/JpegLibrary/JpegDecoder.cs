#nullable enable

using JpegLibrary.ScanDecoder;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
        private byte? _maxHorizontalSamplingFactor;
        private byte? _maxVerticalSamplingFactor;

        private JpegBlockOutputWriter? _outputWriter;

        private List<JpegQuantizationTable>? _quantizationTables;
        private List<JpegHuffmanDecodingTable>? _huffmanTables;

        public MemoryPool<byte>? MemoryPool { get; set; }

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
                        ProcessFrameHeader(ref reader, false, false);
                        break;
                    case JpegMarker.StartOfFrame1:
                        ProcessFrameHeader(ref reader, false, false);
                        break;
                    case JpegMarker.StartOfFrame2:
                        ProcessFrameHeader(ref reader, false, false);
                        break;
                    case JpegMarker.StartOfFrame3:
                        ProcessFrameHeader(ref reader, false, false);
                        break;
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

        internal void SetFrameHeader(JpegFrameHeader frameHeader)
        {
            _frameHeader = frameHeader;
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

        [DoesNotReturn]
        private static void ThrowInvalidDataException(string message)
        {
            throw new InvalidDataException(message);
        }

        [DoesNotReturn]
        private static void ThrowInvalidDataException(int offset, string message)
        {
            throw new InvalidDataException($"Failed to decode JPEG data at offset {offset}. {message}");
        }


        private JpegFrameHeader GetFrameHeader() => _frameHeader.HasValue ? _frameHeader.GetValueOrDefault() : throw new InvalidOperationException("Call Identify() before this operation.");

        public int Width => GetFrameHeader().SamplesPerLine;
        public int Height => GetFrameHeader().NumberOfLines;
        public int Precision => GetFrameHeader().SamplePrecision;
        public int NumberOfComponents => GetFrameHeader().NumberOfComponents;

        public int GetMaximumHorizontalSampling()
        {
            if (_maxHorizontalSamplingFactor.HasValue)
            {
                return _maxHorizontalSamplingFactor.GetValueOrDefault();
            }
            JpegFrameHeader frameHeader = GetFrameHeader();
            if (frameHeader.Components is null)
            {
                throw new InvalidOperationException();
            }
            int maxHorizontalSampling = 1;
            foreach (JpegFrameComponentSpecificationParameters currentFrameComponent in frameHeader.Components)
            {
                maxHorizontalSampling = Math.Max(maxHorizontalSampling, currentFrameComponent.HorizontalSamplingFactor);
            }
            _maxHorizontalSamplingFactor = (byte)maxHorizontalSampling;
            return maxHorizontalSampling;
        }

        public int GetMaximumVerticalSampling()
        {
            if (_maxVerticalSamplingFactor.HasValue)
            {
                return _maxVerticalSamplingFactor.GetValueOrDefault();
            }
            JpegFrameHeader frameHeader = GetFrameHeader();
            if (frameHeader.Components is null)
            {
                throw new InvalidOperationException();
            }
            int maxVerticalSampling = 1;
            foreach (JpegFrameComponentSpecificationParameters currentFrameComponent in frameHeader.Components)
            {
                maxVerticalSampling = Math.Max(maxVerticalSampling, currentFrameComponent.VerticalSamplingFactor);
            }
            _maxVerticalSamplingFactor = (byte)maxVerticalSampling;
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

            JpegScanDecoder? scanDecoder = null;
            try
            {

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
                        case JpegMarker.StartOfFrame1:
                        case JpegMarker.StartOfFrame2:
                        case JpegMarker.StartOfFrame3:
                            ProcessFrameHeader(ref reader, false, true);
                            scanDecoder = JpegScanDecoder.Create(marker, this, _frameHeader.GetValueOrDefault());
                            break;
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
                            if (scanDecoder is null)
                            {
                                ThrowInvalidDataException(reader.ConsumedBytes, "Scan header appears before frame header.");
                            }
                            JpegScanHeader scanHeader = ProcessScanHeader(ref reader, false);
                            scanDecoder!.ProcessScan(ref reader, scanHeader);
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
            finally
            {
                scanDecoder?.Dispose();
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

        public ushort GetRestartInterval() => _restartInterval;

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

        internal JpegHuffmanDecodingTable? GetHuffmanTable(bool isDcTable, byte identifier)
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

        internal JpegQuantizationTable GetQuantizationTable(byte identifier)
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
            _maxHorizontalSamplingFactor = null;
            _maxVerticalSamplingFactor = null;
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

        internal JpegBlockOutputWriter? GetOutputWriter()
        {
            return _outputWriter;
        }

        public void ResetOutputWriter()
        {
            _outputWriter = null;
        }
    }
}
