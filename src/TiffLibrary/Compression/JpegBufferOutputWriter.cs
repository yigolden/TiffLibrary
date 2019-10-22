using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JpegLibrary;
using TiffLibrary.Utils;

namespace TiffLibrary.Compression
{
    internal sealed class JpegBufferOutputWriter : JpegBlockOutputWriter
    {
        private int _width;
        private int _skippedScanlines;
        private int _height;
        private int _componentCount;
        private Memory<byte> _output;

        public JpegBufferOutputWriter()
        {
        }

        public void Update(int width, int skippedScanlines, int height, int componentsCount, Memory<byte> output)
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

        public void Reset()
        {
            _width = default;
            _height = default;
            _componentCount = default;
            _output = default;
        }

        public override void WriteBlock(in JpegBlock8x8 block, int componentIndex, int x, int y, int horizontalSamplingFactor, int verticalSamplingFactor)
        {
            int componentCount = _componentCount;
            int width = _width;

            if (x > width || y > _height)
            {
                return;
            }
            if ((y + 8 * verticalSamplingFactor) <= _skippedScanlines)
            {
                // No need to decode region before the fist requested scanline.
                return;
            }

            int writeWidth = Math.Min(width - x, 8 * horizontalSamplingFactor);
            int writeHeight = Math.Min(_height - y, 8 * verticalSamplingFactor);

            int hShift = 0, vShift = 0;
            while ((horizontalSamplingFactor = horizontalSamplingFactor / 2) != 0)
                hShift++;
            while ((verticalSamplingFactor = verticalSamplingFactor / 2) != 0)
                vShift++;

            ref short sourceRef = ref Unsafe.As<JpegBlock8x8, short>(ref Unsafe.AsRef(in block));

            // destinationSpan Range check ?
            ref byte destinationRef = ref MemoryMarshal.GetReference(_output.Span);

            for (int destY = 0; destY < writeHeight; destY++)
            {
                ref short sourceRowRef = ref Unsafe.Add(ref sourceRef, 8 * (destY >> vShift));
                ref byte rowRef = ref Unsafe.Add(ref destinationRef, ((y + destY) * width + x) * componentCount);

                for (int destX = 0; destX < writeWidth; destX++)
                {
                    // Use bit shift to accelerate
                    //int blockOffset = destX / horizontalSamplingFactor;
                    int blockOffset = destX >> hShift;
                    Unsafe.Add(ref rowRef, destX * componentCount + componentIndex) = TiffMathHelper.ClampTo8Bit(Unsafe.Add(ref sourceRowRef, blockOffset));
                }
            }
        }
    }
}
