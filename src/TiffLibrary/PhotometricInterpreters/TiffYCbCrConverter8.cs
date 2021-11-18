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

            Unsafe.SkipInit(out TiffRgba32 pixel);

            converter.Convert(y64, cb64, cr64, ref pixel);

            return pixel;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TiffRgb24 ConvertToRgb24(byte y, byte cb, byte cr)
        {
            CodingRangeExpander expanderY = _expanderY;
            CodingRangeExpander expanderCb = _expanderCb;
            CodingRangeExpander expanderCr = _expanderCr;
            YCbCrToRgbConverter converter = _converterFrom;

            float y64 = expanderY.Expand(y);
            float cb64 = expanderCb.Expand(cb);
            float cr64 = expanderCr.Expand(cr);

            Unsafe.SkipInit(out TiffRgb24 pixel);

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
                float y = sourceRef;
                float cb = Unsafe.Add(ref sourceRef, 1);
                float cr = Unsafe.Add(ref sourceRef, 2);

                y = expanderY.Expand(y);
                cb = expanderCb.Expand(cb);
                cr = expanderCr.Expand(cr);

                converter.Convert(y, cb, cr, ref Unsafe.Add(ref destinationRef, i));

                sourceRef = ref Unsafe.Add(ref sourceRef, 3);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ConvertToRgb24(ReadOnlySpan<byte> ycbcr, Span<TiffRgb24> destination, int count)
        {
            CodingRangeExpander expanderY = _expanderY;
            CodingRangeExpander expanderCb = _expanderCb;
            CodingRangeExpander expanderCr = _expanderCr;
            YCbCrToRgbConverter converter = _converterFrom;

            ref byte sourceRef = ref MemoryMarshal.GetReference(ycbcr);
            ref TiffRgb24 destinationRef = ref MemoryMarshal.GetReference(destination);

            for (int i = 0; i < count; i++)
            {
                ref TiffRgb24 pixelRef = ref Unsafe.Add(ref destinationRef, i);

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

        public void ConvertFromRgb24(TiffRgb24 rgb, out byte y, out byte cb, out byte cr)
        {
            CodingRangeShrinker shrinkerY = _shrinkerY;
            CodingRangeShrinker shrinkerCb = _shrinkerCb;
            CodingRangeShrinker shrinkerCr = _shrinkerCr;
            RgbToYCbCrConverter converter = _converterTo;

            converter.Convert(rgb, out float fy, out float fcb, out float fcr);

            y = TiffMathHelper.RoundAndClampTo8Bit(shrinkerY.Shrink(fy));
            cb = TiffMathHelper.RoundAndClampTo8Bit(shrinkerCb.Shrink(fcb));
            cr = TiffMathHelper.RoundAndClampTo8Bit(shrinkerCr.Shrink(fcr));
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
                converter.Convert(Unsafe.Add(ref sourceRef, i), out float y, out float cb, out float cr);

                y = shrinkerY.Shrink(y);
                cb = shrinkerCb.Shrink(cb);
                cr = shrinkerCr.Shrink(cr);

                destinationRef = TiffMathHelper.RoundAndClampTo8Bit(y);
                Unsafe.Add(ref destinationRef, 1) = TiffMathHelper.RoundAndClampTo8Bit(cb);
                Unsafe.Add(ref destinationRef, 2) = TiffMathHelper.RoundAndClampTo8Bit(cr);

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
            private readonly float _f1;
            private readonly float _f2;

            public CodingRangeShrinker(TiffRational referenceBlack, TiffRational referenceWhite, int codingRange)
            {
                _f1 = (referenceWhite.ToSingle() - referenceBlack.ToSingle()) / codingRange;
                _f2 = referenceBlack.ToSingle();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public float Shrink(float fullRangeValue)
            {
                return fullRangeValue * _f1 + _f2;
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
                float lr = lumaRed.ToSingle();
                float lg = lumaGreen.ToSingle();
                float lb = lumaBlue.ToSingle();

                _cr2r = 2 - 2 * lr;
                _cb2b = 2 - 2 * lb;
                _y2g = (1 - lb - lr) / lg;
                _cr2g = 2 * lr * (lr - 1) / lg;
                _cb2g = 2 * lb * (lb - 1) / lg;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Convert(float y, float cb, float cr, ref TiffRgba32 pixel)
            {
                pixel.R = TiffMathHelper.RoundAndClampTo8Bit(cr * _cr2r + y);
                pixel.G = TiffMathHelper.RoundAndClampTo8Bit(_y2g * y + _cr2g * cr + _cb2g * cb);
                pixel.B = TiffMathHelper.RoundAndClampTo8Bit(cb * _cb2b + y);
                pixel.A = byte.MaxValue;
            }


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Convert(float y, float cb, float cr, ref TiffRgb24 pixel)
            {
                pixel.R = TiffMathHelper.RoundAndClampTo8Bit(cr * _cr2r + y);
                pixel.G = TiffMathHelper.RoundAndClampTo8Bit(_y2g * y + _cr2g * cr + _cb2g * cb);
                pixel.B = TiffMathHelper.RoundAndClampTo8Bit(cb * _cb2b + y);
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
            public void Convert(TiffRgb24 rgb, out float y, out float cb, out float cr)
            {
                float lR = rgb.R;
                float lB = rgb.B;

                y = _r2y * lR + _g2y * rgb.G + _b2y * lB;
                cr = (lR - y) * _r2cr;
                cb = (lB - y) * _b2cb;
            }
        }

    }
}
