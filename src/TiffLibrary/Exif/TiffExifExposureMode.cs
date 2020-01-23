namespace TiffLibrary.Exif
{
    /// <summary>
    /// Indicates the exposure mode set when the image was shot.
    /// In auto-bracketing mode, the camera shoots a series of frames of the same scene at different exposure settings.
    /// </summary>
    public enum TiffExifExposureMode : ushort
    {
        /// <summary>
        /// Auto exposure
        /// </summary>
        AutoExposure = 0,

        /// <summary>
        /// Manual exposure
        /// </summary>
        ManualExposure = 1,

        /// <summary>
        /// Auto bracket
        /// </summary>
        AutoBracket = 2,
    }
}
