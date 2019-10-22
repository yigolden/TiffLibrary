namespace TiffLibrary
{
    /// <summary>
    /// The color space of the image data.
    /// </summary>
    public enum TiffPhotometricInterpretation : ushort
    {
        /// <summary>
        /// Represents bilevel and grayscale images:  0 is imaged as white. 2**BitsPerSample-1 is imaged as black. This is the normal value for Compression=2.
        /// </summary>
        WhiteIsZero = 0,

        /// <summary>
        /// Represents bilevel and grayscale images:  0 is imaged as black. 2**BitsPerSample-1 is imaged as white. If this value is specified for Compression=2, the image should display and print reversed.
        /// </summary>
        BlackIsZero = 1,

        /// <summary>
        /// Represents RGB images. In the RGB model, a color is described as a combination of the three primary colors of light (red, green, and blue) in particular concentrations. For each of the three components, 0 represents minimum intensity, and 2**BitsPerSample - 1 represents maximum intensity. Thus an RGB value of (0,0,0) represents black, and (255,255,255) represents white, assuming 8-bit components. For PlanarConfiguration = 1, the components are stored in the indicated order:  first Red, then Green, then Blue. For PlanarConfiguration = 2, the StripOffsets for the component planes are stored in the indicated order:  first the Red component plane StripOffsets, then the Green plane StripOffsets, then the Blue plane StripOffsets.
        /// </summary>
        RGB = 2,

        /// <summary>
        /// Represents palette images. In this model, a color is described with a single component. The value of the component is used as an index into the red, green and blue curves in the ColorMap field to retrieve an RGB triplet that defines the color. When PhotometricInterpretation=3 is used, ColorMap must be present and SamplesPerPixel must be 1.
        /// </summary>
        PaletteColor = 3,

        /// <summary>
        /// This means that the image is used to define an irregularly shaped region of another image in the same TIFF file. SamplesPerPixel and BitsPerSample must be 1. PackBits compression is recommended. The 1-bits define the interior of the region; the 0-bits define the exterior of the region.
        /// A reader application can use the mask to determine which parts of the image to display. Main image pixels that correspond to 1-bits in the transparency mask are imaged to the screen or printer, but main image pixels that correspond to 0-bits in the mask are not displayed or printed.
        /// The image mask is typically at a higher resolution than the main image, if the main image is grayscale or color so that the edges can be sharp.
        /// </summary>
        TransparencyMask = 4,

        /// <summary>
        /// The components represent the desired percent dot coverage of each ink, where the larger component values represent a higher percentage of ink dot coverage and smaller values represent less coverage.
        /// </summary>
        Seperated = 5,

        /// <summary>
        /// Represents YCbCr images.
        /// </summary>
        YCbCr = 6,
    }
}
