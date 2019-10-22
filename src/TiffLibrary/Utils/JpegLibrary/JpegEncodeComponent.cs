﻿namespace JpegLibrary
{
    internal class JpegEncodeComponent
    {
        public int ComponentIndex { get; internal set; }
        public byte HorizontalSamplingFactor { get; internal set; }
        public byte VerticalSamplingFactor { get; internal set; }
        internal int DcPredictor { get; set; }
        internal byte DcTableIdentifier { get; set; }
        internal byte AcTableIdentifier { get; set; }
        internal JpegHuffmanEncodingTable DcTable { get; set; }
        internal JpegHuffmanEncodingTable AcTable { get; set; }
        internal JpegQuantizationTable QuantizationTable { get; set; }
    }
}
