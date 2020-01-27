using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JpegLibrary;
using TiffLibrary.Utils;

namespace TiffLibrary.Compression
{
    internal sealed class JpegBuffer12BitOutputWriter : JpegBlockOutputWriter
    {
        private int _width;
        private int _skippedScanlines;
        private int _height;
        private int _componentCount;
        private Memory<byte> _output;

        public JpegBuffer12BitOutputWriter(int width, int skippedScanlines, int height, int componentsCount, Memory<byte> output)
        {
            if (output.Length < (width * height * componentsCount))
            {
                throw new ArgumentException("Destination buffer is too small.");
            }

            _width = width;
            _skippedScanlines = skippedScanlines / 8 * 8; // Align to block
            _height = height;
            _componentCount = componentsCount;
            _output = output;
        }

        public override void WriteBlock(ref short blockRef, int componentIndex, int x, int y)
        {
            int componentCount = _componentCount;
            int width = _width;
            int height = _height;

            if (x > width || y > _height)
            {
                return;
            }
            if ((y + 8) <= _skippedScanlines)
            {
                // No need to decode region before the fist requested scanline.
                return;
            }

            int writeWidth = Math.Min(width - x, 8);
            int writeHeight = Math.Min(height - y, 8);

            ref ushort destinationRef = ref Unsafe.As<byte, ushort>(ref MemoryMarshal.GetReference(_output.Span));
            destinationRef = ref Unsafe.Add(ref destinationRef, y * width * componentCount + x * componentCount + componentIndex);

            for (int destY = 0; destY < writeHeight; destY++)
            {
                ref ushort destinationRowRef = ref Unsafe.Add(ref destinationRef, destY * width * componentCount);
                for (int destX = 0; destX < writeWidth; destX++)
                {
                    Unsafe.Add(ref destinationRowRef, destX * componentCount) = (ushort)FastExpandBits(TiffMathHelper.ClampTo12Bit(Unsafe.Add(ref blockRef, destX)));
                }
                blockRef = ref Unsafe.Add(ref blockRef, 8);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint FastExpandBits(uint bits)
        {
            const int bitCount = 12;
            const int targetBitCount = 16;
            const int remainingBits = targetBitCount - bitCount;
            return (bits << remainingBits) | (bits & ((uint)(1 << remainingBits) - 1));
        }
    }
}
