using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelFormats;
using TiffLibrary.Utils;

namespace TiffLibrary.PhotometricInterpreters
{
    internal sealed class TiffYCbCrConverter16
    {
        private readonly CodingRangeExpander _expander;
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
            _expander = new CodingRangeExpander(referenceBlackWhite);
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
            CodingRangeExpander expander = _expander;
            YCbCrToRgbConverter converter = _converterFrom;

            Vector3 vector = expander.Expand(new Vector3(y, cb, cr));
            vector = converter.Convert(vector);
            vector = Vector3.Clamp(vector, Vector3.Zero, new Vector3(ushort.MaxValue));

            Unsafe.SkipInit(out TiffRgba64 pixel);
            pixel.R = (ushort)TiffMathHelper.Round(vector.X);
            pixel.G = (ushort)TiffMathHelper.Round(vector.Y);
            pixel.B = (ushort)TiffMathHelper.Round(vector.Z);
            pixel.A = ushort.MaxValue;

            return pixel;
        }

        public void ConvertToRgba64(ReadOnlySpan<ushort> ycbcr, Span<TiffRgba64> destination, int count)
        {
            CodingRangeExpander expander = _expander;
            YCbCrToRgbConverter converter = _converterFrom;
            Vector3 vectorClampMax = new Vector3(ushort.MaxValue);

            ref ushort sourceRef = ref MemoryMarshal.GetReference(ycbcr);
            ref TiffRgba64 destinationRef = ref MemoryMarshal.GetReference(destination);
            Unsafe.SkipInit(out TiffRgba64 pixel);
            pixel.A = ushort.MaxValue;

            for (int i = 0; i < count; i++)
            {
                ref TiffRgba64 pixelRef = ref Unsafe.Add(ref destinationRef, i);
                var vector = new Vector3(sourceRef, Unsafe.Add(ref sourceRef, 1), Unsafe.Add(ref sourceRef, 2));

                vector = expander.Expand(vector);
                vector = converter.Convert(vector);
                vector = Vector3.Clamp(vector, Vector3.Zero, vectorClampMax);

                pixel.R = (ushort)TiffMathHelper.Round(vector.X);
                pixel.G = (ushort)TiffMathHelper.Round(vector.Y);
                pixel.B = (ushort)TiffMathHelper.Round(vector.Z);
                pixelRef = pixel;

                float y = sourceRef;
                float cb = Unsafe.Add(ref sourceRef, 1);
                float cr = Unsafe.Add(ref sourceRef, 2);

                sourceRef = ref Unsafe.Add(ref sourceRef, 3);
            }
        }


        readonly struct CodingRangeExpander
        {
            private readonly Vector3 _f1;
            private readonly Vector3 _f2;

            public CodingRangeExpander(TiffRational[] referenceBlackWhite)
            {
                _f1 = new Vector3(
                    (ushort.MaxValue) / (referenceBlackWhite[1].ToSingle() - referenceBlackWhite[0].ToSingle()),
                    (ushort.MaxValue / 2) / (referenceBlackWhite[3].ToSingle() - referenceBlackWhite[2].ToSingle()),
                    (ushort.MaxValue / 2) / (referenceBlackWhite[5].ToSingle() - referenceBlackWhite[4].ToSingle())
                    );
                _f2 = new Vector3(
                    _f1.X * referenceBlackWhite[0].ToSingle(),
                    _f1.Y * referenceBlackWhite[2].ToSingle(),
                    _f1.Z * referenceBlackWhite[4].ToSingle()
                    );
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Vector3 Expand(Vector3 code)
            {
                return code * _f1 - _f2;
            }
        }

        readonly struct YCbCrToRgbConverter
        {
            private readonly Matrix4x4 _transform;

            public YCbCrToRgbConverter(TiffRational lumaRed, TiffRational lumaGreen, TiffRational lumaBlue)
            {
                float cr2r = 2 - 2 * lumaRed.ToSingle();
                float cb2b = 2 - 2 * lumaBlue.ToSingle();
                float y2g = (1 - lumaBlue.ToSingle() - lumaRed.ToSingle()) / lumaGreen.ToSingle();
                float cr2g = 2 * lumaRed.ToSingle() * (lumaRed.ToSingle() - 1) / lumaGreen.ToSingle();
                float cb2g = 2 * lumaBlue.ToSingle() * (lumaBlue.ToSingle() - 1) / lumaGreen.ToSingle();
                _transform = new Matrix4x4(1, y2g, 1, 0, 0, cb2g, cb2b, 0, cr2r, cr2g, 0, 0, 0, 0, 0, 0);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Vector3 Convert(Vector3 yCbCr)
            {
                return Vector3.TransformNormal(yCbCr, _transform);
            }
        }

    }
}
