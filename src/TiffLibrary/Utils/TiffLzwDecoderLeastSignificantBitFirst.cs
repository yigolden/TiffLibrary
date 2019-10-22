// Modified from https://github.com/SixLabors/ImageSharp/blob/9b8d160faf839e681615924a7644e0e8024c99c2/src/ImageSharp/Formats/Gif/LzwDecoder.cs

using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TiffLibrary.Utils
{
    internal ref struct TiffLzwDecoderLeastSignificantBitFirst
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
            _allocatedBuffer = ArrayPool<byte>.Shared.Rent(TotalCodes * (sizeof(byte) + sizeof(byte) + sizeof(short)));
            _reverseBuffer = _allocatedBuffer.AsSpan(0, TotalCodes);
            _terminators = _allocatedBuffer.AsSpan(TotalCodes, TotalCodes);
            _prefixes = MemoryMarshal.Cast<byte, short>(_allocatedBuffer.AsSpan(2 * TotalCodes));
        }

        public int Decode(ReadOnlySpan<byte> source, Span<byte> destination)
        {
            int sourceOffset = 0;
            int destinationOffset = 0;

            int bits = 0;
            uint shifter = 0;

            int codeSize = 9;
            int codeMask = (1 << codeSize) - 1;

            int top = 0;
            int first = 0;
            int availableCode = EndOfInformation;

            int code;
            int oldCode = NullCode;

            ref short prefixRef = ref MemoryMarshal.GetReference(_prefixes);
            ref byte suffixRef = ref MemoryMarshal.GetReference(_terminators);
            ref byte pixelStackRef = ref MemoryMarshal.GetReference(_reverseBuffer);
            ref byte pixelsRef = ref MemoryMarshal.GetReference(destination);

            for (code = 0; code < ClearCode; code++)
            {
                Unsafe.Add(ref suffixRef, code) = (byte)code;
            }

            while (destinationOffset < destination.Length)
            {
                if (top == 0)
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
                        shifter |= (uint)source[sourceOffset++] << bits;
                        bits += 8;
                    }

                    // extract current code
                    code = (int)shifter & codeMask;
                    shifter >>= codeSize;
                    bits -= codeSize;

                    // Interpret the code
                    if (code > availableCode || code == EndOfInformation)
                    {
                        break;
                    }

                    if (code == ClearCode)
                    {
                        codeSize = 9;
                        codeMask = (1 << codeSize) - 1;
                        availableCode = FirstString;
                        oldCode = NullCode;
                        continue;
                    }

                    if (oldCode == NullCode)
                    {
                        Unsafe.Add(ref pixelStackRef, top++) = Unsafe.Add(ref suffixRef, code);
                        oldCode = code;
                        first = code;
                        continue;
                    }

                    int inCode = code;
                    if (code == availableCode)
                    {
                        Unsafe.Add(ref pixelStackRef, top++) = (byte)first;

                        code = oldCode;
                    }

                    while (code > ClearCode)
                    {
                        Unsafe.Add(ref pixelStackRef, top++) = Unsafe.Add(ref suffixRef, code);
                        code = Unsafe.Add(ref prefixRef, code);
                    }

                    int suffixCode = Unsafe.Add(ref suffixRef, code);
                    first = suffixCode;
                    Unsafe.Add(ref pixelStackRef, top++) = (byte)suffixCode;

                    if (availableCode < 4096)
                    {
                        Unsafe.Add(ref prefixRef, availableCode) = (short)oldCode;
                        Unsafe.Add(ref suffixRef, availableCode) = (byte)first;
                        availableCode++;
                        if (availableCode == codeMask + 1 && availableCode < 4096)
                        {
                            codeSize++;
                            codeMask = (1 << codeSize) - 1;
                        }
                    }

                    oldCode = inCode;
                }

                top--;

                Unsafe.Add(ref pixelsRef, destinationOffset++) = Unsafe.Add(ref pixelStackRef, top);
            }

            return destinationOffset;
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
