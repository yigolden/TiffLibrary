using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TiffLibrary.Compression;
using TiffLibrary.PixelFormats;
using Xunit;

namespace TiffLibrary.Tests.Roundtrip
{
    public class DeflateRoundtripTests
    {
        private static readonly TiffRgb24 s_color = new TiffRgb24(192, 129, 0);

        public static IEnumerable<object[]> GetTestData()
        {
            foreach (bool fixedColor in new bool[] { false, true })
            {
                foreach (TiffDeflateCompressionLevel compressionLevel in new TiffDeflateCompressionLevel[] { TiffDeflateCompressionLevel.Optimal, TiffDeflateCompressionLevel.Default, TiffDeflateCompressionLevel.NoCompression })
                {
                    foreach (int width in new int[] { 1, 2, 16, 50 })
                    {
                        foreach (int height in new int[] { 1, 2, 16, 50 })
                        {
                            yield return new object[] { fixedColor, compressionLevel, width, height };

                        }
                    }
                }
            }
        }

        [Theory]
        [MemberData(nameof(GetTestData))]
        public async Task TestRoundTripAsync(bool fixedColor, TiffDeflateCompressionLevel deflateLevel, int width, int height)
        {
            TiffRgb24[] original = new TiffRgb24[width * height];
            if (fixedColor)
            {
                Array.Fill(original, s_color);
            }
            else
            {
                new Random(42).NextBytes(MemoryMarshal.AsBytes(original.AsSpan()));
            }

            var sourceBuffer = TiffPixelBuffer.WrapReadOnly(original, width, height);

            // encode
            byte[] encodedFile;
            {
                var builder = new TiffImageEncoderBuilder();
                builder.PhotometricInterpretation = TiffPhotometricInterpretation.RGB;
                builder.Compression = TiffCompression.Deflate;
                builder.DeflateCompressionLevel = deflateLevel;
                builder.RowsPerStrip = height;
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
