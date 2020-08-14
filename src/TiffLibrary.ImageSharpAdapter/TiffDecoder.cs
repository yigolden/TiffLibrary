using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
        public bool IgnoreOrientation { get; set; }

        /// <inheritdoc />
        public Image<TPixel> Decode<TPixel>(Configuration configuration, Stream stream) where TPixel : unmanaged, IPixel<TPixel>
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var decoder = new TiffDecoderCore(configuration, this);
            return decoder.Decode<TPixel>(stream);
        }

        /// <inheritdoc />
        public Image Decode(Configuration configuration, Stream stream)
            => Decode<Rgba32>(configuration, stream);

        /// <inheritdoc />
        public Task<Image<TPixel>> DecodeAsync<TPixel>(Configuration configuration, Stream stream, CancellationToken cancellationToken) where TPixel : unmanaged, IPixel<TPixel>
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var decoder = new TiffDecoderCore(configuration, this);
            return decoder.DecodeAsync<TPixel>(stream, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Image> DecodeAsync(Configuration configuration, Stream stream, CancellationToken cancellationToken)
            => await DecodeAsync<Rgba32>(configuration, stream, cancellationToken).ConfigureAwait(false);

        /// <inheritdoc />
        public IImageInfo Identify(Configuration configuration, Stream stream)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var decoder = new TiffDecoderCore(configuration, this);
            return decoder.Identify(stream);
        }

        /// <inheritdoc />
        public Task<IImageInfo> IdentifyAsync(Configuration configuration, Stream stream, CancellationToken cancellationToken)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var decoder = new TiffDecoderCore(configuration, this);
            return decoder.IdentifyAsync(stream, cancellationToken);
        }
    }
}
