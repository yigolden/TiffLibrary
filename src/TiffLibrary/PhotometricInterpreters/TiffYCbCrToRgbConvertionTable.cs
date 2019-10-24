using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using TiffLibrary.PixelFormats;

namespace TiffLibrary.PhotometricInterpreters
{
    internal sealed class TiffYCbCrToRgbConvertionTable
    {
        private const int ClampTableOffset = 256;
        private const int Shift = 16;
        private const int OneHalf = 1 << (Shift - 1);

        private byte[] _clampTable;
        private int[] _crRTable;
        private int[] _cbBTable;
        private int[] _crGTable;
        private int[] _cbGTable;
        private int[] _yTable;

        private static TiffRational[] s_defaultLuma = new TiffRational[]
        {
            new TiffRational(299, 1000),
            new TiffRational(587, 1000),
            new TiffRational(114, 1000)
        };
        private static TiffRational[] s_defaultReferenceBlackWhite = new TiffRational[]
        {
            new TiffRational(0, 1), new TiffRational(255, 1),
            new TiffRational(128, 1), new TiffRational(255, 1),
            new TiffRational(128, 1), new TiffRational(255, 1)
        };

        private static TiffYCbCrToRgbConvertionTable s_defaultTable;

        public static TiffYCbCrToRgbConvertionTable Create(TiffRational[] luma, TiffRational[] referenceBlackWhite)
        {
            Debug.Assert(luma.Length == 0 || luma.Length == 3);
            Debug.Assert(referenceBlackWhite.Length == 0 || referenceBlackWhite.Length == 6);

            bool isDefault = true;
            if (luma.Length == 0)
            {
                luma = s_defaultLuma;
            }
            else
            {
                isDefault &= luma[0].Equals(s_defaultLuma[0]) && luma[1].Equals(s_defaultLuma[1]) && luma[2].Equals(s_defaultLuma[2]);
            }

            if (referenceBlackWhite.Length == 0)
            {
                referenceBlackWhite = s_defaultReferenceBlackWhite;
            }
            else
            {
                isDefault &= referenceBlackWhite[0].Equals(s_defaultReferenceBlackWhite[0]) && referenceBlackWhite[1].Equals(s_defaultReferenceBlackWhite[1]) &&
                       referenceBlackWhite[2].Equals(s_defaultReferenceBlackWhite[2]) && referenceBlackWhite[3].Equals(s_defaultReferenceBlackWhite[3]) &&
                       referenceBlackWhite[4].Equals(s_defaultReferenceBlackWhite[4]) && referenceBlackWhite[5].Equals(s_defaultReferenceBlackWhite[5]);
            }

            TiffYCbCrToRgbConvertionTable table;
            if (isDefault)
            {
                if (s_defaultTable is null)
                {
                    table = new TiffYCbCrToRgbConvertionTable();
                    table.Init(luma, referenceBlackWhite);
                    Interlocked.CompareExchange(ref s_defaultTable, table, null);
                }
                return s_defaultTable;
            }

            table = new TiffYCbCrToRgbConvertionTable();
            table.Init(luma, referenceBlackWhite);
            return table;
        }

        private TiffYCbCrToRgbConvertionTable()
        {
            _clampTable = new byte[4 * 256];
            _crRTable = new int[256];
            _cbBTable = new int[256];
            _crGTable = new int[256];
            _cbGTable = new int[256];
            _yTable = new int[256];
        }

