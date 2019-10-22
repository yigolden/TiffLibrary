namespace TiffLibrary
{
    /// <summary>
    /// Positioning of chrominance component samples relative to the luminance samples
    /// </summary>
    public enum TiffYCbCrPositioning : ushort
    {
        /// <summary>
        /// Unspecified.
        /// </summary>
        Unspecified = 0,

        /// <summary>
        /// Centered.
        /// </summary>
        Centered = 1,

        /// <summary>
        /// Cosited.
        /// </summary>
        Cosited = 2,
    }
}
