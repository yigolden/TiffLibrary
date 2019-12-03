using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelFormats;
using TiffLibrary.Utils;

namespace TiffLibrary.PhotometricInterpreters
{
    internal sealed class TiffYCbCrConverter8
    {
        private readonly CodingRangeExpander _expanderY;
        private readonly CodingRangeExpander _expanderCb;
        private readonly CodingRangeExpander _expanderCr;
        private readonly YCbCrToRgbConverter _converterFrom;

        private readonly CodingRangeShrinker _shrinkerY;
        private readonly CodingRangeShrinker _shrinkerCb;
        private readonly CodingRangeShrinker _shrinkerCr;
        private readonly RgbToYCbCrConverter _converterTo;


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

        private static TiffYCbCrConverter8 Default { get; } = new TiffYCbCrConverter8(s_defaultLuma, s_defaultReferenceBlackWhite);

        public static TiffValueCollection<TiffRational> DefaultLuma => TiffValueCollection.UnsafeWrap(s_defaultLuma);

        public static TiffValueCollection<TiffRational> DefaultReferenceBlackWhite => TiffValueCollection.UnsafeWrap(s_defaultReferenceBlackWhite);

        private TiffYCbCrConverter8(TiffRational[] luma, TiffRational[] referenceBlackWhite)
        {
            _expanderY = new CodingRangeExpander(referenceBlackWhite[0], referenceBlackWhite[1], 255);
            _expanderCb = new CodingRangeExpander(referenceBlackWhite[2], referenceBlackWhite[3], 127);
            _expanderCr = new CodingRangeExpander(referenceBlackWhite[4], referenceBlackWhite[5], 127);
            _converterFrom = new YCbCrToRgbConverter(luma[0], luma[1], luma[2]);

            _shrinkerY = new CodingRangeShrinker(referenceBlackWhite[0], referenceBlackWhite[1], 255);
            _shrinkerCb = new CodingRangeShrinker(referenceBlackWhite[2], referenceBlackWhite[3], 127);
            _shrinkerCr = new CodingRangeShrinker(referenceBlackWhite[4], referenceBlackWhite[5], 127);
            _converterTo = new RgbToYCbCrConverter(luma[0], luma[1], luma[2]);
        }

        public static TiffYCbCrConverter8 CreateDefault() => Default;

        public static TiffYCbCrConverter8 Create(TiffRational[] luma, TiffRational[] referenceBlackWhite)
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

            return new TiffYCbCrConverter8(luma, referenceBlackWhite);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TiffRgba32 ConvertToRgba32(byte y, byte cb, byte cr)
        {
            CodingRangeExpander expanderY = _expanderY;
            CodingRangeExpander expanderCb = _expanderCb;
            CodingRangeExpander expanderCr = _expanderCr;
            YCbCrToRgbConverter converter = _converterFrom;

            float y64 = expanderY.Expand(y);
            float cb64 = expanderCb.Expand(cb);
            float cr64 = expanderCr.Expand(cr);

            TiffRgba32 pixel = default; // TODO: SkipInit

            converter.Convert(y64, cb64, cr64, ref pixel);

            return pixel;
        }

        public void ConvertToRgba32(ReadOnlySpan<byte> ycbcr, Span<TiffRgba32> destination, int count)
        {
            CodingRangeExpander expanderY = _expanderY;
            CodingRangeExpander expanderCb = _expanderCb;
            CodingRangeExpander expanderCr = _expanderCr;
            YCbCrToRgbConverter converter = _converterFrom;

            ref byte sourceRef = ref MemoryMarshal.GetReference(ycbcr);
            ref TiffRgba32 destinationRef = ref MemoryMarshal.GetReference(destination);

            for (int i = 0; i < count; i++)
            {
                ref TiffRgba32 pixelRef = ref Unsafe.Add(ref destinationRef, i);

                float y = sourceRef;
                float cb = Unsafe.Add(ref sourceRef, 1);
                float cr = Unsafe.Add(ref sourceRef, 2);

                y = expanderY.Expand(y);
                cb = expanderCb.Expand(cb);
                cr = expanderCr.Expand(cr);

                converter.Convert(y, cb, cr, ref pixelRef);

                sourceRef = ref Unsafe.Add(ref sourceRef, 3);
            }
        }

