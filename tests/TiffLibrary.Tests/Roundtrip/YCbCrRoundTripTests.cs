using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TiffLibrary.PixelFormats;
using Xunit;

namespace TiffLibrary.Tests.Roundtrip
{
    public class YCbCrRoundTripTests
    {
        private static readonly TiffRgb24 s_color = new TiffRgb24(192, 129, 0);

        public static IEnumerable<object[]> GetTestData()
        {
            foreach (int horizontalChromaSubSampling in new int[] { 1, 2, 4 })
            {
                foreach (int verticalChromaSubSampling in new int[] { 1, 2, 4 })
                {
                    yield return new object[]
                    {
                            // width, height
                            1, 1,
                            horizontalChromaSubSampling, verticalChromaSubSampling,
                            // isTile, strileSize
                            false, 100
                    };
                    yield return new object[]
                    {
                            // width, height
                            2, 2,
                            horizontalChromaSubSampling, verticalChromaSubSampling,
                            // isTile, strileSize
                            false, 100
                    };
                    yield return new object[]
                    {
                            // width, height
                            3, 3,
                            horizontalChromaSubSampling, verticalChromaSubSampling,
                            // isTile, strileSize
                            false, 100
                    };
                    yield return new object[]
                    {
                            // width, height
                            4, 4,
                            horizontalChromaSubSampling, verticalChromaSubSampling,
                            // isTile, strileSize
                            false, 100
                    };
                    yield return new object[]
                    {
                            // width, height
                            17, 17,
                            horizontalChromaSubSampling, verticalChromaSubSampling,
                            // isTile, strileSize
                            false, 100
                    };
                    yield return new object[]
                    {
                            // width, height
                            32, 32,
                            horizontalChromaSubSampling, verticalChromaSubSampling,
                            // isTile, strileSize
                            true, 16
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(GetTestData))]
        public async Task TestStripRoundTripAsync(int width, int height, int horizontalChromaSubSampling, int vorizontalChromaSubSampling, bool isTile, int strileSize)
        {
            TiffRgb24[] original = new TiffRgb24[width * height];
            Array.Fill(original, s_color);
            var sourceBuffer = TiffPixelBuffer.WrapReadOnly(original, width, height);

            // encode
            byte[] encodedFile;
            {
                var builder = new TiffImageEncoderBuilder();
                builder.PhotometricInterpretation = TiffPhotometricInterpretation.YCbCr;
                builder.Compression = TiffCompression.NoCompression;
                builder.HorizontalChromaSubSampling = horizontalChromaSubSampling;
                builder.VerticalChromaSubSampling = vorizontalChromaSubSampling;
                if (isTile)
                {
                    builder.IsTiled = true;
                    builder.TileSize = new TiffSize(strileSize, strileSize);
                }
                else
                {
                    builder.RowsPerStrip = strileSize;
                }
                TiffImageEncoder<TiffRgb24> encoder = builder.Build<TiffRgb24>();

                using var ms = new MemoryStream();
                using TiffFileWriter writer = await TiffFileWriter.OpenAsync(ms, true);
                TiffImageFileDirectoryWriter ifdWriter = writer.CreateImageFileDirectory();
                await encoder.EncodeAsync(ifdWriter, sourceBuffer);
                writer.SetFirstImageFileDirectoryOffset(await ifdWriter.FlushAsync());
                await writer.FlushAsync();

                encodedFile = ms.ToArray();
            }

            // decode
            TiffRgb24[] decoded = new TiffRgb24[width * height];
            var destinationBuffer = TiffPixelBuffer.Wrap(decoded, width, height);

            {
                using TiffFileReader reader = await TiffFileReader.OpenAsync(encodedFile);
                TiffImageDecoder decoder = await reader.CreateImageDecoderAsync();

                Assert.Equal(width, decoder.Width);
                Assert.Equal(height, decoder.Height);

                await decoder.DecodeAsync(destinationBuffer);
            }

            Assert.True(original.AsSpan().SequenceEqual(decoded.AsSpan()));
        }

    }
}