        /*
        * Initialize the YCbCr->RGB conversion tables.  The conversion
        * is done according to the 6.0 spec:
        *
        *    R = Y + Cr * (2 - 2 * LumaRed)
        *    B = Y + Cb * (2 - 2 * LumaBlue)
        *    G =   Y
        *        - LumaBlue * Cb * (2 - 2 * LumaBlue) / LumaGreen
        *        - LumaRed * Cr * (2 - 2 * LumaRed) / LumaGreen
        *
        * To avoid floating point arithmetic the fractional constants that
        * come out of the equations are represented as fixed point values
        * in the range 0...2^16.  We also eliminate multiplications by
        * pre-calculating possible values indexed by Cb and Cr (this code
        * assumes conversion is being done for 8-bit samples).
        */
        private void Init(TiffRational[] luma, TiffRational[] referenceBlackWhite)
        {
            byte[] clampTable = _clampTable;

            float referenceBlackWhite0 = referenceBlackWhite[0].ToSingle();
            float referenceBlackWhite1 = referenceBlackWhite[1].ToSingle();
            float referenceBlackWhite2 = referenceBlackWhite[2].ToSingle();
            float referenceBlackWhite3 = referenceBlackWhite[3].ToSingle();
            float referenceBlackWhite4 = referenceBlackWhite[4].ToSingle();
            float referenceBlackWhite5 = referenceBlackWhite[5].ToSingle();

            for (int i = 0; i < 256; i++)
            {
                clampTable[ClampTableOffset + i] = (byte)i;
            }

            int start = ClampTableOffset + 256;
            int stop = start + 2 * 256;

            for (int i = start; i < stop; i++)
            {
                clampTable[i] = 255;
            }

            float lumaRed = luma[0].ToSingle();
            float lumaGreen = luma[1].ToSingle();
            float lumaBlue = luma[2].ToSingle();

            float f1 = 2 - 2 * lumaRed;
            int d1 = Fix(f1);

            float f2 = lumaRed * f1 / lumaGreen;
            int d2 = -Fix(f2);

            float f3 = 2 - 2 * lumaBlue;
            int d3 = Fix(f3);

            float f4 = lumaBlue * f3 / lumaGreen;
            int d4 = -Fix(f4);

            /*
            * i is the actual input pixel value in the range 0..255
            * Cb and Cr values are in the range -128..127 (actually
            * they are in a range defined by the ReferenceBlackWhite
            * tag) so there is some range shifting to do here when
            * constructing tables indexed by the raw pixel data.
            */
            for (int i = 0, x = -128; i < 256; i++, x++)
            {
                int cr = Code2V(x, referenceBlackWhite4 - 128.0f, referenceBlackWhite5 - 128.0f, 127);
                int cb = Code2V(x, referenceBlackWhite2 - 128.0f, referenceBlackWhite3 - 128.0f, 127);

                _crRTable[i] = (d1 * cr + OneHalf) >> Shift;
                _cbBTable[i] = (d3 * cb + OneHalf) >> Shift;
                _crGTable[i] = d2 * cr;
                _cbGTable[i] = d4 * cb + OneHalf;
                _yTable[i] = Code2V(x + 128, referenceBlackWhite0, referenceBlackWhite1, 255);
            }
        }

        private static int Fix(float x)
        {
            return (int)(x * (1L << Shift) + 0.5);
        }

        private static int Code2V(int c, float RB, float RW, float CR)
        {
            return (int)(((c - (int)RB) * CR) / ((int)(RW - RB) != 0 ? (RW - RB) : 1.0f));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TiffRgba32 Convert(byte y, byte cb, byte cr)
        {
            byte[] clampTable = _clampTable;
            int[] yTable = _yTable;

            TiffRgba32 pixel;
            pixel.R = clampTable[ClampTableOffset + yTable[y] + _crRTable[cr]];
            pixel.G = clampTable[ClampTableOffset + yTable[y] + ((_cbGTable[cb] + _crGTable[cr]) >> Shift)];
            pixel.B = clampTable[ClampTableOffset + yTable[y] + _cbBTable[cb]];
            pixel.A = byte.MaxValue;

            return pixel;
        }

        public void Convert(ReadOnlySpan<byte> ycbcr, Span<TiffRgba32> destination, int count)
        {
            byte[] clampTable = _clampTable;
            int[] yTable = _yTable;

            ref byte sourceRef = ref MemoryMarshal.GetReference(ycbcr);
            ref TiffRgba32 destinationRef = ref MemoryMarshal.GetReference(destination);

            byte y, cb, cr;
            int yTableValue;

            for (int i = 0; i < count; i++)
            {
                y = sourceRef;
                cb = Unsafe.Add(ref sourceRef, 1);
                cr = Unsafe.Add(ref sourceRef, 2);
                TiffRgba32 pixel;
                yTableValue = yTable[y];
                pixel.R = clampTable[ClampTableOffset + yTableValue + _crRTable[cr]];
                pixel.G = clampTable[ClampTableOffset + yTableValue + ((_cbGTable[cb] + _crGTable[cr]) >> Shift)];
                pixel.B = clampTable[ClampTableOffset + yTableValue + _cbBTable[cb]];
                pixel.A = byte.MaxValue;

                sourceRef = ref Unsafe.Add(ref sourceRef, 3);
                Unsafe.Add(ref destinationRef, i) = pixel;
            }
        }
    }
}
