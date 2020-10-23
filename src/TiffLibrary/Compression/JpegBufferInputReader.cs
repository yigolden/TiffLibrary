using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JpegLibrary;

namespace TiffLibrary.Compression
{
    internal sealed class JpegBufferInputReader : JpegBlockInputReader
    {
        private int _width;
        private int _height;
        private int _componentCount;
        private ReadOnlyMemory<byte> _buffer;

        public void Update(int width, int height, int componentsCount, ReadOnlyMemory<byte> buffer)
        {
            if (buffer.Length < (width * height * componentsCount))
            {
                throw new ArgumentException("Input buffer is too small.");
            }

            _width = width;
            _height = height;
            _componentCount = componentsCount;
            _buffer = buffer;
        }

        public void Reset()
        {
            _width = default;
            _height = default;
            _componentCount = default;
            _buffer = default;
        }


        public override int Width => _width;

        public override int Height => _height;

        public override void ReadBlock(ref short blockRef, int componentIndex, int x, int y)
        {
            int width = _width;
            int componentCount = _componentCount;

            ref byte sourceRef = ref MemoryMarshal.GetReference(MemoryMarshal.AsBytes(_buffer.Span));

            int blockWidth = Math.Min(width - x, 8);
            int blockHeight = Math.Min(_height - y, 8);

            if (blockWidth != 8 || blockHeight != 8)
            {
                Unsafe.As<short, JpegBlock8x8>(ref blockRef) = default;
            }
            
            for (int offsetY = 0; offsetY < blockHeight; offsetY++)
            {
                int sourceRowOffset = (y + offsetY) * width + x;
                ref short destinationRowRef = ref Unsafe.Add(ref blockRef, offsetY * 8);
                for (int offsetX = 0; offsetX < blockWidth; offsetX++)
                {
                    Unsafe.Add(ref destinationRowRef, offsetX) = Unsafe.Add(ref sourceRef, (sourceRowOffset + offsetX) * componentCount + componentIndex);
                }
            }
        }
    }
}
