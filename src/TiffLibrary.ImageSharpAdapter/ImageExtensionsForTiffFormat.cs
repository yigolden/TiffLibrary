using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
        /// <param name="path">The file path to save the image to.</param>
        public static void SaveAsTiff(this Image source, string path)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (path is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            source.SaveAsTiff(path, null);
        }

        /// <summary>
        /// Saves the image to the given stream with the TIFF format.
        /// </summary>
        /// <param name="source">The image this method extends.</param>
        /// <param name="path">The file path to save the image to.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static Task SaveAsTiffAsync(this Image source, string path)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (path is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return SaveAsTiffAsync(source, path, null);
        }

        /// <summary>
        /// Saves the image to the given stream with the TIFF format.
        /// </summary>
        /// <param name="source">The image this method extends.</param>
        /// <param name="path">The file path to save the image to.</param>
        /// <param name="encoder">The encoder to save the image with.</param>
        public static void SaveAsTiff(this Image source, string path, TiffEncoder? encoder)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (path is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            source.Save(path, encoder ?? source.GetConfiguration().ImageFormatsManager.FindEncoder(TiffFormat.Instance));
        }

        /// <summary>
        /// Saves the image to the given stream with the TIFF format.
        /// </summary>
        /// <param name="source">The image this method extends.</param>
        /// <param name="path">The file path to save the image to.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static Task SaveAsTiffAsync(this Image source, string path, CancellationToken cancellationToken)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (path is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return source.SaveAsync(path, source.GetConfiguration().ImageFormatsManager.FindEncoder(TiffFormat.Instance), cancellationToken);
        }

        /// <summary>
        /// Saves the image to the given stream with the TIFF format.
        /// </summary>
        /// <param name="source">The image this method extends.</param>
        /// <param name="path">The file path to save the image to.</param>
        /// <param name="encoder">The encoder to save the image with.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static Task SaveAsTiffAsync(this Image source, string path, TiffEncoder? encoder, CancellationToken cancellationToken = default)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (path is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return source.SaveAsync(path, encoder ?? source.GetConfiguration().ImageFormatsManager.FindEncoder(TiffFormat.Instance), cancellationToken);
        }

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

        /// <summary>
        /// Saves the image to the given stream with the TIFF format.
        /// </summary>
        /// <param name="source">The image this method extends.</param>
        /// <param name="stream">The stream to save the image to.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static Task SaveAsTiffAsync(this Image source, Stream stream, CancellationToken cancellationToken = default)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            return SaveAsTiffAsync(source, stream, null, cancellationToken);
        }

        /// <summary>
        /// Saves the image to the given stream with the TIFF format.
        /// </summary>
        /// <param name="source">The image this method extends.</param>
        /// <param name="stream">The stream to save the image to.</param>
        /// <param name="encoder">The encoder to save the image with.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static Task SaveAsTiffAsync(this Image source, Stream stream, TiffEncoder? encoder, CancellationToken cancellationToken = default)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }


            return source.SaveAsync(stream, encoder ?? source.GetConfiguration().ImageFormatsManager.FindEncoder(TiffFormat.Instance), cancellationToken);
        }
    }
}
