#nullable enable

using System;
using System.Collections.Generic;

namespace JpegLibrary
{
    internal struct JpegHuffmanEncodingTableCollection
    {
        private List<EncodingTableWithIdentifier>? _tables;

        public bool IsEmpty => _tables is null;

        public JpegHuffmanEncodingTable? GetTable(bool isDcTable, byte identifier)
        {
            if (_tables is null)
            {
                return null;
            }
            byte tableClass = isDcTable ? (byte)0 : (byte)1;
            foreach (EncodingTableWithIdentifier table in _tables)
            {
                if (table.TableClass == tableClass && table.Identifier == identifier)
                {
                    return table.EncodingTable;
                }
            }
            return null;
        }

        public void AddTable(byte tableClass, byte identifier, JpegHuffmanEncodingTable encodingTable)
        {
            if (_tables is null)
            {
                _tables = new List<EncodingTableWithIdentifier>();
            }
            foreach (EncodingTableWithIdentifier table in _tables)
            {
                if (table.TableClass == tableClass && table.Identifier == identifier)
                {
                    throw new InvalidOperationException();
                }
            }
            _tables.Add(new EncodingTableWithIdentifier(tableClass, identifier, encodingTable));
        }

        public ushort GetTotalBytesRequired()
        {
            if (_tables is null)
            {
                throw new InvalidOperationException();
            }

            ushort bytesRequired = 0;
            foreach (EncodingTableWithIdentifier item in _tables)
            {
                bytesRequired++;
                bytesRequired += item.EncodingTable.BytesRequired;
            }
            return bytesRequired;
        }

        public bool TryWrite(Span<byte> buffer, out int bytesWritten)
        {
            if (_tables is null)
            {
                throw new InvalidOperationException();
            }

            bytesWritten = 0;
            foreach (EncodingTableWithIdentifier item in _tables)
            {
                if (buffer.IsEmpty)
                {
                    return false;
                }
                buffer[0] = (byte)(item.TableClass << 4 | (item.Identifier & 0xf));
                buffer = buffer.Slice(1);
                bytesWritten++;

                if (!item.EncodingTable.TryWrite(buffer, out int bytes))
                {
                    bytesWritten += bytes;
                    return false;
                }

                buffer = buffer.Slice(bytes);
                bytesWritten += bytes;
            }
            return true;
        }

        public void Write(ref JpegWriter writer)
        {
            if (_tables is null)
            {
                throw new InvalidOperationException();
            }

            foreach (EncodingTableWithIdentifier item in _tables)
            {
                int bytesRequired = 1 + item.EncodingTable.BytesRequired;
                Span<byte> buffer = writer.GetSpan(bytesRequired);
                buffer[0] = (byte)(item.TableClass << 4 | (item.Identifier & 0xf));
                buffer = buffer.Slice(1);
                item.EncodingTable.TryWrite(buffer, out _);
                writer.Advance(bytesRequired);
            }
        }

        readonly struct EncodingTableWithIdentifier
        {
            public EncodingTableWithIdentifier(byte tableClass, byte identifier, JpegHuffmanEncodingTable encodingTable)
            {
                TableClass = tableClass;
                Identifier = identifier;
                EncodingTable = encodingTable;
            }

            public byte TableClass { get; }
            public byte Identifier { get; }
            public JpegHuffmanEncodingTable EncodingTable { get; }
        }

    }
}
