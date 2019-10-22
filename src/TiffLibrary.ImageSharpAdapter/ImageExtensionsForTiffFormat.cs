using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;

namespace TiffLibrary.ImageSharpAdapter
{
    public static class ImageExtensionsForTiffFormat
    {
        public static void SaveAsTiff(this Image source, Stream stream)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            source.SaveAsTiff(stream, null);
        }

        public static void SaveAsTiff(this Image source, Stream stream, TiffEncoder encoder)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            source.Save(stream, encoder ?? source.GetConfiguration().ImageFormatsManager.FindEncoder(TiffFormat.Instance));
        }
    }
}
