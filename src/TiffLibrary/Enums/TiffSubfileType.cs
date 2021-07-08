using System;

namespace TiffLibrary
{
    /// <summary>
    /// A general indication of the kind of data contained in this subfile.
    /// </summary>
    [CLSCompliant(false)]
    public enum TiffSubfileType : ushort
    {
        /// <summary>
        /// full-resolution image data
        /// </summary>
        FullResolutionImageData = 1,

        /// <summary>
        /// reduced-resolution image data
        /// </summary>
        ReducedResolutionImageData = 2,

        /// <summary>
        /// a single page of a multi-page image (see the PageNumber field description).
        /// </summary>
        SinglePage = 3,
    }
}
