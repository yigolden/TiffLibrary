using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

namespace TiffLibrary.ImageSharpAdapter
{
    /// <summary>
    /// Decoder for generating an image out of a TIFF encoded strea.
    /// </summary>
    public sealed class TiffDecoder : IImageDecoder, ITiffDecoderOptions, IImageInfoDetector
    {

        /// <summary>
        /// When this flag is set, the Orientation tag in the IFD is ignored. Image will not be flipped or oriented according to the tag.
        /// </summary>
        public bool IgnoreOrientation { get; set; } = false;
        
        /// <summary>
        /// Decode the image from the specified stream to the <see cref="ImageFrame{TPixel}"/>.
        /// </summary>
        /// <typeparam name="TPixel">The pixel format.</typeparam>
        /// <param name="configuration">The configuration for the image.</param>
        /// <param name="stream">The <see cref="Stream"/> containing image data.</param>
        /// <returns>The decoded image.</returns>
        public Image<TPixel> Decode<TPixel>(Configuration configuration, Stream stream) where TPixel : unmanaged, IPixel<TPixel>
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var decoder = new TiffDecoderCore(configuration, this);
            return decoder.Decode<TPixel>(stream);
        }

        /// <inheritdoc />
        public Image Decode(Configuration configuration, Stream stream)
            => Decode<Rgba32>(configuration, stream);

        /// <inheritdoc />
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
