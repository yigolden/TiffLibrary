using System;
using SixLabors.ImageSharp.Formats;

namespace TiffLibrary.ImageSharpAdapter
{
    /// <summary>
    /// Detects TIFF file headers.
    /// </summary>
    public sealed class TiffImageFormatDetector : IImageFormatDetector
    {
        /// <inheritdoc/>
        public int HeaderSize => 16;

        /// <inheritdoc/>
        public IImageFormat? DetectFormat(ReadOnlySpan<byte> header)
        {
            return TiffFileHeader.TryParse(header, out _) ? TiffFormat.Instance : null;
        }
    }
}
