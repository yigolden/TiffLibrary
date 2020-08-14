using System;
using System.Collections.Generic;
using TiffLibrary.PhotometricInterpreters;
using TiffLibrary.PixelFormats;
using Xunit;

namespace TiffLibrary.Tests.PhotometricInterpretations
{
    public class YCbCrRoundtripTests
    {
        public static IEnumerable<object[]> GetYCbCr8TestData()
        {
            yield return new object[] { 86, 80, 109 };
            yield return new object[] { 123, 100, 100 };
            yield return new object[] { 216, 128, 128 };
            yield return new object[] { 65, 92, 82 };
        }

        [Theory]
        [MemberData(nameof(GetYCbCr8TestData))]
        public void TestYCbCr8ToRgb24(byte y, byte cb, byte cr)
        {
            var converter = TiffYCbCrConverter8.CreateDefault();

            TiffRgb24 pixel = converter.ConvertToRgb24(y, cb, cr);
            converter.ConvertFromRgb24(pixel, out byte oy, out byte ocb, out byte ocr);
            Assert.Equal(y, oy);
            Assert.Equal(cb, ocb);
            Assert.Equal(cr, ocr);

            Span<byte> ycbcr = stackalloc byte[9];
            ycbcr[0] = y;
            ycbcr[1] = cb;
            ycbcr[2] = cr;
            ycbcr[3] = y;
            ycbcr[4] = cb;
            ycbcr[5] = cr;
            ycbcr[6] = y;
            ycbcr[7] = cb;
            ycbcr[8] = cr;

            Span<TiffRgb24> rgb = stackalloc TiffRgb24[3];
            Span<byte> ycbcrBack = stackalloc byte[9];

            converter.ConvertToRgb24(ycbcr, rgb, 3);
            converter.ConvertFromRgb24(rgb, ycbcrBack, 3);

            Assert.True(ycbcr.SequenceEqual(ycbcrBack));
        }

        [Theory]
        [MemberData(nameof(GetYCbCr8TestData))]
        public void TestYCbCr8ToRgba32(byte y, byte cb, byte cr)
        {
            var converter = TiffYCbCrConverter8.CreateDefault();

            Span<byte> ycbcr = stackalloc byte[9];
            ycbcr[0] = y;
            ycbcr[1] = cb;
            ycbcr[2] = cr;
            ycbcr[3] = y;
            ycbcr[4] = cb;
            ycbcr[5] = cr;
            ycbcr[6] = y;
            ycbcr[7] = cb;
            ycbcr[8] = cr;

            Span<TiffRgba32> rgba = stackalloc TiffRgba32[3];
            Span<byte> ycbcrBack = stackalloc byte[9];

            converter.ConvertToRgba32(ycbcr, rgba, 3);
            for (int i = 0; i < 3; i++)
            {
                converter.ConvertFromRgb24(new TiffRgb24(rgba[i].R, rgba[i].G, rgba[i].B), out ycbcrBack[i * 3], out ycbcrBack[i * 3 + 1], out ycbcrBack[i * 3 + 2]);
            }

            Assert.True(ycbcr.SequenceEqual(ycbcrBack));
        }

    }
}
