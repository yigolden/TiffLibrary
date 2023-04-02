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
        [InlineData(true, 0, false, false)]
        [InlineData(false, 0, false, false)]
        [InlineData(true, 1, false, false)]
        [InlineData(false, 1, false, false)]
        [InlineData(true, 2, false, false)]
        [InlineData(false, 2, false, false)]
        [InlineData(true, 4, false, false)]
        [InlineData(false, 4, false, false)]
        [InlineData(true, 16, false, false)]
        [InlineData(false, 16, false, false)]
        [InlineData(true, 16, true, false)]
        [InlineData(false, 16, true, false)]
        [InlineData(true, 0, false, true)]
        [InlineData(false, 0, false, true)]
        [InlineData(true, 16, false, true)]
        [InlineData(false, 16, false, true)]
        public async Task TestRoundtrip(bool isTile, int maxDegreeOfParallelism, bool isYCbCr, bool is16Bit)
        {
            if (is16Bit)
                await TestRoundTripGeneric<TiffGray16>();
            else
                await TestRoundTripGeneric<TiffGray8>();
            return;
            

            async Task TestRoundTripGeneric<TPixel>() where TPixel : unmanaged, IEquatable<TPixel>
            {
                int dimension = 4096;
                TPixel[] refImage = new TPixel[dimension * dimension];
                TPixel[] testImage = new TPixel[dimension * dimension];

                var rand = new Random(42);
                rand.NextBytes(MemoryMarshal.AsBytes(refImage.AsSpan()));

                var builder = new TiffImageEncoderBuilder();
                if (isYCbCr)
                {
                    builder.PhotometricInterpretation = TiffPhotometricInterpretation.YCbCr;
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
                await GenerateImageAsync(ms, builder, TiffPixelBuffer.WrapReadOnly(refImage, dimension, dimension));

                ms.Seek(0, SeekOrigin.Begin);

                await using TiffFileReader tiff = await TiffFileReader.OpenAsync(ms, leaveOpen: true);
                TiffImageDecoder decoder = await tiff.CreateImageDecoderAsync();
                await decoder.DecodeAsync(TiffPixelBuffer.Wrap(testImage, dimension, dimension));

                Assert.True(refImage.AsSpan().SequenceEqual(testImage.AsSpan()));
            }
        }

        private static async Task GenerateImageAsync<TPixel>(Stream stream, TiffImageEncoderBuilder builder, TiffPixelBuffer<TPixel> image)
            where TPixel : unmanaged
        {
            using (TiffFileWriter writer = await TiffFileWriter.OpenAsync(stream, true))
            {
                TiffStreamOffset ifdOffset;
                using (TiffImageFileDirectoryWriter ifdWriter = writer.CreateImageFileDirectory())
                {
                    TiffImageEncoder<TPixel> encoder = builder.Build<TPixel>();

                    await encoder.EncodeAsync(ifdWriter, image);

                    ifdOffset = await ifdWriter.FlushAsync();
                }

                writer.SetFirstImageFileDirectoryOffset(ifdOffset);
                await writer.FlushAsync();
            }
        }
    }
}
