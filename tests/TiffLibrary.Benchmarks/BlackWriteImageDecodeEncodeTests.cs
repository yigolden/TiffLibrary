using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using TiffLibrary.PixelFormats;

namespace TiffLibrary.Benchmarks
{
    public class BlackWriteImageDecodeEncodeTests
    {
        [Params(true, false)]
        public bool BlackIsZero { get; set; }

        public const int Width = 800;
        public const int Height = 600;

        [Params(1, 2, 4)]
        public int StripCount { get; set; }

        private ITiffPixelBuffer<TiffGray8> _pixelBuffer;
        private byte[] _tiffData;
        private TiffImageEncoder<TiffGray8> _encoder;

        private ITiffPixelBuffer<TiffGray8> _pixelBufferScratchSpace;
        private byte[] _tiffDataScratchSpace;

        [GlobalSetup]
        public async Task Setup()
        {
            var uncompressedData = new TiffGray8[Width * Height];
            new Random(42).NextBytes(MemoryMarshal.AsBytes(uncompressedData.AsSpan()));
            _pixelBuffer = new TiffMemoryPixelBuffer<TiffGray8>(uncompressedData, Width, Height);

            var ms = new MemoryStream();
            {
                var builder = new TiffImageEncoderBuilder();
                builder.PhotometricInterpretation = BlackIsZero ? TiffPhotometricInterpretation.BlackIsZero : TiffPhotometricInterpretation.WhiteIsZero;
                builder.RowsPerStrip = Height / StripCount;
                _encoder = builder.Build<TiffGray8>();

                using TiffFileWriter writer = await TiffFileWriter.OpenAsync(ms, true);

                using (TiffImageFileDirectoryWriter ifdWriter = writer.CreateImageFileDirectory())
                {
                    await _encoder.EncodeAsync(ifdWriter, _pixelBuffer);
                    writer.SetFirstImageFileDirectoryOffset(await ifdWriter.FlushAsync());
                }

                await writer.FlushAsync();
            }

            _tiffData = ms.ToArray();

            _pixelBufferScratchSpace = new TiffMemoryPixelBuffer<TiffGray8>(new TiffGray8[Width * Height], Width, Height, true);
            _tiffDataScratchSpace = new byte[_tiffData.Length];
        }

        [Benchmark]
        public async Task Encode()
        {
            var ms = new MemoryStream(_tiffDataScratchSpace);
            for (int i = 0; i < 10; i++)
            {
                ms.Seek(0, SeekOrigin.Begin);
                using TiffFileWriter writer = await TiffFileWriter.OpenAsync(ms, true);

                using (TiffImageFileDirectoryWriter ifdWriter = writer.CreateImageFileDirectory())
                {
                    await _encoder.EncodeAsync(ifdWriter, _pixelBuffer);
                    writer.SetFirstImageFileDirectoryOffset(await ifdWriter.FlushAsync());
                }

                await writer.FlushAsync();
            }
        }

        [Benchmark]
        public async Task Decode()
        {
            using TiffFileReader reader = await TiffFileReader.OpenAsync(_tiffData);
            TiffImageDecoder decoder = await reader.CreateImageDecoderAsync();

            for (int i = 0; i < 10; i++)
            {
                await decoder.DecodeAsync(_pixelBufferScratchSpace);
            }
        }
    }
}
