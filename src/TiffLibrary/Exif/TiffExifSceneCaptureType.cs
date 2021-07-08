using System;

namespace TiffLibrary.Exif
{
    /// <summary>
    /// Indicates the type of scene that was shot.
    /// </summary>
    [CLSCompliant(false)]
    public enum TiffExifSceneCaptureType : ushort
    {
        /// <summary>
        /// Standard
        /// </summary>
        Standard = 0,

        /// <summary>
        /// Landscape
        /// </summary>
        Landscape = 1,

        /// <summary>
        /// Portrait
        /// </summary>
        Portrait = 2,

        /// <summary>
        /// NightScene
        /// </summary>
        NightScene = 3
    }
}
