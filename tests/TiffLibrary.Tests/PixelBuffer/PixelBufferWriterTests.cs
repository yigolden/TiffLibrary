using System;
using TiffLibrary.PixelBuffer;
using TiffLibrary.PixelFormats;
using Xunit;

namespace TiffLibrary.Tests.PixelBuffer
{
    public class PixelBufferWriterTests
    {

        [Fact]
        public void TestWriterAdapter()
        {
            Assert.Throws<ArgumentNullException>("buffer", () => new TiffPixelBufferWriterAdapter<TiffGray8>(null));

            ITiffPixelBuffer<TiffGray8> pixelBuffer = PixelBufferTests.InitializePixelBuffer(out TiffGray8[] pixels);
            var writer = new TiffPixelBufferWriterAdapter<TiffGray8>(pixelBuffer);

            Assert.Equal(3, writer.Width);
            Assert.Equal(2, writer.Height);
        }

        [Fact]
        public void TestWriteRows()
        {
            ITiffPixelBuffer<TiffGray8> pixelBuffer = PixelBufferTests.InitializePixelBuffer(out TiffGray8[] pixels);
            var writer = new TiffPixelBufferWriterAdapter<TiffGray8>(pixelBuffer);

            // Write to first row
            using (TiffPixelSpanHandle<TiffGray8> pixelSpanHandle = writer.GetRowSpan(0, 0, 1))
            {
                Assert.Equal(1, pixelSpanHandle.Length);
                Span<TiffGray8> span = pixelSpanHandle.GetSpan();
                Assert.Equal(1, span.Length);
                span[0] = new TiffGray8(0xa1);
            }

            // Make sure only the affected pixels are changed while others stay the same.
            Assert.Equal(0xa1, pixels[0].Intensity);
            Assert.Equal(0x12, pixels[1].Intensity);
            Assert.Equal(0x13, pixels[2].Intensity);
            Assert.Equal(0x21, pixels[3].Intensity);
            Assert.Equal(0x22, pixels[4].Intensity);
            Assert.Equal(0x23, pixels[5].Intensity);

            // Write to second row
            using (TiffPixelSpanHandle<TiffGray8> pixelSpanHandle = writer.GetRowSpan(1, 1, 2))
            {
                Assert.Equal(2, pixelSpanHandle.Length);
                Span<TiffGray8> span = pixelSpanHandle.GetSpan();
                Assert.Equal(2, span.Length);
                span[0] = new TiffGray8(0xa2);
                span[1] = new TiffGray8(0xa3);
            }

            // Make sure only the affected pixels are changed while others stay the same.
            Assert.Equal(0xa1, pixels[0].Intensity);
            Assert.Equal(0x12, pixels[1].Intensity);
            Assert.Equal(0x13, pixels[2].Intensity);
            Assert.Equal(0x21, pixels[3].Intensity);
            Assert.Equal(0xa2, pixels[4].Intensity);
            Assert.Equal(0xa3, pixels[5].Intensity);

            // Failure cases
            Assert.Throws<ArgumentOutOfRangeException>("rowIndex", () => writer.GetRowSpan(-1, 1, 1));
            Assert.Throws<ArgumentOutOfRangeException>("rowIndex", () => writer.GetRowSpan(2, 1, 1));
            Assert.Throws<ArgumentOutOfRangeException>("start", () => writer.GetRowSpan(0, -1, 1));
            Assert.Throws<ArgumentOutOfRangeException>("start", () => writer.GetRowSpan(0, 4, 1));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => writer.GetRowSpan(0, 1, -1));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => writer.GetRowSpan(0, 1, 3));
        }

        [Fact]
        public void TestWriteColumns()
        {
            ITiffPixelBuffer<TiffGray8> pixelBuffer = PixelBufferTests.InitializePixelBuffer(out TiffGray8[] pixels);
            var writer = new TiffPixelBufferWriterAdapter<TiffGray8>(pixelBuffer);

            // Write to first column
            using (TiffPixelSpanHandle<TiffGray8> pixelSpanHandle = writer.GetColumnSpan(0, 0, 1))
            {
                Assert.Equal(1, pixelSpanHandle.Length);
                Span<TiffGray8> span = pixelSpanHandle.GetSpan();
                Assert.Equal(1, span.Length);
                span[0] = new TiffGray8(0xb1);
            }

            // Make sure only the affected pixels are changed while others stay the same.
            Assert.Equal(0xb1, pixels[0].Intensity);
            Assert.Equal(0x12, pixels[1].Intensity);
            Assert.Equal(0x13, pixels[2].Intensity);
            Assert.Equal(0x21, pixels[3].Intensity);
            Assert.Equal(0x22, pixels[4].Intensity);
            Assert.Equal(0x23, pixels[5].Intensity);

            // Write to second column
            using (TiffPixelSpanHandle<TiffGray8> pixelSpanHandle = writer.GetColumnSpan(1, 1, 1))
            {
                Assert.Equal(1, pixelSpanHandle.Length);
                Span<TiffGray8> span = pixelSpanHandle.GetSpan();
                Assert.Equal(1, span.Length);
                span[0] = new TiffGray8(0xb2);
            }

            // Make sure only the affected pixels are changed while others stay the same.
            Assert.Equal(0xb1, pixels[0].Intensity);
            Assert.Equal(0x12, pixels[1].Intensity);
            Assert.Equal(0x13, pixels[2].Intensity);
            Assert.Equal(0x21, pixels[3].Intensity);
            Assert.Equal(0xb2, pixels[4].Intensity);
            Assert.Equal(0x23, pixels[5].Intensity);

            // Write to third column
            using (TiffPixelSpanHandle<TiffGray8> pixelSpanHandle = writer.GetColumnSpan(2, 0, 2))
            {
                Assert.Equal(2, pixelSpanHandle.Length);
                Span<TiffGray8> span = pixelSpanHandle.GetSpan();
                Assert.Equal(2, span.Length);
                span[0] = new TiffGray8(0xb3);
                span[1] = new TiffGray8(0xb4);
            }

            // Make sure only the affected pixels are changed while others stay the same.
            Assert.Equal(0xb1, pixels[0].Intensity);
            Assert.Equal(0x12, pixels[1].Intensity);
            Assert.Equal(0xb3, pixels[2].Intensity);
            Assert.Equal(0x21, pixels[3].Intensity);
            Assert.Equal(0xb2, pixels[4].Intensity);
            Assert.Equal(0xb4, pixels[5].Intensity);

            // Failure cases
            Assert.Throws<ArgumentOutOfRangeException>("colIndex", () => writer.GetColumnSpan(-1, 1, 1));
            Assert.Throws<ArgumentOutOfRangeException>("colIndex", () => writer.GetColumnSpan(3, 1, 1));
            Assert.Throws<ArgumentOutOfRangeException>("start", () => writer.GetColumnSpan(0, -1, 1));
            Assert.Throws<ArgumentOutOfRangeException>("start", () => writer.GetColumnSpan(0, 3, 1));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => writer.GetColumnSpan(0, 1, -1));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => writer.GetColumnSpan(0, 1, 2));
        }

        [Fact]
        public void TestCrop()
        {
            ITiffPixelBuffer<TiffGray8> pixelBuffer = PixelBufferTests.InitializePixelBuffer(out TiffGray8[] pixels);
            var writer = new TiffPixelBufferWriterAdapter<TiffGray8>(pixelBuffer);

            TiffPixelBufferWriter<TiffGray8> structWriter = writer.AsPixelBufferWriter();
            Assert.Equal(3, structWriter.Width);
            Assert.Equal(2, structWriter.Height);
            Assert.False(structWriter.IsEmpty);

            Assert.Throws<ArgumentOutOfRangeException>("offset", () => structWriter.Crop(new TiffPoint(4, 0)));
            Assert.Throws<ArgumentOutOfRangeException>("offset", () => structWriter.Crop(new TiffPoint(0, 3)));
            Assert.Throws<ArgumentOutOfRangeException>("offset", () => structWriter.Crop(new TiffPoint(4, 3)));

            Assert.Throws<ArgumentOutOfRangeException>("size", () => structWriter.Crop(new TiffPoint(0, 0), new TiffSize(4, 1)));
            Assert.Throws<ArgumentOutOfRangeException>("size", () => structWriter.Crop(new TiffPoint(0, 0), new TiffSize(1, 3)));
            Assert.Throws<ArgumentOutOfRangeException>("size", () => structWriter.Crop(new TiffPoint(0, 0), new TiffSize(4, 3)));

            structWriter = writer.Crop(new TiffPoint(1, 1), new TiffSize(1, 1));
            Assert.Equal(1, structWriter.Width);
            Assert.Equal(1, structWriter.Height);

            structWriter = writer.AsPixelBufferWriter().Crop(new TiffPoint(1, 1), new TiffSize(1, 1));
            Assert.Equal(1, structWriter.Width);
            Assert.Equal(1, structWriter.Height);

            structWriter = writer.Crop(new TiffPoint(1, 1));
            Assert.Equal(2, structWriter.Width);
            Assert.Equal(1, structWriter.Height);

            structWriter = writer.AsPixelBufferWriter().Crop(new TiffPoint(1, 1));
            Assert.Equal(2, structWriter.Width);
            Assert.Equal(1, structWriter.Height);

            Assert.Throws<ArgumentOutOfRangeException>("rowIndex", () => structWriter.GetRowSpan(-1, 0, 1));
            Assert.Throws<ArgumentOutOfRangeException>("rowIndex", () => structWriter.GetRowSpan(2, 0, 1));
            Assert.Throws<ArgumentOutOfRangeException>("start", () => structWriter.GetRowSpan(0, -1, 1));
            Assert.Throws<ArgumentOutOfRangeException>("start", () => structWriter.GetRowSpan(0, 3, 1));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => structWriter.GetRowSpan(0, 1, -1));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => structWriter.GetRowSpan(0, 1, 2));

            Assert.Throws<ArgumentOutOfRangeException>("colIndex", () => structWriter.GetColumnSpan(-1, 0, 1));
            Assert.Throws<ArgumentOutOfRangeException>("colIndex", () => structWriter.GetColumnSpan(2, 0, 1));
            Assert.Throws<ArgumentOutOfRangeException>("start", () => structWriter.GetColumnSpan(0, -1, 1));
            Assert.Throws<ArgumentOutOfRangeException>("start", () => structWriter.GetColumnSpan(0, 2, 1));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => structWriter.GetColumnSpan(0, 1, -1));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => structWriter.GetColumnSpan(0, 1, 2));

            ITiffPixelBufferWriter<TiffGray8> writer2 = TiffPixelBufferUnsafeMarshal.GetBuffer(structWriter, out TiffPoint offset, out TiffSize size);
            Assert.True(ReferenceEquals(writer, writer2));
            Assert.Equal(1, offset.X);
            Assert.Equal(1, offset.Y);
            Assert.Equal(2, size.Width);
            Assert.Equal(1, size.Height);
        }

        [Fact]
        public void TestWriteOnStructWriter()
        {
            ITiffPixelBuffer<TiffGray8> pixelBuffer = PixelBufferTests.InitializePixelBuffer(out TiffGray8[] pixels);
            var writer = new TiffPixelBufferWriterAdapter<TiffGray8>(pixelBuffer);
            TiffPixelBufferWriter<TiffGray8> structWriter;

            structWriter = writer.Crop(new TiffPoint(1, 0), new TiffSize(1, 1));
            Assert.Equal(1, structWriter.Width);
            Assert.Equal(1, structWriter.Height);

            using (TiffPixelSpanHandle<TiffGray8> pixelSpanHandle = structWriter.GetRowSpan(0, 0, 1))
            {
                Assert.Equal(1, pixelSpanHandle.Length);
                Span<TiffGray8> span = pixelSpanHandle.GetSpan();
                Assert.Equal(1, span.Length);
                span[0] = new TiffGray8(0xa1);
            }

            Assert.Equal(0x11, pixels[0].Intensity);
            Assert.Equal(0xa1, pixels[1].Intensity);
            Assert.Equal(0x13, pixels[2].Intensity);
            Assert.Equal(0x21, pixels[3].Intensity);
            Assert.Equal(0x22, pixels[4].Intensity);
            Assert.Equal(0x23, pixels[5].Intensity);

            using (TiffPixelSpanHandle<TiffGray8> pixelSpanHandle = structWriter.GetColumnSpan(0, 0, 1))
            {
                Assert.Equal(1, pixelSpanHandle.Length);
                Span<TiffGray8> span = pixelSpanHandle.GetSpan();
                Assert.Equal(1, span.Length);
                span[0] = new TiffGray8(0xa2);
            }

            Assert.Equal(0x11, pixels[0].Intensity);
            Assert.Equal(0xa2, pixels[1].Intensity);
            Assert.Equal(0x13, pixels[2].Intensity);
            Assert.Equal(0x21, pixels[3].Intensity);
            Assert.Equal(0x22, pixels[4].Intensity);
            Assert.Equal(0x23, pixels[5].Intensity);

            // Failure cases
            Assert.Throws<ArgumentOutOfRangeException>("rowIndex", () => structWriter.GetRowSpan(-1, 0, 1));
            Assert.Throws<ArgumentOutOfRangeException>("rowIndex", () => structWriter.GetRowSpan(1, 0, 1));
            Assert.Throws<ArgumentOutOfRangeException>("start", () => structWriter.GetRowSpan(0, -1, 1));
            Assert.Throws<ArgumentOutOfRangeException>("start", () => structWriter.GetRowSpan(0, 2, 1));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => structWriter.GetRowSpan(0, 0, -1));
            Assert.Throws<ArgumentOutOfRangeException>("length", () => structWriter.GetRowSpan(0, 0, 2));
        }
    }
}
