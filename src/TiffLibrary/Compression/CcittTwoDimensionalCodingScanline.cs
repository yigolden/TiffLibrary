using System;

namespace TiffLibrary.Compression
{

    internal readonly ref struct CcittTwoDimensionalCodingScanline
    {
        private readonly ReadOnlySpan<byte> _scanline;
        private readonly int _width;
        private readonly byte _whiteByte;

        public bool IsEmpty => _scanline.IsEmpty;

        public CcittTwoDimensionalCodingScanline(bool whiteIsZero, ReadOnlySpan<byte> scanline)
        {
            _scanline = scanline;
            _width = scanline.Length;
            _whiteByte = whiteIsZero ? (byte)0 : (byte)255;
        }

        public CcittTwoDimensionalCodingScanline(bool whiteIsZero, int width)
        {
            _scanline = default;
            _width = width;
            _whiteByte = whiteIsZero ? (byte)0 : (byte)255;
        }

        public int FindA1(int a0, byte a0Byte)
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

        public int FindA2(int a1)
        {
            if (a1 >= _scanline.Length)
            {
                return _scanline.Length;
            }
            byte searchByte = (byte)~_scanline[a1];
            int offset = a1 + 1;
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
