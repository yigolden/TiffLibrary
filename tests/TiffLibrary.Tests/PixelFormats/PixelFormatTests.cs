using System.Runtime.CompilerServices;
using TiffLibrary.PixelFormats;
using Xunit;

namespace TiffLibrary.Tests.PixelFormats
{
    public class PixelFormatTests
    {
        [Fact]
        public void TestGray8()
        {
            Assert.Equal(1, Unsafe.SizeOf<TiffGray8>());

            var defaultPixel = default(TiffGray8);
            Assert.Equal(0, defaultPixel.Intensity);

            var pixel1 = new TiffGray8(0x12);
            Assert.Equal(0x12, pixel1.Intensity);

            Assert.False(pixel1.Equals(defaultPixel));
            Assert.False(defaultPixel.Equals(pixel1));
            Assert.False(pixel1 == defaultPixel);
            Assert.False(defaultPixel == pixel1);
            Assert.True(pixel1 != defaultPixel);
            Assert.True(defaultPixel != pixel1);
            Assert.False(pixel1.GetHashCode() == defaultPixel.GetHashCode());

            var pixel2 = new TiffGray8(0x12);
            Assert.True(pixel1.Equals(pixel2));
            Assert.True(pixel2.Equals(pixel1));
            Assert.True(pixel1 == pixel2);
            Assert.True(pixel2 == pixel1);
            Assert.False(pixel1 != pixel2);
            Assert.False(pixel2 != pixel1);
            Assert.True(pixel1.GetHashCode() == pixel2.GetHashCode());
        }

        [Fact]
        public void TestMask()
        {
            Assert.Equal(1, Unsafe.SizeOf<TiffMask>());

            var defaultPixel = default(TiffMask);
            Assert.Equal(0, defaultPixel.Opacity);

            var pixel1 = new TiffMask(0x12);
            Assert.Equal(0x12, pixel1.Opacity);

            Assert.False(pixel1.Equals(defaultPixel));
            Assert.False(defaultPixel.Equals(pixel1));
            Assert.False(pixel1 == defaultPixel);
            Assert.False(defaultPixel == pixel1);
            Assert.True(pixel1 != defaultPixel);
            Assert.True(defaultPixel != pixel1);
            Assert.False(pixel1.GetHashCode() == defaultPixel.GetHashCode());

            var pixel2 = new TiffMask(0x12);
            Assert.True(pixel1.Equals(pixel2));
            Assert.True(pixel2.Equals(pixel1));
            Assert.True(pixel1 == pixel2);
            Assert.True(pixel2 == pixel1);
            Assert.False(pixel1 != pixel2);
            Assert.False(pixel2 != pixel1);
            Assert.True(pixel1.GetHashCode() == pixel2.GetHashCode());
        }

        [Fact]
        public void TestGray16()
        {
            Assert.Equal(2, Unsafe.SizeOf<TiffGray16>());

            var defaultPixel = default(TiffGray16);
            Assert.Equal(0, defaultPixel.Intensity);

            var pixel1 = new TiffGray16(0x1234);
            Assert.Equal(0x1234, pixel1.Intensity);

            Assert.False(pixel1.Equals(defaultPixel));
            Assert.False(defaultPixel.Equals(pixel1));
            Assert.False(pixel1 == defaultPixel);
            Assert.False(defaultPixel == pixel1);
            Assert.True(pixel1 != defaultPixel);
            Assert.True(defaultPixel != pixel1);
            Assert.False(pixel1.GetHashCode() == defaultPixel.GetHashCode());

            var pixel2 = new TiffGray16(0x1234);
            Assert.True(pixel1.Equals(pixel2));
            Assert.True(pixel2.Equals(pixel1));
            Assert.True(pixel1 == pixel2);
            Assert.True(pixel2 == pixel1);
            Assert.False(pixel1 != pixel2);
            Assert.False(pixel2 != pixel1);
            Assert.True(pixel1.GetHashCode() == pixel2.GetHashCode());
        }

