using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace TiffLibrary.Compression
{
    internal readonly struct CcittCode
    {
        public CcittCode(ushort bits, byte bitsRequired, byte offset)
        {
            Bits = bits;
            BitsRequired = bitsRequired;
            Offset = offset;
        }

        public ushort Bits { get; }
        public byte BitsRequired { get; }
        public byte Offset { get; }
    }

    internal readonly struct CcittCodeValue
    {
        public CcittCodeValue(ushort runLength, byte bitsRequired, bool isEndOfLine)
        {
            RunLength = runLength;
            BitsRequired = bitsRequired;
            IsEndOfLine = isEndOfLine;
        }

        public ushort RunLength { get; }
        public byte BitsRequired { get; }
        public bool IsEndOfLine { get; }

        public bool IsTerminatingCode => RunLength < 64;
    }

    internal class CcittDecodingTable
    {
        private static CcittDecodingTable? s_whiteInstance;
        private static CcittDecodingTable? s_blackInstance;

        public static CcittDecodingTable WhiteInstance => (s_whiteInstance is null) ? s_whiteInstance = new CcittDecodingTable(isWhite: true) : s_whiteInstance;
        public static CcittDecodingTable BlackInstance => (s_blackInstance is null) ? s_blackInstance = new CcittDecodingTable(isWhite: false) : s_blackInstance;

        private const int TotalCodeCount = 105;
        private const int LookupTableTotalBitCount = 9;
        private CcittCode[] _codes;
        private CcittCodeValue[] _codeValues;
        private CcittCodeValue[] _fastTable;

        private CcittDecodingTable(bool isWhite)
        {
            _codes = new CcittCode[TotalCodeCount];
            _codeValues = new CcittCodeValue[TotalCodeCount];
            _fastTable = new CcittCodeValue[1 << LookupTableTotalBitCount];

            if (isWhite)
            {
                BuildWhiteTable();
            }
            else
            {
                BuildBlackTable();
            }
            int fastTableCount = BuildFastTable();
            TrimTable(fastTableCount);
        }

        private void BuildWhiteTable()
        {
            int index = 0;
            index = AddCode(index, 0, 0b00110101, 8);
            index = AddCode(index, 1, 0b000111, 6);
            index = AddCode(index, 2, 0b0111, 4);
            index = AddCode(index, 3, 0b1000, 4);
            index = AddCode(index, 4, 0b1011, 4);
            index = AddCode(index, 5, 0b1100, 4);
            index = AddCode(index, 6, 0b1110, 4);
            index = AddCode(index, 7, 0b1111, 4);
            index = AddCode(index, 8, 0b10011, 5);
            index = AddCode(index, 9, 0b10100, 5);
            index = AddCode(index, 10, 0b00111, 5);
            index = AddCode(index, 11, 0b01000, 5);
            index = AddCode(index, 12, 0b001000, 6);
            index = AddCode(index, 13, 0b000011, 6);
            index = AddCode(index, 14, 0b110100, 6);
            index = AddCode(index, 15, 0b110101, 6);
            index = AddCode(index, 16, 0b101010, 6);
            index = AddCode(index, 17, 0b101011, 6);
            index = AddCode(index, 18, 0b0100111, 7);
            index = AddCode(index, 19, 0b0001100, 7);
            index = AddCode(index, 20, 0b0001000, 7);
            index = AddCode(index, 21, 0b0010111, 7);
            index = AddCode(index, 22, 0b0000011, 7);
            index = AddCode(index, 23, 0b0000100, 7);
            index = AddCode(index, 24, 0b0101000, 7);
            index = AddCode(index, 25, 0b0101011, 7);
            index = AddCode(index, 26, 0b0010011, 7);
            index = AddCode(index, 27, 0b0100100, 7);
            index = AddCode(index, 28, 0b0011000, 7);
            index = AddCode(index, 29, 0b00000010, 8);
            index = AddCode(index, 30, 0b00000011, 8);
            index = AddCode(index, 31, 0b00011010, 8);
            index = AddCode(index, 32, 0b00011011, 8);
            index = AddCode(index, 33, 0b00010010, 8);
            index = AddCode(index, 34, 0b00010011, 8);
            index = AddCode(index, 35, 0b00010100, 8);
            index = AddCode(index, 36, 0b00010101, 8);
            index = AddCode(index, 37, 0b00010110, 8);
            index = AddCode(index, 38, 0b00010111, 8);
            index = AddCode(index, 39, 0b00101000, 8);
            index = AddCode(index, 40, 0b00101001, 8);
            index = AddCode(index, 41, 0b00101010, 8);
            index = AddCode(index, 42, 0b00101011, 8);
            index = AddCode(index, 43, 0b00101100, 8);
            index = AddCode(index, 44, 0b00101101, 8);
            index = AddCode(index, 45, 0b00000100, 8);
            index = AddCode(index, 46, 0b00000101, 8);
            index = AddCode(index, 47, 0b00001010, 8);
            index = AddCode(index, 48, 0b00001011, 8);
            index = AddCode(index, 49, 0b01010010, 8);
            index = AddCode(index, 50, 0b01010011, 8);
            index = AddCode(index, 51, 0b01010100, 8);
            index = AddCode(index, 52, 0b01010101, 8);
            index = AddCode(index, 53, 0b00100100, 8);
            index = AddCode(index, 54, 0b00100101, 8);
            index = AddCode(index, 55, 0b01011000, 8);
            index = AddCode(index, 56, 0b01011001, 8);
            index = AddCode(index, 57, 0b01011010, 8);
            index = AddCode(index, 58, 0b01011011, 8);
            index = AddCode(index, 59, 0b01001010, 8);
            index = AddCode(index, 60, 0b01001011, 8);
            index = AddCode(index, 61, 0b00110010, 8);
            index = AddCode(index, 62, 0b00110011, 8);
            index = AddCode(index, 63, 0b00110100, 8);
            index = AddCode(index, 64, 0b11011, 5);
            index = AddCode(index, 128, 0b10010, 5);
            index = AddCode(index, 192, 0b010111, 6);
            index = AddCode(index, 256, 0b0110111, 7);
            index = AddCode(index, 320, 0b00110110, 8);
            index = AddCode(index, 384, 0b00110111, 8);
            index = AddCode(index, 448, 0b01100100, 8);
            index = AddCode(index, 512, 0b01100101, 8);
            index = AddCode(index, 576, 0b01101000, 8);
            index = AddCode(index, 640, 0b01100111, 8);
            index = AddCode(index, 704, 0b011001100, 9);
            index = AddCode(index, 768, 0b011001101, 9);
            index = AddCode(index, 832, 0b011010010, 9);
            index = AddCode(index, 896, 0b011010011, 9);
            index = AddCode(index, 960, 0b011010100, 9);
            index = AddCode(index, 1024, 0b011010101, 9);
            index = AddCode(index, 1088, 0b011010110, 9);
            index = AddCode(index, 1152, 0b011010111, 9);
            index = AddCode(index, 1216, 0b011011000, 9);
            index = AddCode(index, 1280, 0b011011001, 9);
            index = AddCode(index, 1344, 0b011011010, 9);
            index = AddCode(index, 1408, 0b011011011, 9);
            index = AddCode(index, 1472, 0b010011000, 9);
            index = AddCode(index, 1536, 0b010011001, 9);
            index = AddCode(index, 1600, 0b010011010, 9);
            index = AddCode(index, 1664, 0b011000, 6);
            index = AddCode(index, 1728, 0b010011011, 9);
            index = AddEolCode(index, 0b000000000001, 12);
            index = AddCode(index, 1792, 0b00000001000, 11);
            index = AddCode(index, 1856, 0b00000001100, 11);
            index = AddCode(index, 1920, 0b00000001101, 11);
            index = AddCode(index, 1984, 0b000000010010, 12);
            index = AddCode(index, 2048, 0b000000010011, 12);
            index = AddCode(index, 2112, 0b000000010100, 12);
            index = AddCode(index, 2176, 0b000000010101, 12);
            index = AddCode(index, 2240, 0b000000010110, 12);
            index = AddCode(index, 2304, 0b000000010111, 12);
            index = AddCode(index, 2368, 0b000000011100, 12);
            index = AddCode(index, 2432, 0b000000011101, 12);
            index = AddCode(index, 2496, 0b000000011110, 12);
            index = AddCode(index, 2560, 0b000000011111, 12);
            _ = index;
        }

        private void BuildBlackTable()
        {
            int index = 0;
            index = AddCode(index, 0, 0b0000110111, 10);
            index = AddCode(index, 1, 0b010, 3);
            index = AddCode(index, 2, 0b11, 2);
            index = AddCode(index, 3, 0b10, 2);
            index = AddCode(index, 4, 0b011, 3);
            index = AddCode(index, 5, 0b0011, 4);
            index = AddCode(index, 6, 0b0010, 4);
            index = AddCode(index, 7, 0b00011, 5);
            index = AddCode(index, 8, 0b000101, 6);
            index = AddCode(index, 9, 0b000100, 6);
            index = AddCode(index, 10, 0b0000100, 7);
            index = AddCode(index, 11, 0b0000101, 7);
            index = AddCode(index, 12, 0b0000111, 7);
            index = AddCode(index, 13, 0b00000100, 8);
            index = AddCode(index, 14, 0b00000111, 8);
            index = AddCode(index, 15, 0b000011000, 9);
            index = AddCode(index, 16, 0b0000010111, 10);
            index = AddCode(index, 17, 0b0000011000, 10);
            index = AddCode(index, 18, 0b0000001000, 10);
            index = AddCode(index, 19, 0b00001100111, 11);
            index = AddCode(index, 20, 0b00001101000, 11);
            index = AddCode(index, 21, 0b00001101100, 11);
            index = AddCode(index, 22, 0b00000110111, 11);
            index = AddCode(index, 23, 0b00000101000, 11);
            index = AddCode(index, 24, 0b00000010111, 11);
            index = AddCode(index, 25, 0b00000011000, 11);
            index = AddCode(index, 26, 0b000011001010, 12);
            index = AddCode(index, 27, 0b000011001011, 12);
            index = AddCode(index, 28, 0b000011001100, 12);
            index = AddCode(index, 29, 0b000011001101, 12);
            index = AddCode(index, 30, 0b000001101000, 12);
            index = AddCode(index, 31, 0b000001101001, 12);
            index = AddCode(index, 32, 0b000001101010, 12);
            index = AddCode(index, 33, 0b000001101011, 12);
            index = AddCode(index, 34, 0b000011010010, 12);
            index = AddCode(index, 35, 0b000011010011, 12);
            index = AddCode(index, 36, 0b000011010100, 12);
            index = AddCode(index, 37, 0b000011010101, 12);
            index = AddCode(index, 38, 0b000011010110, 12);
            index = AddCode(index, 39, 0b000011010111, 12);
            index = AddCode(index, 40, 0b000001101100, 12);
            index = AddCode(index, 41, 0b000001101101, 12);
            index = AddCode(index, 42, 0b000011011010, 12);
            index = AddCode(index, 43, 0b000011011011, 12);
            index = AddCode(index, 44, 0b000001010100, 12);
            index = AddCode(index, 45, 0b000001010101, 12);
            index = AddCode(index, 46, 0b000001010110, 12);
            index = AddCode(index, 47, 0b000001010111, 12);
            index = AddCode(index, 48, 0b000001100100, 12);
            index = AddCode(index, 49, 0b000001100101, 12);
            index = AddCode(index, 50, 0b000001010010, 12);
            index = AddCode(index, 51, 0b000001010011, 12);
            index = AddCode(index, 52, 0b000000100100, 12);
            index = AddCode(index, 53, 0b000000110111, 12);
            index = AddCode(index, 54, 0b000000111000, 12);
            index = AddCode(index, 55, 0b000000100111, 12);
            index = AddCode(index, 56, 0b000000101000, 12);
            index = AddCode(index, 57, 0b000001011000, 12);
            index = AddCode(index, 58, 0b000001011001, 12);
            index = AddCode(index, 59, 0b000000101011, 12);
            index = AddCode(index, 60, 0b000000101100, 12);
            index = AddCode(index, 61, 0b000001011010, 12);
            index = AddCode(index, 62, 0b000001100110, 12);
            index = AddCode(index, 63, 0b000001100111, 12);
            index = AddCode(index, 64, 0b0000001111, 10);
            index = AddCode(index, 128, 0b000011001000, 12);
            index = AddCode(index, 192, 0b000011001001, 12);
            index = AddCode(index, 256, 0b000001011011, 12);
            index = AddCode(index, 320, 0b000000110011, 12);
            index = AddCode(index, 384, 0b000000110100, 12);
            index = AddCode(index, 448, 0b000000110101, 12);
            index = AddCode(index, 512, 0b0000001101100, 13);
            index = AddCode(index, 576, 0b0000001101101, 13);
            index = AddCode(index, 640, 0b0000001001010, 13);
            index = AddCode(index, 704, 0b0000001001011, 13);
            index = AddCode(index, 768, 0b0000001001100, 13);
            index = AddCode(index, 832, 0b0000001001101, 13);
            index = AddCode(index, 896, 0b0000001110010, 13);
            index = AddCode(index, 960, 0b0000001110011, 13);
            index = AddCode(index, 1024, 0b0000001110100, 13);
            index = AddCode(index, 1088, 0b0000001110101, 13);
            index = AddCode(index, 1152, 0b0000001110110, 13);
            index = AddCode(index, 1216, 0b0000001110111, 13);
            index = AddCode(index, 1280, 0b0000001010010, 13);
            index = AddCode(index, 1344, 0b0000001010011, 13);
            index = AddCode(index, 1408, 0b0000001010100, 13);
            index = AddCode(index, 1472, 0b0000001010101, 13);
            index = AddCode(index, 1536, 0b0000001011010, 13);
            index = AddCode(index, 1600, 0b0000001011011, 13);
            index = AddCode(index, 1664, 0b0000001100100, 13);
            index = AddCode(index, 1728, 0b0000001100101, 13);
            index = AddEolCode(index, 0b00000000000, 11);
            index = AddCode(index, 1792, 0b00000001000, 11);
            index = AddCode(index, 1856, 0b00000001100, 11);
            index = AddCode(index, 1920, 0b00000001101, 11);
            index = AddCode(index, 1984, 0b000000010010, 12);
            index = AddCode(index, 2048, 0b000000010011, 12);
            index = AddCode(index, 2112, 0b000000010100, 12);
            index = AddCode(index, 2176, 0b000000010101, 12);
            index = AddCode(index, 2240, 0b000000010110, 12);
            index = AddCode(index, 2304, 0b000000010111, 12);
            index = AddCode(index, 2368, 0b000000011100, 12);
            index = AddCode(index, 2432, 0b000000011101, 12);
            index = AddCode(index, 2496, 0b000000011110, 12);
            index = AddCode(index, 2560, 0b000000011111, 12);
            _ = index;
        }

        private int AddCode(int index, ushort runLength, ushort codeWord, byte codeLength)
        {
            _codes[index] = new CcittCode(codeWord, codeLength, (byte)index);
            _codeValues[index] = new CcittCodeValue(runLength, codeLength, isEndOfLine: false);
            return index + 1;
        }
        private int AddEolCode(int index, ushort codeWord, byte codeLength)
        {
            _codes[index] = new CcittCode(codeWord, codeLength, (byte)index);
            _codeValues[index] = new CcittCodeValue(0, codeLength, isEndOfLine: true);
            return index + 1;
        }

        private int BuildFastTable()
        {
            Array.Sort(_codes, (x, y) => x.BitsRequired.CompareTo(y.BitsRequired));
            int count = 0;
            for (int i = 0; i < _codes.Length; i++)
            {
                CcittCode code = _codes[i];
                if (code.BitsRequired > LookupTableTotalBitCount)
                {
                    break;
                }
                FillFastCode(code, _codeValues[code.Offset]);
                count++;
            }
            return count;
        }

        private void TrimTable(int fastCodeCount)
        {
            CcittCode[] codes = _codes;
            CcittCodeValue[] codeValues = _codeValues;
            var trimmedCodes = new CcittCode[TotalCodeCount - fastCodeCount];
            var trimmedCodeValues = new CcittCodeValue[TotalCodeCount - fastCodeCount];
            for (int i = 0; i < trimmedCodes.Length; i++)
            {
                CcittCode code = codes[fastCodeCount + i];
                int offset = code.Offset;
                code = new CcittCode(code.Bits, code.BitsRequired, (byte)i);
                trimmedCodes[i] = code;
                trimmedCodeValues[i] = codeValues[offset];
            }
            _codes = trimmedCodes;
            _codeValues = trimmedCodeValues;
        }

        private void FillFastCode(CcittCode code, CcittCodeValue codeValue)
        {
            CcittCodeValue[] fastTable = _fastTable;
            int bitCount = code.BitsRequired;
            int freeBitCount = LookupTableTotalBitCount - bitCount;
            int bits = code.Bits << freeBitCount;
            for (int i = 0; i < (1 << freeBitCount); i++)
            {
                fastTable[bits + i] = codeValue;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryLookup(uint code16, out CcittCodeValue codeValue)
        {
            codeValue = _fastTable[code16 >> (16 - LookupTableTotalBitCount)];
            return codeValue.BitsRequired == 0 ? TryLookupSlow(code16, out codeValue) : true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool TryLookupSlow(uint code16, out CcittCodeValue codeValue)
        {
            CcittCode[] codes = _codes;
            for (int i = 0; i < codes.Length; i++)
            {
                CcittCode code = codes[i];
                if ((code16 >> (16 - code.BitsRequired)) == code.Bits)
                {
                    codeValue = _codeValues[code.Offset];
                    return true;
                }
            }

            codeValue = default;
            return false;
        }

        public int DecodeRun(ref BitReader reader)
        {
            int unpacked = 0;
            while (true)
            {
                // Read next code word
                if (!TryLookup(reader.Peek(16), out CcittCodeValue tableEntry))
                {
                    ThrowHelper.ThrowInvalidDataException();
                }

                if (tableEntry.IsEndOfLine)
                {
                    ThrowHelper.ThrowInvalidDataException();
                }

                // Record run length
                unpacked += tableEntry.RunLength;
                reader.Advance(tableEntry.BitsRequired);

                // Terminating code is met.
                if (tableEntry.IsTerminatingCode)
                {
                    return unpacked;
                }
            }
        }
    }
}
