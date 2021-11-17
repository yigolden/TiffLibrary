using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.PixelFormats;
using TiffLibrary.Utils;

namespace TiffLibrary.PhotometricInterpreters
{
    internal sealed class TiffYCbCrConverter8
    {
        private readonly CodingRangeExpander _expander;
        private readonly YCbCrToRgbConverter _converterFrom;

        private readonly CodingRangeShrinker _shrinker;
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
            _expander = new CodingRangeExpander(referenceBlackWhite);
            _converterFrom = new YCbCrToRgbConverter(luma[0], luma[1], luma[2]);

            _shrinker = new CodingRangeShrinker(referenceBlackWhite);
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
            CodingRangeExpander expander = _expander;
            YCbCrToRgbConverter converter = _converterFrom;

            Vector3 vector = expander.Expand(new Vector3(y, cb, cr));
            vector = converter.Convert(vector);
            vector = Vector3.Clamp(vector, Vector3.Zero, new Vector3(255));

            Unsafe.SkipInit(out TiffRgba32 pixel);
            pixel.R = (byte)TiffMathHelper.Round(vector.X);
            pixel.G = (byte)TiffMathHelper.Round(vector.Y);
            pixel.B = (byte)TiffMathHelper.Round(vector.Z);
            pixel.A = byte.MaxValue;

            return pixel;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TiffRgb24 ConvertToRgb24(byte y, byte cb, byte cr)
        {
            CodingRangeExpander expander = _expander;
            YCbCrToRgbConverter converter = _converterFrom;

            Vector3 vector = expander.Expand(new Vector3(y, cb, cr));
            vector = converter.Convert(vector);
            vector = Vector3.Clamp(vector, Vector3.Zero, new Vector3(255));

            Unsafe.SkipInit(out TiffRgb24 pixel);
            pixel.R = (byte)TiffMathHelper.Round(vector.X);
            pixel.G = (byte)TiffMathHelper.Round(vector.Y);
            pixel.B = (byte)TiffMathHelper.Round(vector.Z);

            return pixel;
        }

