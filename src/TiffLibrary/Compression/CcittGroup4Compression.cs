using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using CodingScanline = TiffLibrary.Compression.CcittTwoDimensionalCodingScanline;
using ReferenceScanline = TiffLibrary.Compression.CcittTwoDimensionalReferenceScanline;

namespace TiffLibrary.Compression
{
    /// <summary>
    /// Decompression support for CCITT T.6 bi-level encoding.
    /// </summary>
    public class CcittGroup4Compression : ITiffCompressionAlgorithm, ITiffDecompressionAlgorithm
    {
        private bool _higherOrderBitsFirst;

        /// <summary>
        /// Initialize the instance with specified fill order.
        /// </summary>
        /// <param name="higherOrderBitsFirst">If this flag is set, higher order bits are considered to precede lower order bits when reading bits from a byte.</param>
        public CcittGroup4Compression(bool higherOrderBitsFirst)
        {
            _higherOrderBitsFirst = higherOrderBitsFirst;
        }

        internal static CcittGroup4Compression HigherOrderBitsFirstInstance { get; } = new CcittGroup4Compression(true);

        internal static CcittGroup4Compression LowerOrderBitsFirstInstance { get; } = new CcittGroup4Compression(false);

        /// <summary>
        /// Get static cached instance of <see cref="CcittGroup4Compression"/>.
        /// </summary>
        /// <param name="fillOrder">The FillOrder tag specified in the image file directory.</param>
        /// <returns>A cached instance of <see cref="CcittGroup4Compression"/>.</returns>
        [CLSCompliant(false)]
        public static CcittGroup4Compression GetSharedInstance(TiffFillOrder fillOrder)
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

            ReferenceScanline referenceScanline = new ReferenceScanline(whiteIsZero: true, width);

            // Process every scanline
            for (int row = 0; row < height; row++)
            {
                ReadOnlySpan<byte> scanline = inputSpan.Slice(0, width);
                inputSpan = inputSpan.Slice(width);

                Encode2DScanline(ref bitWriter, referenceScanline, scanline);

                referenceScanline = new ReferenceScanline(whiteIsZero: true, scanline);
            }

            bitWriter.Flush();
        }

        private static void Encode2DScanline(ref BitWriter2 bitWriter, ReferenceScanline referenceScanline, ReadOnlySpan<byte> scanline)
        {
            int width = scanline.Length;
            CcittEncodingTable currentTable = CcittEncodingTable.WhiteInstance;
            CcittEncodingTable otherTable = CcittEncodingTable.BlackInstance;
            CodingScanline codingScanline = new CodingScanline(whiteIsZero: true, scanline);

            byte a0Byte = 0;
            int a0 = -1;

            while (true)
            {
                int a1 = codingScanline.FindA1(a0, a0Byte);
                int b1 = referenceScanline.FindB1(a0, a0Byte);
                int b2 = referenceScanline.FindB2(b1);

                if (b2 < a1)
                {
                    EncodePassCode(ref bitWriter);
                    a0 = b2;
                }
                else
                {
                    int a1b1 = b1 - a1;
                    if (a1b1 >= -3 && a1b1 <= 3)
                    {
                        // Vertical mode coding
                        switch (a1b1)
                        {
                            case 3:
                                EncodeVerticalL3Code(ref bitWriter);
                                break;
                            case 2:
                                EncodeVerticalL2Code(ref bitWriter);
                                break;
                            case 1:
                                EncodeVerticalL1Code(ref bitWriter);
                                break;
                            case 0:
                                EncodeVertical0Code(ref bitWriter);
                                break;
                            case -1:
                                EncodeVerticalR1Code(ref bitWriter);
                                break;
                            case -2:
                                EncodeVerticalR2Code(ref bitWriter);
                                break;
                            case -3:
                                EncodeVerticalR3Code(ref bitWriter);
                                break;
                            default:
                                break;
                        }

                        CcittHelper.SwapTable(ref currentTable, ref otherTable);
                        a0 = a1;
                    }
                    else
                    {
                        int a2 = codingScanline.FindA2(a1);
                        EncodeHorizontalCode(ref bitWriter);

                        currentTable.EncodeRun(ref bitWriter, a1 - a0);
                        otherTable.EncodeRun(ref bitWriter, a2 - a1);

                        a0 = a2;
                    }
                }

                if (a0 >= width)
                {
                    break;
                }
                a0Byte = scanline[a0];
            }
        }

