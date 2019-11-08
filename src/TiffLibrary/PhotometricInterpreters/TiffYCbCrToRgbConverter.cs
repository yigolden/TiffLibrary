using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelFormats;
using TiffLibrary.Utils;

namespace TiffLibrary.PhotometricInterpreters
{
    internal sealed class TiffYCbCrToRgbConverter
    {
        private readonly CodingRangeExpander _expanderY;
        private readonly CodingRangeExpander _expanderCb;
        private readonly CodingRangeExpander _expanderCr;
        private readonly YCbCrToRgbConverter _converter;

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

        private static TiffYCbCrToRgbConverter Default { get; } = new TiffYCbCrToRgbConverter(s_defaultLuma, s_defaultReferenceBlackWhite);

        private TiffYCbCrToRgbConverter(TiffRational[] luma, TiffRational[] referenceBlackWhite)
        {
            _expanderY = new CodingRangeExpander(referenceBlackWhite[0], referenceBlackWhite[1], 255);
            _expanderCb = new CodingRangeExpander(referenceBlackWhite[2], referenceBlackWhite[3], 127);
            _expanderCr = new CodingRangeExpander(referenceBlackWhite[4], referenceBlackWhite[5], 127);
            _converter = new YCbCrToRgbConverter(luma[0], luma[1], luma[2]);
        }

        public static TiffYCbCrToRgbConverter Create(TiffRational[] luma, TiffRational[] referenceBlackWhite)
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

            if (isDefault)
            {
                return Default;
            }

            return new TiffYCbCrToRgbConverter(luma, referenceBlackWhite);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TiffRgba32 Convert(byte y, byte cb, byte cr)
        {
            CodingRangeExpander expanderY = _expanderY;
            CodingRangeExpander expanderCb = _expanderCb;
            CodingRangeExpander expanderCr = _expanderCr;
            YCbCrToRgbConverter converter = _converter;

            long y64 = expanderY.Expand(y);
            long cb64 = expanderCb.Expand(cb);
            long cr64 = expanderCr.Expand(cr);

            TiffRgba32 pixel = default; // TODO: SkipInit

            converter.Convert(y64, cb64, cr64, ref pixel);

            return pixel;
        }

        public void Convert(ReadOnlySpan<byte> ycbcr, Span<TiffRgba32> destination, int count)
        {
            CodingRangeExpander expanderY = _expanderY;
            CodingRangeExpander expanderCb = _expanderCb;
            CodingRangeExpander expanderCr = _expanderCr;
            YCbCrToRgbConverter converter = _converter;

            ref byte sourceRef = ref MemoryMarshal.GetReference(ycbcr);
            ref TiffRgba32 destinationRef = ref MemoryMarshal.GetReference(destination);

            for (int i = 0; i < count; i++)
            {
                ref TiffRgba32 pixelRef = ref Unsafe.Add(ref destinationRef, i);

                long y = sourceRef;
                long cb = Unsafe.Add(ref sourceRef, 1);
                long cr = Unsafe.Add(ref sourceRef, 2);

                y = expanderY.Expand(y);
                cb = expanderCb.Expand(cb);
                cr = expanderCr.Expand(cr);

                converter.Convert(y, cb, cr, ref pixelRef);

                sourceRef = ref Unsafe.Add(ref sourceRef, 3);
            }
        }

        readonly struct CodingRangeExpander
        {
            public CodingRangeExpander(TiffRational referenceBlack, TiffRational referenceWhite, int codingRange)
            {
                long f = (1 << Shift) * (long)referenceWhite.Denominator * referenceBlack.Denominator * codingRange / referenceBlack.Denominator / (referenceWhite.Numerator * (long)referenceBlack.Denominator - referenceWhite.Denominator * (long)referenceBlack.Numerator);
                _f1 = referenceBlack.Denominator * f;
                _f2 = referenceBlack.Numerator * f;
            }

            private const int Shift = 16;

            private readonly long _f1;
            private readonly long _f2;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public long Expand(long code)
            {
                return (code * _f1 - _f2) >> Shift;
            }
        }

        readonly struct YCbCrToRgbConverter
        {
            public YCbCrToRgbConverter(TiffRational lumaRed, TiffRational lumaGreen, TiffRational lumaBlue)
            {
                _cr2r = 2 - 2 * lumaRed.ToSingle();
                _cb2b = 2 - 2 * lumaBlue.ToSingle();
                _y2g = (1 - lumaBlue.ToSingle() - lumaRed.ToSingle()) / lumaGreen.ToSingle();
                _cr2g = -(lumaRed.ToSingle() / lumaGreen.ToSingle());
                _cb2g = -(lumaBlue.ToSingle() / lumaGreen.ToSingle());
            }

            private readonly float _cr2r;
            private readonly float _cb2b;
            private readonly float _y2g;
            private readonly float _cr2g;
            private readonly float _cb2g;


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Convert(long y, long cb, long cr, ref TiffRgba32 pixel)
            {
                pixel.R = TiffMathHelper.RoundAndClampTo8Bit(cr * _cr2r + y);
                pixel.G = TiffMathHelper.RoundAndClampTo8Bit(_y2g * y + _cr2g * cr + _cb2g * cb);
                pixel.B = TiffMathHelper.RoundAndClampTo8Bit(cb * _cb2b + y);
                pixel.A = byte.MaxValue;
            }
        }

    }
}
