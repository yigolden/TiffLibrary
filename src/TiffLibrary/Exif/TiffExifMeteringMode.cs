using System;

namespace TiffLibrary.Exif
{
    /// <summary>
    /// The metering mode.
    /// </summary>
    [CLSCompliant(false)]
    public enum TiffExifMeteringMode : ushort
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Average
        /// </summary>
        Average = 1,

        /// <summary>
        /// CenterWeightedAverage
        /// </summary>
        CenterWeightedAverage = 2,

        /// <summary>
        /// Spot
        /// </summary>
        Spot = 3,

        /// <summary>
        /// MultiSpot
        /// </summary>
        MultiSpot = 4,

        /// <summary>
        /// Pattern
        /// </summary>
        Pattern = 5,

        /// <summary>
        /// Partial
        /// </summary>
        Partial = 6,

        /// <summary>
        /// Other
        /// </summary>
        Other = 255,
    }
}