        [Fact]
        public void TestCmyk32()
        {
            Assert.Equal(4, Unsafe.SizeOf<TiffCmyk32>());

            var defaultPixel = default(TiffCmyk32);
            Assert.Equal(0, defaultPixel.C);
            Assert.Equal(0, defaultPixel.M);
            Assert.Equal(0, defaultPixel.Y);
            Assert.Equal(0, defaultPixel.K);

            var pixel1 = new TiffCmyk32(0x12, 0x34, 0x56, 0x78);
            Assert.Equal(0x12, pixel1.C);
            Assert.Equal(0x34, pixel1.M);
            Assert.Equal(0x56, pixel1.Y);
            Assert.Equal(0x78, pixel1.K);

            Assert.False(pixel1.Equals(defaultPixel));
            Assert.False(defaultPixel.Equals(pixel1));
            Assert.False(pixel1 == defaultPixel);
            Assert.False(defaultPixel == pixel1);
            Assert.True(pixel1 != defaultPixel);
            Assert.True(defaultPixel != pixel1);
            Assert.False(pixel1.GetHashCode() == defaultPixel.GetHashCode());

            var pixel2 = new TiffCmyk32(0x12, 0x34, 0x56, 0x78);
            Assert.True(pixel1.Equals(pixel2));
            Assert.True(pixel2.Equals(pixel1));
            Assert.True(pixel1 == pixel2);
            Assert.True(pixel2 == pixel1);
            Assert.False(pixel1 != pixel2);
            Assert.False(pixel2 != pixel1);
            Assert.True(pixel1.GetHashCode() == pixel2.GetHashCode());
        }

        [Fact]
        public void TestCmyk64()
        {
            Assert.Equal(8, Unsafe.SizeOf<TiffCmyk64>());

            var defaultPixel = default(TiffCmyk64);
            Assert.Equal(0, defaultPixel.C);
            Assert.Equal(0, defaultPixel.M);
            Assert.Equal(0, defaultPixel.Y);
            Assert.Equal(0, defaultPixel.K);

            var pixel1 = new TiffCmyk64(0x1221, 0x3443, 0x5665, 0x7887);
            Assert.Equal(0x1221, pixel1.C);
            Assert.Equal(0x3443, pixel1.M);
            Assert.Equal(0x5665, pixel1.Y);
            Assert.Equal(0x7887, pixel1.K);

            Assert.False(pixel1.Equals(defaultPixel));
            Assert.False(defaultPixel.Equals(pixel1));
            Assert.False(pixel1 == defaultPixel);
            Assert.False(defaultPixel == pixel1);
            Assert.True(pixel1 != defaultPixel);
            Assert.True(defaultPixel != pixel1);
            Assert.False(pixel1.GetHashCode() == defaultPixel.GetHashCode());

            var pixel2 = new TiffCmyk64(0x1221, 0x3443, 0x5665, 0x7887);
            Assert.True(pixel1.Equals(pixel2));
            Assert.True(pixel2.Equals(pixel1));
            Assert.True(pixel1 == pixel2);
            Assert.True(pixel2 == pixel1);
            Assert.False(pixel1 != pixel2);
            Assert.False(pixel2 != pixel1);
            Assert.True(pixel1.GetHashCode() == pixel2.GetHashCode());
        }

        [Fact]
        public void TestRgba32()
        {
            Assert.Equal(4, Unsafe.SizeOf<TiffRgba32>());

            var defaultPixel = default(TiffRgba32);
            Assert.Equal(0, defaultPixel.R);
            Assert.Equal(0, defaultPixel.G);
            Assert.Equal(0, defaultPixel.B);
            Assert.Equal(0, defaultPixel.A);

            var pixel1 = new TiffRgba32(0x12, 0x34, 0x56, 0x78);
            Assert.Equal(0x12, pixel1.R);
            Assert.Equal(0x34, pixel1.G);
            Assert.Equal(0x56, pixel1.B);
            Assert.Equal(0x78, pixel1.A);

            Assert.False(pixel1.Equals(defaultPixel));
            Assert.False(defaultPixel.Equals(pixel1));
            Assert.False(pixel1 == defaultPixel);
            Assert.False(defaultPixel == pixel1);
            Assert.True(pixel1 != defaultPixel);
            Assert.True(defaultPixel != pixel1);
            Assert.False(pixel1.GetHashCode() == defaultPixel.GetHashCode());

            var pixel2 = new TiffRgba32(0x12, 0x34, 0x56, 0x78);
            Assert.True(pixel1.Equals(pixel2));
            Assert.True(pixel2.Equals(pixel1));
            Assert.True(pixel1 == pixel2);
            Assert.True(pixel2 == pixel1);
            Assert.False(pixel1 != pixel2);
            Assert.False(pixel2 != pixel1);
            Assert.True(pixel1.GetHashCode() == pixel2.GetHashCode());
        }

