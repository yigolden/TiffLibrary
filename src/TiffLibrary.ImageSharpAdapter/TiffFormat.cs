using System.Collections.Generic;
using SixLabors.ImageSharp.Formats;

namespace TiffLibrary.ImageSharpAdapter
{
    public sealed class TiffFormat : IImageFormat<TiffMetadata>
    {
        private TiffFormat() { }

        public static TiffFormat Instance { get; } = new TiffFormat();

        public string Name => "TIFF";

        public string DefaultMimeType => "image/tiff";

        public IEnumerable<string> MimeTypes => TiffConstants.MimeTypes;

        public IEnumerable<string> FileExtensions => TiffConstants.FileExtensions;

        public TiffMetadata CreateDefaultFormatMetadata() => new TiffMetadata();
    }
}
