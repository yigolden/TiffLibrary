namespace TiffLibrary.ImageSharpAdapter
{
    internal interface ITiffDecoderOptions
    {
        /// <summary>
        /// When this flag is set, the Orientation tag in the IFD is ignored. Image will not be flipped or oriented according to the tag.
        /// </summary>
        bool IgnoreOrientation { get; }
    }
}
