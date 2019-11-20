using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace TiffLibrary.Compression
{
    internal class CcittTwoDimensionalDecodingTable
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

        private static CcittTwoDimensionalDecodingTable s_instance;
        public static CcittTwoDimensionalDecodingTable Instance => (s_instance is null) ? s_instance = new CcittTwoDimensionalDecodingTable() : s_instance;

        public const int PeekCount = 12;
        private const int LookupTableTotalBitCount = 7;

        private readonly Entry[] _entries;

        private void AddCode(CodeType type, int code, int codeLength)
        {
            var entry = new Entry(type, codeLength);
            int freeBitCount = LookupTableTotalBitCount - codeLength;
            code = code << freeBitCount;
            for (int i = 0; i < (1 << freeBitCount); i++)
            {
                _entries[code + i] = entry;
            }
        }

        public CcittTwoDimensionalDecodingTable()
        {
            _entries = new Entry[1 << LookupTableTotalBitCount];

            AddCode(CodeType.Pass, 0b0001, 4);
            AddCode(CodeType.Horizontal, 0b001, 3);
            AddCode(CodeType.Vertical0, 0b1, 1);
            AddCode(CodeType.VerticalR1, 0b011, 3);
            AddCode(CodeType.VerticalR2, 0b000011, 6);
            AddCode(CodeType.VerticalR3, 0b0000011, 7);
            AddCode(CodeType.VerticalL1, 0b010, 3);
            AddCode(CodeType.VerticalL2, 0b000010, 6);
            AddCode(CodeType.VerticalL3, 0b0000010, 7);
            AddCode(CodeType.Extensions2D, 0b0000001, 7);
            AddCode(CodeType.Extensions1D, 0b0000000, 7);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryLookup(int code12, out Entry entry)
        {
            entry = _entries[code12 >> (PeekCount - LookupTableTotalBitCount)];
            return entry.Type == CodeType.None ? false : entry.Type < CodeType.Extensions1D ? true : TryHandleExtensionsCode(code12, ref entry);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool TryHandleExtensionsCode(int code, ref Entry entry)
        {
            code = code & 0b11111;
            if (entry.Type == CodeType.Extensions2D)
            {
                entry = new Entry(CodeType.Extensions2D, 10, code >> 2);
                return true;
            }
            else if (entry.Type == CodeType.Extensions1D)
            {
                if ((code & 0b11000) == 0b01000)
                {
                    entry = new Entry(CodeType.Extensions1D, 12, code & 0b111);
                    return false;
                }
            }
            return false;
        }

    }
}
