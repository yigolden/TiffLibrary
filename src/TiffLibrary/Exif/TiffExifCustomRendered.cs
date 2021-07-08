using System;

namespace TiffLibrary.Exif
{
    /// <summary>
    /// Indicates the use of special processing on image data, such as rendering geared to output.
    /// When special processing is performed, the reader is expected to disable or minimize any further processing.
    /// </summary>
    [CLSCompliant(false)]
    public enum TiffExifCustomRendered : ushort
    {
        /// <summary>
        /// Normal process
        /// </summary>
        NormalProcess = 0,

        /// <summary>
        /// Custom process
        /// </summary>
        CustomProcess = 1,
    }
}
