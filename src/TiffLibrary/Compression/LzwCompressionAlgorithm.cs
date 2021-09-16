using System;
using System.Buffers;
using TiffLibrary.Utils;

namespace TiffLibrary.Compression
{
    /// <summary>
    /// Compression and decompression support for LZW algorithm.
    /// </summary>
    public class LzwCompressionAlgorithm : ITiffCompressionAlgorithm, ITiffDecompressionAlgorithm
    {
        /// <summary>
        /// A shared instance of <see cref="LzwCompressionAlgorithm"/>. It should be used across the application.
        /// </summary>
        public static LzwCompressionAlgorithm Instance { get; } = new LzwCompressionAlgorithm();


        /// <inheritdoc />
        public void Compress(TiffCompressionContext context, ReadOnlyMemory<byte> input, IBufferWriter<byte> outputWriter)
        {
            var lzw = new TiffLzwEncoder();
            try
            {
                lzw.Initialize(input.Span, outputWriter);
                lzw.Encode();
            }
            finally
            {
                lzw.Dispose();
            }
        }


        /// <inheritdoc />
        public int Decompress(TiffDecompressionContext context, ReadOnlyMemory<byte> input, Memory<byte> output)
        {
            ReadOnlySpan<byte> inputSpan = input.Span;
            Span<byte> scanlinesBufferSpan = output.Span;

            if (inputSpan.IsEmpty)
            {
                return 0;
            }

            byte first = inputSpan[0];
            if (first == 0)
            {
                return DecompressLeastSignificantBitFirst(inputSpan, scanlinesBufferSpan);
            }
            else if (first == 128)
            {
                return DecompressMostSignificantBitFirst(inputSpan, scanlinesBufferSpan);
            }
            else
            {
                ThrowHelper.ThrowInvalidDataException();
                return default;
            }
        }

        private static int DecompressLeastSignificantBitFirst(ReadOnlySpan<byte> input, Span<byte> scanlinesBuffer)
        {
            var lzw = new TiffLzwDecoderLeastSignificantBitFirst();
            try
            {
                lzw.Initialize();
                return lzw.Decode(input, scanlinesBuffer);
            }
            finally
            {
                lzw.Dispose();
            }
        }

        private static int DecompressMostSignificantBitFirst(ReadOnlySpan<byte> input, Span<byte> scanlinesBuffer)
        {
            var lzw = new TiffLzwDecoderMostSignificantBitFirst();
            try
            {
                lzw.Initialize();
                return lzw.Decode(input, scanlinesBuffer);
            }
            finally
            {
                lzw.Dispose();
            }
        }

    }
}
