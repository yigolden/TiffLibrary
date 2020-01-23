namespace TiffLibrary.Exif
{
    /// <summary>
    /// Indicates the white balance mode set when the image was shot.
    /// </summary>
    public enum TiffExifWhiteBalance : ushort
    {
        /// <summary>
        /// Auto white balance
        /// </summary>
        Auto = 0,

        /// <summary>
        /// Manual white balance
        /// </summary>
        Manual = 1
    }
}
