namespace TiffLibrary
{
    /// <summary>
    /// Specifies that each pixel has m extra components whose interpretation is defined by one of the values listed below. When this field is used, the SamplesPerPixel field has a value greater than the PhotometricInterpretation field suggests.
    /// ExtraSamples is typically used to include non-color information, such as opacity, in an image.
    /// </summary>
    public enum TiffExtraSample : ushort
    {
        /// <summary>
        /// Unspecified data.
        /// </summary>
        UnspecifiedData = 0,

        /// <summary>
        /// Associated alpha data (with pre-multiplied color).
        /// </summary>
        AssociatedAlphaData = 1,

        /// <summary>
        /// Unassociated alpha data.
        /// </summary>
        UnassociatedAlphaData = 2,
    }
}
