using System;
using BenchmarkDotNet.Attributes;

namespace TiffLibrary.Benchmarks
{
    public class BitReaderBenchmark
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
            var rand = new Random(42);
            rand.NextBytes(data);
        }

        [Benchmark]
        public void Read()
        {
            var rand = new Random(42);
            int totalBits = 8 * ByteCount;
            int readBits = 0;
            var bitReader = new BitReader(data);

            int bitCount = rand.Next(1, 32);
            while ((readBits + bitCount) <= totalBits)
            {
                bitReader.Read(bitCount);
                readBits += bitCount;
                bitCount = rand.Next(1, 32);
            }
        }

    }
}
