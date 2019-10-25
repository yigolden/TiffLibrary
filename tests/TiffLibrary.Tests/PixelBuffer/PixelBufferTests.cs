using System;
using System.Runtime.CompilerServices;
using TiffLibrary.PixelBuffer;
using TiffLibrary.PixelFormats;
using Xunit;

namespace TiffLibrary.Tests.PixelBuffer
{
    public class PixelBufferTests
    {
        internal static ITiffPixelBuffer<TiffGray8> InitializePixelBuffer(out TiffGray8[] pixels)
        {
            pixels = new TiffGray8[3 * 2];
            pixels[0] = new TiffGray8(0x11);
            pixels[1] = new TiffGray8(0x12);
            pixels[2] = new TiffGray8(0x13);
            pixels[3] = new TiffGray8(0x21);
            pixels[4] = new TiffGray8(0x22);
            pixels[5] = new TiffGray8(0x23);
            return new TiffMemoryPixelBuffer<TiffGray8>(pixels, 3, 2);
        }

        [Fact]
        public void TestMemoryPixelBuffer()
        {
            ITiffPixelBuffer<TiffGray8> pixelBuffer = InitializePixelBuffer(out TiffGray8[] pixels);
            Assert.Equal(3, pixelBuffer.Width);
            Assert.Equal(2, pixelBuffer.Height);
            Span<TiffGray8> span = pixelBuffer.GetSpan();
            Assert.True(Unsafe.AreSame(ref pixels[0], ref span[0]));
            Assert.Equal(6, span.Length);

            span[0] = new TiffGray8(0xCD);
            span[5] = new TiffGray8(0xEF);

            Assert.Equal(0xCD, pixels[0].Intensity);
            Assert.Equal(0xEF, pixels[5].Intensity);

            Assert.Throws<ArgumentOutOfRangeException>("width", () => new TiffMemoryPixelBuffer<TiffGray8>(Array.Empty<TiffGray8>(), -1, 0));
            Assert.Throws<ArgumentOutOfRangeException>("height", () => new TiffMemoryPixelBuffer<TiffGray8>(Array.Empty<TiffGray8>(), 0, -1));
        }

        [Fact]
        public void TestEmptyPixelBuffer()
        {
            var structBuffer = default(TiffPixelBuffer<TiffGray8>);
            Assert.Equal(0, structBuffer.Width);
            Assert.Equal(0, structBuffer.Height);
            Assert.True(structBuffer.IsEmpty);

            var pixelBuffer = new TiffMemoryPixelBuffer<TiffGray8>(Array.Empty<TiffGray8>(), 0, 0);
            Assert.Equal(0, pixelBuffer.Width);
            Assert.Equal(0, pixelBuffer.Height);
            Assert.True(pixelBuffer.GetSpan().IsEmpty);

            structBuffer = pixelBuffer.AsPixelBuffer();
            Assert.Equal(0, structBuffer.Width);
            Assert.Equal(0, structBuffer.Height);
            Assert.True(structBuffer.IsEmpty);
        }

        [Fact]
        public void TestCrop()
        {
            ITiffPixelBuffer<TiffGray8> pixelBuffer = InitializePixelBuffer(out TiffGray8[] pixels);
            TiffPixelBuffer<TiffGray8> structBuffer = pixelBuffer.AsPixelBuffer();
            Assert.Equal(3, structBuffer.Width);
            Assert.Equal(2, structBuffer.Height);
            Assert.False(structBuffer.IsEmpty);

            Assert.Throws<ArgumentOutOfRangeException>("offset", () => structBuffer.Crop(new TiffPoint(4, 0)));
            Assert.Throws<ArgumentOutOfRangeException>("offset", () => structBuffer.Crop(new TiffPoint(0, 3)));
            Assert.Throws<ArgumentOutOfRangeException>("offset", () => structBuffer.Crop(new TiffPoint(4, 3)));

            Assert.Throws<ArgumentOutOfRangeException>("size", () => structBuffer.Crop(new TiffPoint(0, 0), new TiffSize(4, 1)));
            Assert.Throws<ArgumentOutOfRangeException>("size", () => structBuffer.Crop(new TiffPoint(0, 0), new TiffSize(1, 3)));
            Assert.Throws<ArgumentOutOfRangeException>("size", () => structBuffer.Crop(new TiffPoint(0, 0), new TiffSize(4, 3)));

            structBuffer = pixelBuffer.Crop(new TiffPoint(1, 1), new TiffSize(1, 1));
            Assert.Equal(1, structBuffer.Width);
            Assert.Equal(1, structBuffer.Height);

            structBuffer = pixelBuffer.AsPixelBuffer().Crop(new TiffPoint(1, 1), new TiffSize(1, 1));
            Assert.Equal(1, structBuffer.Width);
            Assert.Equal(1, structBuffer.Height);

            structBuffer = pixelBuffer.Crop(new TiffPoint(1, 1));
            Assert.Equal(2, structBuffer.Width);
            Assert.Equal(1, structBuffer.Height);

            structBuffer = pixelBuffer.AsPixelBuffer().Crop(new TiffPoint(1, 1));
            Assert.Equal(2, structBuffer.Width);
            Assert.Equal(1, structBuffer.Height);

            ITiffPixelBuffer<TiffGray8> pixelBuffer2 = TiffPixelBufferUnsafeMarshal.GetBuffer(structBuffer, out TiffPoint offset, out TiffSize size);
            Assert.True(ReferenceEquals(pixelBuffer, pixelBuffer2));
            Assert.Equal(1, offset.X);
            Assert.Equal(1, offset.Y);
            Assert.Equal(2, size.Width);
            Assert.Equal(1, size.Height);
        }
    }
}
