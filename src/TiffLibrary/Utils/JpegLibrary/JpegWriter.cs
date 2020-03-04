#nullable enable

using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;

namespace JpegLibrary
{
    internal ref struct JpegWriter
    {
        private readonly IBufferWriter<byte> _writer;
        private readonly int _minimumBufferSize;
        private Span<byte> _buffer;
        private int _bufferConsunmed;

        private ulong _register; // left-justified bit buffer
        private bool _bitMode;
        private byte _bitsInRegister;

        public JpegWriter(IBufferWriter<byte> writer, int minimumBufferSize)
        {
            _writer = writer;
            _buffer = default;
            _bufferConsunmed = default;
            _register = default;
            _bitMode = default;
            _bitsInRegister = default;
            _minimumBufferSize = minimumBufferSize;
        }

        private void EnsureBuffer()
        {
            if (_writer is null)
            {
                throw new InvalidOperationException("Writer is not initialized.");
            }

            if (_buffer.IsEmpty)
            {
                Flush();

                _buffer = _writer.GetSpan();
            }
        }

        private void EnsureBuffer(int byteCount)
        {
            if (_writer is null)
            {
                throw new InvalidOperationException("Writer is not initialized.");
            }

            if (_buffer.Length < byteCount)
            {
                Flush();

                _buffer = _writer.GetSpan(Math.Max(_minimumBufferSize, byteCount));
            }
        }

        public void Flush()
        {
            if (_writer is null)
            {
                throw new InvalidOperationException("Writer is not initialized.");
            }

            FlushBuffer();

            if (_bitMode)
            {
                if (_buffer.Length < 16)
                {
                    _buffer = _writer.GetSpan(Math.Max(_minimumBufferSize, 16));
                }

                FlushRegister();
            }
        }

        private void FlushBuffer()
        {
            Debug.Assert(!(_writer is null));

            if (_bufferConsunmed != 0)
            {
                _writer!.Advance(_bufferConsunmed);
            }
            _bufferConsunmed = 0;
        }

        private void FlushRegister()
        {
            Debug.Assert(_bitMode);
            Debug.Assert(_buffer.Length >= 16);

            while (_bitsInRegister >= 8)
            {
                byte b = (byte)(_register >> 56);
                _register <<= 8;
                _bitsInRegister -= 8;
                if (b == 0xff)
                {
                    _buffer[1] = 0;
                    _buffer[0] = 0xff;
                    _buffer = _buffer.Slice(2);
                    _bufferConsunmed += 2;
                }
                else
                {
                    _buffer[0] = b;
                    _buffer = _buffer.Slice(1);
                    _bufferConsunmed++;
                }
            }
        }

        public void EnterBitMode()
        {
            _bitMode = true;
        }

        public void ExitBitMode()
        {
            if (_bitMode)
            {
                Flush();

                if (_bitsInRegister > 0)
                {
                    Debug.Assert(_bitsInRegister < 8);
                    _register |= (((ulong)1 << (8 - _bitsInRegister)) - 1) << 56;
                    _bitsInRegister = 8;

                    FlushRegister();
                }

                _bitMode = false;
            }
        }

        public Span<byte> GetSpan(int length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }
            if (_bitMode)
            {
                throw new InvalidOperationException();
            }
            EnsureBuffer(length);
            return _buffer.Slice(0, length);
        }

        public void Advance(int length)
        {
            if ((uint)length > (uint)_buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }
            _buffer = _buffer.Slice(length);
            _bufferConsunmed += length;
        }

        // bits: right justified bits
        public void WriteBits(uint bits, int bitLength)
        {
            if ((uint)bitLength > 32u)
            {
                throw new ArgumentOutOfRangeException(nameof(bitLength));
            }

            if (!_bitMode)
            {
                throw new InvalidOperationException("Bit mode is not enabled.");
            }

            if (_bitsInRegister > 32)
            {
                Flush();
            }

            ulong bits64 = ((ulong)bits) << (64 - _bitsInRegister - bitLength);
            _register |= bits64;
            _bitsInRegister += (byte)bitLength;
        }

        public void WriteBytes(ReadOnlySequence<byte> bytes)
        {
            if (_bitMode)
            {
                throw new InvalidOperationException("When bit mode is enabled, you are not allowed to write bytes to the stream.");
            }

            while (!bytes.IsEmpty)
            {
#if NO_READONLYSEQUENCE_FISTSPAN
                ReadOnlySpan<byte> segment = bytes.First.Span;
#else
                ReadOnlySpan<byte> segment = bytes.FirstSpan;
#endif
                bytes = bytes.Slice(segment.Length);

                while (!segment.IsEmpty)
                {
                    EnsureBuffer();

                    int count = Math.Min(_buffer.Length, segment.Length);
                    segment.Slice(0, count).CopyTo(_buffer);
                    segment = segment.Slice(count);
                    _buffer = _buffer.Slice(count);
                    _bufferConsunmed += count;
                }
            }

        }

        public void WriteBytes(ReadOnlySpan<byte> bytes)
        {
            if (_bitMode)
            {
                throw new InvalidOperationException("When bit mode is enabled, you are not allowed to write bytes to the stream.");
            }
            while (!bytes.IsEmpty)
            {
                EnsureBuffer();

                int count = Math.Min(_buffer.Length, bytes.Length);
                bytes.Slice(0, count).CopyTo(_buffer);
                bytes = bytes.Slice(count);
                _buffer = _buffer.Slice(count);
                _bufferConsunmed += count;
            }
        }

        public void WriteMarker(JpegMarker marker)
        {
            EnsureBuffer(2);

            Span<byte> buffer = _buffer;
            buffer[1] = (byte)marker;
            buffer[0] = 0xff;
            _buffer = buffer.Slice(2);
            _bufferConsunmed += 2;
        }

        public void WriteLength(ushort length)
        {
            EnsureBuffer(2);

            BinaryPrimitives.WriteUInt16BigEndian(_buffer, (ushort)(length + 2));
            _buffer = _buffer.Slice(2);
            _bufferConsunmed += 2;
        }

    }
}
