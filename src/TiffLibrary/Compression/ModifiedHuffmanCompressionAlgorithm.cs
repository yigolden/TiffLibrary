using System;
using System.IO;

namespace TiffLibrary.Compression
{
    /// <summary>
    /// Decompression support for CCITT Group 3 1-Dimensional Modified Huffman run length encoding.
    /// </summary>
    public class ModifiedHuffmanCompressionAlgorithm : ITiffDecompressionAlgorithm
    {
        private bool _higherOrderBitsFirst;

        /// <summary>
        /// Initialize the instance with specified fill order.
        /// </summary>
        /// <param name="higherOrderBitsFirst">If this flag is set, higher order bits are considered to precede lower order bits when reading bits from a byte.</param>
        public ModifiedHuffmanCompressionAlgorithm(bool higherOrderBitsFirst)
        {
            _higherOrderBitsFirst = higherOrderBitsFirst;
        }

        internal static ModifiedHuffmanCompressionAlgorithm HigherOrderBitsFirstInstance { get; } = new ModifiedHuffmanCompressionAlgorithm(true);

        internal static ModifiedHuffmanCompressionAlgorithm LowerOrderBitsFirstInstance { get; } = new ModifiedHuffmanCompressionAlgorithm(false);

        /// <summary>
        /// Get static cached instance of <see cref="ModifiedHuffmanCompressionAlgorithm"/>.
        /// </summary>
        /// <param name="fillOrder">The FillOrder tag specified in the image file directory.</param>
        /// <returns>A cached instance of <see cref="ModifiedHuffmanCompressionAlgorithm"/>.</returns>
        public static ModifiedHuffmanCompressionAlgorithm GetSharedInstance(TiffFillOrder fillOrder)
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
                throw new NotSupportedException("Modified Huffman compression does not support this photometric interpretation format.");
            }

            if (context.BitsPerSample.Count != 1 || context.BitsPerSample[0] != 1)
            {
                throw new NotSupportedException("Unsupported bits per sample.");
            }

            ReadOnlySpan<byte> inputSpan = input.Span;
            Span<byte> scanlinesBufferSpan = output.Span;

            bool whiteIsZero = context.PhotometricInterpretation == TiffPhotometricInterpretation.WhiteIsZero;
            int width = context.ImageSize.Width;
            var bitReader = new BitReader(inputSpan, higherOrderBitsFirst: _higherOrderBitsFirst);

            // Process every scanline
            for (int i = 0; i < (context.SkippedScanlines + context.RequestedScanlines); i++)
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
                    tableEntry = currentTable.Lookup(bitReader.Peek(16));

                    if (tableEntry.IsEndOfLine)
                    {
                        // EOL code is not used in modified huffman algorithm
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
                            break;
                        }
                    }
                    else if (runLength == 0)
                    {
                        throw new InvalidDataException();
                    }
                    else if (unpacked > width)
                    {
                        throw new InvalidDataException();
                    }
                }

                bitReader.AdvanceAlignByte();
            }
        }
    }
}
