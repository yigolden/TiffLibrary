using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace TiffLibrary.Compression
{
    internal static class CcittCodeLookupTableTwoDimensional
    {
        public enum CodeType : byte
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
        public readonly struct Entry
        {
            private readonly ushort _value;

            public Entry(int value)
            {
                _value = (ushort)value;
            }

            public Entry(CodeType type, int bitsRequired, int extensionBits = 0)
            {
                _value = (ushort)((byte)type | (bitsRequired & 0b1111) << 8 | (extensionBits & 0b111) << 11);
            }

            public CodeType Type => (CodeType)(_value & 0b11111111);
            public int BitsRequired => (_value >> 8 & 0b1111);
            public int ExtensionBits => (_value >> 11 & 0b111);
        }

        public const int PeekCount = 12;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entry Lookup(int code)
        {
            Entry entry = s_Entries[code >> (PeekCount - LookupTableTotalBitCount)];
            if (entry.Type < CodeType.Extensions1D)
            {
                return entry;
            }
            return HandleExtensionsCode(code, entry);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Entry HandleExtensionsCode(int code, Entry entry)
        {
            code = code & 0b11111;
            if (entry.Type == CodeType.Extensions1D)
            {
                return new Entry(CodeType.Extensions2D, 10, code >> 2);
            }
            else
            {
                if ((code & 0b11000) == 0b01000)
                {
                    return new Entry(CodeType.Extensions1D, 12, code & 0b111);
                }
            }
            return default;
        }

        private const int LookupTableTotalBitCount = 7;
        static CcittCodeLookupTableTwoDimensional()
        {
            FillLookupTable(0b0001, new Entry(CodeType.Pass, 4));
            FillLookupTable(0b001, new Entry(CodeType.Horizontal, 3));
            FillLookupTable(0b1, new Entry(CodeType.Vertical0, 1));
            FillLookupTable(0b011, new Entry(CodeType.VerticalR1, 3));
            FillLookupTable(0b000011, new Entry(CodeType.VerticalR2, 6));
            FillLookupTable(0b0000011, new Entry(CodeType.VerticalR3, 7));
            FillLookupTable(0b010, new Entry(CodeType.VerticalL1, 3));
            FillLookupTable(0b000010, new Entry(CodeType.VerticalL2, 6));
            FillLookupTable(0b0000010, new Entry(CodeType.VerticalL3, 7));
            FillLookupTable(0b0000001, new Entry(CodeType.Extensions2D, 7));
            FillLookupTable(0b0000000, new Entry(CodeType.Extensions1D, 7));
        }

        private static void FillLookupTable(int code, Entry entry)
        {
            int bitCount = entry.BitsRequired;
            int freeBitCount = LookupTableTotalBitCount - bitCount;
            code = code << freeBitCount;
            for (int i = 0; i < (1 << freeBitCount); i++)
            {
                s_Entries[code + i] = entry;
            }
        }

        private static readonly Entry[] s_Entries = new Entry[1 << LookupTableTotalBitCount];
    }
}
