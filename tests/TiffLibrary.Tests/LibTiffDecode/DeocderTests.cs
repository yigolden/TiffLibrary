using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TiffLibrary.PixelFormats;
using Xunit;

namespace TiffLibrary.Tests.LibTiffDecode
{
    public class DeocderTests
    {
        public static IEnumerable<object[]> GetImagePairs()
        {
            yield return new object[]
            {
                // Reference image
                "Assets/LibTiffDecode/cramps.png",
                // Test image
                "Assets/LibTiffDecode/cramps.tif"
            };
            yield return new object[]
            {
                // Reference image
                "Assets/LibTiffDecode/cramps.png",
                // Test image
                "Assets/LibTiffDecode/cramps-tile.tif"
            };
            yield return new object[]
            {
                // Reference image
                "Assets/LibTiffDecode/fax2d.png",
                // Test image
                "Assets/LibTiffDecode/fax2d.tif"
            };
            yield return new object[]
            {
                // Reference image
                "Assets/LibTiffDecode/jello.png",
                // Test image
                "Assets/LibTiffDecode/jello.tif"
            };
            yield return new object[]
            {
                // Reference image
                "Assets/LibTiffDecode/oxford.png",
                // Test image
                "Assets/LibTiffDecode/oxford.tif"
            };
            yield return new object[]
            {
                // Reference image
                "Assets/LibTiffDecode/pc260001.png",
                // Test image
                "Assets/LibTiffDecode/pc260001.tif"
            };
            yield return new object[]
            {
                // Reference image
                "Assets/LibTiffDecode/quad-jpeg.png",
                // Test image
                "Assets/LibTiffDecode/quad-jpeg.tif"
            };
            yield return new object[]
            {
                // Reference image
                "Assets/LibTiffDecode/quad.png",
                // Test image
                "Assets/LibTiffDecode/quad-lzw.tif"
            };
            yield return new object[]
            {
                // Reference image
                "Assets/LibTiffDecode/quad.png",
                // Test image
                "Assets/LibTiffDecode/quad-tile.tif"
            };
            yield return new object[]
            {
                // Reference image
                "Assets/LibTiffDecode/smallliz.png",
                // Test image
                "Assets/LibTiffDecode/smallliz.tif"
            };
            yield return new object[]
            {
                // Reference image
                "Assets/LibTiffDecode/strike.png",
                // Test image
                "Assets/LibTiffDecode/strike.tif"
            };
            yield return new object[]
            {
                // Reference image
                "Assets/LibTiffDecode/ycbcr-cat.png",
                // Test image
                "Assets/LibTiffDecode/ycbcr-cat.tif"
            };
            yield return new object[]
            {
                // Reference image
                "Assets/LibTiffDecode/zackthecat.png",
                // Test image
                "Assets/LibTiffDecode/zackthecat.tif"
            };
            yield return new object[]
            {
                // Reference image
                "Assets/LibTiffDecode/thunderscan.png",
                // Test image
                "Assets/LibTiffDecode/thunderscan.tif"
            };
        }

        [Theory]
        [MemberData(nameof(GetImagePairs))]
        public void Test(string reference, string test)
        {
            using var refImage = Image.Load<Rgba32>(reference);

            using var tiff = TiffFileReader.Open(test);

            TiffStreamOffset ifdOffset = tiff.FirstImageFileDirectoryOffset;
            while (!ifdOffset.IsZero)
            {
                TiffImageFileDirectory ifd = tiff.ReadImageFileDirectory(ifdOffset);
                TiffImageDecoder decoder = tiff.CreateImageDecoder(ifd, new TiffImageDecoderOptions { UndoColorPreMultiplying = true });

                Assert.Equal(refImage.Width, decoder.Width);
                Assert.Equal(refImage.Height, decoder.Height);
                TiffRgba32[] pixels = new TiffRgba32[decoder.Width * decoder.Height];

                decoder.Decode(TiffPixelBuffer.Wrap(pixels, decoder.Width, decoder.Height));

                AssertEqual(refImage, pixels);

                using (var image = new Image<Rgba32>(decoder.Width, decoder.Height))
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
            using var refImage = Image.Load<Rgba32>(reference);

            await using TiffFileReader tiff = await TiffFileReader.OpenAsync(test);

            TiffStreamOffset ifdOffset = tiff.FirstImageFileDirectoryOffset;
            while (!ifdOffset.IsZero)
            {
                TiffImageFileDirectory ifd = await tiff.ReadImageFileDirectoryAsync(ifdOffset);
                TiffImageDecoder decoder = await tiff.CreateImageDecoderAsync(ifd, new TiffImageDecoderOptions { UndoColorPreMultiplying = true });

                Assert.Equal(refImage.Width, decoder.Width);
                Assert.Equal(refImage.Height, decoder.Height);
                TiffRgba32[] pixels = new TiffRgba32[decoder.Width * decoder.Height];

                await decoder.DecodeAsync(TiffPixelBuffer.Wrap(pixels, decoder.Width, decoder.Height));

                AssertEqual(refImage, pixels);

                using (var image = new Image<Rgba32>(decoder.Width, decoder.Height))
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
