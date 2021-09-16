using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TiffLibrary.Utils;

namespace TiffLibrary
{
    internal ref struct BitReader
    {
        private ReadOnlySpan<byte> _source;
        private int _totalConsumedBits;
        private ulong _bitsBuffer;
        private byte _availableBufferBits;
        private bool _higherOrderBitsFirst;

        public int ConsumedBits => _totalConsumedBits;
        public int ConsumedBytes => (_totalConsumedBits + 7) / 8;
        public int RemainingBits => _source.Length * 8 + _availableBufferBits;

        public BitReader(ReadOnlySpan<byte> source, bool higherOrderBitsFirst = true)
        {
            _source = source;
            _totalConsumedBits = 0;
            _bitsBuffer = 0;
            _availableBufferBits = 0;
            _higherOrderBitsFirst = higherOrderBitsFirst;
        }

        public bool EnsureBitsAvailable(int bitsCount)
        {
            if ((uint)bitsCount > 32)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(bitsCount));
            }
            if (_availableBufferBits >= bitsCount)
            {
                return true;
            }
            Debug.Assert(_availableBufferBits < 32);
            FillBufferSlow(4);
            return _availableBufferBits >= bitsCount;
        }

        public uint Peek(int bitsCount)
        {
            if ((uint)bitsCount > 32)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(bitsCount));
            }
            int availableBufferBits = _availableBufferBits;
            ulong value = _bitsBuffer & (((ulong)1 << availableBufferBits) - 1);
            if (bitsCount <= availableBufferBits)
            {
                int shiftCount = availableBufferBits - bitsCount;
                value = value >> shiftCount;
                return (uint)value;
            }
            bitsCount -= availableBufferBits;
            ulong buffer = PeekBuffer();
            return (uint)(value << bitsCount | (buffer >> (64 - bitsCount)));
        }

        private ulong PeekBuffer()
        {
            if (_higherOrderBitsFirst && BinaryPrimitives.TryReadUInt64BigEndian(_source, out ulong bitsBuffer))
            {
                return bitsBuffer;
            }
            return PeekBufferSlow();
        }

        private ulong PeekBufferSlow()
        {
            ulong bitsBuffer = 0;
            ReadOnlySpan<byte> source = _source;
            if (_higherOrderBitsFirst)
            {
                for (int i = 0; i < 8; i++)
                {
                    bitsBuffer = bitsBuffer << 8;
                    if (!source.IsEmpty)
                    {
                        bitsBuffer |= source[0];
                        source = source.Slice(1);
                    }
                }
            }
            else
            {
                for (int i = 0; i < 8; i++)
                {
                    bitsBuffer = bitsBuffer << 8;
                    if (!source.IsEmpty)
                    {
                        bitsBuffer |= ReverseBits(source[0]);
                        source = source.Slice(1);
                    }
                }
            }
            return bitsBuffer;
        }

        public uint Read(int bitsCount)
        {
            if ((uint)bitsCount > 32)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(bitsCount));
            }
            int availableBufferBits = _availableBufferBits;
            ulong value = _bitsBuffer & (((ulong)1 << availableBufferBits) - 1);
            if (bitsCount <= availableBufferBits)
            {
                int shiftCount = availableBufferBits - bitsCount;
                value = value >> shiftCount;
                _availableBufferBits -= (byte)bitsCount;
                _totalConsumedBits += bitsCount;
                return (uint)value;
            }
            _totalConsumedBits += availableBufferBits;
            bitsCount -= availableBufferBits;
            FillBuffer();
            if (_availableBufferBits < bitsCount)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(bitsCount));
            }
            value = value << bitsCount | (_bitsBuffer >> (_availableBufferBits - bitsCount));
            _availableBufferBits -= (byte)bitsCount;
            _totalConsumedBits += bitsCount;
            return (uint)value;
        }

        private void FillBuffer()
        {
            if (_higherOrderBitsFirst && BinaryPrimitives.TryReadUInt64BigEndian(_source, out _bitsBuffer))
            {
                _source = _source.Slice(8);
                _availableBufferBits = 64;
                return;
            }
            _availableBufferBits = 0;
            FillBufferSlow(8);
        }

        private void FillBufferSlow(int byteCount)
        {
            Debug.Assert(0 < byteCount && byteCount <= 8);

            ulong bitsBuffer = _bitsBuffer;
            ReadOnlySpan<byte> source = _source;
            int availableBufferBits = _availableBufferBits;
            if (_higherOrderBitsFirst)
            {
                for (int i = 0; i < byteCount; i++)
                {
                    if (source.IsEmpty)
                    {
                        break;
                    }
                    bitsBuffer = (bitsBuffer << 8) | source[0];
                    source = source.Slice(1);
                    availableBufferBits += 8;
                }
            }
            else
            {
                for (int i = 0; i < byteCount; i++)
                {
                    if (source.IsEmpty)
                    {
                        break;
                    }
                    bitsBuffer = (bitsBuffer << 8) | ReverseBits(source[0]);
                    source = source.Slice(1);
                    availableBufferBits += 8;
                }
            }
            _bitsBuffer = bitsBuffer;
            _source = source;
            _availableBufferBits = (byte)availableBufferBits;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(int bitsCount)
        {
            if (bitsCount < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(bitsCount));
            }
            if (bitsCount <= _availableBufferBits)
            {
                _availableBufferBits -= (byte)bitsCount;
                _totalConsumedBits += bitsCount;
                return;
            }
            AdvanceSlow(bitsCount);
        }

        private void AdvanceSlow(int bitsCount)
        {
            Debug.Assert(bitsCount > _availableBufferBits);
            bitsCount -= _availableBufferBits;
            _totalConsumedBits += _availableBufferBits;
            int qwordCount = TiffMathHelper.DivRem64(bitsCount, out int remainingBits);
            if (_source.Length <= 8 * qwordCount)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(bitsCount));
            }
            _source = _source.Slice(8 * qwordCount);
            _totalConsumedBits += 64 * qwordCount;
            _availableBufferBits = 0;
            FillBufferSlow(8);
            if (_availableBufferBits < remainingBits)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(bitsCount));
            }
            _availableBufferBits -= (byte)remainingBits;
            _totalConsumedBits += remainingBits;
        }

        public void AdvanceAlignByte()
        {
            int oldAvailableBufferBits = _availableBufferBits;
            int availableBufferBits = (int)((uint)oldAvailableBufferBits / 8 * 8);
            _totalConsumedBits += oldAvailableBufferBits - availableBufferBits;
            _availableBufferBits = (byte)availableBufferBits;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte ReverseBits(byte b)
        {
            // http://graphics.stanford.edu/~seander/bithacks.html#ReverseByteWith64Bits
            return (byte)(((b * 0x80200802UL) & 0x0884422110UL) * 0x0101010101UL >> 32);
        }
    }
}
