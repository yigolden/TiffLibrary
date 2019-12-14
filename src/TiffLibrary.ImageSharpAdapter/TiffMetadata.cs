using SixLabors.ImageSharp;

namespace TiffLibrary.ImageSharpAdapter
{
    /// <summary>
    /// Provides TIFF specific metadata information for the image.
    /// </summary>
    public sealed class TiffMetadata : IDeepCloneable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TiffMetadata"/> class.
        /// </summary>
        public TiffMetadata() { }

        /// <inheritdoc/>
        public IDeepCloneable DeepClone()
        {
            return new TiffMetadata();
        }
    }
}
