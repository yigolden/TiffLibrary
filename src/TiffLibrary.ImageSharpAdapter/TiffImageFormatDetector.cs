using System;
using SixLabors.ImageSharp.Formats;

namespace TiffLibrary.ImageSharpAdapter
{
    public sealed class TiffImageFormatDetector : IImageFormatDetector
    {
        public int HeaderSize => 16;

        public IImageFormat DetectFormat(ReadOnlySpan<byte> header)
        {
            return TiffFileHeader.TryParse(header, out _) ? TiffFormat.Instance : null;
        }
    }
}
