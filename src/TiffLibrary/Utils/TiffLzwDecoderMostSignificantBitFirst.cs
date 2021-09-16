// Modified from https://github.com/dbry/lzw-ab/blob/master/lzw-lib.c

using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TiffLibrary.Utils
{
    internal ref struct TiffLzwDecoderMostSignificantBitFirst
    {
        private const int NullCode = -1;
        private const int ClearCode = 256;
        private const int EndOfInformation = 257;
        private const int FirstString = 258;

        private const int MaxBits = 12;
        private const int TotalCodes = 1 << MaxBits;

        private byte[] _allocatedBuffer;
        private Span<byte> _reverseBuffer;
        private Span<byte> _terminators;
        private Span<short> _prefixes;

        public void Initialize()
        {
            _allocatedBuffer = ArrayPool<byte>.Shared.Rent((TotalCodes - 256) * (sizeof(byte) + sizeof(byte) + sizeof(short)));
            _reverseBuffer = _allocatedBuffer.AsSpan(0, TotalCodes - 256);
            _terminators = _allocatedBuffer.AsSpan(TotalCodes - 256, TotalCodes - 256);
            _prefixes = MemoryMarshal.Cast<byte, short>(_allocatedBuffer.AsSpan(2 * (TotalCodes - 256)));
        }

        public int Decode(ReadOnlySpan<byte> source, Span<byte> destination)
        {
            int sourceOffset = 0;
            int destinationOffset = 0;

            int next = FirstString;
            int prefix = ClearCode;
            int bits = 0;
            uint shifter = 0;

            int codeSize = 9;
            int codeMask = (1 << codeSize) - 1;

            // This is the main loop where we read input symbols. The values range from 0 to the code value
            // of the "next" string in the dictionary (although the actual "next" code cannot be used yet,
            // and so we reserve that code for the END_CODE). Note that receiving an EOF from the input
            // stream is actually an error because we should have gotten the END_CODE first.
            while (true)
            {
                // read enough bits
                while (bits < codeSize)
                {
                    // check whether there is any data in source
                    if (sourceOffset == source.Length)
                    {
                        //throw new InvalidDataException();
                        return destinationOffset;
                    }

                    // read 8 bits
                    shifter = (shifter << 8) | source[sourceOffset++];
                    bits += 8;
                }

                // extract current code
                int code = (int)(shifter >> (bits - codeSize)) & codeMask;
                bits -= codeSize;

                if (code == EndOfInformation)
                {
                    return destinationOffset;
                }

                if (code >= next)
                {
                    ThrowHelper.ThrowInvalidDataException();
                }
                // otherwise check for a CLEAR_CODE to start over early
                if (code == ClearCode)
                {
                    next = FirstString;
                    codeSize = 9;
                    codeMask = (1 << codeSize) - 1;
                }
                // this only happens at the first symbol which is always sent
                // literally and becomes our initial prefix
                else if (prefix == ClearCode)
                {
                    destination[destinationOffset++] = (byte)code;
                    next++;
                }
                // Otherwise we have a valid prefix so we step through the string from end to beginning storing the
                // bytes in the "reverse_buffer", and then we send them out in the proper order. One corner-case
                // we have to handle here is that the string might be the same one that is actually being defined
                // now (code == next-1). Also, the first 256 entries of "terminators" and "prefixes" are fixed and
                // not allocated, so that messes things up a bit.
                else
                {
                    int cti = (code == next - 1) ? prefix : code;
                    ref byte rbp0 = ref MemoryMarshal.GetReference(_reverseBuffer);
                    ref byte rbp = ref rbp0;

                    do
                    {
                        // step backward through string...
                        rbp = cti < 256 ? (byte)cti : _terminators[cti - 256];
                        rbp = ref Unsafe.Add(ref rbp, 1);
                    } while ((cti = (cti < 256) ? NullCode : _prefixes[cti - 256]) != NullCode);

                    // the first byte in this string is the terminator for the last string, which is
                    // the one that we'll create a new dictionary entry for this time
                    rbp = ref Unsafe.Add(ref rbp, -1);
                    byte c = rbp;

                    // send string in corrected order (except for the terminator
                    // which we don't know yet)
                    do
                    {
                        destination[destinationOffset++] = rbp;
                        rbp = ref Unsafe.Add(ref rbp, -1);
                    } while (!Unsafe.IsAddressLessThan(ref rbp, ref rbp0));


                    if (code == next - 1)
                    {
                        destination[destinationOffset++] = c;
                    }

                    // now update the next dictionary entry with the new string
                    // (but we're always one behind, so it's not the string just sent)
                    _prefixes[next - 1 - 256] = (short)prefix;
                    _terminators[next - 1 - 256] = c;

                    // check for full dictionary, which forces a reset (and, BTW,
                    // means we'll never use the dictionary entry we just wrote)
                    if (++next == TotalCodes)
                    {
                        next = FirstString;
                        codeSize = 9;
                        codeMask = (1 << codeSize) - 1;
                    }
                }

                // the code we just received becomes the prefix for the next dictionary string entry
                // (which we'll create once we find out the terminator)
                prefix = code;

                if (next > codeMask)
                {
                    codeSize++;
                    codeMask = (1 << codeSize) - 1;
                }
            }
        }

        public void Dispose()
        {
            if (_allocatedBuffer != null)
            {
                ArrayPool<byte>.Shared.Return(_allocatedBuffer);
                _reverseBuffer = default;
                _terminators = default;
                _prefixes = default;
            }
        }
    }
}
