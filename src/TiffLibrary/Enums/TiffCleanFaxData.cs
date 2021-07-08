using System;

namespace TiffLibrary
{
    /// <summary>
    /// Regenerated line info.
    /// </summary>
    [CLSCompliant(false)]
    public enum TiffCleanFaxData : ushort
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
