using System;
using Xunit;

namespace TiffLibrary.Tests.Utils
{
    public class BitWriterTests
    {
        [Fact]
        public void SimpleWriterTest()
        {
            const int ByteCount = 20;
            const bool HigherOrderFirst = true;
            byte[] data = new byte[ByteCount];
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


        [Theory]
        [InlineData(10, true)]
        [InlineData(10, false)]
        [InlineData(100, true)]
        [InlineData(100, false)]
        [InlineData(1000, true)]
        [InlineData(1000, false)]
        public void RoundTripTest(int arrayLength, bool higherOrderBitsFirst)
        {
            var rand = new Random(42);

            byte[] input = new byte[arrayLength];
            byte[] output = new byte[arrayLength];

            rand.NextBytes(input);

            int totalBits = arrayLength * 8;
            int processedBits = 0;
            int nextProcessBits = rand.Next(1, 32);
            uint buffer = 0;

            var reader = new BitReader(input, higherOrderBitsFirst);
            var writer = new BitWriter(output, higherOrderBitsFirst);

            while ((processedBits + nextProcessBits) <= totalBits)
            {
                buffer = (buffer << nextProcessBits) | reader.Read(nextProcessBits);
                writer.Write(buffer, nextProcessBits);
                processedBits += nextProcessBits;
                nextProcessBits = rand.Next(1, 32);
            }

            if (processedBits < totalBits)
            {
                nextProcessBits = totalBits - processedBits;
                buffer = (buffer << nextProcessBits) | reader.Read(nextProcessBits);
                writer.Write(buffer, nextProcessBits);
            }

            writer.Flush();

            Assert.True(input.AsSpan().SequenceEqual(output));
        }
    }
}
