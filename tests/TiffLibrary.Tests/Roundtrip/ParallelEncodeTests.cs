using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TiffLibrary.PixelFormats;
using Xunit;

namespace TiffLibrary.Tests.Roundtrip
{
    public class ParallelEncodeTests
    {
        [Theory]
        [InlineData(true, 0, false)]
        [InlineData(false, 0, false)]
        [InlineData(true, 1, false)]
        [InlineData(false, 1, false)]
        [InlineData(true, 2, false)]
        [InlineData(false, 2, false)]
        [InlineData(true, 4, false)]
        [InlineData(false, 4, false)]
        [InlineData(true, 16, false)]
        [InlineData(false, 16, false)]
        [InlineData(true, 16, true)]
        [InlineData(false, 16, true)]
        public async Task TestRoundtrip(bool isTile, int maxDegreeOfParallelism, bool isYCbCr)
        {
            TiffGray8[] refImage = new TiffGray8[4096 * 4096];
            TiffGray8[] testImage = new TiffGray8[4096 * 4096];

            var rand = new Random(42);
            rand.NextBytes(MemoryMarshal.AsBytes(refImage.AsSpan()));

            var builder = new TiffImageEncoderBuilder();
            if (isYCbCr)
            {
                builder.PhotometricInterpretation = TiffPhotometricInterpretation.BlackIsZero;
                builder.HorizontalChromaSubSampling = 2;
                builder.VerticalChromaSubSampling = 2;
            }
            else
            {
                builder.PhotometricInterpretation = TiffPhotometricInterpretation.BlackIsZero;
                builder.Predictor = TiffPredictor.HorizontalDifferencing;
            }
            
            builder.Compression = TiffCompression.Lzw;
            builder.IsTiled = isTile;
            builder.RowsPerStrip = 64;
            builder.TileSize = new TiffSize(64, 64);
            builder.MaxDegreeOfParallelism = maxDegreeOfParallelism;

            var ms = new MemoryStream();
            await GenerateImageAsync(ms, builder, TiffPixelBuffer.WrapReadOnly(refImage, 4096, 4096));

            ms.Seek(0, SeekOrigin.Begin);

            await using TiffFileReader tiff = await TiffFileReader.OpenAsync(ms, leaveOpen: true);
            TiffImageDecoder decoder = await tiff.CreateImageDecoderAsync();
            await decoder.DecodeAsync(TiffPixelBuffer.Wrap(testImage, 4096, 4096));

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
