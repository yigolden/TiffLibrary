using System;

namespace TiffLibrary.Exif
{
    /// <summary>
    /// Indicates the direction of contrast processing applied by the camera when the image was shot.
    /// </summary>
    [CLSCompliant(false)]
    public enum TiffExifContrast : ushort
    {
        /// <summary>
        /// Normal
        /// </summary>
        Normal = 0,

        /// <summary>
        /// Soft
        /// </summary>
        Soft = 1,

        /// <summary>
        /// Hard
        /// </summary>
        Hard = 2,
    }
}