        public void ConvertFromRgb24(ReadOnlySpan<TiffRgb24> rgb, Span<byte> destination, int count)
        {
            CodingRangeShrinker shrinkerY = _shrinkerY;
            CodingRangeShrinker shrinkerCb = _shrinkerCb;
            CodingRangeShrinker shrinkerCr = _shrinkerCr;
            RgbToYCbCrConverter converter = _converterTo;

            ref TiffRgb24 sourceRef = ref MemoryMarshal.GetReference(rgb);
            ref byte destinationRef = ref MemoryMarshal.GetReference(destination);

            for (int i = 0; i < count; i++)
            {
                converter.Convert(Unsafe.Add(ref sourceRef, i), out long y, out long cb, out long cr);

                y = shrinkerY.Shrink(y);
                cb = shrinkerCb.Shrink(cb);
                cr = shrinkerCr.Shrink(cr);

                destinationRef = TiffMathHelper.ClampTo8Bit(y);
                Unsafe.Add(ref destinationRef, 1) = TiffMathHelper.ClampTo8Bit(cb);
                Unsafe.Add(ref destinationRef, 2) = TiffMathHelper.ClampTo8Bit(cr);

                destinationRef = ref Unsafe.Add(ref destinationRef, 3);
            }
        }

        readonly struct CodingRangeExpander
        {
            public CodingRangeExpander(TiffRational referenceBlack, TiffRational referenceWhite, int codingRange)
            {
                _f1 = codingRange / (referenceWhite.ToSingle() - referenceBlack.ToSingle());
                _f2 = _f1 * referenceBlack.ToSingle();
            }

            private readonly float _f1;
            private readonly float _f2;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public float Expand(float code)
            {
                return code * _f1 - _f2;
            }
        }

        readonly struct CodingRangeShrinker
        {
            private const int Shift = 16;

            private readonly long _f1;
            private readonly long _f2;

            public CodingRangeShrinker(TiffRational referenceBlack, TiffRational referenceWhite, int codingRange)
            {
                _f1 = (1 << Shift) * (referenceWhite.Numerator * (long)referenceBlack.Denominator - referenceWhite.Denominator * (long)referenceBlack.Numerator) / referenceWhite.Denominator / referenceBlack.Denominator / codingRange;
                _f2 = (1 << Shift) * (long)referenceWhite.Denominator * referenceBlack.Numerator / referenceWhite.Denominator / referenceBlack.Denominator;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public long Shrink(long fullRangeValue)
            {
                return (_f1 * fullRangeValue + _f2) >> Shift;
            }
        }

        readonly struct YCbCrToRgbConverter
        {
            private readonly float _cr2r;
            private readonly float _cb2b;
            private readonly float _y2g;
            private readonly float _cr2g;
            private readonly float _cb2g;

            public YCbCrToRgbConverter(TiffRational lumaRed, TiffRational lumaGreen, TiffRational lumaBlue)
            {
                _cr2r = 2 - 2 * lumaRed.ToSingle();
                _cb2b = 2 - 2 * lumaBlue.ToSingle();
                _y2g = (1 - lumaBlue.ToSingle() - lumaRed.ToSingle()) / lumaGreen.ToSingle();
                _cr2g = -(lumaRed.ToSingle() / lumaGreen.ToSingle());
                _cb2g = -(lumaBlue.ToSingle() / lumaGreen.ToSingle());
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Convert(float y, float cb, float cr, ref TiffRgba32 pixel)
            {
                pixel.R = TiffMathHelper.RoundAndClampTo8Bit(cr * _cr2r + y);
                pixel.G = TiffMathHelper.RoundAndClampTo8Bit(_y2g * y + _cr2g * cr + _cb2g * cb);
                pixel.B = TiffMathHelper.RoundAndClampTo8Bit(cb * _cb2b + y);
                pixel.A = byte.MaxValue;
            }
        }

        readonly struct RgbToYCbCrConverter
        {
            private readonly float _r2y;
            private readonly float _g2y;
            private readonly float _b2y;
            private readonly float _r2cr;
            private readonly float _b2cb;

            public RgbToYCbCrConverter(TiffRational lumaRed, TiffRational lumaGreen, TiffRational lumaBlue)
            {
                _r2y = lumaRed.ToSingle();
                _g2y = lumaGreen.ToSingle();
                _b2y = lumaBlue.ToSingle();
                _r2cr = 1 / (2 - 2 * lumaRed.ToSingle());
                _b2cb = 1 / (2 - 2 * lumaBlue.ToSingle());
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Convert(TiffRgb24 rgb, out long y, out long cb, out long cr)
            {
                float lR = rgb.R;
                float lB = rgb.B;

                float fY = _r2y * lR + _g2y * rgb.G + _b2y * lB;
                float fCr = lR * _r2cr - fY * _r2cr;
                float fCb = lB * _b2cb - fY * _b2cb;

                y = TiffMathHelper.RoundToInt64(fY);
                cb = TiffMathHelper.RoundToInt64(fCb);
                cr = TiffMathHelper.RoundToInt64(fCr);
            }
        }

    }
}
