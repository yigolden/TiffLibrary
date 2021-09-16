using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace TiffLibrary
{
    internal ref struct BitWriter2
    {
        private readonly IBufferWriter<byte> _writer;
        private readonly int _minimumBufferSize;
        private readonly bool _higherOrderBitsFirst;
        private Span<byte> _buffer;
        private int _bufferConsunmed;

        private ulong _register; // left-justified bit buffer
        private byte _bitsInRegister;

        public BitWriter2(IBufferWriter<byte> writer, int minimumBufferSize, bool higherOrderBitsFirst = true)
        {
            _writer = writer;
            _buffer = default;
            _bufferConsunmed = default;
            _register = default;
            _bitsInRegister = default;
            _minimumBufferSize = minimumBufferSize;
            _higherOrderBitsFirst = higherOrderBitsFirst;
        }

        public void Flush()
        {
            if (_writer is null)
            {
                ThrowHelper.ThrowInvalidOperationException("Writer is not initialized.");
            }

            FlushBuffer();

            if (_buffer.Length < 16)
            {
                _buffer = _writer.GetSpan(Math.Max(_minimumBufferSize, 16));
            }

            FlushRegister();

            FlushBuffer();
        }

        private void FlushInternal()
        {
            if (_writer is null)
            {
                ThrowHelper.ThrowInvalidOperationException("Writer is not initialized.");
            }

            FlushBuffer();

            if (_buffer.Length < 16)
            {
                _buffer = _writer.GetSpan(Math.Max(_minimumBufferSize, 16));
            }

            FlushRegister();
        }

        private void FlushBuffer()
        {
            Debug.Assert(_writer is not null);

            if (_bufferConsunmed != 0)
            {
                _writer!.Advance(_bufferConsunmed);
            }
            _bufferConsunmed = 0;
        }

        private void FlushRegister()
        {
            Debug.Assert(_buffer.Length >= 16);

            if (_higherOrderBitsFirst)
            {
                while (_bitsInRegister >= 8)
                {
                    byte b = (byte)(_register >> 56);
                    _register <<= 8;
                    _bitsInRegister -= 8;
                    _buffer[0] = b;
                    _buffer = _buffer.Slice(1);
                    _bufferConsunmed++;
                }
            }
            else
            {
                while (_bitsInRegister >= 8)
                {
                    byte b = (byte)(_register >> 56);
                    _register <<= 8;
                    _bitsInRegister -= 8;
                    _buffer[0] = ReverseBits(b);
                    _buffer = _buffer.Slice(1);
                    _bufferConsunmed++;
                }
            }
        }

        // bits: right justified bits
        public void Write(uint bits, int bitLength)
        {
            if ((uint)bitLength > 32u)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(bitLength));
            }

            if (_bitsInRegister > 32)
            {
                FlushInternal();
            }

            ulong bits64 = ((ulong)bits) << (64 - _bitsInRegister - bitLength);
            _register |= bits64;
            _bitsInRegister += (byte)bitLength;
        }

        public void AdvanceAlignByte()
        {
            int remainingBits = _bitsInRegister - _bitsInRegister / 8 * 8;
            if (remainingBits != 0)
            {
                Write(0, 8 - remainingBits);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte ReverseBits(byte b)
        {
            // http://graphics.stanford.edu/~seander/bithacks.html#ReverseByteWith64Bits
            return (byte)(((b * 0x80200802UL) & 0x0884422110UL) * 0x0101010101UL >> 32);
        }
    }
}
