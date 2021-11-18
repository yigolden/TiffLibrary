using System;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using TiffLibrary.PhotometricInterpreters;
using TiffLibrary.PixelFormats;

namespace TiffLibrary.Benchmarks
{
    public class YCbCrConverterTests
    {
        [Params(1, 10, 100, 500)]
        public int PixelCount { get; set; }

        private TiffYCbCrConverter8 converter;
        private TiffRgb24[] rgb24;
        private TiffRgba32[] rgba32;
        private byte[] ycbcr;

        private byte[] buffer;

        [GlobalSetup]
        public void Setup()
        {
            converter = TiffYCbCrConverter8.CreateDefault();

            rgb24 = new TiffRgb24[PixelCount];
            new Random(42).NextBytes(MemoryMarshal.AsBytes(rgb24.AsSpan()));

            ycbcr = new byte[PixelCount * 3];
            converter.ConvertFromRgb24(rgb24, ycbcr, PixelCount);

            rgba32 = new TiffRgba32[PixelCount];
            converter.ConvertToRgba32(ycbcr, rgba32, PixelCount);

            buffer = new byte[4 * PixelCount];
        }

        [Benchmark]
        public void YCbCr8ToRgb24()
        {
            converter.ConvertToRgb24(ycbcr, MemoryMarshal.Cast<byte, TiffRgb24>(buffer), PixelCount);
        }

        [Benchmark]
        public void YCbCr8ToRgba32()
        {
            converter.ConvertToRgba32(ycbcr, MemoryMarshal.Cast<byte, TiffRgba32>(buffer), PixelCount);
        }

        [Benchmark]
        public void Rgb24ToYCbCr8()
        {
            converter.ConvertFromRgb24(rgb24, buffer, PixelCount);
        }
    }
}
