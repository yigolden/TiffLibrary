using System;
using System.Buffers;

namespace TiffLibrary.Compression
{
    /// <summary>
    /// Compression and decompression support for PackBits algorithm.
    /// </summary>
    public class PackBitsCompressionAlgorithm : ITiffCompressionAlgorithm, ITiffDecompressionAlgorithm
    {
        /// <summary>
        /// A shared instance of <see cref="PackBitsCompressionAlgorithm"/>. It should be used across the application.
        /// </summary>
        public static PackBitsCompressionAlgorithm Instance { get; } = new PackBitsCompressionAlgorithm();


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

            int height = context.ImageSize.Height;
            int bytesPerScanline = context.BytesPerScanline;
            ReadOnlySpan<byte> inputSpan = input.Span;

            for (int row = 0; row < height; row++)
            {
                ReadOnlySpan<byte> run = inputSpan.Slice(0, bytesPerScanline);
                inputSpan = inputSpan.Slice(bytesPerScanline);
                PackBits(run, outputWriter);
            }

        }

        private void PackBits(ReadOnlySpan<byte> inputSpan, IBufferWriter<byte> outputWriter)
        {
            Span<byte> buffer;

            while (!inputSpan.IsEmpty)
            {
                // Literal bytes of data
                int literalRunLength = 1;
                for (int i = 1; i < inputSpan.Length; i++)
                {
                    if (inputSpan[i] != inputSpan[i - 1] || i == 1)
                    {
                        // Noop
                        //literalRunLength++;
                        //continue;
                    }
                    else if (inputSpan[i] == inputSpan[i - 2])
                    {
                        literalRunLength -= 2;
                        break;
                    }
                    literalRunLength++;
                }
                if (literalRunLength > 0)
                {
                    if (literalRunLength > 128)
                    {
                        literalRunLength = 128;
                    }

                    buffer = outputWriter.GetSpan(1 + literalRunLength);
                    buffer[0] = (byte)(literalRunLength - 1);
                    inputSpan.Slice(0, literalRunLength).CopyTo(buffer.Slice(1));
                    outputWriter.Advance(literalRunLength + 1);
                    inputSpan = inputSpan.Slice(literalRunLength);
                    continue;
                }

                // Repeated bytes
                int repeatedLength = 1;
                for (int i = 1; i < inputSpan.Length; i++)
                {
                    if (inputSpan[i] == inputSpan[0])
                    {
                        repeatedLength++;
                        if (repeatedLength == 128)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                buffer = outputWriter.GetSpan(2);
                buffer[0] = (byte)(sbyte)(1 - repeatedLength);
                buffer[1] = inputSpan[0];
                outputWriter.Advance(2);
                inputSpan = inputSpan.Slice(repeatedLength);
            }
        }


        /// <inheritdoc />
        public void Decompress(TiffDecompressionContext context, ReadOnlyMemory<byte> input, Memory<byte> output)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            int bytesPerScanline = context.BytesPerScanline;
            ReadOnlySpan<byte> inputSpan = input.Span;
            Span<byte> scanlinesBufferSpan = output.Span;

            int totalScanlines = context.SkippedScanlines + context.RequestedScanlines;
            for (int i = 0; i < totalScanlines; i++)
            {
                if (scanlinesBufferSpan.Length < bytesPerScanline)
                {
                    throw new ArgumentException("destination too short.", nameof(output));
                }
                Span<byte> scanline = scanlinesBufferSpan.Slice(0, bytesPerScanline);
                scanlinesBufferSpan = scanlinesBufferSpan.Slice(bytesPerScanline);

                int unpacked = 0;
                while (unpacked < bytesPerScanline)
                {
                    sbyte n = (sbyte)inputSpan[0];
                    inputSpan = inputSpan.Slice(1);
                    if (n >= 0)
                    {
                        inputSpan.Slice(0, n + 1).CopyTo(scanline);
                        inputSpan = inputSpan.Slice(n + 1);
                        scanline = scanline.Slice(n + 1);
                        unpacked += n + 1;
                    }
                    else if (n != -128)
                    {
                        byte v = inputSpan[0];
                        inputSpan = inputSpan.Slice(1);
                        scanline.Slice(0, 1 - n).Fill(v);
                        scanline = scanline.Slice(1 - n);
                        unpacked += 1 - n;
                    }
                }
            }
        }
    }
}
