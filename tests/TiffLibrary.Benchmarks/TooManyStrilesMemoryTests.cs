using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using TiffLibrary.PixelFormats;

namespace TiffLibrary.Benchmarks
{
    [MemoryDiagnoser]
    public class TooManyStrilesMemoryTests
    {
        private byte[] _stripTestTiff;
        private byte[] _tileTestTiff;

        private TiffPixelBuffer<TiffGray8> _scratchSpace;

        [GlobalSetup]
        public async Task Setup()
        {
            TiffGray8[] image = new TiffGray8[8192 * 8192];
            _scratchSpace = TiffPixelBuffer.Wrap(image, 8192, 8192);

            var builder = new TiffImageEncoderBuilder();
            builder.PhotometricInterpretation = TiffPhotometricInterpretation.BlackIsZero;
            builder.Compression = TiffCompression.NoCompression;

            // Generate a image with many strips
            var ms = new MemoryStream();
            builder.IsTiled = false;
            builder.RowsPerStrip = 1;
            await GenerateImageAsync(ms, builder, _scratchSpace);
            _stripTestTiff = ms.ToArray();

            ms.SetLength(0);

            // Generate a image with many tiles
            builder.IsTiled = true;
            builder.TileSize = new TiffSize(16, 16); // the minimum tile size
            await GenerateImageAsync(ms, builder, _scratchSpace);
            _tileTestTiff = ms.ToArray();
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

        [Benchmark]
        public async Task DecodeStripImage()
        {
            await using TiffFileReader tiff = await TiffFileReader.OpenAsync(_stripTestTiff);
            TiffImageDecoder decoder = await tiff.CreateImageDecoderAsync();
            await decoder.DecodeAsync(_scratchSpace);
        }

        [Benchmark]
        public async Task DecodeTileImage()
        {
            await using TiffFileReader tiff = await TiffFileReader.OpenAsync(_tileTestTiff);
            TiffImageDecoder decoder = await tiff.CreateImageDecoderAsync();
            await decoder.DecodeAsync(_scratchSpace);
        }
    }
}
