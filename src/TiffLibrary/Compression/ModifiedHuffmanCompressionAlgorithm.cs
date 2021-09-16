using System;
using System.Buffers;
using System.IO;

namespace TiffLibrary.Compression
{
    /// <summary>
    /// Decompression support for CCITT Group 3 1-Dimensional Modified Huffman run length encoding.
    /// </summary>
    public class ModifiedHuffmanCompressionAlgorithm : ITiffCompressionAlgorithm, ITiffDecompressionAlgorithm
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
        [CLSCompliant(false)]
        public static ModifiedHuffmanCompressionAlgorithm GetSharedInstance(TiffFillOrder fillOrder)
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
            ThrowHelper.ThrowIfNull(context);

            if (context.PhotometricInterpretation != TiffPhotometricInterpretation.WhiteIsZero && context.PhotometricInterpretation != TiffPhotometricInterpretation.BlackIsZero)
            {
                ThrowHelper.ThrowNotSupportedException("Modified Huffman compression does not support this photometric interpretation.");
            }

            if (context.BitsPerSample.Count != 1 || context.BitsPerSample[0] != 8)
            {
                ThrowHelper.ThrowNotSupportedException("Unsupported bits per sample.");
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

                bitWriter.AdvanceAlignByte();
            }

            bitWriter.Flush();
        }

        /// <inheritdoc />
        public int Decompress(TiffDecompressionContext context, ReadOnlyMemory<byte> input, Memory<byte> output)
        {
            ThrowHelper.ThrowIfNull(context);

            if (context.PhotometricInterpretation != TiffPhotometricInterpretation.WhiteIsZero && context.PhotometricInterpretation != TiffPhotometricInterpretation.BlackIsZero)
            {
                ThrowHelper.ThrowNotSupportedException("Modified Huffman compression does not support this photometric interpretation.");
            }

            if (context.BitsPerSample.Count != 1 || context.BitsPerSample[0] != 1)
            {
                ThrowHelper.ThrowNotSupportedException("Unsupported bits per sample.");
            }

            ReadOnlySpan<byte> inputSpan = input.Span;
            Span<byte> scanlinesBufferSpan = output.Span;

            bool whiteIsZero = context.PhotometricInterpretation == TiffPhotometricInterpretation.WhiteIsZero;
            int width = context.ImageSize.Width;
            int height = context.SkippedScanlines + context.RequestedScanlines;
            var bitReader = new BitReader(inputSpan, higherOrderBitsFirst: _higherOrderBitsFirst);

            // Process every scanline
            for (int i = 0; i < height; i++)
            {
                if (scanlinesBufferSpan.Length < width)
                {
                    ThrowHelper.ThrowInvalidDataException("Destination buffer is too small");
                }
                Span<byte> scanline = scanlinesBufferSpan.Slice(0, width);
                scanlinesBufferSpan = scanlinesBufferSpan.Slice(width);

                // Process every code word in this scanline
                byte fillValue = whiteIsZero ? (byte)0 : (byte)255;
                CcittDecodingTable currentTable = CcittDecodingTable.WhiteInstance;
                CcittDecodingTable otherTable = CcittDecodingTable.BlackInstance;
                int unpacked = 0;
                while (true)
                {
                    if (!currentTable.TryLookup(bitReader.Peek(16), out CcittCodeValue tableEntry))
                    {
                        ThrowHelper.ThrowInvalidDataException();
                    }

                    if (tableEntry.IsEndOfLine)
                    {
                        // EOL code is not used in modified huffman algorithm
                        ThrowHelper.ThrowInvalidDataException();
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
                }

                bitReader.AdvanceAlignByte();
            }

            return context.BytesPerScanline * context.ImageSize.Height;
        }
    }
}
