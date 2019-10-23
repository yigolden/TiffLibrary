using System;
using Xunit;

namespace TiffLibrary.Tests.Utils
{
    public class BitReaderTests
    {

        [Theory]
        [InlineData(100, false)]
        [InlineData(1000, false)]
        [InlineData(100, true)]
        [InlineData(1000, true)]
        public void TestAlignedRead(int byteCount, bool testAdvance)
        {
            var rand = new Random(42);
            byte[] data = new byte[byteCount];
            rand.NextBytes(data);

            var bitReader = new BitReader(data);
            for (int i = 0; i < byteCount; i++)
            {
                Assert.Equal(i, bitReader.ConsumedBytes);
                Assert.Equal(i * 8, bitReader.ConsumedBits);
                uint value = bitReader.Peek(8);
                Assert.Equal(i, bitReader.ConsumedBytes);
                Assert.Equal(i * 8, bitReader.ConsumedBits);
                Assert.Equal(data[i], value);
                if (testAdvance)
                {
                    bitReader.Advance(8);
                }
                else
                {
                    value = bitReader.Read(8);
                    Assert.Equal(data[i], value);
                }
                Assert.Equal(i + 1, bitReader.ConsumedBytes);
                Assert.Equal((i + 1) * 8, bitReader.ConsumedBits);
            }
        }

        [Fact]
        public void TestAlignedAdvanceByte()
        {
            byte[] data = new byte[] { 0xab, 0xcd, 0xef };
            var bitReader = new BitReader(data);

            bitReader.Read(8);
            bitReader.AdvanceAlignByte();
            Assert.Equal(1, bitReader.ConsumedBytes);
            Assert.Equal(8, bitReader.ConsumedBits);
            Assert.Equal((uint)0xcd, bitReader.Read(8));
        }

        [Fact]
        public void TestUnalignedReadHigherOrderBitsFirst()
        {
            byte[] data = new byte[] { 0x12, 0x23, 0x34, 0x45, 0x56, 0x67, 0x78, 0x89, 0x9a, 0xab, 0xbc };
            var bitReader = new BitReader(data);

            Assert.Equal((uint)0, bitReader.Peek(1));
            Assert.Equal(0, bitReader.ConsumedBits);
            Assert.Equal(0, bitReader.ConsumedBytes);
            Assert.Equal((uint)0, bitReader.Read(1));
            Assert.Equal(1, bitReader.ConsumedBits);
            Assert.Equal(1, bitReader.ConsumedBytes);

            Assert.Equal((uint)0, bitReader.Read(2));
            Assert.Equal(3, bitReader.ConsumedBits);
            Assert.Equal(1, bitReader.ConsumedBytes);

            Assert.Equal((uint)0b100, bitReader.Peek(3));
            Assert.Equal((uint)0b100, bitReader.Read(3));
            Assert.Equal(6, bitReader.ConsumedBits);
            Assert.Equal(1, bitReader.ConsumedBytes);

            Assert.Equal((uint)0b10001, bitReader.Peek(5));
            bitReader.AdvanceAlignByte();
            Assert.Equal(8, bitReader.ConsumedBits);
            Assert.Equal(1, bitReader.ConsumedBytes);

            Assert.Equal((uint)0b001000, bitReader.Read(6));
            Assert.Equal((uint)0b1100110, bitReader.Read(7));
            Assert.Equal((uint)0b10001000, bitReader.Read(8));
            Assert.Equal((uint)0b101, bitReader.Peek(3));
            Assert.Equal(29, bitReader.ConsumedBits);

            Assert.Equal((uint)0b1010101011, bitReader.Peek(10));
            Assert.Equal((uint)0b101010, bitReader.Read(6));
            Assert.Equal((uint)0b10110011, bitReader.Peek(8));
            Assert.Equal(35, bitReader.ConsumedBits);

            bitReader.AdvanceAlignByte();

            Assert.Equal(40, bitReader.ConsumedBits);
            Assert.Equal(5, bitReader.ConsumedBytes);

            Assert.Equal((uint)0x6778899a, bitReader.Peek(32));
            Assert.Equal((uint)0b01100, bitReader.Read(5));

            Assert.Equal(0b111_01111000_10001001_10011010_10101, (uint)bitReader.Read(32));
            bitReader.AdvanceAlignByte();
            Assert.Equal(80, bitReader.ConsumedBits);
            Assert.Equal(10, bitReader.ConsumedBytes);

            Assert.Equal((uint)0xbc, bitReader.Peek(8));
            Assert.Equal((uint)0xbc00, bitReader.Peek(16));
        }

        [Fact]
        public void TestUnalignedReadLowerOrderBitsFirst()
        {
            byte[] data = new byte[] { 0x12, 0x23, 0x34, 0x45, 0x56, 0x67, 0x78, 0x89, 0x9a, 0xab, 0xbc };
            var bitReader = new BitReader(data, false);

            Assert.Equal((uint)0, bitReader.Peek(1));
            Assert.Equal(0, bitReader.ConsumedBits);
            Assert.Equal(0, bitReader.ConsumedBytes);
            Assert.Equal((uint)0, bitReader.Read(1));
            Assert.Equal(1, bitReader.ConsumedBits);
            Assert.Equal(1, bitReader.ConsumedBytes);

            Assert.Equal((uint)0b10, bitReader.Read(2));
            Assert.Equal(3, bitReader.ConsumedBits);
            Assert.Equal(1, bitReader.ConsumedBytes);

            Assert.Equal((uint)0b010, bitReader.Peek(3));
            Assert.Equal((uint)0b010, bitReader.Read(3));
            Assert.Equal(6, bitReader.ConsumedBits);
            Assert.Equal(1, bitReader.ConsumedBytes);

            Assert.Equal((uint)0b00110, bitReader.Peek(5));
            bitReader.AdvanceAlignByte();
            Assert.Equal(8, bitReader.ConsumedBits);
            Assert.Equal(1, bitReader.ConsumedBytes);

            Assert.Equal((uint)0b110001, bitReader.Read(6));
            Assert.Equal((uint)0b0000101, bitReader.Read(7));
            Assert.Equal((uint)0b10010100, bitReader.Read(8));
            Assert.Equal((uint)0b010, bitReader.Peek(3));
            Assert.Equal(29, bitReader.ConsumedBits);

            Assert.Equal((uint)0b0100110101, bitReader.Peek(10));
            Assert.Equal((uint)0b010011, bitReader.Read(6));
            Assert.Equal((uint)0b01010111, bitReader.Peek(8));
            Assert.Equal(35, bitReader.ConsumedBits);

            bitReader.AdvanceAlignByte();

            Assert.Equal(40, bitReader.ConsumedBits);
            Assert.Equal(5, bitReader.ConsumedBytes);

            Assert.Equal((uint)0xe61e9159, bitReader.Peek(32));
            Assert.Equal((uint)0b11100, bitReader.Read(5));

            Assert.Equal(0b110_00011110_10010001_01011001_11010, bitReader.Read(32));
            bitReader.AdvanceAlignByte();
            Assert.Equal(80, bitReader.ConsumedBits);
            Assert.Equal(10, bitReader.ConsumedBytes);

            Assert.Equal((uint)0x3d, bitReader.Peek(8));
            Assert.Equal((uint)0x3d00, bitReader.Peek(16));
        }

        [Fact]
        public void TestLargeAdvance()
        {
            byte[] data = new byte[24];
            var reader = new BitReader(data);
            reader.Advance(32);
            Assert.Equal(32, reader.ConsumedBits);
            reader.Advance(128);
            Assert.Equal(32 + 128, reader.ConsumedBits);
        }
    }
}
