using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TiffLibrary.PixelFormats;
using Xunit;

namespace TiffLibrary.Tests.PhotometricInterpretations
{
    public class RgbTests
    {
        public static IEnumerable<object[]> GetTestFileList()
        {
            int[] bitDepths = new int[] { 32, 24, 16, 14, 12, 10, 8, 4, 2 };
            for (int i = 0; i < bitDepths.Length; i++)
            {
                // ref: chunky big endian  -  test: chunky little endian
                yield return new object[]
                {
                    // Reference image
                    "Assets/PhotometricInterpretation/flower-rgb-contig-big-endian.tif",
                    // Reference IFD index
                    i,
                    // Bit depth
                    bitDepths[i],
                    // Test image
                    "Assets/PhotometricInterpretation/flower-rgb-contig-little-endian.tif",
                    // Test IFD index
                    i,
                    // Bit depth
                    bitDepths[i]
                };

                // ref: chunky big endian  -  test: planar big endian
                yield return new object[]
                {
                    // Reference image
                    "Assets/PhotometricInterpretation/flower-rgb-contig-big-endian.tif",
                    // Reference IFD index
                    i,
                    // Bit depth
                    bitDepths[i],
                    // Test image
                    "Assets/PhotometricInterpretation/flower-rgb-planar-big-endian.tif",
                    // Test IFD index
                    i,
                    // Bit depth
                    bitDepths[i]
                };

                // ref: planar big endian  -  test: planr little endian
                yield return new object[]
                {
                    // Reference image
                    "Assets/PhotometricInterpretation/flower-rgb-planar-big-endian.tif",
                    // Reference IFD index
                    i,
                    // Bit depth
                    bitDepths[i],
                    // Test image
                    "Assets/PhotometricInterpretation/flower-rgb-planar-little-endian.tif",
                    // Test IFD index
                    i,
                    // Bit depth
                    bitDepths[i]
                };

                // ref: planar little endian  -  test: chunky little endian
                yield return new object[]
                {
                    // Reference image
                    "Assets/PhotometricInterpretation/flower-rgb-planar-little-endian.tif",
                    // Reference IFD index
                    i,
                    // Bit depth
                    bitDepths[i],
                    // Test image
                    "Assets/PhotometricInterpretation/flower-rgb-contig-little-endian.tif",
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
            TiffImageDecoder refDecoder = await refTiff.CreateImageDecoderAsync(ifd);

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
            TiffImageDecoder testDecoder = await testTiff.CreateImageDecoderAsync(ifd);

            Assert.Equal(refDecoder.Width, testDecoder.Width);
            Assert.Equal(refDecoder.Height, testDecoder.Height);

            TiffRgba64[] refBuffer = new TiffRgba64[refDecoder.Width * refDecoder.Height];
            TiffRgba64[] testBuffer = new TiffRgba64[testDecoder.Width * testDecoder.Height];

            await refDecoder.DecodeAsync(TiffPixelBuffer.Wrap(refBuffer, refDecoder.Width, refDecoder.Height));
            await testDecoder.DecodeAsync(TiffPixelBuffer.Wrap(testBuffer, testDecoder.Width, testDecoder.Height));

            ShiftPixels(MemoryMarshal.Cast<TiffRgba64, ushort>(refBuffer), BufferBitDepth - refBitDepth);
            ShiftPixels(MemoryMarshal.Cast<TiffRgba64, ushort>(testBuffer), BufferBitDepth - testBitDepth);

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
