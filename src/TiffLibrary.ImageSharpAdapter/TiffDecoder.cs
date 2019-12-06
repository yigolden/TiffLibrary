using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

namespace TiffLibrary.ImageSharpAdapter
{
    public sealed class TiffDecoder : IImageDecoder, ITiffDecoderOptions, IImageInfoDetector
    {
        public bool IgnoreOrientation { get; set; } = false;

        public Image<TPixel> Decode<TPixel>(Configuration configuration, Stream stream) where TPixel : struct, IPixel<TPixel>
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var decoder = new TiffDecoderCore(configuration, this);
            return decoder.Decode<TPixel>(stream);
        }

        public Image Decode(Configuration configuration, Stream stream)
            => Decode<Rgba32>(configuration, stream);

        public IImageInfo Identify(Configuration configuration, Stream stream)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var decoder = new TiffDecoderCore(configuration, this);
            return decoder.Identify(stream);
        }
    }
}
