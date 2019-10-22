using System;

namespace TiffLibrary
{
    /// <summary>
    /// Options for T4 encoding.
    /// </summary>
    [Flags]
    public enum TiffT4Options
    {
        /// <summary>
        /// None.
        /// </summary>
        None = 0,

        /// <summary>
        /// Bit 0 is 1 for 2-dimensional coding (otherwise 1-dimensional is assumed). For 2-D coding, if more than one strip is specified, each strip must begin with a 1-dimensionally coded line. That is, RowsPerStrip should be a multiple of “Parameter K,” as documented in the CCITT specification.
        /// </summary>
        Is2DimensionalCoding = 1,

        /// <summary>
        /// Bit 1 is 1 if uncompressed mode is used.
        /// </summary>
        UseUncompressedMode = 2,

        /// <summary>
        /// Bit 2 is 1 if fill bits have been added as necessary before EOL codes such that EOL always ends on a byte boundary, thus ensuring an EOL-sequence of 1 byte preceded by a zero nibble:  xxxx-0000 0000-0001.
        /// </summary>
        FillBitsBeforeEOLCodes = 4,
    }
}
