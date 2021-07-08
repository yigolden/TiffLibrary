using System;

namespace TiffLibrary.Exif
{
    /// <summary>
    /// Indicates the direction of saturation processing applied by the camera when the image was shot.
    /// </summary>
    [CLSCompliant(false)]
    public enum TiffExifSaturation : ushort
    {
        /// <summary>
        /// Normal
        /// </summary>
        Normal = 0,

        /// <summary>
        /// Low saturation
        /// </summary>
        Low = 1,

        /// <summary>
        /// High saturation
        /// </summary>
        High = 2,
    }
}
