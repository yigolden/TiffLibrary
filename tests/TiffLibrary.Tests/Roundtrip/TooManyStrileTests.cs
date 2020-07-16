using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TiffLibrary.PixelFormats;
using Xunit;

namespace TiffLibrary.Tests.Roundtrip
{
    public class TooManyStrileTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task TestRoundtrip(bool isTile)
        {
            TiffGray8[] refImage = new TiffGray8[2048 * 2048];
            TiffGray8[] testImage = new TiffGray8[2048 * 2048];

            var rand = new Random(42);
            rand.NextBytes(MemoryMarshal.AsBytes(refImage.AsSpan()));

            var builder = new TiffImageEncoderBuilder();
            builder.PhotometricInterpretation = TiffPhotometricInterpretation.BlackIsZero;
            builder.Compression = TiffCompression.NoCompression;
            builder.IsTiled = isTile;
            // Make sure we have at least CacheSize strips or tiles
            // Cache size in the current implementation is 256
            // Any value greater than 256 should be OK.
            builder.RowsPerStrip = 2; // 1024 strips
            builder.TileSize = new TiffSize(16, 16); // 16384 tiles

            var ms = new MemoryStream();
            await GenerateImageAsync(ms, builder, TiffPixelBuffer.WrapReadOnly(refImage, 2048, 2048));

            ms.Seek(0, SeekOrigin.Begin);

            await using TiffFileReader tiff = await TiffFileReader.OpenAsync(ms, leaveOpen: true);
            TiffImageDecoder decoder = await tiff.CreateImageDecoderAsync();
            await decoder.DecodeAsync(TiffPixelBuffer.Wrap(testImage, 2048, 2048));

            Assert.True(refImage.AsSpan().SequenceEqual(testImage.AsSpan()));
        }

        private static async Task GenerateImageAsync(Stream stream, TiffImageEncoderBuilder builder, TiffPixelBuffer<TiffGray8> image)
        {
            using (TiffFileWriter writer = await TiffFileWriter.OpenAsync(stream, true))
            {
                TiffStreamOffset ifdOffset;
                using (TiffImageFileDirectoryWriter ifdWriter = writer.CreateImageFileDirectory())
                {
                    TiffImageEncoder<TiffGray8> encoder = builder.Build<TiffGray8>();

                    await encoder.EncodeAsync(ifdWriter, image);

                    ifdOffset = await ifdWriter.FlushAsync();
                }

                writer.SetFirstImageFileDirectoryOffset(ifdOffset);
                await writer.FlushAsync();
            }
        }
    }
}
