using System;

namespace TiffLibrary.Exif
{
    /// <summary>
    /// Indicates the distance to the subject.
    /// </summary>
    [CLSCompliant(false)]
    public enum TiffExifSubjectDistanceRange : ushort
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Macro
        /// </summary>
        Macro = 1,

        /// <summary>
        /// CloseView
        /// </summary>
        CloseView = 2,

        /// <summary>
        /// DistantView
        /// </summary>
        DistantView = 3,
    }
}
