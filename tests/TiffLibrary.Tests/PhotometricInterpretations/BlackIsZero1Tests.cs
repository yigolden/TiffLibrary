﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using TiffLibrary.PixelFormats;
using Xunit;

namespace TiffLibrary.Tests.PhotometricInterpretations
{
    public class BlackIsZero1Tests
    {

        public static async Task<(byte[] FileContent, TiffGray8[] Pixels)> GenerateFileAsync(int pixelCount, bool reverse)
        {
            byte[] buffer = new byte[(pixelCount + 7) / 8];
            TiffGray8[] pixels = new TiffGray8[pixelCount];
            new Random(42).NextBytes(buffer);
            for (int i = 0; i < pixels.Length; i++)
            {
                int bufferIndex = Math.DivRem(i, 8, out int offset);
                pixels[i] = ((buffer[bufferIndex] >> (7 - offset)) & 0b1) == 0 ? new TiffGray8(0) : new TiffGray8(255);
            }
            if (reverse)
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = ReverseBits(buffer[i]);
                }
            }

            var ms = new MemoryStream();
            using (TiffFileWriter writer = await TiffFileWriter.OpenAsync(ms, true))
            {
                using (TiffImageFileDirectoryWriter ifdWriter = writer.CreateImageFileDirectory())
                {
                    TiffStreamOffset region = await writer.WriteAlignedBytesAsync(buffer);

                    await ifdWriter.WriteTagAsync(TiffTag.PhotometricInterpretation, TiffValueCollection.Single((ushort)TiffPhotometricInterpretation.BlackIsZero));
                    await ifdWriter.WriteTagAsync(TiffTag.Compression, TiffValueCollection.Single((ushort)TiffCompression.NoCompression));
                    await ifdWriter.WriteTagAsync(TiffTag.SamplesPerPixel, TiffValueCollection.Single((ushort)1));
                    await ifdWriter.WriteTagAsync(TiffTag.BitsPerSample, TiffValueCollection.UnsafeWrap(new ushort[] { 1 }));
                    await ifdWriter.WriteTagAsync(TiffTag.ImageWidth, TiffValueCollection.Single((uint)pixelCount));
                    await ifdWriter.WriteTagAsync(TiffTag.ImageLength, TiffValueCollection.Single((uint)1));
                    await ifdWriter.WriteTagAsync(TiffTag.StripOffsets, TiffValueCollection.Single((uint)region.Offset));
                    await ifdWriter.WriteTagAsync(TiffTag.StripByteCounts, TiffValueCollection.Single((uint)buffer.Length));
                    if (reverse)
                    {
                        await ifdWriter.WriteTagAsync(TiffTag.FillOrder, TiffValueCollection.Single((ushort)TiffFillOrder.LowerOrderBitsFirst));
                    }
                    else
                    {
                        await ifdWriter.WriteTagAsync(TiffTag.FillOrder, TiffValueCollection.Single((ushort)TiffFillOrder.HigherOrderBitsFirst));
                    }

                    writer.SetFirstImageFileDirectoryOffset(await ifdWriter.FlushAsync());
                }

                await writer.FlushAsync();
            }

            return (ms.ToArray(), pixels);
        }

        [Theory]
        [InlineData(8, 0, 8)]
        [InlineData(10, 0, 10)]
        [InlineData(10, 1, 5)]
        [InlineData(80, 0, 80)]
        [InlineData(100, 0, 100)]
        [InlineData(100, 10, 50)]
        public async Task TestScanline(int pixelCount, int offset, int count)
        {
            Debug.Assert(pixelCount >= 0);
            Debug.Assert((uint)offset <= (uint)pixelCount);
            Debug.Assert((uint)(pixelCount - offset) >= (uint)count);

            foreach (bool reverse in new bool[] { true, false })
            {
                (byte[] fileContent, TiffGray8[] reference) = await GenerateFileAsync(pixelCount, reverse);
                using TiffFileReader reader = await TiffFileReader.OpenAsync(fileContent);
                TiffImageDecoder decoder = await reader.CreateImageDecoderAsync();

                TiffGray8[] pixels = new TiffGray8[count];
                await decoder.DecodeAsync(new TiffPoint(offset, 0), TiffPixelBuffer.Wrap(pixels, count, 1));

                Assert.True(pixels.AsSpan().SequenceEqual(reference.AsSpan(offset, count)));
            }
        }

        private static byte ReverseBits(byte b)
        {
            // http://graphics.stanford.edu/~seander/bithacks.html#ReverseByteWith64Bits
            return (byte)(((b * 0x80200802UL) & 0x0884422110UL) * 0x0101010101UL >> 32);
        }
    }
}
