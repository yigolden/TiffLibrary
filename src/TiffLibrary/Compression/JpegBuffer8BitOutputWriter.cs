using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JpegLibrary;
using TiffLibrary.Utils;

namespace TiffLibrary.Compression
{
    internal sealed class JpegBuffer8BitOutputWriter : JpegBlockOutputWriter
    {
        private int _width;
        private int _skippedScanlines;
        private int _height;
        private int _componentCount;
        private Memory<byte> _output;

        public JpegBuffer8BitOutputWriter() { }

        public JpegBuffer8BitOutputWriter(int width, int skippedScanlines, int height, int componentsCount, Memory<byte> output)
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

        public override void WriteBlock(in JpegBlock8x8 block, int componentIndex, int x, int y)
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

            ref short blockRef = ref Unsafe.As<JpegBlock8x8, short>(ref Unsafe.AsRef(block));
            ref byte destinationRef = ref MemoryMarshal.GetReference(_output.Span);

            for (int destY = 0; destY < writeHeight; destY++)
            {
                ref short blockRowRef = ref Unsafe.Add(ref blockRef, destY * 8);
                ref byte destinationRowRef = ref Unsafe.Add(ref destinationRef, ((y + destY) * width + x) * componentCount + componentIndex);
                for (int destX = 0; destX < writeWidth; destX++)
                {
                    Unsafe.Add(ref destinationRowRef, destX * componentCount) = TiffMathHelper.ClampTo8Bit(Unsafe.Add(ref blockRowRef, destX));
                }
            }
        }
    }
}
