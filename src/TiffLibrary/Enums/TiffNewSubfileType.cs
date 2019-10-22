using System;

namespace TiffLibrary
{
    /// <summary>
    /// A general indication of the kind of data contained in this subfile.
    /// </summary>
    [Flags]
#pragma warning disable CA1714 // CA1714: Flags enums should have plural names
    public enum TiffNewSubfileType : uint
#pragma warning restore CA1714 // CA1714: Flags enums should have plural names
    {
        /// <summary>
        /// None.
        /// </summary>
        None = 0,

        /// <summary>
        /// Bit 0 is 1 if the image is a reduced-resolution version of another image in this TIFF file; else the bit is 0.
        /// </summary>
        ReducedResolution = 0b1,

        /// <summary>
        /// Bit 1 is 1 if the image is a single page of a multi-page image (see the PageNumber field description); else the bit is 0.
        /// </summary>
        Page = 0b10,

        /// <summary>
        /// Bit 2 is 1 if the image defines a transparency mask for another image in this TIFF file. The PhotometricInterpretation value must be 4, designating a transparency mask.
        /// </summary>
        TransparencyMask = 0b100,
    }
}
