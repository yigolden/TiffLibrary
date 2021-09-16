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
            ThrowHelper.ThrowIfNull(context);
            ThrowHelper.ThrowIfNull(outputWriter);

            input.CopyTo(outputWriter.GetMemory(input.Length));
            outputWriter.Advance(input.Length);
        }

        /// <inheritdoc />
        public int Decompress(TiffDecompressionContext context, ReadOnlyMemory<byte> input, Memory<byte> output)
        {
            input.CopyTo(output);
            return input.Length;
        }
    }
}
