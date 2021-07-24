using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace TiffLibrary.Tests.ImageDecode
{
    public class OddIfdTests
    {
        [Theory]
        [InlineData("Assets/Image/odd_ifd.tif")]
        [InlineData("Assets/Image/odd_ifd_bigtiff.tif")]
        public void Test(string fileName)
        {
            using var tiff = TiffFileReader.Open(fileName);

            TiffImageFileDirectory ifd = tiff.ReadImageFileDirectory();
            TiffImageDecoder decoder = tiff.CreateImageDecoder(ifd);

            Assert.Equal(16, decoder.Width);
            Assert.Equal(16, decoder.Height);

            using var refImage = new Image<Rgba32>(16, 16, Color.White);
            using var testImage = new Image<Rgba32>(16, 16);
            decoder.Decode(testImage);
            AssertEqual(refImage, testImage);

            Assert.False(ifd.NextOffset.IsZero);
            ifd = tiff.ReadImageFileDirectory(ifd.NextOffset);
            decoder = tiff.CreateImageDecoder(ifd);

            Assert.Equal(16, decoder.Width);
            Assert.Equal(16, decoder.Height);

            using var refImage2 = new Image<Rgba32>(16, 16, Color.Black);
            decoder.Decode(testImage);
            AssertEqual(refImage2, testImage);

            Assert.True(ifd.NextOffset.IsZero);
        }

        [Theory]
        [InlineData("Assets/Image/odd_ifd.tif")]
        [InlineData("Assets/Image/odd_ifd_bigtiff.tif")]
        public async Task TestAsync(string fileName)
        {
            await using TiffFileReader tiff = await TiffFileReader.OpenAsync(fileName);

            TiffImageFileDirectory ifd = await tiff.ReadImageFileDirectoryAsync();
            TiffImageDecoder decoder = await tiff.CreateImageDecoderAsync(ifd);

            Assert.Equal(16, decoder.Width);
            Assert.Equal(16, decoder.Height);

            using var refImage = new Image<Rgba32>(16, 16, Color.White);
            using var testImage = new Image<Rgba32>(16, 16);
            await decoder.DecodeAsync(testImage);
            AssertEqual(refImage, testImage);

            Assert.False(ifd.NextOffset.IsZero);
            ifd = await tiff.ReadImageFileDirectoryAsync(ifd.NextOffset);
            decoder = await tiff.CreateImageDecoderAsync(ifd);

            Assert.Equal(16, decoder.Width);
            Assert.Equal(16, decoder.Height);

            using var refImage2 = new Image<Rgba32>(16, 16, Color.Black);
            await decoder.DecodeAsync(testImage);
            AssertEqual(refImage2, testImage);

            Assert.True(ifd.NextOffset.IsZero);
        }


        private static void AssertEqual<T1, T2>(Image<T1> refImage, Image<T2> testImage) where T1 : unmanaged, IPixel<T1> where T2 : unmanaged, IPixel<T2>
        {
            Assert.Equal(Unsafe.SizeOf<T1>(), Unsafe.SizeOf<T2>());
            Assert.Equal(refImage.Width, testImage.Width);
            Assert.Equal(refImage.Height, testImage.Height);
            for (int i = 0; i < refImage.Height; i++)
            {
                ReadOnlySpan<byte> refSpan = MemoryMarshal.AsBytes(refImage.GetPixelRowSpan(i));
                ReadOnlySpan<byte> testSpan = MemoryMarshal.AsBytes(testImage.GetPixelRowSpan(i));
                Assert.True(refSpan.SequenceEqual(testSpan));
            }
        }
    }
}
