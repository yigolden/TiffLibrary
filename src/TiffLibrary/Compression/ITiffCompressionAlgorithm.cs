using System;
using System.Buffers;

namespace TiffLibrary.Compression
{
    /// <summary>
    /// Represents an object that is capable of decompressing image data in TIFF file.
    /// </summary>
    public interface ITiffDecompressionAlgorithm
    {
        /// <summary>
        /// Decompress the image data.
        /// </summary>
        /// <param name="context">Information about the TIFF file.</param>
        /// <param name="input">The input data.</param>
        /// <param name="output">The output data.</param>
        /// <returns>The number of bytes written to <paramref name="output"/>.</returns>
        int Decompress(TiffDecompressionContext context, ReadOnlyMemory<byte> input, Memory<byte> output);
    }

    /// <summary>
    /// Represents an object that is capable of compressing image data into TIFF file.
    /// </summary>
    public interface ITiffCompressionAlgorithm
    {
        /// <summary>
        /// Compress image data.
        /// </summary>
        /// <param name="context">Information about the TIFF file.</param>
        /// <param name="input">The input data.</param>
        /// <param name="outputWriter">The output writer.</param>
        void Compress(TiffCompressionContext context, ReadOnlyMemory<byte> input, IBufferWriter<byte> outputWriter);
    }
}
