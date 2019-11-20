using System;
using System.IO;
using ReferenceScanline = TiffLibrary.Compression.CcittGroup3TwoDimensionalReferenceScanline;

namespace TiffLibrary.Compression
{
    /// <summary>
    /// Decompression support for CCITT T.4 bi-level encoding  (2-dimensional).
    /// </summary>
    public class CcittGroup3TwoDimensionalCompressionAlgorithm : ITiffDecompressionAlgorithm
    {
        private bool _higherOrderBitsFirst;

        /// <summary>
        /// Initialize the instance with specified fill order.
        /// </summary>
        /// <param name="higherOrderBitsFirst">If this flag is set, higher order bits are considered to precede lower order bits when reading bits from a byte.</param>
        public CcittGroup3TwoDimensionalCompressionAlgorithm(bool higherOrderBitsFirst)
        {
            _higherOrderBitsFirst = higherOrderBitsFirst;
        }

        internal static CcittGroup3TwoDimensionalCompressionAlgorithm HigherOrderBitsFirstInstance { get; } = new CcittGroup3TwoDimensionalCompressionAlgorithm(true);

        internal static CcittGroup3TwoDimensionalCompressionAlgorithm LowerOrderBitsFirstInstance { get; } = new CcittGroup3TwoDimensionalCompressionAlgorithm(false);

        /// <summary>
        /// Get static cached instance of <see cref="CcittGroup3TwoDimensionalCompressionAlgorithm"/>.
        /// </summary>
        /// <param name="fillOrder">The FillOrder tag specified in the image file directory.</param>
        /// <returns>A cached instance of <see cref="CcittGroup3TwoDimensionalCompressionAlgorithm"/>.</returns>
        public static CcittGroup3TwoDimensionalCompressionAlgorithm GetSharedInstance(TiffFillOrder fillOrder)
        {
            if (fillOrder != TiffFillOrder.LowerOrderBitsFirst)
            {
                return HigherOrderBitsFirstInstance;
            }
            return LowerOrderBitsFirstInstance;
        }

        /// <summary>
        /// Decompress the image data.
        /// </summary>
        /// <param name="context">Information about the TIFF file.</param>
        /// <param name="input">The input data.</param>
        /// <param name="output">The output data.</param>
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
            var bitReader = new BitReader(inputSpan, _higherOrderBitsFirst);

            ReferenceScanline referenceScanline = default;

            // Process every scanline
            for (int i = 0; i < (context.SkippedScanlines + context.RequestedScanlines); i++)
            {
                if (scanlinesBufferSpan.Length < width)
                {
                    throw new InvalidDataException();
                }
                Span<byte> scanline = scanlinesBufferSpan.Slice(0, width);
                scanlinesBufferSpan = scanlinesBufferSpan.Slice(width);

                // Read the first EOL.
                if (bitReader.Peek(12) == 0b000000000001) // EOL code is 000000000001 (12bits).
                {
                    bitReader.Advance(12);
                }
                else // Or EOL code is zero filled.
                {
                    // Align to 8 bits
                    int filledBits = 8 - (bitReader.ConsumedBits + 12) % 8;
                    if (bitReader.Peek(filledBits) != 0)
                    {
                        throw new InvalidDataException();
                    }

                    // Confirm it is indeed an EOL code.
                    int value = (int)bitReader.Read(filledBits + 12);
                    if (value != 0b000000000001)
                    {
                        throw new InvalidDataException();
                    }
                }

                // Determine the type of this line (1d vs 2d)
                bool is1D = bitReader.Read(1) != 0;

                if (is1D)
                {
                    Decode1DScanline(ref bitReader, whiteIsZero, scanline);
                }
                else
                {
                    if (referenceScanline.IsEmpty)
                    {
                        throw new InvalidDataException();
                    }
                    Decode2DScanline(ref bitReader, whiteIsZero, referenceScanline, scanline);
                }
                referenceScanline = new ReferenceScanline(whiteIsZero, scanline);
            }
        }

        private static void Decode1DScanline(ref BitReader bitReader, bool whiteIsZero, Span<byte> scanline)
        {
            int width = scanline.Length;

            // Process every code word in this scanline
            byte fillValue = whiteIsZero ? (byte)0 : (byte)255;
            CcittDecodingTable currentTable = CcittDecodingTable.WhiteInstance;
            CcittDecodingTable otherTable = CcittDecodingTable.BlackInstance;
            int unpacked = 0;
            while (true)
            {
                int runLength = currentTable.DecodeRun(ref bitReader);
                if ((uint)runLength > (uint)scanline.Length)
                {
                    throw new InvalidOperationException();
                }
                scanline.Slice(0, runLength).Fill(fillValue);
                scanline = scanline.Slice(runLength);
                unpacked += runLength;

                // Switch to the other color.
                fillValue = (byte)~fillValue;
                CcittHelper.SwapTable(ref currentTable, ref otherTable);

                if (unpacked == width)
                {
                    // This line is fully unpacked. Should exit and process next line.
                    break;
                }
                else if (unpacked > width)
                {
                    throw new InvalidDataException();
                }
            }
        }

