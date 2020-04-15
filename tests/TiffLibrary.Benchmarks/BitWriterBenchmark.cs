using System;
using BenchmarkDotNet.Attributes;

namespace TiffLibrary.Benchmarks
{
    public class BitWriterBenchmark
    {
        [Params(true, false)]
        public bool HigherOrderFirst { get; set; }

        [Params(500, 2000)]
        public int ByteCount { get; set; }

        private byte[] data;

        [GlobalSetup]
        public void Setup()
        {
            data = new byte[ByteCount];
        }

        [Benchmark]
        public void Read()
        {
            uint value = 0x12345678;
            var rand = new Random(42);
            int totalBits = 8 * ByteCount;
            int writeBits = 0;
            var bitWriter = new BitWriter(data, HigherOrderFirst);

            int bitCount = rand.Next(1, 32);
            while ((writeBits + bitCount) <= totalBits)
            {
                bitWriter.Write(value, bitCount);
                writeBits += bitCount;
                bitCount = rand.Next(1, 32);
            }

            bitWriter.Flush();
        }

    }
}
