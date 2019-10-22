using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Metadata;

namespace TiffLibrary.ImageSharpAdapter
{
    internal sealed class ImageInfo : IImageInfo
    {
        public PixelTypeInfo PixelType { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public ImageMetadata Metadata { get; set; }
    }
}
