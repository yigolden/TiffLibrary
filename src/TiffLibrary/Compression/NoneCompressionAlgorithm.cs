using System;
using System.Buffers;
using System.IO;

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
            if (input.Length <= output.Length)
            {
                input.CopyTo(output);
            }
            else
            {
                DecompressSlow(context, input, output.Length);
            }
        }

        private void DecompressSlow(TiffDecompressionContext context, ReadOnlyMemory<byte> input, int outputLength)
        {
            const int GrowthLimit = 1024;

            Memory<byte> replacedBuffer;
            if ((input.Length - outputLength) > GrowthLimit)
            {
                throw new InvalidDataException("The input buffer contains too many data.");
            }
            replacedBuffer = context.ReplaceBuffer(input.Length);
            input.CopyTo(replacedBuffer);
        }
    }
}
