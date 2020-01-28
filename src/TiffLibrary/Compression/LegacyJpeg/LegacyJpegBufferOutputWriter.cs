using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JpegLibrary;
using TiffLibrary.Utils;

namespace TiffLibrary.Compression
{
    internal class LegacyJpegBufferOutputWriter : JpegBlockOutputWriter
    {
        private readonly int _skippedWidth;
        private readonly int _bufferWidth;
        private readonly int _skippedHeight;
        private readonly int _bufferHeight;
        private readonly int _componentCount;
        private readonly Memory<byte> _output;

        public LegacyJpegBufferOutputWriter(int skippedWidth, int bufferWidth, int skippedHeight, int bufferHeight, int componentCount, Memory<byte> output)
        {
            if (skippedWidth % 8 != 0)
            {
                throw new ArgumentException("must be a multiple of 8.", nameof(skippedWidth));
            }
            if (skippedHeight % 8 != 0)
            {
                throw new ArgumentException("must be a multiple of 8.", nameof(skippedHeight));
            }

            if (output.Length < (bufferWidth * bufferHeight * componentCount))
            {
                throw new ArgumentException("buffer is too small.", nameof(output));
            }

            _skippedWidth = skippedWidth;
            _bufferWidth = bufferWidth;
            _skippedHeight = skippedHeight;
            _bufferHeight = bufferHeight;
            _componentCount = componentCount;
            _output = output;
        }

        public override void WriteBlock(ref short blockRef, int componentIndex, int x, int y)
        {
            if ((x + 8) <= _skippedWidth || (y + 8) <= _skippedHeight)
            {
                return;
            }

            x -= _skippedWidth;
            y -= _skippedHeight;

            int width = _bufferWidth;
            if (x >= width || y >= _bufferHeight)
            {
                return;
            }

            int writeWidth = Math.Min(width - x, 8);
            int writeHeight = Math.Min(_bufferHeight - y, 8);

            if (writeWidth <= 0 || writeHeight <= 0)
            {
                return;
            }

            int componentCount = _componentCount;

            ref byte destinationRef = ref MemoryMarshal.GetReference(_output.Span);
            destinationRef = ref Unsafe.Add(ref destinationRef, y * width * componentCount + x * componentCount + componentIndex);

            for (int destY = 0; destY < writeHeight; destY++)
            {
                ref byte destinationRowRef = ref Unsafe.Add(ref destinationRef, destY * width * componentCount);
                for (int destX = 0; destX < writeWidth; destX++)
                {
                    Unsafe.Add(ref destinationRowRef, destX * componentCount) = TiffMathHelper.ClampTo8Bit(Unsafe.Add(ref blockRef, destX));
                }
                blockRef = ref Unsafe.Add(ref blockRef, 8);
            }
        }
    }
}
