using TiffLibrary.PixelConverter;

namespace TiffLibrary
{
    /// <summary>
    /// A series of options to control the behavior of <see cref="TiffImageDecoder"/>.
    /// </summary>
    public class TiffImageDecoderOptions
    {
        internal static TiffImageDecoderOptions Default { get; } = new TiffImageDecoderOptions();

        /// <summary>
        /// An <see cref="ITiffPixelConverterFactory"/> instance used to create pixel converters to convert pixels in one color space to another.
        /// </summary>
        public ITiffPixelConverterFactory PixelConverterFactory { get; set; } = TiffDefaultPixelConverterFactory.Instance;

        /// <summary>
        /// When this flag is set, the decoder will utilize the associated alpha channel in RGB image if possible and undo color pre-multiplying to restore alpha chanel in the output RGBA image. Otherwise the associated alpha channel is ignored.
        /// </summary>
        public bool UndoColorPreMultiplying { get; set; } = false;

        /// <summary>
        /// When this flag is set, the Orientation tag in the IFD is ignored. Image will not be flipped or oriented according to the tag.
        /// </summary>
        public bool IgnoreOrientation { get; set; } = false;
    }
}
