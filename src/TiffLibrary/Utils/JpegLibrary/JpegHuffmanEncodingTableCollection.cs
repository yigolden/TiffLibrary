#nullable enable

using System;
using System.Collections.Generic;

namespace JpegLibrary
{
    internal struct JpegHuffmanEncodingTableCollection
    {
        private List<EncodingTableWithIdentifier>? _tables;

        public bool IsEmpty => _tables is null;

        public bool ContainsTableBuilder()
        {
            if (_tables is null)
            {
                return false;
            }
            foreach (EncodingTableWithIdentifier table in _tables)
            {
                if (table.EncodingTable is JpegHuffmanEncodingTableBuilder)
                {
                    return true;
                }
            }
            return false;
        }

        public JpegHuffmanEncodingTableCollection DeepClone()
        {
            if (_tables is null)
            {
                return default;
            }
            return new JpegHuffmanEncodingTableCollection()
            {
                _tables = new List<EncodingTableWithIdentifier>(_tables)
            };
        }

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
                    return table.EncodingTable as JpegHuffmanEncodingTable;
                }
            }
            return null;
        }

        public JpegHuffmanEncodingTableBuilder? GetTableBuilder(bool isDcTable, byte identifier)
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
                    return table.EncodingTable as JpegHuffmanEncodingTableBuilder;
                }
            }
            return null;
        }

        public void AddTable(byte tableClass, byte identifier, JpegHuffmanEncodingTable? encodingTable)
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
            if (encodingTable is null)
            {
                _tables.Add(new EncodingTableWithIdentifier(tableClass, identifier, new JpegHuffmanEncodingTableBuilder()));
            }
            else
            {
                _tables.Add(new EncodingTableWithIdentifier(tableClass, identifier, encodingTable));
            }
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
                if (!(item.EncodingTable is JpegHuffmanEncodingTable encodingTable))
                {
                    throw new InvalidOperationException();
                }
                bytesRequired++;
                bytesRequired += encodingTable.BytesRequired;
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
                if (!(item.EncodingTable is JpegHuffmanEncodingTable encodingTable))
                {
                    throw new InvalidOperationException();
                }

                if (buffer.IsEmpty)
                {
                    return false;
                }
                buffer[0] = (byte)(item.TableClass << 4 | (item.Identifier & 0xf));
                buffer = buffer.Slice(1);
                bytesWritten++;

                if (!encodingTable.TryWrite(buffer, out int bytes))
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
                if (!(item.EncodingTable is JpegHuffmanEncodingTable encodingTable))
                {
                    throw new InvalidOperationException();
                }

                int bytesRequired = 1 + encodingTable.BytesRequired;
                Span<byte> buffer = writer.GetSpan(bytesRequired);
                buffer[0] = (byte)(item.TableClass << 4 | (item.Identifier & 0xf));
                buffer = buffer.Slice(1);
                encodingTable.TryWrite(buffer, out _);
                writer.Advance(bytesRequired);
            }
        }

        public void BuildTables(bool optimal)
        {
            List<EncodingTableWithIdentifier>? tables = _tables;
            if (tables is null)
            {
                return;
            }
            for (int i = 0; i < tables.Count; i++)
            {
                EncodingTableWithIdentifier current = tables[i];
                if (current.EncodingTable is JpegHuffmanEncodingTableBuilder builder)
                {
                    tables[i] = new EncodingTableWithIdentifier(current.TableClass, current.Identifier, builder.Build(optimal));
                }
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
            public EncodingTableWithIdentifier(byte tableClass, byte identifier, JpegHuffmanEncodingTableBuilder tableBuilder)
            {
                TableClass = tableClass;
                Identifier = identifier;
                EncodingTable = tableBuilder;
            }

            public byte TableClass { get; }
            public byte Identifier { get; }

            // This can either be JpegHuffmanEncodingTable or JpegHuffmanEncodingTableBuilder
            public object EncodingTable { get; }
        }

    }
}