        private static void Decode2DScanline(ref BitReader bitReader, bool whiteIsZero, ReferenceScanline referenceScanline, Span<byte> scanline)
        {
            int width = scanline.Length;
            CcittDecodingTable currentTable = CcittDecodingTable.WhiteInstance;
            CcittDecodingTable otherTable = CcittDecodingTable.BlackInstance;
            CcittTwoDimensionalDecodingTable decodingTable = CcittTwoDimensionalDecodingTable.Instance;
            CcittTwoDimensionalDecodingTable.Entry tableEntry;
            const int PeekCount = CcittTwoDimensionalDecodingTable.PeekCount;

            // 2D Encoding variables.
            int a0 = -1, a1, b1;
            byte fillByte = whiteIsZero ? (byte)0 : (byte)255;

            // Process every code word in this scanline
            int unpacked = 0;
            while (true)
            {
                // Read next code word and advance pass it.
                int value = (int)bitReader.Peek(PeekCount); // PeekCount = 12

                // Special case handling for EOL.
                // Lucky we don't need to peek again for EOL, because PeekCount(12) is excatly the length of EOL code.
                if (value == 0b000000000001)
                {
                    scanline.Fill(fillByte);
                    break;
                }

                // Look up in the table and advance past this code.
                if (!decodingTable.TryLookup(value, out tableEntry))
                {
                    throw new InvalidDataException();
                }
                bitReader.Advance(tableEntry.BitsRequired);

                // Update 2D Encoding variables.
                b1 = referenceScanline.FindB1(a0, fillByte);

                // Switch on the code word.
                switch (tableEntry.Type)
                {
                    case CcittTwoDimensionalDecodingTable.CodeType.Pass:
                        int b2 = referenceScanline.FindB2(b1);
                        scanline.Slice(unpacked, b2 - unpacked).Fill(fillByte);
                        unpacked = b2;
                        a0 = b2;
                        break;
                    case CcittTwoDimensionalDecodingTable.CodeType.Horizontal:
                        // Decode M(a0a1)
                        int runLength = currentTable.DecodeRun(ref bitReader);
                        if ((uint)runLength > (uint)(scanline.Length - unpacked))
                        {
                            throw new InvalidOperationException();
                        }
                        scanline.Slice(unpacked, runLength).Fill(fillByte);
                        unpacked += runLength;
                        fillByte = (byte)~fillByte;
                        CcittHelper.SwapTable(ref currentTable, ref otherTable);
                        // Decode M(a1a2)
                        runLength = currentTable.DecodeRun(ref bitReader);
                        if ((uint)runLength > (uint)(scanline.Length - unpacked))
                        {
                            throw new InvalidOperationException();
                        }
                        scanline.Slice(unpacked, runLength).Fill(fillByte);
                        unpacked += runLength;
                        fillByte = (byte)~fillByte;
                        CcittHelper.SwapTable(ref currentTable, ref otherTable);
                        // Prepare next a0
                        a0 = unpacked;
                        break;
                    case CcittTwoDimensionalDecodingTable.CodeType.Vertical0:
                        a1 = b1;
                        scanline.Slice(unpacked, a1 - unpacked).Fill(fillByte);
                        unpacked = a1;
                        a0 = a1;
                        fillByte = (byte)~fillByte;
                        CcittHelper.SwapTable(ref currentTable, ref otherTable);
                        break;
                    case CcittTwoDimensionalDecodingTable.CodeType.VerticalR1:
                        a1 = b1 + 1;
                        scanline.Slice(unpacked, a1 - unpacked).Fill(fillByte);
                        unpacked = a1;
                        a0 = a1;
                        fillByte = (byte)~fillByte;
                        CcittHelper.SwapTable(ref currentTable, ref otherTable);
                        break;
                    case CcittTwoDimensionalDecodingTable.CodeType.VerticalR2:
                        a1 = b1 + 2;
                        scanline.Slice(unpacked, a1 - unpacked).Fill(fillByte);
                        unpacked = a1;
                        a0 = a1;
                        fillByte = (byte)~fillByte;
                        CcittHelper.SwapTable(ref currentTable, ref otherTable);
                        break;
                    case CcittTwoDimensionalDecodingTable.CodeType.VerticalR3:
                        a1 = b1 + 3;
                        scanline.Slice(unpacked, a1 - unpacked).Fill(fillByte);
                        unpacked = a1;
                        a0 = a1;
                        fillByte = (byte)~fillByte;
                        CcittHelper.SwapTable(ref currentTable, ref otherTable);
                        break;
                    case CcittTwoDimensionalDecodingTable.CodeType.VerticalL1:
                        a1 = b1 - 1;
                        scanline.Slice(unpacked, a1 - unpacked).Fill(fillByte);
                        unpacked = a1;
                        a0 = a1;
                        fillByte = (byte)~fillByte;
                        CcittHelper.SwapTable(ref currentTable, ref otherTable);
                        break;
                    case CcittTwoDimensionalDecodingTable.CodeType.VerticalL2:
                        a1 = b1 - 2;
                        scanline.Slice(unpacked, a1 - unpacked).Fill(fillByte);
                        unpacked = a1;
                        a0 = a1;
                        fillByte = (byte)~fillByte;
                        CcittHelper.SwapTable(ref currentTable, ref otherTable);
                        break;
                    case CcittTwoDimensionalDecodingTable.CodeType.VerticalL3:
                        a1 = b1 - 3;
                        scanline.Slice(unpacked, a1 - unpacked).Fill(fillByte);
                        unpacked = a1;
                        a0 = a1;
                        fillByte = (byte)~fillByte;
                        CcittHelper.SwapTable(ref currentTable, ref otherTable);
                        break;
                    default:
                        throw new NotSupportedException("1D and 2D Extensions not supportted.");
                }

                // This line is fully unpacked. Should exit and process next line.
                if (unpacked == width)
                {
                    break;
                }
                else if (unpacked > width)
                {
                    throw new InvalidDataException();
                }
            }

        }

    }
}