        [Fact]
        public void TestBgra32()
        {
            Assert.Equal(4, Unsafe.SizeOf<TiffBgra32>());

            var defaultPixel = default(TiffBgra32);
            Assert.Equal(0, defaultPixel.B);
            Assert.Equal(0, defaultPixel.G);
            Assert.Equal(0, defaultPixel.R);
            Assert.Equal(0, defaultPixel.A);

            var pixel1 = new TiffBgra32(0x12, 0x34, 0x56, 0x78);
            Assert.Equal(0x12, pixel1.B);
            Assert.Equal(0x34, pixel1.G);
            Assert.Equal(0x56, pixel1.R);
            Assert.Equal(0x78, pixel1.A);

            Assert.False(pixel1.Equals(defaultPixel));
            Assert.False(defaultPixel.Equals(pixel1));
            Assert.False(pixel1 == defaultPixel);
            Assert.False(defaultPixel == pixel1);
            Assert.True(pixel1 != defaultPixel);
            Assert.True(defaultPixel != pixel1);
            Assert.False(pixel1.GetHashCode() == defaultPixel.GetHashCode());

            var pixel2 = new TiffBgra32(0x12, 0x34, 0x56, 0x78);
            Assert.True(pixel1.Equals(pixel2));
            Assert.True(pixel2.Equals(pixel1));
            Assert.True(pixel1 == pixel2);
            Assert.True(pixel2 == pixel1);
            Assert.False(pixel1 != pixel2);
            Assert.False(pixel2 != pixel1);
            Assert.True(pixel1.GetHashCode() == pixel2.GetHashCode());
        }

        [Fact]
        public void TestRgb24()
        {
            Assert.Equal(3, Unsafe.SizeOf<TiffRgb24>());

            var defaultPixel = default(TiffRgb24);
            Assert.Equal(0, defaultPixel.R);
            Assert.Equal(0, defaultPixel.G);
            Assert.Equal(0, defaultPixel.B);

            var pixel1 = new TiffRgb24(0x12, 0x34, 0x56);
            Assert.Equal(0x12, pixel1.R);
            Assert.Equal(0x34, pixel1.G);
            Assert.Equal(0x56, pixel1.B);

            Assert.False(pixel1.Equals(defaultPixel));
            Assert.False(defaultPixel.Equals(pixel1));
            Assert.False(pixel1 == defaultPixel);
            Assert.False(defaultPixel == pixel1);
            Assert.True(pixel1 != defaultPixel);
            Assert.True(defaultPixel != pixel1);
            Assert.False(pixel1.GetHashCode() == defaultPixel.GetHashCode());

            var pixel2 = new TiffRgb24(0x12, 0x34, 0x56);
            Assert.True(pixel1.Equals(pixel2));
            Assert.True(pixel2.Equals(pixel1));
            Assert.True(pixel1 == pixel2);
            Assert.True(pixel2 == pixel1);
            Assert.False(pixel1 != pixel2);
            Assert.False(pixel2 != pixel1);
            Assert.True(pixel1.GetHashCode() == pixel2.GetHashCode());
        }

