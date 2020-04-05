namespace TiffLibrary
{
    /// <summary>
    /// Compression type.
    /// </summary>
    public enum TiffCompression : ushort
    {
        /// <summary>
        /// Compression type is not specified.
        /// </summary>
        Unspecified = 0,

        /// <summary>
        /// No compression, but pack data into bytes as tightly as possible, leaving no unused bits (except at the end of a row). The component values are stored as an array of type BYTE. Each scan line (row) is padded to the next BYTE boundary.
        /// </summary>
        NoCompression = 1,

        /// <summary>
        /// CCITT Group 3 1-Dimensional Modified Huffman run length encoding. See Section 10 for a description of Modified Huffman Compression.
        /// </summary>
        ModifiedHuffmanCompression = 2,

        /// <summary>
        /// CCITT T.4 bi-level encoding as specified in section 4, Coding, of CCITT Recommendation T.4: “Standardization of Group 3 Facsimile apparatus for document transmission.” International Telephone and Telegraph Consultative Committee (CCITT, Geneva: 1988).
        /// </summary>
        T4Encoding = 3,

        /// <summary>
        /// CCITT T.6 bi-level encoding as specified in section 2 of CCITT Recommendation T.6: “Facsimile coding schemes and coding control functions for Group 4 facsimile apparatus.” International Telephone and Telegraph Consultative Committee (CCITT, Geneva: 1988).
        /// </summary>
        T6Encoding = 4,

        /// <summary>
        /// LZW compression.
        /// </summary>
        Lzw = 5,

        /// <summary>
        /// Obsoleted JPEG compression as specified in section 22 of TIFF 6.0 Specification.
        /// </summary>
        OldJpeg = 6,

        /// <summary>
        /// JPEG compression (see Adobe Photoshop® TIFF Technical Notes (March 22, 2002)).
        /// </summary>
        Jpeg = 7,

        /// <summary>
        /// Deflate compression, using zlib data format (see Adobe Photoshop® TIFF Technical Notes (March 22, 2002)).
        /// </summary>
        Deflate = 8,

        /// <summary>
        /// PackBits compression, a simple byte-oriented run length scheme.
        /// </summary>
        PackBits = 32773,

        /// <summary>
        /// Deflate compression - old.
        /// </summary>
        OldDeflate = 32946,

        /// <summary>
        /// ThunderScan 4-bit compression.
        /// </summary>
        ThunderScan = 32809,
    }
}
