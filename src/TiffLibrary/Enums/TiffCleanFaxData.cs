namespace TiffLibrary
{
#pragma warning disable CA1717 // CA1717: Only FlagsAttribute enums should have plural names
    /// <summary>
    /// Regenerated line info.
    /// </summary>
    public enum TiffCleanFaxData : ushort
#pragma warning restore CA1717 // CA1717: Only FlagsAttribute enums should have plural names
    {
        /// <summary>
        /// No errors detected.
        /// </summary>
        Clean = 0,

        /// <summary>
        /// Receiver regenerated lines.
        /// </summary>
        Regenerated = 1,

        /// <summary>
        /// Uncorrected errors exist.
        /// </summary>
        Unclean = 2,
    }
}
