using System.Collections.Generic;
using SixLabors.ImageSharp.Formats;

namespace TiffLibrary.ImageSharpAdapter
{
    /// <summary>
    /// Registers the image encoders, decoders and mime type detectors for the TIFF format.
    /// </summary>
    public sealed class TiffFormat : IImageFormat<TiffMetadata>
    {
        private TiffFormat() { }

        /// <summary>
        /// Gets the current instance.
        /// </summary>
        public static TiffFormat Instance { get; } = new TiffFormat();

        /// <inheritdoc/>
        public string Name => "TIFF";

        /// <inheritdoc/>
        public string DefaultMimeType => "image/tiff";

        /// <inheritdoc/>
        public IEnumerable<string> MimeTypes => TiffConstants.MimeTypes;

        /// <inheritdoc/>
        public IEnumerable<string> FileExtensions => TiffConstants.FileExtensions;

        /// <inheritdoc/>
        public TiffMetadata CreateDefaultFormatMetadata() => new TiffMetadata();
    }
}