        public void ConvertToRgba32(ReadOnlySpan<byte> ycbcr, Span<TiffRgba32> destination, int count)
        {
            CodingRangeExpander expander = _expander;
            YCbCrToRgbConverter converter = _converterFrom;
            Vector3 vectorClampMax = new Vector3(255);
            Vector3 roundVector = new Vector3(0.5f);

            ref byte sourceRef = ref MemoryMarshal.GetReference(ycbcr);
            ref TiffRgba32 destinationRef = ref MemoryMarshal.GetReference(destination);

            while (count >= 4)
            {
                var vector1 = new Vector3(sourceRef, Unsafe.Add(ref sourceRef, 1), Unsafe.Add(ref sourceRef, 2));
                var vector2 = new Vector3(Unsafe.Add(ref sourceRef, 3), Unsafe.Add(ref sourceRef, 4), Unsafe.Add(ref sourceRef, 5));
                var vector3 = new Vector3(Unsafe.Add(ref sourceRef, 6), Unsafe.Add(ref sourceRef, 7), Unsafe.Add(ref sourceRef, 8));
                var vector4 = new Vector3(Unsafe.Add(ref sourceRef, 9), Unsafe.Add(ref sourceRef, 10), Unsafe.Add(ref sourceRef, 11));

                vector1 = expander.Expand(vector1);
                vector2 = expander.Expand(vector2);
                vector3 = expander.Expand(vector3);
                vector4 = expander.Expand(vector4);

                vector1 = converter.Convert(vector1);
                vector2 = converter.Convert(vector2);
                vector3 = converter.Convert(vector3);
                vector4 = converter.Convert(vector4);

                vector1 = vector1 + roundVector;
                vector2 = vector2 + roundVector;
                vector3 = vector3 + roundVector;
                vector4 = vector4 + roundVector;

                vector1 = Vector3.Clamp(vector1, Vector3.Zero, vectorClampMax);
                vector2 = Vector3.Clamp(vector2, Vector3.Zero, vectorClampMax);
                vector3 = Vector3.Clamp(vector3, Vector3.Zero, vectorClampMax);
                vector4 = Vector3.Clamp(vector4, Vector3.Zero, vectorClampMax);

                destinationRef.R = (byte)vector1.X;
                destinationRef.G = (byte)vector1.Y;
                destinationRef.B = (byte)vector1.Z;
                destinationRef.A = byte.MaxValue;
                destinationRef = Unsafe.Add(ref destinationRef, 1);

                destinationRef.R = (byte)vector2.X;
                destinationRef.G = (byte)vector2.Y;
                destinationRef.B = (byte)vector2.Z;
                destinationRef.A = byte.MaxValue;
                destinationRef = Unsafe.Add(ref destinationRef, 1);

                destinationRef.R = (byte)vector3.X;
                destinationRef.G = (byte)vector3.Y;
                destinationRef.B = (byte)vector3.Z;
                destinationRef.A = byte.MaxValue;
                destinationRef = Unsafe.Add(ref destinationRef, 1);

                destinationRef.R = (byte)vector4.X;
                destinationRef.G = (byte)vector4.Y;
                destinationRef.B = (byte)vector4.Z;
                destinationRef.A = byte.MaxValue;
                destinationRef = Unsafe.Add(ref destinationRef, 1);

                count -= 4;
                sourceRef = ref Unsafe.Add(ref sourceRef, 12);
            }

            for (int i = 0; i < count; i++)
            {
                var vector = new Vector3(sourceRef, Unsafe.Add(ref sourceRef, 1), Unsafe.Add(ref sourceRef, 2));

                vector = expander.Expand(vector);
                vector = converter.Convert(vector);
                vector = vector + new Vector3(0.5f);
                vector = Vector3.Clamp(vector, Vector3.Zero, vectorClampMax);

                destinationRef.R = (byte)vector.X;
                destinationRef.G = (byte)vector.Y;
                destinationRef.B = (byte)vector.Z;
                destinationRef.A = byte.MaxValue;

                sourceRef = ref Unsafe.Add(ref sourceRef, 3);
                destinationRef = Unsafe.Add(ref destinationRef, 1);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ConvertToRgb24(ReadOnlySpan<byte> ycbcr, Span<TiffRgb24> destination, int count)
        {
            CodingRangeExpander expander = _expander;
            YCbCrToRgbConverter converter = _converterFrom;
            Vector3 vectorClampMax = new Vector3(255);

            ref byte sourceRef = ref MemoryMarshal.GetReference(ycbcr);
            ref TiffRgb24 destinationRef = ref MemoryMarshal.GetReference(destination);
            Unsafe.SkipInit(out TiffRgb24 pixel);

            for (int i = 0; i < count; i++)
            {
                ref TiffRgb24 pixelRef = ref Unsafe.Add(ref destinationRef, i);
                var vector = new Vector3(sourceRef, Unsafe.Add(ref sourceRef, 1), Unsafe.Add(ref sourceRef, 2));

                vector = expander.Expand(vector);
                vector = converter.Convert(vector);
                vector = Vector3.Clamp(vector, Vector3.Zero, vectorClampMax);

                pixel.R = (byte)TiffMathHelper.Round(vector.X);
                pixel.G = (byte)TiffMathHelper.Round(vector.Y);
                pixel.B = (byte)TiffMathHelper.Round(vector.Z);
                pixelRef = pixel;

                sourceRef = ref Unsafe.Add(ref sourceRef, 3);
            }
        }

        public void ConvertFromRgb24(TiffRgb24 rgb, out byte y, out byte cb, out byte cr)
        {
            RgbToYCbCrConverter converter = _converterTo;
            CodingRangeShrinker shrinker = _shrinker;

            var vector = new Vector3(rgb.R, rgb.G, rgb.B);
            vector = converter.Convert(vector);
            vector = shrinker.Shrink(vector);
            vector = Vector3.Clamp(vector, Vector3.Zero, new Vector3(255));

            y = (byte)TiffMathHelper.Round(vector.X);
            cb = (byte)TiffMathHelper.Round(vector.Y);
            cr = (byte)TiffMathHelper.Round(vector.Z);
        }

        public void ConvertFromRgb24(ReadOnlySpan<TiffRgb24> rgb, Span<byte> destination, int count)
        {
            RgbToYCbCrConverter converter = _converterTo;
            CodingRangeShrinker shrinker = _shrinker;
            Vector3 vectorClampMax = new Vector3(255);

            ref TiffRgb24 sourceRef = ref MemoryMarshal.GetReference(rgb);
            ref byte destinationRef = ref MemoryMarshal.GetReference(destination);

            for (int i = 0; i < count; i++)
            {
                TiffRgb24 pixel = Unsafe.Add(ref sourceRef, i);
                var vector = new Vector3(pixel.R, pixel.G, pixel.B);

                vector = converter.Convert(vector);
                vector = shrinker.Shrink(vector);

                destinationRef = (byte)TiffMathHelper.Round(vector.X);
                Unsafe.Add(ref destinationRef, 1) = (byte)TiffMathHelper.Round(vector.Y);
                Unsafe.Add(ref destinationRef, 2) = (byte)TiffMathHelper.Round(vector.Z);

                destinationRef = ref Unsafe.Add(ref destinationRef, 3);
            }
        }

        readonly struct CodingRangeExpander
        {
            private readonly Vector3 _f1;
            private readonly Vector3 _f2;

            public CodingRangeExpander(TiffRational[] referenceBlackWhite)
            {
                _f1 = new Vector3(
                    255 / (referenceBlackWhite[1].ToSingle() - referenceBlackWhite[0].ToSingle()),
                    127 / (referenceBlackWhite[3].ToSingle() - referenceBlackWhite[2].ToSingle()),
                    127 / (referenceBlackWhite[5].ToSingle() - referenceBlackWhite[4].ToSingle())
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

        readonly struct CodingRangeShrinker
        {
            private readonly Vector3 _f1;
            private readonly Vector3 _f2;

            public CodingRangeShrinker(TiffRational[] referenceBlackWhite)
            {
                _f1 = new Vector3(
                      (referenceBlackWhite[1].ToSingle() - referenceBlackWhite[0].ToSingle()) / 255,
                      (referenceBlackWhite[3].ToSingle() - referenceBlackWhite[2].ToSingle()) / 127,
                      (referenceBlackWhite[5].ToSingle() - referenceBlackWhite[4].ToSingle()) / 127
                      );
                _f2 = new Vector3(
                      referenceBlackWhite[0].ToSingle(),
                      referenceBlackWhite[2].ToSingle(),
                      referenceBlackWhite[4].ToSingle()
                    );
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Vector3 Shrink(Vector3 fullRangeValue)
            {
                return fullRangeValue * _f1 + _f2;
            }
        }

        readonly struct YCbCrToRgbConverter
        {
            private readonly Matrix4x4 _transform;

            public YCbCrToRgbConverter(TiffRational lumaRed, TiffRational lumaGreen, TiffRational lumaBlue)
            {
                float lr = lumaRed.ToSingle();
                float lg = lumaGreen.ToSingle();
                float lb = lumaBlue.ToSingle();

                float cr2r = 2 - 2 * lr;
                float cb2b = 2 - 2 * lb;
                float y2g = (1 - lb - lr) / lg;
                float cr2g = 2 * lr * (lr - 1) / lg;
                float cb2g = 2 * lb * (lb - 1) / lg;

                _transform = new Matrix4x4(1, y2g, 1, 0, 0, cb2g, cb2b, 0, cr2r, cr2g, 0, 0, 0, 0, 0, 0);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Vector3 Convert(Vector3 yCbCr)
            {
                return Vector3.TransformNormal(yCbCr, _transform);
            }
        }

        readonly struct RgbToYCbCrConverter
        {
            private readonly float _r2y;
            private readonly float _g2y;
            private readonly float _b2y;
            private readonly float _r2cr;
            private readonly float _b2cb;

            private readonly Matrix4x4 _transform;

            public RgbToYCbCrConverter(TiffRational lumaRed, TiffRational lumaGreen, TiffRational lumaBlue)
            {
                _r2y = lumaRed.ToSingle();
                _g2y = lumaGreen.ToSingle();
                _b2y = lumaBlue.ToSingle();
                _r2cr = 1 / (2 - 2 * lumaRed.ToSingle());
                _b2cb = 1 / (2 - 2 * lumaBlue.ToSingle());


                float r2y = lumaRed.ToSingle();
                float r2cb = lumaRed.ToSingle() / (2 * lumaBlue.ToSingle() - 2);
                const float r2cr = 0.5f;
                float g2y = lumaGreen.ToSingle();
                float g2cb = lumaGreen.ToSingle() / (2 * lumaBlue.ToSingle() - 2);
                float g2cr = lumaGreen.ToSingle() / (2 * lumaRed.ToSingle() - 2);
                float b2y = lumaBlue.ToSingle();
                const float b2cb = 0.5f;
                float b2cr = lumaBlue.ToSingle() / (2 * lumaRed.ToSingle() - 2);

                _transform = new Matrix4x4(r2y, r2cb, r2cr, 0, g2y, g2cb, g2cr, 0, b2y, b2cb, b2cr, 0, 0, 0, 0, 0);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Convert(TiffRgb24 rgb, out long y, out long cb, out long cr)
            {
                float lR = rgb.R;
                float lB = rgb.B;

                float fY = _r2y * lR + _g2y * rgb.G + _b2y * lB;
                float fCr = (lR - fY) * _r2cr;
                float fCb = (lB - fY) * _b2cb;

                y = TiffMathHelper.RoundToInt64(fY);
                cb = TiffMathHelper.RoundToInt64(fCb);
                cr = TiffMathHelper.RoundToInt64(fCr);
            }


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Vector3 Convert(Vector3 rgb)
            {
                return Vector3.TransformNormal(rgb, _transform);
            }
        }

    }
}
