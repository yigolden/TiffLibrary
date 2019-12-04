using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using TiffLibrary.PixelFormats;
using Xunit;

namespace TiffLibrary.Tests.ImageDecode
{
    public class OrientationImageTests
    {
        public static IEnumerable<object[]> GetImagePairs()
        {
            yield return new object[]
            {
                // Reference image
                "Assets/Image/orientation.png",
                // Test image
                "Assets/Image/orientation.tif"
            };
        }


        [Theory]
        [MemberData(nameof(GetImagePairs))]
        public void Test(string reference, string test)
        {
            using var refImage = Image.Load<Rgb24>(reference);

            using var tiff = TiffFileReader.Open(test);

            TiffStreamOffset ifdOffset = tiff.FirstImageFileDirectoryOffset;
            while (!ifdOffset.IsZero)
            {
                TiffImageFileDirectory ifd = tiff.ReadImageFileDirectory(ifdOffset);
                TiffImageDecoder decoder = tiff.CreateImageDecoder(ifd, new TiffImageDecoderOptions { UndoColorPreMultiplying = true });

                Assert.Equal(refImage.Width, decoder.Width);
                Assert.Equal(refImage.Height, decoder.Height);
                TiffRgb24[] pixels = new TiffRgb24[decoder.Width * decoder.Height];

                decoder.Decode(new TiffMemoryPixelBuffer<TiffRgb24>(pixels, decoder.Width, decoder.Height));

                AssertEqual(refImage, pixels);

                ifdOffset = ifd.NextOffset;
            }
        }

        [Theory]
        [MemberData(nameof(GetImagePairs))]
        public async Task TestAsync(string reference, string test)
        {
            using var refImage = Image.Load<Rgb24>(reference);

            await using TiffFileReader tiff = await TiffFileReader.OpenAsync(test);

            TiffStreamOffset ifdOffset = tiff.FirstImageFileDirectoryOffset;
            while (!ifdOffset.IsZero)
            {
                TiffImageFileDirectory ifd = await tiff.ReadImageFileDirectoryAsync(ifdOffset);
                TiffImageDecoder decoder = await tiff.CreateImageDecoderAsync(ifd, new TiffImageDecoderOptions { UndoColorPreMultiplying = true });

                Assert.Equal(refImage.Width, decoder.Width);
                Assert.Equal(refImage.Height, decoder.Height);
                TiffRgb24[] pixels = new TiffRgb24[decoder.Width * decoder.Height];

                await decoder.DecodeAsync(new TiffMemoryPixelBuffer<TiffRgb24>(pixels, decoder.Width, decoder.Height));

                AssertEqual(refImage, pixels);

                ifdOffset = ifd.NextOffset;
            }
        }

        private static void AssertEqual<T1, T2>(Image<T1> refImage, T2[] testImage) where T1 : struct, IPixel<T1> where T2 : unmanaged
        {
            Span<byte> refImageSpan = MemoryMarshal.AsBytes(refImage.GetPixelSpan());
            Span<byte> testImageSpan = MemoryMarshal.AsBytes(testImage.AsSpan());
            Assert.True(refImageSpan.SequenceEqual(testImageSpan));
        }
    }
}
