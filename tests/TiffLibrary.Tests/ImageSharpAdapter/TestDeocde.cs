using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TiffLibrary.ImageSharpAdapter;
using Xunit;

namespace TiffLibrary.Tests.ImageSharpAdapter
{
    public class TestDeocde
    {
        [Fact]
        public void TestUnknownPixelType()
        {
            using var reference = Image.Load<Rgba1010102>(@"Assets/LibTiffDecode/quad-lzw.tif", new TiffDecoder());
            using var test = Image.Load<Rgba1010102>(@"Assets/LibTiffDecode/quad.png");

            Assert.Equal(reference.Width, test.Width);
            Assert.Equal(reference.Height, test.Height);

            for (int i = 0; i < reference.Height; i++)
            {
                Assert.True(reference.GetPixelRowSpan(i).SequenceEqual(test.GetPixelRowSpan(i)));
            }
        }
    }
}
