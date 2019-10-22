using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace TiffLibrary
{
    internal ref struct BitWriter
    {
        private Span<byte> _destination;
        private ulong _bitsBuffer;
        private sbyte _usedBufferBits;
        private bool _higherOrderBitsFirst;

        public BitWriter(Span<byte> destination, bool higherOrderBitsFirst = true)
        {
            _destination = destination;
            _bitsBuffer = 0;
            _usedBufferBits = 0;
            _higherOrderBitsFirst = higherOrderBitsFirst;
        }

        public void Write(uint value, int bitsCount)
        {
            if ((uint)bitsCount > 32)
            {
                ThrowArgumentOutOfRangeException(nameof(bitsCount));
            }
            int availableBits = 64 - _usedBufferBits;
            value = value & (((uint)1 << bitsCount) - 1);
            if (bitsCount <= availableBits)
            {
                _bitsBuffer = _bitsBuffer << bitsCount | value;
                _usedBufferBits += (sbyte)bitsCount;
                if (_usedBufferBits == 64)
                {
                    Flush();
                }
                return;
            }
            _bitsBuffer = _bitsBuffer << availableBits | (value >> (bitsCount - availableBits));
            _usedBufferBits = 64;
            Flush();
            _usedBufferBits = (sbyte)(bitsCount - availableBits);
            _bitsBuffer = value & (((ulong)1 << _usedBufferBits) - 1);
        }

        public void Flush()
        {
            if (_higherOrderBitsFirst)
            {
                if (_usedBufferBits == 64)
                {
                    BinaryPrimitives.WriteUInt64BigEndian(_destination, _bitsBuffer);
                    _destination = _destination.Slice(8);
                    _usedBufferBits = 0;
                    return;
                }
            }
            FlushSlow();
        }

        private void FlushSlow()
        {
            var usedBufferBits = _usedBufferBits;
            var bitsBuffer = _bitsBuffer << (64 - usedBufferBits);
            if (_higherOrderBitsFirst)
            {
                while (usedBufferBits > 0)
                {
                    byte b = (byte)(bitsBuffer >> 56);
                    bitsBuffer = bitsBuffer << 8;
                    usedBufferBits -= 8;
                    _destination[0] = b;
                    _destination = _destination.Slice(1);
                }
            }
            else
            {
                while (usedBufferBits > 0)
                {
                    byte b = (byte)(bitsBuffer >> 56);
                    bitsBuffer = bitsBuffer << 8;
                    usedBufferBits -= 8;
                    _destination[0] = ReverseBits(b);
                    _destination = _destination.Slice(1);
                }
            }
            _usedBufferBits = 0;
        }

        private static void ThrowArgumentOutOfRangeException(string paramName)
        {
            throw new ArgumentOutOfRangeException(paramName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte ReverseBits(byte b)
        {
            // http://graphics.stanford.edu/~seander/bithacks.html#ReverseByteWith64Bits
            return (byte)(((b * 0x80200802UL) & 0x0884422110UL) * 0x0101010101UL >> 32);
        }
    }
}
