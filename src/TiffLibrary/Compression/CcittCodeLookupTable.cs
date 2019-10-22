using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TiffLibrary.Compression
{
    internal static class CcittCodeLookupTable
    {
        internal readonly struct Entry
        {
            private readonly uint _value;

            public Entry(int value)
            {
                _value = (uint)value;
            }

            public int RunLength => (int)(_value & 0b111111111111);
            public int BitsRequired => (int)(_value >> 12 & 0b1111);
            public bool IsWhiteRun => (_value & (0b1 << 16)) != 0;
            public bool IsBlackRun => (_value & (0b1 << 17)) != 0;
            public bool IsTerminatingCode => (_value & (0b1 << 18)) != 0;
            public bool IsEol => (_value & (0b1 << 19)) != 0;
        }

        static CcittCodeLookupTable()
        {
            Unpack(s_packedWhiteEntries, s_whiteEntries);
            Unpack(s_packedBlackEntries, s_blackEntries);
        }

        private static void Unpack(uint[] source, uint[] destination)
        {
            Debug.Assert((source.Length & 1) == 0);
            Span<uint> destSpan = destination.AsSpan();
            for (int i = 0; i < source.Length; i += 2)
            {
                uint value = source[i];
                int count = (int)source[i + 1];
                destSpan.Slice(0, count).Fill(value);
                destSpan = destSpan.Slice(count);
            }
        }

        internal static ReadOnlySpan<Entry> WhiteEntries => MemoryMarshal.Cast<uint, Entry>(s_whiteEntries);
        internal static ReadOnlySpan<Entry> BlackEntries => MemoryMarshal.Cast<uint, Entry>(s_blackEntries);

        private static readonly uint[] s_whiteEntries = new uint[8192];
        private static readonly uint[] s_blackEntries = new uint[8192];


        private static readonly uint[] s_packedWhiteEntries = new uint[]
        {
            0, 2, 638976, 2, 0, 28, 243456, 4, 247744, 2, 247808, 2, 247872, 2, 247936, 2, 248000,
            2, 248064, 2, 243520, 4, 243584, 4, 248128, 2, 248192, 2, 248256, 2, 248320, 2, 360477,
            32, 360478, 32, 360493, 32, 360494, 32, 356374, 64, 356375, 64, 360495, 32, 360496, 32, 352269,
            128, 356372, 64, 360481, 32, 360482, 32, 360483, 32, 360484, 32, 360485, 32, 360486, 32, 356371,
            64, 360479, 32, 360480, 32, 352257, 128, 352268, 128, 360501, 32, 360502, 32, 356378, 64, 360487,
            32, 360488, 32, 360489, 32, 360490, 32, 360491, 32, 360492, 32, 356373, 64, 356380, 64, 360509,
            32, 360510, 32, 360511, 32, 360448, 32, 98624, 32, 98688, 32, 348170, 256, 348171, 256, 356379,
            64, 360507, 32, 360508, 32, 103872, 16, 103936, 16, 104000, 16, 104128, 16, 356370, 64, 356376,
            64, 360497, 32, 360498, 32, 360499, 32, 360500, 32, 356377, 64, 360503, 32, 360504, 32, 360505,
            32, 360506, 32, 90304, 128, 91776, 128, 98752, 32, 98816, 32, 103104, 16, 103168, 16, 98944,
            32, 98880, 32, 103232, 16, 103296, 16, 103360, 16, 103424, 16, 103488, 16, 103552, 16, 103616,
            16, 103680, 16, 103744, 16, 103808, 16, 94464, 64, 344066, 512, 344067, 512, 86144, 256, 348168,
            256, 348169, 256, 352272, 128, 352273, 128, 344068, 512, 344069, 512, 352270, 128, 352271, 128, 86080,
            256, 344070, 512, 344071, 512
        };

        private static readonly uint[] s_packedBlackEntries = new uint[]
        {
            700416, 4, 0, 28, 243456, 4, 247744, 2, 247808, 2, 247872, 2, 247936, 2, 248000, 2, 248064,
            2, 243520, 4, 243584, 4, 248128, 2, 248192, 2, 248256, 2, 248320, 2, 434194, 8, 442420,
            2, 184960, 1, 185024, 1, 185088, 1, 185152, 1, 442423, 2, 442424, 2, 185600, 1, 185664,
            1, 185728, 1, 185792, 1, 442427, 2, 442428, 2, 185856, 1, 185920, 1, 438296, 4, 438297,
            4, 185984, 1, 186048, 1, 180544, 2, 180608, 2, 180672, 2, 184832, 1, 184896, 1, 442421,
            2, 442422, 2, 185216, 1, 185280, 1, 185344, 1, 185408, 1, 185472, 1, 185536, 1, 172096,
            8, 425997, 32, 438295, 4, 442418, 2, 442419, 2, 442412, 2, 442413, 2, 442414, 2, 442415,
            2, 442425, 2, 442426, 2, 442429, 2, 180480, 2, 434192, 8, 434193, 8, 442416, 2, 442417,
            2, 442430, 2, 442431, 2, 442398, 2, 442399, 2, 442400, 2, 442401, 2, 442408, 2, 442409,
            2, 438294, 4, 425998, 32, 421898, 64, 421899, 64, 430095, 16, 180352, 2, 180416, 2, 442394,
            2, 442395, 2, 442396, 2, 442397, 2, 438291, 4, 438292, 4, 442402, 2, 442403, 2, 442404,
            2, 442405, 2, 442406, 2, 442407, 2, 438293, 4, 442410, 2, 442411, 2, 434176, 8, 421900,
            64, 417801, 128, 417800, 128, 413703, 256, 409606, 512, 409605, 512, 405505, 1024, 405508, 1024, 401411,
            2048, 401410, 2048
        };
    }
}
