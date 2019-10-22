using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace TiffLibrary.Compression
{
    /// <summary>
    /// Compression and decompression support for Deflate algorithm.
    /// </summary>
    public class DeflateCompressionAlgorithm : ITiffCompressionAlgorithm, ITiffDecompressionAlgorithm
    {
        private readonly CompressionLevel _compressionLevel;

        /// <summary>
        /// The compression level used in Deflate algorithm.
        /// </summary>
        public enum CompressionLevel
        {
            /// <summary>
            /// Optimal.
            /// </summary>
            Optimal = 9,

            /// <summary>
            /// Fatest.
            /// </summary>
            Fastest = 1,

            /// <summary>
            /// Default.
            /// </summary>
            Default = -1,

            /// <summary>
            /// NoCompression.
            /// </summary>
            NoCompression = 0,
        }

        /// <summary>
        /// A shared instance of <see cref="DeflateCompressionAlgorithm"/> using default compression level. When decompressing or compressing using default level, this instance can be used to avoid an extra allocation of <see cref="DeflateCompressionAlgorithm"/>.
        /// </summary>
        public static DeflateCompressionAlgorithm Instance { get; } = new DeflateCompressionAlgorithm(CompressionLevel.Default);

        /// <summary>
        /// Initialize the object with the specified compression level.
        /// </summary>
        /// <param name="compressionLevel"></param>
        public DeflateCompressionAlgorithm(CompressionLevel compressionLevel)
        {
            _compressionLevel = compressionLevel;
        }


        /// <summary>
        /// Compress image data.
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

            var deflater = new Deflater((int)_compressionLevel, noZlibHeaderOrFooter: false);

            if (MemoryMarshal.TryGetArray(input, out ArraySegment<byte> segment))
            {
                CompressOnArray(deflater, segment, outputWriter);
            }
            else
            {
                CompressOnSpan(deflater, input.Span, outputWriter);
            }
        }

        private static void CompressOnArray(Deflater deflater, ArraySegment<byte> source, IBufferWriter<byte> destinationWriter)
        {
            int bytesWritten = deflater.Deflate(destinationWriter.GetSpan());
            destinationWriter.Advance(bytesWritten);

            deflater.SetInput(source.Array, source.Offset, source.Count);

            while (true)
            {
                bytesWritten = deflater.Deflate(destinationWriter.GetSpan());

                if (bytesWritten != 0)
                {
                    destinationWriter.Advance(bytesWritten);
                }

                if (deflater.IsFinished)
                {
                    break;
                }

                if (deflater.IsNeedingInput)
                {
                    deflater.Finish();
                    continue;
                }
            }

        }

        private static void CompressOnSpan(Deflater deflater, ReadOnlySpan<byte> source, IBufferWriter<byte> destinationWriter)
        {
            byte[] buf1 = ArrayPool<byte>.Shared.Rent(8192);
            try
            {
                while (true)
                {
                    int bytesWritten = deflater.Deflate(destinationWriter.GetSpan());

                    if (bytesWritten != 0)
                    {
                        destinationWriter.Advance(bytesWritten);
                    }

                    if (deflater.IsFinished)
                    {
                        break;
                    }

                    if (deflater.IsNeedingInput)
                    {
                        int bytesToRead = Math.Min(source.Length, 8192);

                        if (bytesToRead == 0)
                        {
                            deflater.Finish();
                            continue;
                        }

                        source.Slice(0, bytesToRead).CopyTo(buf1);
                        source = source.Slice(bytesToRead);

                        deflater.SetInput(buf1, 0, bytesToRead);
                    }
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buf1);
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
            var inflater = new Inflater(noHeader: false);

            if (MemoryMarshal.TryGetArray(input, out ArraySegment<byte> segment))
            {
                DecompressOnArray(inflater, segment, output.Span);
            }
            else
            {
                DecompressOnSpan(inflater, input.Span, output.Span);
            }
        }

        private void DecompressOnArray(Inflater inflater, ArraySegment<byte> source, Span<byte> destination)
        {
            while (!destination.IsEmpty)
            {
                int bytesWritten = inflater.Inflate(destination);

                if (inflater.IsFinished)
                {
                    break;
                }

                if (inflater.IsNeedingInput)
                {
                    if (source.Count == 0)
                    {
                        throw new InvalidDataException();
                    }

                    inflater.SetInput(source.Array, source.Offset, source.Count);
                    source = default;
                }
                else if (bytesWritten == 0)
                {
                    throw new InvalidDataException();
                }

                destination = destination.Slice(bytesWritten);
            }
        }

        private void DecompressOnSpan(Inflater inflater, ReadOnlySpan<byte> source, Span<byte> destination)
        {
            byte[] buf1 = ArrayPool<byte>.Shared.Rent(8192);
            try
            {
                while (!destination.IsEmpty)
                {
                    int bytesWritten = inflater.Inflate(destination);

                    if (inflater.IsFinished)
                    {
                        break;
                    }

                    if (inflater.IsNeedingInput)
                    {
                        int bytesToRead = Math.Min(source.Length, 8192);

                        if (bytesToRead == 0)
                        {
                            throw new InvalidDataException();
                        }

                        source.Slice(0, bytesToRead).CopyTo(buf1);
                        source = source.Slice(bytesToRead);

                        inflater.SetInput(buf1, 0, bytesToRead);
                    }
                    else if (bytesWritten == 0)
                    {
                        throw new InvalidDataException();
                    }

                    destination = destination.Slice(bytesWritten);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buf1);
            }
        }

    }
}
