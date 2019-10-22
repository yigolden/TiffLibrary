using System.Collections.Generic;

namespace TiffLibrary.ImageSharpAdapter
{
    internal static class TiffConstants
    {

        public static readonly IEnumerable<string> MimeTypes = new[] { "image/tiff" };

        public static readonly IEnumerable<string> FileExtensions = new[] { "tif", "tiff" };
    }
}
