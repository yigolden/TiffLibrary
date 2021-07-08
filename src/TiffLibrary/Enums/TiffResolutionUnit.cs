using System;

namespace TiffLibrary
{
    /// <summary>
    /// The unit of measurement for XResolution and YResolution.
    /// </summary>
    [CLSCompliant(false)]
    public enum TiffResolutionUnit : ushort
    {
        /// <summary>
        /// No absolute unit of measurement. Used for images that may have a non-square aspect ratio, but no meaningful absolute dimensions.
        /// </summary>
        None = 1,

        /// <summary>
        /// Inch.
        /// </summary>
        Inch = 2,

        /// <summary>
        /// Centimeter.
        /// </summary>
        Centimeter = 3,
    }
}