        /// <inheritdoc />
        public int Decompress(TiffDecompressionContext context, ReadOnlyMemory<byte> input, Memory<byte> output)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.PhotometricInterpretation != TiffPhotometricInterpretation.WhiteIsZero && context.PhotometricInterpretation != TiffPhotometricInterpretation.BlackIsZero)
            {
                throw new NotSupportedException("T6 compression does not support this photometric interpretation.");
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

            ReferenceScanline referenceScanline = new ReferenceScanline(whiteIsZero, width);

            // Process every scanline
            for (int i = 0; i < height; i++)
            {
                Span<byte> scanline = scanlinesBufferSpan.Slice(0, width);
                scanlinesBufferSpan = scanlinesBufferSpan.Slice(width);

                Decode2DScanline(ref bitReader, whiteIsZero, referenceScanline, scanline);

                referenceScanline = new ReferenceScanline(whiteIsZero, scanline);
            }

            return context.BytesPerScanline * context.ImageSize.Height;
        }

        private static void Decode2DScanline(ref BitReader bitReader, bool whiteIsZero, ReferenceScanline referenceScanline, Span<byte> scanline)
        {
            int width = scanline.Length;
            CcittDecodingTable currentTable = CcittDecodingTable.WhiteInstance;
            CcittDecodingTable otherTable = CcittDecodingTable.BlackInstance;
            CcittTwoDimensionalDecodingTable decodingTable = CcittTwoDimensionalDecodingTable.Instance;
            CcittTwoDimensionalCodeValue tableEntry;
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

                // Special case handling for EOL (EOLB).
                // Lucky we don't need to peek again for EOL, because PeekCount(12) is excatly the length of EOL code.
                if (value == 0b000000000001)
                {
                    // If a TIFF reader encounters EOFB before the expected number of lines has been extracted,
                    // it is appropriate to assume that the missing rows consist entirely of white pixels.
                    scanline.Fill(whiteIsZero ? (byte)0 : (byte)255);
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
                    case CcittTwoDimensionalCodeType.Pass:
                        int b2 = referenceScanline.FindB2(b1);
                        scanline.Slice(unpacked, b2 - unpacked).Fill(fillByte);
                        unpacked = b2;
                        a0 = b2;
                        break;
                    case CcittTwoDimensionalCodeType.Horizontal:
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
                    case CcittTwoDimensionalCodeType.Vertical0:
                        a1 = b1;
                        scanline.Slice(unpacked, a1 - unpacked).Fill(fillByte);
                        unpacked = a1;
                        a0 = a1;
                        fillByte = (byte)~fillByte;
                        CcittHelper.SwapTable(ref currentTable, ref otherTable);
                        break;
                    case CcittTwoDimensionalCodeType.VerticalR1:
                        a1 = b1 + 1;
                        scanline.Slice(unpacked, a1 - unpacked).Fill(fillByte);
                        unpacked = a1;
                        a0 = a1;
                        fillByte = (byte)~fillByte;
                        CcittHelper.SwapTable(ref currentTable, ref otherTable);
                        break;
                    case CcittTwoDimensionalCodeType.VerticalR2:
                        a1 = b1 + 2;
                        scanline.Slice(unpacked, a1 - unpacked).Fill(fillByte);
                        unpacked = a1;
                        a0 = a1;
                        fillByte = (byte)~fillByte;
                        CcittHelper.SwapTable(ref currentTable, ref otherTable);
                        break;
                    case CcittTwoDimensionalCodeType.VerticalR3:
                        a1 = b1 + 3;
                        scanline.Slice(unpacked, a1 - unpacked).Fill(fillByte);
                        unpacked = a1;
                        a0 = a1;
                        fillByte = (byte)~fillByte;
                        CcittHelper.SwapTable(ref currentTable, ref otherTable);
                        break;
                    case CcittTwoDimensionalCodeType.VerticalL1:
                        a1 = b1 - 1;
                        scanline.Slice(unpacked, a1 - unpacked).Fill(fillByte);
                        unpacked = a1;
                        a0 = a1;
                        fillByte = (byte)~fillByte;
                        CcittHelper.SwapTable(ref currentTable, ref otherTable);
                        break;
                    case CcittTwoDimensionalCodeType.VerticalL2:
                        a1 = b1 - 2;
                        scanline.Slice(unpacked, a1 - unpacked).Fill(fillByte);
                        unpacked = a1;
                        a0 = a1;
                        fillByte = (byte)~fillByte;
                        CcittHelper.SwapTable(ref currentTable, ref otherTable);
                        break;
                    case CcittTwoDimensionalCodeType.VerticalL3:
                        a1 = b1 - 3;
                        scanline.Slice(unpacked, a1 - unpacked).Fill(fillByte);
                        unpacked = a1;
                        a0 = a1;
                        fillByte = (byte)~fillByte;
                        CcittHelper.SwapTable(ref currentTable, ref otherTable);
                        break;
                    default:
                        throw new NotSupportedException("Extensions not supportted.");
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



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EncodePassCode(ref BitWriter2 writer)
        {
            writer.Write(0b0001, 4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EncodeHorizontalCode(ref BitWriter2 writer)
        {
            writer.Write(0b001, 3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EncodeVertical0Code(ref BitWriter2 writer)
        {
            writer.Write(0b1, 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EncodeVerticalR1Code(ref BitWriter2 writer)
        {
            writer.Write(0b011, 3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EncodeVerticalR2Code(ref BitWriter2 writer)
        {
            writer.Write(0b000011, 6);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EncodeVerticalR3Code(ref BitWriter2 writer)
        {
            writer.Write(0b0000011, 7);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EncodeVerticalL1Code(ref BitWriter2 writer)
        {
            writer.Write(0b010, 3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EncodeVerticalL2Code(ref BitWriter2 writer)
        {
            writer.Write(0b000010, 6);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EncodeVerticalL3Code(ref BitWriter2 writer)
        {
            writer.Write(0b0000010, 7);
        }
    }
}
