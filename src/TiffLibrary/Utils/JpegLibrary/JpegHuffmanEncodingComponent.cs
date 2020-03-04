#nullable enable

namespace JpegLibrary
{
    internal class JpegHuffmanEncodingComponent
    {
        public int ComponentIndex { get; internal set; }
        public byte HorizontalSamplingFactor { get; internal set; }
        public byte VerticalSamplingFactor { get; internal set; }
        internal int DcPredictor { get; set; }
        internal byte DcTableIdentifier { get; set; }
        internal byte AcTableIdentifier { get; set; }
        internal JpegHuffmanEncodingTable? DcTable { get; set; }
        internal JpegHuffmanEncodingTable? AcTable { get; set; }
        internal JpegHuffmanEncodingTableBuilder? DcTableBuilder { get; set; }
        internal JpegHuffmanEncodingTableBuilder? AcTableBuilder { get; set; }
        internal JpegQuantizationTable QuantizationTable { get; set; }

        internal int HorizontalSubsamplingFactor { get; set; }
        internal int VerticalSubsamplingFactor { get; set; }
    }
}
