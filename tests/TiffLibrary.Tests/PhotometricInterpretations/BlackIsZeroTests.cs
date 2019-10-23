using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TiffLibrary.PixelFormats;
using Xunit;

namespace TiffLibrary.Tests.PhotometricInterpretations
{
    public class BlackIsZeroTests
    {
        public static IEnumerable<object[]> GetTestFileList()
        {
            int[] bitDepths = new int[] { 32, 24, 16, 14, 12, 10, 8, 6, 4, 2 };
            for (int i = 0; i < bitDepths.Length; i++)
            {
                yield return new object[]
                {
                    // Reference image
                    "Assets/PhotometricInterpretation/flower-minisblack-big-endian.tif",
                    // Reference IFD index
                    i,
                    // Bit depth
                    bitDepths[i],
                    // Test image
                    "Assets/PhotometricInterpretation/flower-minisblack-little-endian.tif",
                    // Test IFD index
                    i,
                    // Bit depth
                    bitDepths[i]
                };
            }
        }

        [Theory]
        [MemberData(nameof(GetTestFileList))]
        public async Task TestImageDecodeAsync(string refImage, int refIfd, int refBitDepth, string testImage, int testIfd, int testBitDepth)
        {
            const int BufferBitDepth = 16;

            // We do not support decoding into 32 bit buffer yet.
            refBitDepth = Math.Min(refBitDepth, BufferBitDepth);
            testBitDepth = Math.Min(testBitDepth, BufferBitDepth);

            TiffStreamOffset ifdOffset;
            TiffImageFileDirectory ifd;

            // Load reference image
            await using TiffFileReader refTiff = await TiffFileReader.OpenAsync(refImage);
            ifdOffset = refTiff.FirstImageFileDirectoryOffset;
            Assert.False(ifdOffset.IsZero);
            for (int i = 0; i < refIfd; i++)
            {
                ifd = await refTiff.ReadImageFileDirectoryAsync(ifdOffset);
                ifdOffset = ifd.NextOffset;
                Assert.False(ifdOffset.IsZero);
            }
            ifd = await refTiff.ReadImageFileDirectoryAsync(ifdOffset);
            var refDecoder = await refTiff.CreateImageDecoderAsync(ifd);

            // Load test image
            await using TiffFileReader testTiff = await TiffFileReader.OpenAsync(testImage);
            ifdOffset = testTiff.FirstImageFileDirectoryOffset;
            Assert.False(ifdOffset.IsZero);
            for (int i = 0; i < testIfd; i++)
            {
                ifd = await testTiff.ReadImageFileDirectoryAsync(ifdOffset);
                ifdOffset = ifd.NextOffset;
                Assert.False(ifdOffset.IsZero);
            }
            ifd = await testTiff.ReadImageFileDirectoryAsync(ifdOffset);
            var testDecoder = await testTiff.CreateImageDecoderAsync(ifd);

            Assert.Equal(refDecoder.Width, testDecoder.Width);
            Assert.Equal(refDecoder.Height, testDecoder.Height);

            TiffGray16[] refBuffer = new TiffGray16[refDecoder.Width * refDecoder.Height];
            TiffGray16[] testBuffer = new TiffGray16[testDecoder.Width * testDecoder.Height];

            await refDecoder.DecodeAsync(new TiffMemoryPixelBuffer<TiffGray16>(refBuffer, refDecoder.Width, refDecoder.Height));
            await testDecoder.DecodeAsync(new TiffMemoryPixelBuffer<TiffGray16>(testBuffer, testDecoder.Width, testDecoder.Height));

            ShiftPixels(MemoryMarshal.Cast<TiffGray16, ushort>(refBuffer), BufferBitDepth - refBitDepth);
            ShiftPixels(MemoryMarshal.Cast<TiffGray16, ushort>(testBuffer), BufferBitDepth - testBitDepth);

            Assert.True(refBuffer.AsSpan().SequenceEqual(testBuffer));
        }

        private static void ShiftPixels(Span<ushort> buffer, int bitCount)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (ushort)(buffer[i] >> bitCount);
            }

        }

    }
}
