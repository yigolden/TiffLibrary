namespace TiffLibrary
{
    /// <summary>
    /// The precision of the information contained in the GrayResponseCurve.
    /// </summary>
    public enum TiffGrayResponseUnit : ushort
    {
        /// <summary>
        /// Number represents tenths of a unit.
        /// </summary>
        Tenths = 1,

        /// <summary>
        /// Number represents hundredths of a unit.
        /// </summary>
        Hundredths = 2,

        /// <summary>
        /// Number represents thousandths of a unit.
        /// </summary>
        Thousandths = 3,

        /// <summary>
        /// Number represents ten-thousandths of a unit.
        /// </summary>
        TenThousandths = 4,

        /// <summary>
        /// Number represents hundred-thousandths of a unit.
        /// </summary>
        HundredThousandths = 5,
    }
}
