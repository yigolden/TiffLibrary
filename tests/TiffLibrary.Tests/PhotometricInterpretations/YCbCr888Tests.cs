using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TiffLibrary.PhotometricInterpreters;
using TiffLibrary.PixelFormats;
using Xunit;

namespace TiffLibrary.Tests.PhotometricInterpretations
{
    public class YCbCr888Tests
    {
        public static async Task<(byte[] FileContent, TiffRgb24[] Pixels)> GenerateFileAsync(int pixelCount)
        {
            byte[] buffer = new byte[pixelCount * 3];
            TiffRgb24[] pixels = new TiffRgb24[pixelCount];
            new Random().NextBytes(MemoryMarshal.AsBytes(pixels.AsSpan()));
            TiffYCbCrConverter8.CreateDefault().ConvertFromRgb24(pixels, buffer, pixelCount);

            var ms = new MemoryStream();
            using (TiffFileWriter writer = await TiffFileWriter.OpenAsync(ms, true))
            {
                using (TiffImageFileDirectoryWriter ifdWriter = writer.CreateImageFileDirectory())
                {
                    TiffStreamOffset region = await writer.WriteAlignedBytesAsync(buffer);

                    await ifdWriter.WriteTagAsync(TiffTag.PhotometricInterpretation, TiffValueCollection.Single((ushort)TiffPhotometricInterpretation.YCbCr));
                    await ifdWriter.WriteTagAsync(TiffTag.Compression, TiffValueCollection.Single((ushort)TiffCompression.NoCompression));
                    await ifdWriter.WriteTagAsync(TiffTag.SamplesPerPixel, TiffValueCollection.Single((ushort)3));
                    await ifdWriter.WriteTagAsync(TiffTag.BitsPerSample, TiffValueCollection.UnsafeWrap(new ushort[] { 8, 8, 8 }));
                    await ifdWriter.WriteTagAsync(TiffTag.ImageWidth, TiffValueCollection.Single((uint)pixelCount));
                    await ifdWriter.WriteTagAsync(TiffTag.ImageLength, TiffValueCollection.Single((uint)1));
                    await ifdWriter.WriteTagAsync(TiffTag.StripOffsets, TiffValueCollection.Single((uint)region.Offset));
                    await ifdWriter.WriteTagAsync(TiffTag.StripByteCounts, TiffValueCollection.Single((uint)buffer.Length));
                    await ifdWriter.WriteTagAsync(TiffTag.YCbCrSubSampling, TiffValueCollection.UnsafeWrap(new ushort[] { 1, 1 }));

                    writer.SetFirstImageFileDirectoryOffset(await ifdWriter.FlushAsync());
                }

                await writer.FlushAsync();
            }

            return (ms.ToArray(), pixels);
        }

        [Theory]
        [InlineData(8, 0, 8)]
        [InlineData(10, 4, 4)]
        [InlineData(10, 1, 5)]
        [InlineData(80, 0, 80)]
        [InlineData(100, 0, 100)]
        [InlineData(100, 10, 50)]
        [InlineData(100, 11, 50)]
        public async Task TestScanline(int pixelCount, int offset, int count)
        {
            Debug.Assert(pixelCount >= 0);
            Debug.Assert((uint)offset <= (uint)pixelCount);
            Debug.Assert((uint)(pixelCount - offset) >= (uint)count);

            (byte[] fileContent, TiffRgb24[] reference) = await GenerateFileAsync(pixelCount);
            var converter = TiffYCbCrConverter8.CreateDefault();
            byte[] ycbcrReference = new byte[count * 3];
            converter.ConvertFromRgb24(reference.AsSpan(offset, count), ycbcrReference, count);

            using TiffFileReader reader = await TiffFileReader.OpenAsync(fileContent);
            TiffImageDecoder decoder = await reader.CreateImageDecoderAsync();

            TiffRgb24[] pixels = new TiffRgb24[count];
            await decoder.DecodeAsync(new TiffPoint(offset, 0), TiffPixelBuffer.Wrap(pixels, count, 1));
            byte[] ycbcrPixels = new byte[count * 3];
            converter.ConvertFromRgb24(pixels, ycbcrPixels, count);

            Assert.True(ycbcrPixels.AsSpan().SequenceEqual(ycbcrReference));
        }

    }
}
