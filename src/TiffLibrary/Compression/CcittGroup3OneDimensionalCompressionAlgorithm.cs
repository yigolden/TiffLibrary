using System;
using System.Buffers;
using System.IO;

namespace TiffLibrary.Compression
{
    /// <summary>
    /// Decompression support for CCITT T.4 bi-level encoding (1-dimensional).
    /// </summary>
    public class CcittGroup3OneDimensionalCompressionAlgorithm : ITiffCompressionAlgorithm, ITiffDecompressionAlgorithm
    {
        private bool _higherOrderBitsFirst;

        /// <summary>
        /// Initialize the instance with specified fill order.
        /// </summary>
        /// <param name="higherOrderBitsFirst">If this flag is set, higher order bits are considered to precede lower order bits when reading bits from a byte.</param>
        public CcittGroup3OneDimensionalCompressionAlgorithm(bool higherOrderBitsFirst)
        {
            _higherOrderBitsFirst = higherOrderBitsFirst;
        }

        internal static CcittGroup3OneDimensionalCompressionAlgorithm HigherOrderBitsFirstInstance { get; } = new CcittGroup3OneDimensionalCompressionAlgorithm(true);

        internal static CcittGroup3OneDimensionalCompressionAlgorithm LowerOrderBitsFirstInstance { get; } = new CcittGroup3OneDimensionalCompressionAlgorithm(false);

        /// <summary>
        /// Get static cached instance of <see cref="CcittGroup3OneDimensionalCompressionAlgorithm"/>.
        /// </summary>
        /// <param name="fillOrder">The FillOrder tag specified in the image file directory.</param>
        /// <returns>A cached instance of <see cref="CcittGroup3OneDimensionalCompressionAlgorithm"/>.</returns>
        public static CcittGroup3OneDimensionalCompressionAlgorithm GetSharedInstance(TiffFillOrder fillOrder)
        {
            if (fillOrder != TiffFillOrder.LowerOrderBitsFirst)
            {
                return HigherOrderBitsFirstInstance;
            }
            return LowerOrderBitsFirstInstance;
        }

        /// <inheritdoc />
        public void Compress(TiffCompressionContext context, ReadOnlyMemory<byte> input, IBufferWriter<byte> outputWriter)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.PhotometricInterpretation != TiffPhotometricInterpretation.WhiteIsZero && context.PhotometricInterpretation != TiffPhotometricInterpretation.BlackIsZero)
            {
                throw new NotSupportedException("Modified Huffman compression does not support this photometric interpretation.");
            }

            if (context.BitsPerSample.Count != 1 || context.BitsPerSample[0] != 8)
            {
                throw new NotSupportedException("Unsupported bits per sample.");
            }

            context.BitsPerSample = TiffValueCollection.Single<ushort>(1);

            ReadOnlySpan<byte> inputSpan = input.Span;

            int width = context.ImageSize.Width;
            int height = context.ImageSize.Height;
            var bitWriter = new BitWriter2(outputWriter, 4096);

            // Process every scanline
            for (int row = 0; row < height; row++)
            {
                ReadOnlySpan<byte> rowSpan = inputSpan.Slice(0, width);
                inputSpan = inputSpan.Slice(width);

                CcittEncodingTable currentTable = CcittEncodingTable.WhiteInstance;
                CcittEncodingTable otherTable = CcittEncodingTable.BlackInstance;

                // EOL code
                bitWriter.Write(0b000000000001, 12);

                // ModifiedHuffman compression assumes WhiteIsZero photometric interpretation is used.
                // Since the first run is white run, we look for black pixel in the first iteration.
                byte nextRunPixel = 255;

                while (!rowSpan.IsEmpty)
                {
                    // Get the length of the current run
                    int runLength = rowSpan.IndexOf(nextRunPixel);
                    if (runLength < 0)
                    {
                        runLength = rowSpan.Length;
                    }
                    currentTable.EncodeRun(ref bitWriter, runLength);
                    rowSpan = rowSpan.Slice(runLength);

                    // Switch to the other color
                    CcittHelper.SwapTable(ref currentTable, ref otherTable);
                    nextRunPixel = (byte)~nextRunPixel;
                }
            }

            // RTC
            bitWriter.Write(0b000000000001, 12);
            bitWriter.Write(0b000000000001, 12);
            bitWriter.Write(0b000000000001, 12);
            bitWriter.Write(0b000000000001, 12);
            bitWriter.Write(0b000000000001, 12);
            bitWriter.Write(0b000000000001, 12);

