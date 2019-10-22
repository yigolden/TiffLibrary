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

        /// <summary>
        /// Decompress the image data.
        /// </summary>
        /// <param name="context">Information about the TIFF file.</param>
        /// <param name="input">The input data.</param>
        /// <param name="outputWriter">The output writer.</param>
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

        /// <summary>
        /// Decompress the image data.
        /// </summary>
        /// <param name="context">Information about the TIFF file.</param>
        /// <param name="input">The input data.</param>
        /// <param name="output">The output data.</param>
        public void Decompress(TiffDecompressionContext context, ReadOnlyMemory<byte> input, Memory<byte> output)
        {
            input.CopyTo(output);
        }
    }
}
