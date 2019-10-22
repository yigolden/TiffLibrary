using SixLabors.ImageSharp;

namespace TiffLibrary.ImageSharpAdapter
{
    public sealed class TiffMetadata : IDeepCloneable
    {
        public TiffMetadata() { }

        public IDeepCloneable DeepClone()
        {
            return new TiffMetadata();
        }
    }
}
