using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO;

namespace TiffLibrary.Compression
{
    /// <summary>
    /// Compression and decompression support for NeXT 2-bit Grey Scale compression algorithm.
    /// </summary>
    public class NeXTCompressionAlgorithm : ITiffCompressionAlgorithm, ITiffDecompressionAlgorithm
    {
        /// <summary>
        /// A shared instance of <see cref="NeXTCompressionAlgorithm"/>. It should be used across the application.
        /// </summary>
        public static NeXTCompressionAlgorithm Instance { get; } = new NeXTCompressionAlgorithm();

        /// <inheritdoc />
        public void Compress(TiffCompressionContext context, ReadOnlyMemory<byte> input, IBufferWriter<byte> outputWriter)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.PhotometricInterpretation != TiffPhotometricInterpretation.WhiteIsZero && context.PhotometricInterpretation != TiffPhotometricInterpretation.BlackIsZero)
            {
                throw new NotSupportedException("NeXT compression does not support this photometric interpretation.");
            }

            if (context.BitsPerSample.Count != 1 || context.BitsPerSample[0] != 8)
            {
                throw new NotSupportedException("Unsupported bits per sample.");
            }

            context.BitsPerSample = TiffValueCollection.Single<ushort>(2);

            var encoder = new NeXTEncoder(outputWriter);

            int height = context.ImageSize.Height;
            int bytesPerScanline = context.BytesPerScanline;
            ReadOnlySpan<byte> inputSpan = input.Span;

