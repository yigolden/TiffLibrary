using System;

namespace TiffLibrary.Compression
{

    internal readonly ref struct CcittTwoDimensionalReferenceScanline
    {
        private readonly ReadOnlySpan<byte> _scanline;
        private readonly int _width;
        private readonly byte _whiteByte;

        public bool IsEmpty => _scanline.IsEmpty;

        public CcittTwoDimensionalReferenceScanline(bool whiteIsZero, ReadOnlySpan<byte> scanline)
        {
            _scanline = scanline;
            _width = scanline.Length;
            _whiteByte = whiteIsZero ? (byte)0 : (byte)255;
        }

        public CcittTwoDimensionalReferenceScanline(bool whiteIsZero, int width)
        {
            _scanline = default;
            _width = width;
            _whiteByte = whiteIsZero ? (byte)0 : (byte)255;
        }

        // b1: The first changing element on the reference line to the right of a0 and of opposite colour to a0
        public int FindB1(int a0, byte a0Byte)
        {
            if (_scanline.IsEmpty)
            {
                return FindB1ForImaginaryWhiteLine(a0, a0Byte);
            }
            return FindB1ForNormalLine(a0, a0Byte);
        }

        private int FindB1ForImaginaryWhiteLine(int a0, byte a0Byte)
        {
            if (a0 < 0)
            {
                if (a0Byte != _whiteByte)
                {
                    return 0;
                }
            }
            return _width;
        }

        private int FindB1ForNormalLine(int a0, byte a0Byte)
        {
            int offset = 0;
            if (a0 < 0)
            {
                if (a0Byte != _scanline[0])
                {
                    return 0;
                }
            }
            else
            {
                offset = a0;
            }
            ReadOnlySpan<byte> searchSpace = _scanline.Slice(offset);
            byte searchByte = (byte)~a0Byte;
            int index = searchSpace.IndexOf(searchByte);
            if (index < 0)
            {
                return _scanline.Length;
            }
            if (index != 0)
            {
                return offset + index;
            }
            searchByte = (byte)~searchSpace[0];
            index = searchSpace.IndexOf(searchByte);
            if (index < 0)
            {
                return _scanline.Length;
            }
            searchSpace = searchSpace.Slice(index);
            offset += index;
            index = searchSpace.IndexOf((byte)~searchByte);
            if (index < 0)
            {
                return _scanline.Length;
            }
            return index + offset;
        }

        // b2: The next changing element to the right of b1 on the reference line. 
        public int FindB2(int b1)
        {
            if (_scanline.IsEmpty)
            {
                return FindB2ForImaginaryWhiteLine();
            }
            return FindB2ForNormalLine(b1);
        }

        private int FindB2ForImaginaryWhiteLine()
        {
            return _width;
        }

        private int FindB2ForNormalLine(int b1)
        {
            if (b1 >= _scanline.Length)
            {
                return _scanline.Length;
            }
            byte searchByte = (byte)~_scanline[b1];
            int offset = b1 + 1;
            ReadOnlySpan<byte> searchSpace = _scanline.Slice(offset);
            int index = searchSpace.IndexOf(searchByte);
            if (index == -1)
            {
                return _scanline.Length;
            }
            return offset + index;
        }

    }
}
