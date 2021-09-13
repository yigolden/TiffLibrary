using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TiffLibrary.Compression
{
    /// <summary>
    /// Compression and decompression support for ThunderScan 4-bit compression algorithm.
    /// </summary>
    public class ThunderScanCompressionAlgorithm : ITiffCompressionAlgorithm, ITiffDecompressionAlgorithm
    {
        /// <summary>
        /// A shared instance of <see cref="ThunderScanCompressionAlgorithm"/>. It should be used across the application.
        /// </summary>
        public static ThunderScanCompressionAlgorithm Instance { get; } = new ThunderScanCompressionAlgorithm();

        private static ReadOnlySpan<byte> TwoBitDiffDecodeTable => new byte[] { 0, 1, 0, unchecked((byte)-1) };
        private static ReadOnlySpan<byte> ThreeBitDiffDecodeTable => new byte[] { 0, 1, 2, 3, 0, unchecked((byte)-3), unchecked((byte)-2), unchecked((byte)-1) };

        /// <inheritdoc />
        public void Compress(TiffCompressionContext context, ReadOnlyMemory<byte> input, IBufferWriter<byte> outputWriter)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.PhotometricInterpretation != TiffPhotometricInterpretation.WhiteIsZero && context.PhotometricInterpretation != TiffPhotometricInterpretation.BlackIsZero)
            {
                throw new NotSupportedException("ThunderScan compression does not support this photometric interpretation.");
            }

            if (context.BitsPerSample.Count != 1 || context.BitsPerSample[0] != 8)
            {
                throw new NotSupportedException("Unsupported bits per sample.");
            }

            context.BitsPerSample = TiffValueCollection.Single<ushort>(4);

            Span<byte> buffer = stackalloc byte[4];
            var encoder = new ThunderScanEncoder(outputWriter, buffer);

            int height = context.ImageSize.Height;
            int bytesPerScanline = context.BytesPerScanline;
            ReadOnlySpan<byte> inputSpan = input.Span;

            for (int row = 0; row < height; row++)
            {
                ReadOnlySpan<byte> run = inputSpan.Slice(0, bytesPerScanline);
                inputSpan = inputSpan.Slice(bytesPerScanline);
                encoder.Encode(run);
                encoder.Reset();
            }
        }

        /// <inheritdoc />
        public int Decompress(TiffDecompressionContext context, ReadOnlyMemory<byte> input, Memory<byte> output)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.PhotometricInterpretation != TiffPhotometricInterpretation.WhiteIsZero && context.PhotometricInterpretation != TiffPhotometricInterpretation.BlackIsZero)
            {
                throw new NotSupportedException("ThunderScan compression does not support this photometric interpretation.");
            }

            if (context.BitsPerSample.Count != 1 || context.BitsPerSample[0] != 4)
            {
                throw new NotSupportedException("Unsupported bits per sample.");
            }

            ReadOnlySpan<sbyte> twoBitDiffs = MemoryMarshal.Cast<byte, sbyte>(TwoBitDiffDecodeTable);
            ReadOnlySpan<sbyte> threeBitDiffs = MemoryMarshal.Cast<byte, sbyte>(ThreeBitDiffDecodeTable);

            ReadOnlySpan<byte> inputSpan = input.Span;
            OutputWriter writer = new OutputWriter(output.Span, context.ImageSize.Width, context.ImageSize.Height);

            int delta;
            uint lastPixel = 0;
            ulong buffer = 0;
            uint bufferCount = 0;

            for (int i = 0; i < inputSpan.Length; i++)
            {
                uint inputByte = inputSpan[i];

                switch (inputByte & 0b11_000000)
                {
                    case 0b00_000000:
                        for (uint j = 0; j < inputByte; j++)
                        {
                            if (bufferCount == 16)
                            {
                                writer.Write(buffer, 16);
                                bufferCount = 0;
                            }
                            buffer = (buffer << 4) | lastPixel;
                            bufferCount++;
                        }
                        break;
                    case 0b01_000000:
                        if (bufferCount > (16 - 3))
                        {
                            writer.Write(buffer, bufferCount);
                            bufferCount = 0;
                        }
                        if ((delta = (int)((inputByte >> 4) & 0b11)) != 0b10)
                        {
                            lastPixel = (uint)((int)lastPixel + twoBitDiffs[delta]) & 0b1111;
                            buffer = (buffer << 4) | lastPixel;
                            bufferCount++;
                        }
                        if ((delta = (int)((inputByte >> 2) & 0b11)) != 0b10)
                        {
                            lastPixel = (uint)((int)lastPixel + twoBitDiffs[delta]) & 0b1111;
                            buffer = (buffer << 4) | lastPixel;
                            bufferCount++;
                        }
                        if ((delta = (int)(inputByte & 0b11)) != 0b10)
                        {
                            lastPixel = (uint)((int)lastPixel + twoBitDiffs[delta]) & 0b1111;
                            buffer = (buffer << 4) | lastPixel;
                            bufferCount++;
                        }
                        break;
                    case 0b10_000000:
                        if (bufferCount > (16 - 2))
                        {
                            writer.Write(buffer, bufferCount);
                            bufferCount = 0;
                        }
                        if ((delta = (int)((inputByte >> 3) & 0b111)) != 0b100)
                        {
                            lastPixel = (uint)((int)lastPixel + threeBitDiffs[delta]) & 0b1111;
                            buffer = (buffer << 4) | lastPixel;
                            bufferCount++;
                        }
                        if ((delta = (int)(inputByte & 0b111)) != 0b100)
                        {
                            lastPixel = (uint)((int)lastPixel + threeBitDiffs[delta]) & 0b1111;
                            buffer = (buffer << 4) | lastPixel;
                            bufferCount++;
                        }
                        break;
                    case 0b11_000000:
                        if (bufferCount == 16)
                        {
                            writer.Write(buffer, 16);
                            bufferCount = 0;
                        }
                        lastPixel = inputByte & 0b1111;
                        buffer = (buffer << 4) | lastPixel;
                        bufferCount++;
                        break;
                }
            }

            if (bufferCount != 0)
            {
                writer.Write(buffer, bufferCount);
            }
            if (!writer.RemainingSpan.IsEmpty)
            {
                writer.RemainingSpan.Clear();
            }

            return context.BytesPerScanline * context.ImageSize.Height;
        }

        ref struct OutputWriter
        {
            private readonly int _width;

            private Span<byte> _outputSpan;
            private int _pixelsInCurrentScanline;
            private int _remainingRowCount;

            public Span<byte> RemainingSpan => _outputSpan;

            public OutputWriter(Span<byte> outputSpan, int width, int height)
            {
                int bytesPerScanline = (width + 1) / 2;
                if (outputSpan.Length < (bytesPerScanline * height))
                {
                    throw new ArgumentException("Destination is too small.");
                }

                _outputSpan = outputSpan;
                _width = width;

                _pixelsInCurrentScanline = 0;
                _remainingRowCount = height;
            }

            public void Write(ulong buffer, uint count)
            {
                Debug.Assert(count <= 16);

                // left-align buffer
                buffer <<= (int)((16 - count) * 4);

                while (count > 0)
                {
                    if (_pixelsInCurrentScanline == _width)
                    {
                        EncureAvailableSpace();
                    }
                    else if ((_pixelsInCurrentScanline & 0b1) != 0)
                    {
                        _outputSpan[0] |= (byte)(buffer >> 60);
                        _outputSpan = _outputSpan.Slice(1);
                        _pixelsInCurrentScanline++;
                        buffer <<= 4;
                        count--;
                        continue;
                    }
                    int remainingBytes = (_width - _pixelsInCurrentScanline) / 2;
                    for (; count >= 2 && remainingBytes > 0; count -= 2, remainingBytes--)
                    {
                        _outputSpan[0] = (byte)(buffer >> 56);
                        _outputSpan = _outputSpan.Slice(1);
                        _pixelsInCurrentScanline += 2;
                        buffer <<= 8;
                    }
                    if (_pixelsInCurrentScanline == _width)
                    {
                        EncureAvailableSpace();
                    }
                    if (count != 0)
                    {
                        if ((_pixelsInCurrentScanline & 0b1) == 0)
                        {
                            _outputSpan[0] = (byte)((buffer >> 56) & 0xf0);
                        }
                        else
                        {
                            _outputSpan[0] |= (byte)(buffer >> 60);
                            _outputSpan = _outputSpan.Slice(1);
                        }
                        _pixelsInCurrentScanline++;
                        buffer <<= 4;
                        count--;
                    }
                }
            }

            private void EncureAvailableSpace()
            {
                Debug.Assert(_pixelsInCurrentScanline == _width);
                if ((_width & 0b1) != 0)
                {
                    _outputSpan = _outputSpan.Slice(1);
                }
                _pixelsInCurrentScanline = 0;
                _remainingRowCount--;
                if (_remainingRowCount < 0)
                {
                    throw new InvalidDataException("Too much data are unpacked.");
                }
            }
        }

        ref struct ThunderScanEncoder
        {
            private readonly IBufferWriter<byte> _writer;
            private readonly Span<sbyte> _temp;

            private State _state;
            private int _previous;
            private int _runLength;

            private Span<byte> _outputSpan;
            private int _bytesWritten;

            public ThunderScanEncoder(IBufferWriter<byte> writer, Span<byte> temp)
            {
                Debug.Assert(temp.Length >= 3);

                _writer = writer;
                _temp = MemoryMarshal.Cast<byte, sbyte>(temp);

                _state = State.None;
                _previous = 0;
                _runLength = 0;

                _outputSpan = default;
                _bytesWritten = 0;
            }

            public void Encode(ReadOnlySpan<byte> pixels)
            {
                for (int i = 0; i < pixels.Length; i++)
                {
                    int pixel = (pixels[i] & 0xf0) >> 4;
                    int diff = pixel - _previous;

                    switch (_state)
                    {
                        case State.None:
                            Emit((byte)(pixel | 0b11_000000));
                            _state = State.Symbol;
                            break;
                        case State.Symbol:
                            if (diff >= -1 && diff <= 1)
                            {
                                _temp[_runLength++] = (sbyte)diff;
                                _state = State.TwoBitDiff;
                            }
                            else if (diff >= -3 && diff <= 3)
                            {
                                _temp[_runLength++] = (sbyte)diff;
                                _state = State.ThreeBitDiff;
                            }
                            else
                            {
                                Emit((byte)(pixel | 0b11_000000));
                            }
                            break;
                        case State.TwoBitDiff:
                            if (diff >= -1 && diff <= 1)
                            {
                                _temp[_runLength++] = (sbyte)diff;

                                if (_runLength == 3)
                                {
                                    // If all the diffs are zero. We can Encode this run length using only one symbol.
                                    if (CheckIsRunLength())
                                    {
                                        _state = State.RunLength;
                                        break;
                                    }

                                    EmitTwoBitDiff();
                                    _state = State.Symbol;
                                    _runLength = 0;
                                }
                            }
                            else
                            {
                                Debug.Assert(_runLength <= 2);

                                // Flush Two Bit Diff
                                EmitTwoBitDiff();
                                _state = State.Symbol;
                                _runLength = 0;

                                // Rewind and reset the state machine.
                                i--;
                                continue;
                            }
                            break;
                        case State.ThreeBitDiff:
                            if (diff >= -3 && diff <= 3)
                            {
                                _temp[_runLength++] = (sbyte)diff;

                                Debug.Assert(_runLength == 2);

                                EmitThreeBitDiff();
                                _state = State.Symbol;
                                _runLength = 0;
                            }
                            else
                            {
                                Debug.Assert(_runLength == 1);

                                // Flush Three Bit Diff
                                EmitThreeBitDiff();
                                _state = State.Symbol;
                                _runLength = 0;

                                // Rewind and reset the state machine.
                                i--;
                                continue;
                            }
                            break;
                        case State.RunLength:
                            if (diff == 0)
                            {
                                _runLength++;
                                break;
                            }

                            // Flush run length
                            EmitRunLength();
                            _runLength = 0;

                            // Rewind and reset the state machine.
                            i--;
                            _state = State.Symbol;
                            continue;
                    }

                    _previous = pixel;
                }
            }

            public void Flush()
            {
                FlushCore();

                _runLength = 0;
                _state = State.Symbol;
            }

            public void Reset()
            {
                FlushCore();

                _state = State.None;
                _previous = 0;
                _runLength = 0;
            }

            private void FlushCore()
            {
                switch (_state)
                {
                    case State.TwoBitDiff:
                        EmitTwoBitDiff();
                        break;
                    case State.ThreeBitDiff:
                        EmitThreeBitDiff();
                        break;
                    case State.RunLength:
                        EmitRunLength();
                        break;
                }

                FlushOutput();
            }

            private static ReadOnlySpan<byte> TwoBitDiffEncodeTable => new byte[] { 0b11, 0b00, 0b01 };

            private static ReadOnlySpan<byte> ThreeBitDiffEncodeTable => new byte[] { 0b101, 0b110, 0b111, 0b000, 0b001, 0b010, 0b011 };

            private bool CheckIsRunLength()
            {
                Debug.Assert(_state == State.TwoBitDiff);
                Debug.Assert(_runLength > 1);

                for (int i = 0; i < _runLength; i++)
                {
                    if (_temp[i] != 0)
                    {
                        return false;
                    }
                }

                return true;
            }

            private void EmitTwoBitDiff()
            {
                Debug.Assert(_state == State.TwoBitDiff);
                Debug.Assert(_runLength <= 3);
                ReadOnlySpan<byte> table = TwoBitDiffEncodeTable;

                int buffer = 0b10_10_10;
                for (int i = 0; i < _runLength; i++)
                {
                    buffer = (buffer << 2) | table[_temp[i] + 1];
                }

                Emit((byte)((buffer & 0b111111) | 0b01_000000));
            }

            private void EmitThreeBitDiff()
            {
                Debug.Assert(_state == State.ThreeBitDiff);
                Debug.Assert(_runLength <= 2);
                ReadOnlySpan<byte> table = ThreeBitDiffEncodeTable;

                int buffer = 0b100_100;
                for (int i = 0; i < _runLength; i++)
                {
                    buffer = (buffer << 3) | table[_temp[i] + 3];
                }

                Emit((byte)((buffer & 0b111111) | 0b10_000000));
            }

            private void EmitRunLength()
            {
                Debug.Assert(_state == State.RunLength);
                Debug.Assert(_runLength > 0);

                int runLength = _runLength;
                while (runLength > 0b111111)
                {
                    Emit(0b00_111111);
                    runLength -= 0b111111;
                }
                if (runLength > 0)
                {
                    Emit((byte)runLength);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void Emit(byte symbol)
            {
                if (_outputSpan.IsEmpty)
                {
                    EnsureOutputSpan();
                }
                _outputSpan[0] = symbol;
                _outputSpan = _outputSpan.Slice(1);
                _bytesWritten++;
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private void EnsureOutputSpan()
            {
                if (_bytesWritten != 0)
                {
                    _writer.Advance(_bytesWritten);
                }
                _outputSpan = _writer.GetSpan();
                _bytesWritten = 0;
            }

            private void FlushOutput()
            {
                if (_bytesWritten != 0)
                {
                    _writer.Advance(_bytesWritten);
                    _bytesWritten = 0;
                }
            }

            private enum State
            {
                None,
                Symbol,
                TwoBitDiff,
                ThreeBitDiff,
                RunLength,
            }
        }
    }
}
