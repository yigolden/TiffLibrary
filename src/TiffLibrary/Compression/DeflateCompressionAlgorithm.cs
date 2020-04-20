using System;
using System.Buffers;
using System.IO;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace TiffLibrary.Compression
{
    /// <summary>
    /// Compression and decompression support for Deflate algorithm.
    /// </summary>
    public class DeflateCompressionAlgorithm : ITiffCompressionAlgorithm, ITiffDecompressionAlgorithm
    {
        private readonly TiffDeflateCompressionLevel _compressionLevel;

        /// <summary>
        /// A shared instance of <see cref="DeflateCompressionAlgorithm"/> using default compression level. When decompressing or compressing using default level, this instance can be used to avoid an extra allocation of <see cref="DeflateCompressionAlgorithm"/>.
        /// </summary>
        public static DeflateCompressionAlgorithm Instance { get; } = new DeflateCompressionAlgorithm(TiffDeflateCompressionLevel.Default);

        /// <summary>
        /// Initialize the object with the specified compression level.
        /// </summary>
        /// <param name="compressionLevel"></param>
        public DeflateCompressionAlgorithm(TiffDeflateCompressionLevel compressionLevel)
        {
            if ((int)compressionLevel != -1 && (uint)compressionLevel > 9)
            {
                throw new ArgumentOutOfRangeException(nameof(compressionLevel));
            }
            _compressionLevel = compressionLevel;
        }


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

            var deflater = new Deflater((int)_compressionLevel, noZlibHeaderOrFooter: false);

            int bytesWritten = deflater.Deflate(outputWriter.GetSpan());
            outputWriter.Advance(bytesWritten);

            deflater.SetInput(input);

            while (true)
            {
                bytesWritten = deflater.Deflate(outputWriter.GetSpan());

                if (bytesWritten != 0)
                {
                    outputWriter.Advance(bytesWritten);
                }

                if (deflater.IsFinished)
                {
                    break;
                }

                if (deflater.IsNeedingInput)
                {
                    deflater.Finish();
                }
            }
        }


        /// <inheritdoc />
        public void Decompress(TiffDecompressionContext context, ReadOnlyMemory<byte> input, Memory<byte> output)
        {
            var inflater = new Inflater(noHeader: false);

            Span<byte> destination = output.Span;

            while (!destination.IsEmpty)
            {
                int bytesWritten = inflater.Inflate(destination);

                if (inflater.IsFinished)
                {
                    break;
                }

                if (inflater.IsNeedingInput)
                {
                    if (input.IsEmpty)
                    {
                        throw new InvalidDataException();
                    }

                    inflater.SetInput(input);
                    input = default;
                }
                else if (bytesWritten == 0)
                {
                    throw new InvalidDataException();
                }

                destination = destination.Slice(bytesWritten);
            }
        }

    }
}
