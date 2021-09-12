using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TiffLibrary.PixelFormats;
using Xunit;

namespace TiffLibrary.Tests.ImageDecode
{
    public class YCbCrSubsamplingTests
    {
        public static IEnumerable<object[]> GetImagePairs()
        {
            yield return new object[]
            {
                // Reference image
                "Assets/Image/flower-ycbcr-contig-08_h1v1.png",
                // Test image
                "Assets/Image/flower-ycbcr-contig-08_h1v1.tiff"
            };
            yield return new object[]
            {
                // Reference image
                "Assets/Image/flower-ycbcr-contig-08_h2v1.png",
                // Test image
                "Assets/Image/flower-ycbcr-contig-08_h2v1.tiff"
            };
            yield return new object[]
            {
                // Reference image
                "Assets/Image/flower-ycbcr-contig-08_h2v2.png",
                // Test image
                "Assets/Image/flower-ycbcr-contig-08_h2v2.tiff"
            };
            yield return new object[]
            {
                // Reference image
                "Assets/Image/flower-ycbcr-contig-08_h4v4.png",
                // Test image
                "Assets/Image/flower-ycbcr-contig-08_h4v4.tiff"
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

                decoder.Decode(TiffPixelBuffer.Wrap(pixels, decoder.Width, decoder.Height));

                AssertEqual(refImage, pixels);

                using (var image = new Image<Rgb24>(decoder.Width, decoder.Height))
                {
                    decoder.Decode(image);
                    AssertEqual(refImage, image);
                }

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

                await decoder.DecodeAsync(TiffPixelBuffer.Wrap(pixels, decoder.Width, decoder.Height));

                AssertEqual(refImage, pixels);

                using (var image = new Image<Rgb24>(decoder.Width, decoder.Height))
                {
                    decoder.Decode(image);
                    AssertEqual(refImage, image);
                }

                ifdOffset = ifd.NextOffset;
            }
        }

        private static void AssertEqual<T1, T2>(Image<T1> refImage, T2[] testImage) where T1 : unmanaged, IPixel<T1> where T2 : unmanaged
        {
            Assert.Equal(Unsafe.SizeOf<T1>(), Unsafe.SizeOf<T2>());
            int width = refImage.Width;
            int height = refImage.Height;
            Assert.Equal(width * height, testImage.Length);
            for (int i = 0; i < height; i++)
            {
                Assert.True(MemoryMarshal.AsBytes(refImage.GetPixelRowSpan(i)).SequenceEqual(MemoryMarshal.AsBytes(testImage.AsSpan(i * width, width))));
            }
        }

        private static void AssertEqual<T1, T2>(Image<T1> refImage, Image<T2> testImage) where T1 : unmanaged, IPixel<T1> where T2 : unmanaged, IPixel<T2>
        {
            Assert.Equal(Unsafe.SizeOf<T1>(), Unsafe.SizeOf<T2>());
            Assert.Equal(refImage.Width, testImage.Width);
            Assert.Equal(refImage.Height, testImage.Height);
            for (int i = 0; i < refImage.Height; i++)
            {
                Assert.True(MemoryMarshal.AsBytes(refImage.GetPixelRowSpan(i)).SequenceEqual(MemoryMarshal.AsBytes(testImage.GetPixelRowSpan(i))));
            }
        }
    }
}
