using System;
using System.Buffers;

namespace JpegLibrary
{
    internal readonly struct JpegQuantizationTable
    {
        private readonly ushort[] _elements;

        public JpegQuantizationTable(byte elementPrecision, byte identifier, ushort[] elements)
        {
            ElementPrecision = elementPrecision;
            Identifier = identifier;
            _elements = elements ?? throw new ArgumentNullException(nameof(elements));

            if (elements.Length != 64)
            {
                throw new ArgumentException("elements.Length must be 64.");
            }
        }

        public byte ElementPrecision { get; }
        public byte Identifier { get; }

        public ReadOnlySpan<ushort> Elements => _elements;

        public bool IsEmpty => ElementPrecision == 0 && Identifier == 0 && _elements is null;

        public byte BytesRequired => ElementPrecision == 0 ? (byte)(64 + 1) : (byte)(128 + 1);

        public static bool TryParse(ReadOnlySequence<byte> buffer, out JpegQuantizationTable quantizationTable, out int bytesConsumed)
        {
            if (buffer.IsSingleSegment)
            {
#if NO_READONLYSEQUENCE_FISTSPAN
                return TryParse(buffer.First.Span, out quantizationTable, out bytesConsumed);
#else
                return TryParse(buffer.FirstSpan, out quantizationTable, out bytesConsumed);
#endif
            }

            bytesConsumed = 0;
            if (buffer.IsEmpty)
            {
                quantizationTable = default;
                return false;
            }

#if NO_READONLYSEQUENCE_FISTSPAN
            byte b = buffer.First.Span[0];
#else
            byte b = buffer.FirstSpan[0];
#endif
            bytesConsumed++;

            return TryParse((byte)(b >> 4), (byte)(b & 0xf), buffer.Slice(1), out quantizationTable, ref bytesConsumed);
        }

        public static bool TryParse(ReadOnlySpan<byte> buffer, out JpegQuantizationTable quantizationTable, out int bytesConsumed)
        {
            bytesConsumed = 0;
            if (buffer.IsEmpty)
            {
                quantizationTable = default;
                return false;
            }

            byte b = buffer[0];
            bytesConsumed++;

            return TryParse((byte)(b >> 4), (byte)(b & 0xf), buffer.Slice(1), out quantizationTable, ref bytesConsumed);
        }

        public static bool TryParse(byte precision, byte identifier, ReadOnlySequence<byte> buffer, out JpegQuantizationTable quantizationTable, ref int bytesConsumed)
        {
            if (buffer.IsSingleSegment)
            {
#if NO_READONLYSEQUENCE_FISTSPAN
                return TryParse(precision, identifier, buffer.First.Span, out quantizationTable, ref bytesConsumed);
#else
                return TryParse(precision, identifier, buffer.FirstSpan, out quantizationTable, ref bytesConsumed);
#endif
            }

            ushort[] elements;
            Span<byte> local = stackalloc byte[128];
            if (precision == 0)
            {
                if (buffer.Length < 64)
                {
                    quantizationTable = default;
                    return false;
                }

                buffer.Slice(0, 64).CopyTo(local);

                elements = new ushort[64];
                for (int i = 0; i < 64; i++)
                {
                    elements[i] = local[i];
                }
                bytesConsumed += 64;
            }
            else if (precision == 1)
            {
                if (buffer.Length < 128)
                {
                    quantizationTable = default;
                    return false;
                }

                buffer.Slice(0, 128).CopyTo(local);

                elements = new ushort[64];
                for (int i = 0; i < 64; i++)
                {
                    elements[i] = (ushort)(local[2 * i] << 8 | local[2 * i + 1]);
                }
                bytesConsumed += 128;
            }
            else
            {
                quantizationTable = default;
                return false;
            }

            quantizationTable = new JpegQuantizationTable(precision, identifier, elements);
            return true;
        }

        public static bool TryParse(byte precision, byte identifier, ReadOnlySpan<byte> buffer, out JpegQuantizationTable quantizationTable, ref int bytesConsumed)
        {
            ushort[] elements;
            if (precision == 0)
            {
                if (buffer.Length < 64)
                {
                    quantizationTable = default;
                    return false;
                }

                elements = new ushort[64];
                for (int i = 0; i < 64; i++)
                {
                    elements[i] = buffer[i];
                }
                bytesConsumed += 64;
            }
            else if (precision == 1)
            {
                if (buffer.Length < 128)
                {
                    quantizationTable = default;
                    return false;
                }

                elements = new ushort[64];
                for (int i = 0; i < 64; i++)
                {
                    elements[i] = (ushort)(buffer[2 * i] << 8 | buffer[2 * i + 1]);
                }
                bytesConsumed += 128;
            }
            else
            {
                quantizationTable = default;
                return false;
            }

            quantizationTable = new JpegQuantizationTable(precision, identifier, elements);
            return true;
        }

        public bool TryWrite(Span<byte> buffer, out int bytesWritten)
        {
            bytesWritten = 0;
            if (buffer.IsEmpty)
            {
                return false;
            }

            buffer[0] = (byte)(ElementPrecision << 4 | (Identifier & 0xf));
            buffer = buffer.Slice(1);
            bytesWritten++;

            ReadOnlySpan<ushort> elements = Elements;
            if (ElementPrecision == 0)
            {
                if (buffer.Length < 64)
                {
                    return false;
                }

                for (int i = 0; i < 64; i++)
                {
                    buffer[i] = (byte)elements[i];
                }
                bytesWritten += 64;
            }
            else if (ElementPrecision == 1)
            {
                if (buffer.Length < 128)
                {
                    return false;
                }

                for (int i = 0; i < 64; i++)
                {
                    buffer[2 * i] = (byte)(elements[i] >> 8);
                    buffer[2 * i + 1] = (byte)elements[i];
                }
                bytesWritten += 128;
            }

            return true;
        }
    }
}
