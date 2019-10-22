using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace JpegLibrary.ColorConverters
{
    internal class JpegRgbToYCbCrConverter
    {
        public static JpegRgbToYCbCrConverter Shared { get; } = new JpegRgbToYCbCrConverter();

        private int[] _yRTable;
        private int[] _yGTable;
        private int[] _yBTable;
        private int[] _cbRTable;
        private int[] _cbGTable;
        private int[] _cbBTable;
        private int[] _crGTable;
        private int[] _crBTable;

        private const int ScaleBits = 16;
        private const int CBCrOffset = 128 << ScaleBits;
        private const int Half = 1 << (ScaleBits - 1);

        public JpegRgbToYCbCrConverter()
        {
            _yRTable = new int[256];
            _yGTable = new int[256];
            _yBTable = new int[256];
            _cbRTable = new int[256];
            _cbGTable = new int[256];
            _cbBTable = new int[256];
            _crGTable = new int[256];
            _crBTable = new int[256];

            for (int i = 0; i < 256; i++)
            {
                // The values for the calculations are left scaled up since we must add them together before rounding.
                _yRTable[i] = Fix(0.299F) * i;
                _yGTable[i] = Fix(0.587F) * i;
                _yBTable[i] = (Fix(0.114F) * i) + Half;
                _cbRTable[i] = (-Fix(0.168735892F)) * i;
                _cbGTable[i] = (-Fix(0.331264108F)) * i;

                // We use a rounding fudge - factor of 0.5 - epsilon for Cb and Cr.
                // This ensures that the maximum output will round to 255
                // not 256, and thus that we don't have to range-limit.
                //
                // B=>Cb and R=>Cr tables are the same
                _cbBTable[i] = (Fix(0.5F) * i) + CBCrOffset + Half - 1;

                _crGTable[i] = (-Fix(0.418687589F)) * i;
                _crBTable[i] = (-Fix(0.081312411F)) * i;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Fix(float x)
        {
            return (int)((x * (1L << ScaleBits)) + 0.5F);
        }

        public void ConvertRgb24ToYCbCr8(ReadOnlySpan<byte> rgb, Span<byte> ycbcr, int count)
        {
            if (rgb.Length < 3 * count)
            {
                throw new ArgumentException("RGB buffer is too small.", nameof(rgb));
            }
            if (ycbcr.Length < 3 * count)
            {
                throw new ArgumentException("YCbCr buffer is too small.", nameof(ycbcr));
            }

            ref byte sourceRef = ref MemoryMarshal.GetReference(rgb);
            ref byte destinationRef = ref MemoryMarshal.GetReference(ycbcr);

            byte r, g, b;

            for (int i = 0; i < count; i++)
            {
                r = sourceRef;
                g = Unsafe.Add(ref sourceRef, 1);
                b = Unsafe.Add(ref sourceRef, 2);

                destinationRef = (byte)((_yRTable[r] + _yGTable[g] + _yBTable[b]) >> ScaleBits);
                Unsafe.Add(ref destinationRef, 1) = (byte)((_cbRTable[r] + _cbGTable[g] + _cbBTable[b]) >> ScaleBits);
                Unsafe.Add(ref destinationRef, 2) = (byte)((_cbBTable[r] + _crGTable[g] + _crBTable[b]) >> ScaleBits);

                sourceRef = ref Unsafe.Add(ref sourceRef, 3);
                destinationRef = ref Unsafe.Add(ref destinationRef, 3);
            }
        }

        public void ConvertRgba32ToYCbCr8(ReadOnlySpan<byte> rgba, Span<byte> ycbcr, int count)
        {
            if (rgba.Length < 4 * count)
            {
                throw new ArgumentException("RGBA buffer is too small.", nameof(rgba));
            }
            if (ycbcr.Length < 3 * count)
            {
                throw new ArgumentException("YCbCr buffer is too small.", nameof(ycbcr));
            }

            ref byte sourceRef = ref MemoryMarshal.GetReference(rgba);
            ref byte destinationRef = ref MemoryMarshal.GetReference(ycbcr);

            byte r, g, b;

            for (int i = 0; i < count; i++)
            {
                r = sourceRef;
                g = Unsafe.Add(ref sourceRef, 1);
                b = Unsafe.Add(ref sourceRef, 2);

                destinationRef = (byte)((_yRTable[r] + _yGTable[g] + _yBTable[b]) >> ScaleBits);
                Unsafe.Add(ref destinationRef, 1) = (byte)((_cbRTable[r] + _cbGTable[g] + _cbBTable[b]) >> ScaleBits);
                Unsafe.Add(ref destinationRef, 2) = (byte)((_cbBTable[r] + _crGTable[g] + _crBTable[b]) >> ScaleBits);

                sourceRef = ref Unsafe.Add(ref sourceRef, 4);
                destinationRef = ref Unsafe.Add(ref destinationRef, 3);
            }
        }
    }
}
