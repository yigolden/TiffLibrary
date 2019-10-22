using System;

namespace JpegLibrary
{
    internal static class JpegStandardQuantizationTable
    {
        private static readonly ushort[] s_luminanceTable = new ushort[]
        {
            16, 11, 12, 14, 12, 10, 16, 14,
            13, 14, 18, 17, 16, 19, 24, 40,
            26, 24, 22, 22, 24, 49, 35, 37,
            29, 40, 58, 51, 61, 60, 57, 51,
            56, 55, 64, 72, 92, 78, 64, 68,
            87, 69, 55, 56, 80, 109, 81, 87,
            95, 98, 103, 104, 103, 62, 77, 113,
            121, 112, 100, 120, 92, 101, 103, 99
        };

        private static readonly ushort[] s_chrominanceTable = new ushort[]
        {
            17, 18, 18, 24, 21, 24, 47, 26,
            26, 47, 99, 66, 56, 66, 99, 99,
            99, 99, 99, 99, 99, 99, 99, 99,
            99, 99, 99, 99, 99, 99, 99, 99,
            99, 99, 99, 99, 99, 99, 99, 99,
            99, 99, 99, 99, 99, 99, 99, 99,
            99, 99, 99, 99, 99, 99, 99, 99,
            99, 99, 99, 99, 99, 99, 99, 99
        };

        public static JpegQuantizationTable GetLuminanceTable(JpegElementPrecision elementPrecision, byte identifier)
        {
            return new JpegQuantizationTable((byte)elementPrecision, identifier, s_luminanceTable);
        }

        public static JpegQuantizationTable GetChrominanceTable(JpegElementPrecision elementPrecision, byte identifier)
        {
            return new JpegQuantizationTable((byte)elementPrecision, identifier, s_chrominanceTable);
        }

        public static JpegQuantizationTable ScaleByQuality(JpegQuantizationTable quantizationTable, int quality)
        {
            if (quantizationTable.IsEmpty)
            {
                throw new ArgumentException("Quantization table is not initialized.", nameof(quantizationTable));
            }
            if ((uint)quality > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(quality));
            }

            int scale = quality < 50 ? 5000 / quality : 200 - (quality * 2);

            ReadOnlySpan<ushort> source = quantizationTable.Elements;
            ushort[] elements = new ushort[64];
            for (int i = 0; i < elements.Length; i++)
            {
                int x = source[i];
                x = ((x * scale) + 50) / 100;
                elements[i] = (ushort)JpegMathHelper.Clamp(x, 1, 255);
            }

            return new JpegQuantizationTable(quantizationTable.ElementPrecision, quantizationTable.Identifier, elements);
        }
    }
}
