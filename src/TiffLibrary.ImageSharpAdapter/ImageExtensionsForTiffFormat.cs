using System;
using System.IO;
using SixLabors.ImageSharp.Advanced;
using TiffLibrary.ImageSharpAdapter;

namespace SixLabors.ImageSharp
{
    /// <summary>
    /// Extension methods for the <see cref="Image"/> type.
    /// </summary>
    public static class ImageExtensionsForTiffFormat
    {
        /// <summary>
        /// Saves the image to the given stream with the TIFF format.
        /// </summary>
        /// <param name="source">The image this method extends.</param>
        /// <param name="stream">The stream to save the image to.</param>
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

        /// <summary>
        /// Saves the image to the given stream with the TIFF format.
        /// </summary>
        /// <param name="source">The image this method extends.</param>
        /// <param name="stream">The stream to save the image to.</param>
        /// <param name="encoder">The options for the encoder.</param>
        public static void SaveAsTiff(this Image source, Stream stream, TiffEncoder? encoder)
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
