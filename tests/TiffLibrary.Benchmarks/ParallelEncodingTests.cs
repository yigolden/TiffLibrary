using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using TiffLibrary.PixelFormats;

namespace TiffLibrary.Benchmarks
{
    [MemoryDiagnoser]
    public class ParallelEncodingTests
    {
        [Params(1, 2, 4)]
        public int DegreeOfParallelism { get; set; }

        [Params(TiffCompression.T4Encoding, TiffCompression.Lzw)]
        public TiffCompression Compression { get; set; }

        private TiffPixelBuffer<TiffGray8> _image;

        private TiffImageEncoder<TiffGray8> _stripEncoder;
        private TiffImageEncoder<TiffGray8> _tileEncoder;

        [GlobalSetup]
        public void Setup()
        {
            TiffGray8[] image = new TiffGray8[8192 * 8192];
            _image = TiffPixelBuffer.WrapReadOnly(image, 8192, 8192);

            var builder = new TiffImageEncoderBuilder();
            builder.PhotometricInterpretation = TiffPhotometricInterpretation.WhiteIsZero;
            builder.Compression = Compression;
            builder.MaxDegreeOfParallelism = DegreeOfParallelism;

            // Strip encoder
            builder.IsTiled = false;
            builder.RowsPerStrip = 256;
            _stripEncoder = builder.Build<TiffGray8>();

            // Tile encoder
            builder.IsTiled = false;
            builder.TileSize = new TiffSize(512, 512);
            _tileEncoder = builder.Build<TiffGray8>();
        }

        [Benchmark]
        public async Task EncodeStripImage()
        {
            await using TiffFileWriter writer = await TiffFileWriter.OpenAsync(new EmptyContentReaderWriter(), false, false);
            using (TiffImageFileDirectoryWriter ifdWriter = writer.CreateImageFileDirectory())
            {
                await _stripEncoder.EncodeAsync(ifdWriter, _image);
                writer.SetFirstImageFileDirectoryOffset(await ifdWriter.FlushAsync());
            }
            await writer.FlushAsync();
        }

        [Benchmark]
        public async Task EncodeTileImage()
        {
            await using TiffFileWriter writer = await TiffFileWriter.OpenAsync(new EmptyContentReaderWriter(), false, false);
            using (TiffImageFileDirectoryWriter ifdWriter = writer.CreateImageFileDirectory())
            {
                await _tileEncoder.EncodeAsync(ifdWriter, _image);
                writer.SetFirstImageFileDirectoryOffset(await ifdWriter.FlushAsync());
            }
            await writer.FlushAsync();
        }

    }
}
