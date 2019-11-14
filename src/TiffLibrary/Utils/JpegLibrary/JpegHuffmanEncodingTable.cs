#nullable enable

using System;
using System.Diagnostics;

namespace JpegLibrary
{
    internal class JpegHuffmanEncodingTable
    {
        private readonly int _codeCount;
        private readonly JpegCanonicalCode[] _codes;
        private readonly byte[] _symbolMap;

        public JpegHuffmanEncodingTable(JpegCanonicalCode[] codes)
        {
            _codes = codes;

            int codeCount = 0;
            _symbolMap = new byte[256];
            for (int i = 0; i < codes.Length; i++)
            {
                JpegCanonicalCode code = codes[i];
                if (code.CodeLength != 0)
                {
                    _symbolMap[code.Symbol] = (byte)i;
                    codeCount++;
                }
            }
            _codeCount = codeCount;
        }

        public ushort BytesRequired => (ushort)(16 + _codeCount);

        public bool TryWrite(Span<byte> buffer, out int bytesWritten)
        {
            bytesWritten = 0;
            if (buffer.Length < 16)
            {
                return false;
            }

            for (int len = 1; len <= 16; len++)
            {
                int count = 0;
                for (int i = _codes.Length - _codeCount; i < _codes.Length; i++)
                {
                    if (_codes[i].CodeLength == len)
                    {
                        count++;
                    }
                }
                buffer[len - 1] = (byte)count;
            }
            buffer = buffer.Slice(16);
            bytesWritten += 16;

            if (buffer.Length < _codeCount)
            {
                return false;
            }

            int index = 0;
            for (int i = _codes.Length - _codeCount; i < _codes.Length; i++)
            {
                buffer[index++] = _codes[i].Symbol;
            }
            bytesWritten += index;

            return true;
        }

        public void GetCode(int symbol, out ushort code, out int codeLength)
        {
            Debug.Assert((uint)symbol < 256);
            JpegCanonicalCode c = _codes[_symbolMap[symbol]];
            code = c.Code;
            codeLength = c.CodeLength;
        }
    }
}