            bitWriter.AdvanceAlignByte();
            bitWriter.Flush();
        }

        /// <inheritdoc />
        public void Decompress(TiffDecompressionContext context, ReadOnlyMemory<byte> input, Memory<byte> output)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.PhotometricInterpretation != TiffPhotometricInterpretation.WhiteIsZero && context.PhotometricInterpretation != TiffPhotometricInterpretation.BlackIsZero)
            {
                throw new NotSupportedException("T4 compression does not support this photometric interpretation.");
            }

            if (context.BitsPerSample.Count != 1 || context.BitsPerSample[0] != 1)
            {
                throw new NotSupportedException("Unsupported bits per sample.");
            }

            ReadOnlySpan<byte> inputSpan = input.Span;
            Span<byte> scanlinesBufferSpan = output.Span;

            bool whiteIsZero = context.PhotometricInterpretation == TiffPhotometricInterpretation.WhiteIsZero;
            int width = context.ImageSize.Width;
            int height = context.SkippedScanlines + context.RequestedScanlines;
            var bitReader = new BitReader(inputSpan, higherOrderBitsFirst: _higherOrderBitsFirst);

            // Skip the first EOL code
            if (bitReader.Peek(12) == 0b000000000001) // EOL code is 000000000001 (12bits).
            {
                bitReader.Advance(12);
            }
            else if (bitReader.Peek(16) == 1) // Or EOL code is zero filled.
            {
                bitReader.Advance(16);
            }

            // Process every scanline
            for (int i = 0; i < height; i++)
            {
                if (scanlinesBufferSpan.Length < width)
                {
                    throw new InvalidDataException();
                }
                Span<byte> scanline = scanlinesBufferSpan.Slice(0, width);
                scanlinesBufferSpan = scanlinesBufferSpan.Slice(width);

                // Process every code word in this scanline
                byte fillValue = whiteIsZero ? (byte)0 : (byte)255;
                CcittDecodingTable currentTable = CcittDecodingTable.WhiteInstance;
                CcittDecodingTable otherTable = CcittDecodingTable.BlackInstance;
                int unpacked = 0;
                CcittCodeValue tableEntry;
                while (true)
                {
                    uint value = bitReader.Peek(16);
                    if (!currentTable.TryLookup(value, out tableEntry))
                    {
                        throw new InvalidDataException();
                    }

                    if (tableEntry.IsEndOfLine)
                    {
                        // fill the rest of scanline
                        scanline.Fill(fillValue);
                        bitReader.Advance(tableEntry.BitsRequired);
                        break;
                    }

                    // Check to see whether we are encountering a "filled" EOL ?
                    if ((value & 0b1111111111110000) == 0)
                    {
                        // Align to 8 bits
                        int filledBits = 8 - (bitReader.ConsumedBits + 12) % 8;
                        if (bitReader.Peek(filledBits) != 0)
                        {
                            throw new InvalidDataException();
                        }

                        // Confirm it is indeed an EOL code.
                        value = bitReader.Read(filledBits + 12);
                        if (value == 0b000000000001)
                        {
                            // fill the rest of scanline
                            scanline.Fill(fillValue);
                            break;
                        }

                        throw new InvalidDataException();
                    }

                    // Process normal code.
                    int runLength = tableEntry.RunLength;
                    scanline.Slice(0, runLength).Fill(fillValue);
                    scanline = scanline.Slice(runLength);
                    unpacked += runLength;
                    bitReader.Advance(tableEntry.BitsRequired);

                    // Terminating code is met. Switch to the other color.
                    if (tableEntry.IsTerminatingCode)
                    {
                        fillValue = (byte)~fillValue;
                        CcittHelper.SwapTable(ref currentTable, ref otherTable);

                        // This line is fully unpacked. Should exit and process next line.
                        if (unpacked == width)
                        {
                            // If this is the last line of the image, we don't process any code after this.
                            if (i == height - 1)
                            {
                                break;
                            }

                            // Is the next code word EOL ?
                            value = bitReader.Peek(16);
                            if ((value >> 4) == 0b000000000001)
                            {
                                // Skip the EOL code
                                bitReader.Advance(12);
                                break;
                            }

                            // Maybe the EOL is zero filled? First align to 8 bits
                            int filledBits = 8 - (bitReader.ConsumedBits + 12) % 8;
                            if (bitReader.Peek(filledBits) != 0)
                            {
                                bitReader.AdvanceAlignByte();
                                break;
                            }

                            // Confirm it is indeed an EOL code.
                            value = bitReader.Peek(filledBits + 12);
                            if (value == 0b000000000001)
                            {
                                bitReader.Advance(filledBits + 12);
                            }

                            bitReader.AdvanceAlignByte();
                            break;
                        }
                    }
                }
            }
        }


    }
}
