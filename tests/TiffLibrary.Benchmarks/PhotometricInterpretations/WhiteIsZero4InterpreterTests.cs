using System;
using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using TiffLibrary.PixelFormats;

namespace TiffLibrary.Benchmarks.PhotometricInterpretations
{
    public class BlackIsZero4InterpreterTests
    {
        [Params(1, 20, 100, 2000, 5000)]
        public int PixelCount { get; set; }

        private TiffImageDecoder decoder;
        private TiffPixelBuffer<TiffGray8> pixelBuffer;

        [GlobalSetup]
        public async Task Setup()
        {
            byte[] buffer = new byte[(PixelCount + 1) / 2];
            new Random(42).NextBytes(buffer);

            var ms = new MemoryStream();
            using (TiffFileWriter writer = await TiffFileWriter.OpenAsync(ms, true))
            {
                using (TiffImageFileDirectoryWriter ifdWriter = writer.CreateImageFileDirectory())
                {
                    TiffStreamOffset region = await writer.WriteAlignedBytesAsync(buffer);

                    await ifdWriter.WriteTagAsync(TiffTag.PhotometricInterpretation, TiffValueCollection.Single((ushort)TiffPhotometricInterpretation.BlackIsZero));
                    await ifdWriter.WriteTagAsync(TiffTag.Compression, TiffValueCollection.Single((ushort)TiffCompression.NoCompression));
                    await ifdWriter.WriteTagAsync(TiffTag.SamplesPerPixel, TiffValueCollection.Single((ushort)1));
                    await ifdWriter.WriteTagAsync(TiffTag.BitsPerSample, TiffValueCollection.UnsafeWrap(new ushort[] { 4 }));
                    await ifdWriter.WriteTagAsync(TiffTag.ImageWidth, TiffValueCollection.Single((uint)PixelCount));
                    await ifdWriter.WriteTagAsync(TiffTag.ImageLength, TiffValueCollection.Single((uint)1));
                    await ifdWriter.WriteTagAsync(TiffTag.StripOffsets, TiffValueCollection.Single((uint)region.Offset));
                    await ifdWriter.WriteTagAsync(TiffTag.StripByteCounts, TiffValueCollection.Single((uint)buffer.Length));

                    writer.SetFirstImageFileDirectoryOffset(await ifdWriter.FlushAsync());
                }

                await writer.FlushAsync();
            }

            byte[] fileContent = ms.ToArray();
            var reader = TiffFileReader.Open(fileContent);
            decoder = reader.CreateImageDecoder();

            pixelBuffer = TiffPixelBuffer.Wrap(new TiffGray8[PixelCount], PixelCount, 1);
        }

        [Benchmark]
        public void Decode()
        {
            decoder.Decode(pixelBuffer);
        }
    }
}