        [Fact]
        public void TestBgr24()
        {
            Assert.Equal(3, Unsafe.SizeOf<TiffBgr24>());

            var defaultPixel = default(TiffBgr24);
            Assert.Equal(0, defaultPixel.B);
            Assert.Equal(0, defaultPixel.G);
            Assert.Equal(0, defaultPixel.R);

            var pixel1 = new TiffBgr24(0x12, 0x34, 0x56);
            Assert.Equal(0x12, pixel1.B);
            Assert.Equal(0x34, pixel1.G);
            Assert.Equal(0x56, pixel1.R);

            Assert.False(pixel1.Equals(defaultPixel));
            Assert.False(defaultPixel.Equals(pixel1));
            Assert.False(pixel1 == defaultPixel);
            Assert.False(defaultPixel == pixel1);
            Assert.True(pixel1 != defaultPixel);
            Assert.True(defaultPixel != pixel1);
            Assert.False(pixel1.GetHashCode() == defaultPixel.GetHashCode());

            var pixel2 = new TiffBgr24(0x12, 0x34, 0x56);
            Assert.True(pixel1.Equals(pixel2));
            Assert.True(pixel2.Equals(pixel1));
            Assert.True(pixel1 == pixel2);
            Assert.True(pixel2 == pixel1);
            Assert.False(pixel1 != pixel2);
            Assert.False(pixel2 != pixel1);
            Assert.True(pixel1.GetHashCode() == pixel2.GetHashCode());
        }

        [Fact]
        public void TestRgba64()
        {
            Assert.Equal(8, Unsafe.SizeOf<TiffRgba64>());

            var defaultPixel = default(TiffRgba64);
            Assert.Equal(0, defaultPixel.R);
            Assert.Equal(0, defaultPixel.G);
            Assert.Equal(0, defaultPixel.B);
            Assert.Equal(0, defaultPixel.A);

            var pixel1 = new TiffRgba64(0x1221, 0x3443, 0x5665, 0x7887);
            Assert.Equal(0x1221, pixel1.R);
            Assert.Equal(0x3443, pixel1.G);
            Assert.Equal(0x5665, pixel1.B);
            Assert.Equal(0x7887, pixel1.A);

            Assert.False(pixel1.Equals(defaultPixel));
            Assert.False(defaultPixel.Equals(pixel1));
            Assert.False(pixel1 == defaultPixel);
            Assert.False(defaultPixel == pixel1);
            Assert.True(pixel1 != defaultPixel);
            Assert.True(defaultPixel != pixel1);
            Assert.False(pixel1.GetHashCode() == defaultPixel.GetHashCode());

            var pixel2 = new TiffRgba64(0x1221, 0x3443, 0x5665, 0x7887);
            Assert.True(pixel1.Equals(pixel2));
            Assert.True(pixel2.Equals(pixel1));
            Assert.True(pixel1 == pixel2);
            Assert.True(pixel2 == pixel1);
            Assert.False(pixel1 != pixel2);
            Assert.False(pixel2 != pixel1);
            Assert.True(pixel1.GetHashCode() == pixel2.GetHashCode());
        }

        [Fact]
        public void TestBgra64()
        {
            Assert.Equal(8, Unsafe.SizeOf<TiffBgra64>());

            var defaultPixel = default(TiffBgra64);
            Assert.Equal(0, defaultPixel.B);
            Assert.Equal(0, defaultPixel.G);
            Assert.Equal(0, defaultPixel.R);
            Assert.Equal(0, defaultPixel.A);

            var pixel1 = new TiffBgra64(0x1221, 0x3443, 0x5665, 0x7887);
            Assert.Equal(0x1221, pixel1.B);
            Assert.Equal(0x3443, pixel1.G);
            Assert.Equal(0x5665, pixel1.R);
            Assert.Equal(0x7887, pixel1.A);

            Assert.False(pixel1.Equals(defaultPixel));
            Assert.False(defaultPixel.Equals(pixel1));
            Assert.False(pixel1 == defaultPixel);
            Assert.False(defaultPixel == pixel1);
            Assert.True(pixel1 != defaultPixel);
            Assert.True(defaultPixel != pixel1);
            Assert.False(pixel1.GetHashCode() == defaultPixel.GetHashCode());

            var pixel2 = new TiffBgra64(0x1221, 0x3443, 0x5665, 0x7887);
            Assert.True(pixel1.Equals(pixel2));
            Assert.True(pixel2.Equals(pixel1));
            Assert.True(pixel1 == pixel2);
            Assert.True(pixel2 == pixel1);
            Assert.False(pixel1 != pixel2);
            Assert.False(pixel2 != pixel1);
            Assert.True(pixel1.GetHashCode() == pixel2.GetHashCode());
        }

    }
}
