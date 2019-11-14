#nullable enable

using System;
using System.Collections.Generic;

namespace JpegLibrary
{
    internal struct JpegHuffmanEncodingTableBuilderCollection : IDisposable
    {
        private List<TableBuilderWithIdentifier>? _builders;

        public JpegHuffmanEncodingTableBuilder GetOrCreateTableBuilder(bool isDcTable, byte identifier)
        {
            byte tableClass = isDcTable ? (byte)0 : (byte)1;
            List<TableBuilderWithIdentifier>? builders = _builders;
            if (builders is null)
            {
                builders = _builders = new List<TableBuilderWithIdentifier>(4);
            }
            foreach (TableBuilderWithIdentifier builder in builders)
            {
                if (builder.TableClass == tableClass && builder.Identifier == identifier)
                {
                    return builder.TableBuilder;
                }
            }
            var table = new JpegHuffmanEncodingTableBuilder();
            builders.Add(new TableBuilderWithIdentifier(tableClass, identifier, table));
            return table;
        }

        public JpegHuffmanEncodingTableCollection BuildTables()
        {
            if (_builders is null)
            {
                return default;
            }
            var collection = new JpegHuffmanEncodingTableCollection();
            foreach (TableBuilderWithIdentifier builder in _builders)
            {
                collection.AddTable(builder.TableClass, builder.Identifier, builder.TableBuilder.Build());
            }
            return collection;
        }

        public void Dispose()
        {
            if (!(_builders is null))
            {
                foreach (TableBuilderWithIdentifier tableBuilder in _builders)
                {
                    //tableBuilder.TableBuilder.Dispose();
                }
                _builders.Clear();
                _builders = null;
            }
        }


        readonly struct TableBuilderWithIdentifier
        {
            public TableBuilderWithIdentifier(byte tableClass, byte identifier, JpegHuffmanEncodingTableBuilder tableBuilder)
            {
                TableClass = tableClass;
                Identifier = identifier;
                TableBuilder = tableBuilder;
            }

            public byte TableClass { get; }
            public byte Identifier { get; }
            public JpegHuffmanEncodingTableBuilder TableBuilder { get; }
        }
    }
}
