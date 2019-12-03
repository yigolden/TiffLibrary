using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelFormats;
using TiffLibrary.Utils;

namespace TiffLibrary.PhotometricInterpreters
{
    internal sealed class TiffYCbCrConverter16
    {
        private readonly CodingRangeExpander _expanderY;
        private readonly CodingRangeExpander _expanderCb;
        private readonly CodingRangeExpander _expanderCr;
        private readonly YCbCrToRgbConverter _converterFrom;


        private static TiffRational[] s_defaultLuma = new TiffRational[]
        {
            new TiffRational(299, 1000),
            new TiffRational(587, 1000),
            new TiffRational(114, 1000)
        };
        private static TiffRational[] s_defaultReferenceBlackWhite = new TiffRational[]
        {
            new TiffRational(0, 1), new TiffRational(255 << 8 | 255, 1),
            new TiffRational(128 << 8 | 128, 1), new TiffRational(255 << 8 | 255, 1),
            new TiffRational(128 << 8 | 128, 1), new TiffRational(255 << 8 | 255, 1)
        };

        private static TiffYCbCrConverter16 Default { get; } = new TiffYCbCrConverter16(s_defaultLuma, s_defaultReferenceBlackWhite);

        public static TiffValueCollection<TiffRational> DefaultLuma => TiffValueCollection.UnsafeWrap(s_defaultLuma);

        public static TiffValueCollection<TiffRational> DefaultReferenceBlackWhite => TiffValueCollection.UnsafeWrap(s_defaultReferenceBlackWhite);

        private TiffYCbCrConverter16(TiffRational[] luma, TiffRational[] referenceBlackWhite)
        {
            _expanderY = new CodingRangeExpander(referenceBlackWhite[0], referenceBlackWhite[1], ushort.MaxValue);
            _expanderCb = new CodingRangeExpander(referenceBlackWhite[2], referenceBlackWhite[3], ushort.MaxValue / 2);
            _expanderCr = new CodingRangeExpander(referenceBlackWhite[4], referenceBlackWhite[5], ushort.MaxValue / 2);
            _converterFrom = new YCbCrToRgbConverter(luma[0], luma[1], luma[2]);
        }

        public static TiffYCbCrConverter16 CreateDefault() => Default;

        public static TiffYCbCrConverter16 Create(TiffRational[] luma, TiffRational[] referenceBlackWhite)
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

            return new TiffYCbCrConverter16(luma, referenceBlackWhite);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TiffRgba64 ConvertToRgba64(ushort y, ushort cb, ushort cr)
        {
            CodingRangeExpander expanderY = _expanderY;
            CodingRangeExpander expanderCb = _expanderCb;
            CodingRangeExpander expanderCr = _expanderCr;
            YCbCrToRgbConverter converter = _converterFrom;

            float y64 = expanderY.Expand(y);
            float cb64 = expanderCb.Expand(cb);
            float cr64 = expanderCr.Expand(cr);

            TiffRgba64 pixel = default; // TODO: SkipInit

            converter.Convert(y64, cb64, cr64, ref pixel);

            return pixel;
        }

        public void ConvertToRgba64(ReadOnlySpan<ushort> ycbcr, Span<TiffRgba64> destination, int count)
        {
            CodingRangeExpander expanderY = _expanderY;
            CodingRangeExpander expanderCb = _expanderCb;
            CodingRangeExpander expanderCr = _expanderCr;
            YCbCrToRgbConverter converter = _converterFrom;

            ref ushort sourceRef = ref MemoryMarshal.GetReference(ycbcr);
            ref TiffRgba64 destinationRef = ref MemoryMarshal.GetReference(destination);

            for (int i = 0; i < count; i++)
            {
                ref TiffRgba64 pixelRef = ref Unsafe.Add(ref destinationRef, i);

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
            public void Convert(float y, float cb, float cr, ref TiffRgba64 pixel)
            {
                pixel.R = TiffMathHelper.RoundAndClampTo16Bit(cr * _cr2r + y);
                pixel.G = TiffMathHelper.RoundAndClampTo16Bit(_y2g * y + _cr2g * cr + _cb2g * cb);
                pixel.B = TiffMathHelper.RoundAndClampTo16Bit(cb * _cb2b + y);
                pixel.A = byte.MaxValue;
            }
        }

    }
}
