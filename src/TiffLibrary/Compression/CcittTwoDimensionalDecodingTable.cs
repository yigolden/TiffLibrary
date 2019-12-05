using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace TiffLibrary.Compression
{
    internal enum CcittTwoDimensionalCodeType
    {
        None,
        Pass,
        Horizontal,
        Vertical0,
        VerticalR1,
        VerticalR2,
        VerticalR3,
        VerticalL1,
        VerticalL2,
        VerticalL3,
        Extensions1D,
        Extensions2D,
    }

    [DebuggerDisplay("Type = {Type}")]
    internal readonly struct CcittTwoDimensionalCodeValue
    {
        private readonly ushort _value;

        public CcittTwoDimensionalCodeValue(int value)
        {
            _value = (ushort)value;
        }

        public CcittTwoDimensionalCodeValue(CcittTwoDimensionalCodeType type, int bitsRequired, int extensionBits = 0)
        {
            _value = (ushort)((byte)type | (bitsRequired & 0b1111) << 8 | (extensionBits & 0b111) << 11);
        }

        public CcittTwoDimensionalCodeType Type => (CcittTwoDimensionalCodeType)(_value & 0b11111111);
        public int BitsRequired => (_value >> 8 & 0b1111);
        public int ExtensionBits => (_value >> 11 & 0b111);
    }

    internal class CcittTwoDimensionalDecodingTable
    {
        private static CcittTwoDimensionalDecodingTable? s_instance;
        public static CcittTwoDimensionalDecodingTable Instance => (s_instance is null) ? s_instance = new CcittTwoDimensionalDecodingTable() : s_instance;

        public const int PeekCount = 12;
        private const int LookupTableTotalBitCount = 7;

        private readonly CcittTwoDimensionalCodeValue[] _entries;

        private void AddCode(CcittTwoDimensionalCodeType type, int code, int codeLength)
        {
            var entry = new CcittTwoDimensionalCodeValue(type, codeLength);
            int freeBitCount = LookupTableTotalBitCount - codeLength;
            code = code << freeBitCount;
            for (int i = 0; i < (1 << freeBitCount); i++)
            {
                _entries[code + i] = entry;
            }
        }

        public CcittTwoDimensionalDecodingTable()
        {
            _entries = new CcittTwoDimensionalCodeValue[1 << LookupTableTotalBitCount];

            AddCode(CcittTwoDimensionalCodeType.Pass, 0b0001, 4);
            AddCode(CcittTwoDimensionalCodeType.Horizontal, 0b001, 3);
            AddCode(CcittTwoDimensionalCodeType.Vertical0, 0b1, 1);
            AddCode(CcittTwoDimensionalCodeType.VerticalR1, 0b011, 3);
            AddCode(CcittTwoDimensionalCodeType.VerticalR2, 0b000011, 6);
            AddCode(CcittTwoDimensionalCodeType.VerticalR3, 0b0000011, 7);
            AddCode(CcittTwoDimensionalCodeType.VerticalL1, 0b010, 3);
            AddCode(CcittTwoDimensionalCodeType.VerticalL2, 0b000010, 6);
            AddCode(CcittTwoDimensionalCodeType.VerticalL3, 0b0000010, 7);
            AddCode(CcittTwoDimensionalCodeType.Extensions2D, 0b0000001, 7);
            AddCode(CcittTwoDimensionalCodeType.Extensions1D, 0b0000000, 7);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryLookup(int code12, out CcittTwoDimensionalCodeValue entry)
        {
            entry = _entries[code12 >> (PeekCount - LookupTableTotalBitCount)];
            return entry.Type == CcittTwoDimensionalCodeType.None ? false : entry.Type < CcittTwoDimensionalCodeType.Extensions1D ? true : TryHandleExtensionsCode(code12, ref entry);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool TryHandleExtensionsCode(int code, ref CcittTwoDimensionalCodeValue entry)
        {
            code = code & 0b11111;
            if (entry.Type == CcittTwoDimensionalCodeType.Extensions2D)
            {
                entry = new CcittTwoDimensionalCodeValue(CcittTwoDimensionalCodeType.Extensions2D, 10, code >> 2);
                return true;
            }
            else if (entry.Type == CcittTwoDimensionalCodeType.Extensions1D)
            {
                if ((code & 0b11000) == 0b01000)
                {
                    entry = new CcittTwoDimensionalCodeValue(CcittTwoDimensionalCodeType.Extensions1D, 12, code & 0b111);
                    return false;
                }
            }
            return false;
        }

    }
}
