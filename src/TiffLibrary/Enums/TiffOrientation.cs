namespace TiffLibrary
{
    /// <summary>
    /// The orientation of the image with respect to the rows and columns.
    /// </summary>
    public enum TiffOrientation : ushort
    {
        /// <summary>
        /// The 0th row represents the visual top of the image, and the 0th column represents the visual left-hand side.
        /// </summary>
        TopLeft = 1,

        /// <summary>
        /// The 0th row represents the visual top of the image, and the 0th column represents the visual right-hand side.
        /// </summary>
        TopRight = 2,

        /// <summary>
        ///  The 0th row represents the visual bottom of the image, and the 0th column represents the visual right-hand side.
        /// </summary>
        BottomRight = 3,

        /// <summary>
        /// The 0th row represents the visual bottom of the image, and the 0th column represents the visual left-hand side.
        /// </summary>
        BottomLeft = 4,

        /// <summary>
        /// The 0th row represents the visual left-hand side of the image, and the 0th column represents the visual top.
        /// </summary>
        LeftTop = 5,

        /// <summary>
        /// The 0th row represents the visual right-hand side of the image, and the 0th column represents the visual top.
        /// </summary>
        RightTop = 6,

        /// <summary>
        /// The 0th row represents the visual right-hand side of the image, and the 0th column represents the visual bottom.
        /// </summary>
        RightBottom = 7,

        /// <summary>
        /// The 0th row represents the visual left-hand side of the image, and the 0th column represents the visual bottom.
        /// </summary>
        LeftBottom = 8,
    }
}
