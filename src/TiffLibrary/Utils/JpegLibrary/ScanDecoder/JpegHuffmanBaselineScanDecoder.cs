#nullable enable

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace JpegLibrary.ScanDecoder
{
    internal sealed class JpegHuffmanBaselineScanDecoder : JpegHuffmanScanDecoder
    {
        private readonly JpegDecoder _decoder;
        private readonly JpegFrameHeader _frameHeader;

        private readonly ushort _restartInterval;

        public JpegHuffmanBaselineScanDecoder(JpegDecoder decoder, JpegFrameHeader frameHeader) : base(decoder)
        {
            _decoder = decoder;
            _frameHeader = frameHeader;

            _restartInterval = decoder.GetRestartInterval();
        }

        public override void ProcessScan(ref JpegReader reader, JpegScanHeader scanHeader)
        {
            JpegFrameHeader frameHeader = _frameHeader;

            if (frameHeader.Components is null)
            {
                throw new InvalidOperationException();
            }
            if (scanHeader.Components is null)
            {
                throw new InvalidOperationException();
            }
            if (_decoder.GetOutputWriter() is null)
            {
                throw new InvalidOperationException();
            }

            // Compute maximum sampling factor
            int maxHorizontalSampling = 1;
            int maxVerticalSampling = 1;
            foreach (JpegFrameComponentSpecificationParameters currentFrameComponent in frameHeader.Components)
            {
                maxHorizontalSampling = Math.Max(maxHorizontalSampling, currentFrameComponent.HorizontalSamplingFactor);
                maxVerticalSampling = Math.Max(maxVerticalSampling, currentFrameComponent.VerticalSamplingFactor);
            }

            // Resolve each component
            JpegDecodeComponent[] components = InitDecodeComponents(frameHeader, scanHeader);

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

                    // Handle restart
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

        private void WriteBlock(in JpegBlock8x8 block, int componentIndex, int x, int y, int horizontalSamplingFactor, int verticalSamplingFactor)
        {
            JpegBlockOutputWriter? outputWriter = _decoder.GetOutputWriter();
            Debug.Assert(!(outputWriter is null));

            if (horizontalSamplingFactor == 1 && verticalSamplingFactor == 1)
            {
                outputWriter!.WriteBlock(block, componentIndex, x, y);
            }
            else
            {
                JpegBlock8x8 tempBlock = default;

                int hShift = JpegMathHelper.CalculateShiftFactor(horizontalSamplingFactor);
                int vShift = JpegMathHelper.CalculateShiftFactor(verticalSamplingFactor);

                ref short blockRef = ref Unsafe.As<JpegBlock8x8, short>(ref Unsafe.AsRef(block));
                ref short tempRef = ref Unsafe.As<JpegBlock8x8, short>(ref Unsafe.AsRef(tempBlock));

                for (int v = 0; v < verticalSamplingFactor; v++)
                {
                    for (int h = 0; h < horizontalSamplingFactor; h++)
                    {
                        int vBlock = 8 * v;
                        int hBlock = 8 * h;
                        // Fill tempBlock
                        for (int i = 0; i < 8; i++)
                        {
                            ref short tempRowRef = ref Unsafe.Add(ref tempRef, 8 * i);
                            ref short blockRowRef = ref Unsafe.Add(ref blockRef, ((vBlock + i) >> vShift) * 8);
                            for (int j = 0; j < 8; j++)
                            {
                                Unsafe.Add(ref tempRowRef, j) = Unsafe.Add(ref blockRowRef, (hBlock + j) >> hShift);
                            }
                        }

                        // Write tempBlock to output
                        outputWriter!.WriteBlock(tempBlock, componentIndex, x + 8 * h, y + 8 * v);
                    }
                }
            }
        }

        public override void Dispose()
        {
            // Do nothing
        }
    }
}
