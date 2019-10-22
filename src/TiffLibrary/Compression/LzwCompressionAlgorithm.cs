using System;
using System.Buffers;
using System.IO;
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


        /// <summary>
        /// Compress image data.
        /// </summary>
        /// <param name="context">Information about the TIFF file.</param>
        /// <param name="input">The input data.</param>
        /// <param name="outputWriter">The output writer.</param>
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


        /// <summary>
        /// Decompress the image data.
        /// </summary>
        /// <param name="context">Information about the TIFF file.</param>
        /// <param name="input">The input data.</param>
        /// <param name="output">The output data.</param>
        public void Decompress(TiffDecompressionContext context, ReadOnlyMemory<byte> input, Memory<byte> output)
        {
            ReadOnlySpan<byte> inputSpan = input.Span;
            Span<byte> scanlinesBufferSpan = output.Span;

            if (inputSpan.IsEmpty)
            {
                return;
            }

            byte first = inputSpan[0];
            if (first == 0)
            {
                DecompressLeastSignificantBitFirst(inputSpan, scanlinesBufferSpan);
            }
            else if (first == 128)
            {
                DecompressMostSignificantBitFirst(inputSpan, scanlinesBufferSpan);
            }
            else
            {
                throw new InvalidDataException();
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
