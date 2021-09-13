using System;
using System.Buffers;

namespace TiffLibrary.Compression
{
    /// <summary>
    /// Support for Compression=1
    /// </summary>
    public class NoneCompressionAlgorithm : ITiffCompressionAlgorithm, ITiffDecompressionAlgorithm
    {
        /// <summary>
        /// A shared instance of <see cref="NoneCompressionAlgorithm"/>.
        /// </summary>
        public static NoneCompressionAlgorithm Instance { get; } = new NoneCompressionAlgorithm();

        /// <inheritdoc />
        public void Compress(TiffCompressionContext context, ReadOnlyMemory<byte> input, IBufferWriter<byte> outputWriter)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (outputWriter is null)
            {
                throw new ArgumentNullException(nameof(outputWriter));
            }

            input.CopyTo(outputWriter.GetMemory(input.Length));
            outputWriter.Advance(input.Length);
        }

        /// <inheritdoc />
        public void Decompress(TiffDecompressionContext context, ReadOnlyMemory<byte> input, Memory<byte> output)
        {
            input.CopyTo(output);
        }
    }
}
