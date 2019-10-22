namespace TiffLibrary
{
    /// <summary>
    /// The logical order of bits within a byte.
    /// </summary>
    public enum TiffFillOrder : ushort
    {
        /// <summary>
        /// pixels are arranged within a byte such that pixels with lower column values are stored in the higher-order bits of the byte.
        /// 1-bit uncompressed data example: Pixel 0 of a row is stored in the high-order bit of byte 0, pixel 1 is stored in the next-highest bit, ..., pixel 7 is stored in the loworder bit of byte 0, pixel 8 is stored in the high-order bit of byte 1, and so on.
        /// CCITT 1-bit compressed data example: The high-order bit of the first compression code is stored in the high-order bit of byte 0, the next-highest bit of the first compression code is stored in the next-highest bit of byte 0, and so on.
        /// </summary>
        HigherOrderBitsFirst = 1,

        /// <summary>
        /// pixels are arranged within a byte such that pixels with lower column values are stored in the lower-order bits of the byte.
        /// We recommend that FillOrder=2 be used only in special-purpose applications. It is easy and inexpensive for writers to reverse bit order by using a 256-byte lookup table. FillOrder = 2 should be used only when BitsPerSample = 1 and the data is either uncompressed or compressed using CCITT 1D or 2D compression, to avoid potentially ambigous situations.
        /// </summary>
        LowerOrderBitsFirst = 2,
    }
}
