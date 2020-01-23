namespace TiffLibrary.Exif
{
    /// <summary>
    /// The class of the program used by the camera to set exposure when the picture is taken.
    /// </summary>
    public enum TiffExifExposureProgram : ushort
    {
        /// <summary>
        /// Not defined
        /// </summary>
        NotDefined = 0,

        /// <summary>
        /// Manual
        /// </summary>
        Manual = 1,

        /// <summary>
        /// Normal program
        /// </summary>
        NormalProgram = 2,

        /// <summary>
        /// Aperture priority
        /// </summary>
        AperturePriority = 3,

        /// <summary>
        /// Shutter priority
        /// </summary>
        ShutterPriority = 4,

        /// <summary>
        /// Creative program (biased toward depth of field)
        /// </summary>
        CreativeProgram = 5,

        /// <summary>
        /// Action program (biased toward fast shutter speed)
        /// </summary>
        ActionProgram = 6,

        /// <summary>
        /// Portrait mode (for closeup photos with the background out of focus)
        /// </summary>
        PortraitMode = 7,

        /// <summary>
        /// Landscape mode (for landscape photos with the background in focus)
        /// </summary>
        LandscapeMode = 8,
    }

}
