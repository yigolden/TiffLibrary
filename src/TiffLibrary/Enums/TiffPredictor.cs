using System;

namespace TiffLibrary
{
    /// <summary>
    /// A predictor is a mathematical operator that is applied to the image data before an encoding scheme is applied. Currently this field is used only with LZW (Compression=5) encoding because LZW is probably the only TIFF encoding scheme that benefits significantly from a predictor step. See Section 13.
    /// </summary>
    [CLSCompliant(false)]
    public enum TiffPredictor : ushort
    {
        /// <summary>
        /// No prediction scheme used before coding.
        /// </summary>
        None = 1,

        /// <summary>
        /// Horizontal differencing.
        /// </summary>
        HorizontalDifferencing = 2,
    }
}