            for (int row = 0; row < height; row++)
            {
                ReadOnlySpan<byte> run = inputSpan.Slice(0, bytesPerScanline);
                inputSpan = inputSpan.Slice(bytesPerScanline);
                encoder.EncodeScan(run, 0b11000000);
            }
        }

        /// <inheritdoc />
        public void Decompress(TiffDecompressionContext context, ReadOnlyMemory<byte> input, Memory<byte> output)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.PhotometricInterpretation != TiffPhotometricInterpretation.WhiteIsZero && context.PhotometricInterpretation != TiffPhotometricInterpretation.BlackIsZero)
            {
                throw new NotSupportedException("ThunderScan compression does not support this photometric interpretation.");
            }

            if (context.BitsPerSample.Count != 1 || context.BitsPerSample[0] != 2)
            {
                throw new NotSupportedException("Unsupported bits per sample.");
            }

            int width = context.ImageSize.Width;
            int height = context.SkippedScanlines + context.RequestedScanlines;
            int bytesPerScanline = context.BytesPerScanline;

            ReadOnlySpan<byte> inputSpan = input.Span;
            Span<byte> outputSpan = output.Span;

            for (int row = 0; row < height; row++)
            {
                if (inputSpan.IsEmpty)
                {
                    ThrowMoreDataIsExpected();
                }

                Span<byte> scanline = outputSpan.Slice(0, bytesPerScanline);
                outputSpan = outputSpan.Slice(bytesPerScanline);

                int leadingByte = inputSpan[0];
                if (leadingByte == 0x00)
                {
                    // Literal row
                    inputSpan = inputSpan.Slice(1);
                    if (inputSpan.Length < scanline.Length)
                    {
                        ThrowMoreDataIsExpected();
                    }

                    inputSpan.Slice(0, scanline.Length).CopyTo(scanline);
                    inputSpan = inputSpan.Slice(scanline.Length);
                }
                else if (leadingByte == 0x40)
                {
                    // Literal span
                    if (inputSpan.Length < 5)
                    {
                        ThrowMoreDataIsExpected();
                    }

                    int offset = BinaryPrimitives.ReadUInt16BigEndian(inputSpan.Slice(1));
                    int count = BinaryPrimitives.ReadUInt16BigEndian(inputSpan.Slice(3));
                    inputSpan = inputSpan.Slice(5);
                    if (inputSpan.Length < count)
                    {
                        ThrowMoreDataIsExpected();
                    }

                    if (offset > scanline.Length || (offset + count) > scanline.Length)
                    {
                        throw new InvalidDataException("Incorrect offset and count for literal span.");
                    }

                    scanline.Slice(0, offset).Fill(0xff);
                    inputSpan.Slice(0, count).CopyTo(scanline.Slice(offset, count));
                    scanline.Slice(offset + count).Fill(0xff);

                    inputSpan = inputSpan.Slice(count);
                }
                else
                {
                    // RLE
                    var writer = new RunLengthWriter(scanline, width);
                    while (!writer.IsEndOfLine)
                    {
                        if (inputSpan.IsEmpty)
                        {
                            ThrowMoreDataIsExpected();
                        }
                        writer.WriteRunLength(inputSpan[0]);
                        inputSpan = inputSpan.Slice(1);
                    }
                }
            }
        }

        private static void ThrowMoreDataIsExpected()
        {
            throw new InvalidDataException("More data is expected.");
        }

        ref struct RunLengthWriter
        {
            private Span<byte> _destination;
            private readonly int _width;
            private int _pixelsWritten;

            public bool IsEndOfLine => _pixelsWritten == _width;

            public RunLengthWriter(Span<byte> destination, int width)
            {
                Debug.Assert(destination.Length == ((width + 3) / 4));
                _destination = destination;
                _width = width;
                _pixelsWritten = 0;
            }

            public void WriteRunLength(int data)
            {
                Debug.Assert(data >= 0 && data <= 0xff);

                int pixel = data & 0b11000000;
                int runLength = data & 0b111111;
                if (_pixelsWritten + runLength > _width)
                {
                    ThrowIncorrectRunLength();
                }

                int fill = (pixel >> 2) | pixel;
                fill = (fill >> 4) | fill;

                while (runLength != 0)
                {
                    int slots = _pixelsWritten & 0b11;
                    if (slots != 0)
                    {
                        int iterations = 4 - slots;
                        for (; iterations > 0 && runLength > 0; iterations--, runLength--, slots++)
                        {
                            _destination[0] = (byte)(_destination[0] | (pixel >> (slots * 2)));
                            _pixelsWritten++;
                        }
                        if (iterations == 0)
                        {
                            _destination = _destination.Slice(1);
                            continue;
                        }
                        Debug.Assert(runLength == 0);
                        break;
                    }
                    for (; runLength >= 4; runLength -= 4)
                    {
                        _destination[0] = (byte)fill;
                        _destination = _destination.Slice(1);
                        _pixelsWritten += 4;
                    }
                    if (runLength != 0)
                    {
                        _destination[0] = (byte)pixel;
                        _pixelsWritten++;
                        runLength--;
                    }
                }
            }

            private static void ThrowIncorrectRunLength()
            {
                throw new InvalidDataException("Incorredt run length encountered.");
            }
        }

        readonly struct NeXTEncoder
        {
            private readonly IBufferWriter<byte> _writer;

            public NeXTEncoder(IBufferWriter<byte> writer)
            {
                _writer = writer;
            }

            public void EncodeScan(ReadOnlySpan<byte> pixels, int whiteValue)
            {
                Debug.Assert(whiteValue == 0 || whiteValue == 0b11000000);

                // determine how many bytes can be saved if we use literal span or RLE
                int uncompressedSize = pixels.Length + 1;
                int literalSpanLength = GetLiteralSpanCompressedLength(pixels, whiteValue);
                int rleLength = GetRunLengthCompressedLength(pixels, whiteValue);

                if (literalSpanLength <= rleLength && literalSpanLength <= uncompressedSize)
                {
                    EncodeLiteralSpan(pixels, whiteValue);
                }
                else if (rleLength <= literalSpanLength && rleLength <= uncompressedSize)
                {
                    EncodeRunLength(pixels, whiteValue, rleLength);
                }
                else
                {
                    EncodeLiteralRow(pixels);
                }
            }

            private static int GetLiteralSpanCompressedLength(ReadOnlySpan<byte> pixels, int whiteValue)
            {
                Debug.Assert(whiteValue == 0 || whiteValue == 0b11000000);

                if (pixels.IsEmpty)
                {
                    return int.MaxValue;
                }

                int startPosition = 0;
                for (int i = 0; i < pixels.Length; i++)
                {
                    if ((pixels[i] & 0b11000000) != whiteValue)
                    {
                        startPosition = i;
                        break;
                    }
                }

                int endPosition = 0;
                for (int i = pixels.Length - 1; i >= startPosition; i--)
                {
                    if ((pixels[i] & 0b11000000) != whiteValue)
                    {
                        endPosition = i;
                        break;
                    }
                }

                startPosition &= ~0b11;
                return 5 + (endPosition - startPosition + 3) / 4;
            }

            private static int GetRunLengthCompressedLength(ReadOnlySpan<byte> pixels, int whiteValue)
            {
                Debug.Assert(whiteValue == 0 || whiteValue == 0b11000000);

                int lastPixel = whiteValue;
                int runLength = 0;
                int compressedLength = 0;

                for (int i = 0; i < pixels.Length; i++)
                {
                    int pixel = pixels[i] & 0b11000000;
                    if (pixel == lastPixel)
                    {
                        runLength++;
                    }
                    else
                    {
                        compressedLength += (runLength + 0b111111 - 1) / 0b111111;
                        lastPixel = pixel;
                        runLength = 1;
                    }
                }

                compressedLength += (runLength + 0b111111 - 1) / 0b111111;
                return compressedLength;
            }

            private void EncodeLiteralRow(ReadOnlySpan<byte> pixels)
            {
                int index = 0;
                Span<byte> span = _writer.GetSpan(1 + (pixels.Length + 3) / 4);
                span[index++] = 0x00;

                while (pixels.Length >= 4)
                {
                    span[index++] = (byte)((pixels[0] & 0b11000000) | ((pixels[1] & 0b11000000) >> 2) | ((pixels[2] & 0b11000000) >> 4) | ((pixels[3] & 0b11000000) >> 6));
                    pixels = pixels.Slice(4);
                }

                switch (pixels.Length)
                {
                    case 1:
                        span[index++] = (byte)((pixels[0] & 0b11000000));
                        break;
                    case 2:
                        span[index++] = (byte)((pixels[0] & 0b11000000) | ((pixels[1] & 0b11000000) >> 2));
                        break;
                    case 3:
                        span[index++] = (byte)((pixels[0] & 0b11000000) | ((pixels[1] & 0b11000000) >> 2) | ((pixels[2] & 0b11000000) >> 4));
                        break;
                }

                _writer.Advance(index);
            }

            private void EncodeLiteralSpan(ReadOnlySpan<byte> pixels, int whiteValue)
            {
                Debug.Assert(whiteValue == 0 || whiteValue == 0b11000000);

                int startPosition = 0;
                for (int i = 0; i < pixels.Length; i++)
                {
                    if ((pixels[i] & 0b11000000) != whiteValue)
                    {
                        startPosition = i;
                        break;
                    }
                }

                int endPosition = 0;
                for (int i = pixels.Length - 1; i >= startPosition; i--)
                {
                    if ((pixels[i] & 0b11000000) != whiteValue)
                    {
                        endPosition = i;
                        break;
                    }
                }

                startPosition &= ~0b11;
                endPosition = Math.Min(pixels.Length - 1, (endPosition + 3) / 4 * 4);
                pixels = pixels.Slice(startPosition, endPosition - startPosition);

                int index = 5;
                Span<byte> span = _writer.GetSpan(5 + (pixels.Length + 3) / 4);
                span[0] = 0x40;
                BinaryPrimitives.WriteUInt16BigEndian(span.Slice(1), (ushort)(startPosition / 4));
                BinaryPrimitives.WriteUInt16BigEndian(span.Slice(3), (ushort)((pixels.Length + 3) / 4));

                while (pixels.Length >= 4)
                {
                    span[index++] = (byte)((pixels[0] & 0b11000000) | ((pixels[1] & 0b11000000) >> 2) | ((pixels[2] & 0b11000000) >> 4) | ((pixels[3] & 0b11000000) >> 6));
                    pixels = pixels.Slice(4);
                }

                switch (pixels.Length)
                {
                    case 1:
                        span[index++] = (byte)((pixels[0] & 0b11000000));
                        break;
                    case 2:
                        span[index++] = (byte)((pixels[0] & 0b11000000) | ((pixels[1] & 0b11000000) >> 2));
                        break;
                    case 3:
                        span[index++] = (byte)((pixels[0] & 0b11000000) | ((pixels[1] & 0b11000000) >> 2) | ((pixels[2] & 0b11000000) >> 4));
                        break;
                }

                _writer.Advance(index);
            }

            private void EncodeRunLength(ReadOnlySpan<byte> pixels, int whiteValue, int outputLength)
            {
                Debug.Assert(whiteValue == 0 || whiteValue == 0b11000000);

                if (outputLength == 0)
                {
                    return;
                }

                int index = 0;
                Span<byte> span = _writer.GetSpan(outputLength);

                int lastPixel = whiteValue;
                int runLength = 0;

                for (int i = 0; i < pixels.Length; i++)
                {
                    int pixel = pixels[i] & 0b11000000;
                    if (pixel == lastPixel)
                    {
                        runLength++;
                    }
                    else
                    {
                        while (runLength >= 0b111111)
                        {
                            span[index++] = (byte)(lastPixel | 0b111111);
                            runLength -= 0b111111;
                        }
                        if (runLength != 0)
                        {
                            span[index++] = (byte)(lastPixel | runLength);
                        }
                        lastPixel = pixel;
                        runLength = 1;
                    }
                }

                while (runLength >= 0b111111)
                {
                    span[index++] = (byte)(lastPixel | 0b111111);
                    runLength -= 0b111111;
                }
                if (runLength != 0)
                {
                    span[index++] = (byte)(lastPixel | runLength);
                }
                _writer.Advance(index);
            }


        }
    }
}
