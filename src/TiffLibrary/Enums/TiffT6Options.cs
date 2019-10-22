using System;

namespace TiffLibrary
{
    /// <summary>
    /// Options for T4 encoding.
    /// </summary>
    [Flags]
    public enum TiffT6Options
    {
        /// <summary>
        /// None.
        /// </summary>
        None = 0,

        /// <summary>
        /// Bit 1 is 1 if uncompressed mode is allowed in the encoding
        /// </summary>
        AllowUncompressedMode = 2,
    }
}
