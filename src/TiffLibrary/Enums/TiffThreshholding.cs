using System;

namespace TiffLibrary
{
    /// <summary>
    /// For black and white TIFF files that represent shades of gray, the technique used to convert from gray to black and white pixels.
    /// </summary>
    [CLSCompliant(false)]
    public enum TiffThreshholding : ushort
    {

        /// <summary>
        /// No dithering or halftoning has been applied to the image data.
        /// </summary>
        NoThreshholding = 1,

        /// <summary>
        /// An ordered dither or halftone technique has been applied to the image data.
        /// </summary>
        Ordered = 2,

        /// <summary>
        /// A randomized process such as error diffusion has been applied to the image data.
        /// </summary>
        Randomized = 3,
    }
}
