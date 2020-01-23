namespace TiffLibrary.Exif
{
    /// <summary>
    /// Indicates the degree of overall image gain adjustment.
    /// </summary>
    public enum TiffExifGainControl : ushort
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,

        /// <summary>
        /// Low gain up
        /// </summary>
        LowGainUp = 1,

        /// <summary>
        /// High gain up
        /// </summary>
        HighGainUp = 2,

        /// <summary>
        /// Low gain down
        /// </summary>
        LowGainDown = 3,

        /// <summary>
        /// High gain down
        /// </summary>
        HighGainDown = 4,
    }
}
