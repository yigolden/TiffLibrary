using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using TiffLibrary.PixelFormats;

namespace TiffLibrary.Benchmarks
{
    [MemoryDiagnoser]
    public class ParallelDecodingTests
    {
        [Params(1, 2, 4)]
        public int DegreeOfParallelism { get; set; }

        [Params(TiffCompression.T4Encoding, TiffCompression.Lzw)]
        public TiffCompression Compression { get; set; }

        private byte[] _stripTestTiff;
        private byte[] _tileTestTiff;

        private TiffPixelBuffer<TiffGray8> _scratchSpace;

        private TiffFileReader _stripReader;
        private TiffFileReader _tileReader;

        private TiffImageDecoder _stripDecoder;
        private TiffImageDecoder _tileDecoder;

        [GlobalSetup]
        public async Task Setup()
        {
            TiffGray8[] image = new TiffGray8[8192 * 8192];
            _scratchSpace = TiffPixelBuffer.Wrap(image, 8192, 8192);

            var builder = new TiffImageEncoderBuilder();
            builder.PhotometricInterpretation = TiffPhotometricInterpretation.WhiteIsZero;
            builder.Compression = Compression;

            // Generate a image with many strips
            var ms = new MemoryStream();
            builder.IsTiled = false;
            builder.RowsPerStrip = 256;
            await GenerateImageAsync(ms, builder, _scratchSpace);
            _stripTestTiff = ms.ToArray();

            ms.SetLength(0);

            // Generate a image with many tiles
            builder.IsTiled = true;
            builder.TileSize = new TiffSize(512, 512); // the minimum tile size
            await GenerateImageAsync(ms, builder, _scratchSpace);
            _tileTestTiff = ms.ToArray();

            _stripReader = await TiffFileReader.OpenAsync(_stripTestTiff);
            _stripDecoder = await _stripReader.CreateImageDecoderAsync(new TiffImageDecoderOptions { MaxDegreeOfParallelism = DegreeOfParallelism });
            await _stripDecoder.DecodeAsync(_scratchSpace);

            _tileReader = await TiffFileReader.OpenAsync(_tileTestTiff);
            _tileDecoder = await _tileReader.CreateImageDecoderAsync(new TiffImageDecoderOptions { MaxDegreeOfParallelism = DegreeOfParallelism });
            await _tileDecoder.DecodeAsync(_scratchSpace);
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
            await _stripDecoder.DecodeAsync(_scratchSpace);
        }

        [Benchmark]
        public async Task DecodeTileImage()
        {
            await _stripDecoder.DecodeAsync(_scratchSpace);
        }
    }
}
