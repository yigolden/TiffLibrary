namespace TiffLibrary
{
    /// <summary>
    /// The set of inks used in a separated (PhotometricInterpretation=5) image.
    /// </summary>
    public enum TiffInkSet : ushort
    {
        /// <summary>
        /// The order of the components is cyan, magenta, yellow, black. Usually, a value of 0 represents 0% ink coverage and a value of 255 represents 100% ink coverage for that component, but see DotRange below. The InkNames field should not exist when InkSet=1.
        /// </summary>
        CMYK = 1,

        /// <summary>
        /// Not CMYK. See the InkNames field for a description of the inks to be used.
        /// </summary>
        NotCMYK = 2,
    }
}
