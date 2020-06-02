using System;

namespace TiffLibrary.ImageEncoder
{
    /// <summary>
    /// Options to use when encoding with JPEG compression.
    /// </summary>
    public class TiffJpegEncodingOptions
    {
        internal static TiffJpegEncodingOptions Default { get; } = new TiffJpegEncodingOptions();

        /// <summary>
        /// Gets or sets the JPEG encoding quality factor when compressing using JPEG.
        /// </summary>
        public int Quality { get; set; } = 75;

        /// <summary>
        /// When <see cref="UseSharedHuffmanTables"/> is set, JPEG huffman tables are written into the JPEGTables tag in the IFD. This option enables sharing huffman table definitions across strips or tiles.
        /// It is ignored and assumed to be false when <see cref="OptimizeCoding"/> is set.
        /// </summary>
        public bool UseSharedHuffmanTables { get; set; } = true;

        /// <summary>
        /// When <see cref="UseSharedQuantizationTables"/> is set, JPEG quantization tables are written into the JPEGTables tag in the IFD. This option enables sharing quantization table definitions across strips or tiles.
        /// </summary>
        public bool UseSharedQuantizationTables { get; set; } = true;

        /// <summary>
        /// When this flag is set, optimal Huffman tables are generated for each strip or tile.
        /// </summary>
        public bool OptimizeCoding { get; set; } = false;
    }
}
