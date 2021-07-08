using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace TiffLibrary.ImageSharpAdapter
{
    internal sealed class TiffDecoderCore
    {
        private readonly Configuration _configuration;
        private readonly TiffImageDecoderOptions _options;

        public TiffDecoderCore(Configuration configuration, ITiffDecoderOptions options)
        {
            _configuration = configuration;
            _options = new TiffImageDecoderOptions { MemoryPool = new ImageSharpMemoryPool(configuration.MemoryAllocator), IgnoreOrientation = options.IgnoreOrientation };
        }

        public IImageInfo Identify(Stream stream)
        {
            if (!stream.CanSeek)
            {
                throw new InvalidOperationException("TIFF stream must be seekable.");
            }
            return IdentifyCoreAsync(new ImageSharpContentSource(stream), CancellationToken.None).GetAwaiter().GetResult();
        }

        public Task<IImageInfo> IdentifyAsync(Stream stream, CancellationToken cancellationToken)
        {
            if (!stream.CanSeek)
            {
                throw new InvalidOperationException("TIFF stream must be seekable.");
            }
            return IdentifyCoreAsync(TiffFileContentSource.Create(stream, true), cancellationToken);
        }

        private async Task<IImageInfo> IdentifyCoreAsync(TiffFileContentSource contentSource, CancellationToken cancellationToken)
        {
            using TiffFileReader tiff = await TiffFileReader.OpenAsync(contentSource, cancellationToken: cancellationToken).ConfigureAwait(false);
            TiffImageFileDirectory ifd = await tiff.ReadImageFileDirectoryAsync(cancellationToken).ConfigureAwait(false);
            TiffImageDecoder decoder = await tiff.CreateImageDecoderAsync(ifd, _options, cancellationToken).ConfigureAwait(false);

            return new ImageInfo()
            {
                Width = decoder.Width,
                Height = decoder.Height
            };
        }

        public Image<TPixel> Decode<TPixel>(Stream stream) where TPixel : unmanaged, IPixel<TPixel>
        {
            if (!stream.CanSeek)
            {
                throw new InvalidOperationException("TIFF stream must be seekable.");
            }
            return DecodeCoreAsync<TPixel>(new ImageSharpContentSource(stream), CancellationToken.None).GetAwaiter().GetResult();
        }

        public Task<Image<TPixel>> DecodeAsync<TPixel>(Stream stream, CancellationToken cancellationToken) where TPixel : unmanaged, IPixel<TPixel>
        {
            if (!stream.CanSeek)
            {
                throw new InvalidOperationException("TIFF stream must be seekable.");
            }
            return DecodeCoreAsync<TPixel>(TiffFileContentSource.Create(stream, true), cancellationToken);
        }

        private async Task<Image<TPixel>> DecodeCoreAsync<TPixel>(TiffFileContentSource contentSource, CancellationToken cancellationToken)
           where TPixel : unmanaged, IPixel<TPixel>
        {
            using TiffFileReader tiff = await TiffFileReader.OpenAsync(contentSource, cancellationToken: cancellationToken).ConfigureAwait(false);
            TiffImageFileDirectory ifd = await tiff.ReadImageFileDirectoryAsync(cancellationToken).ConfigureAwait(false);
            TiffImageDecoder decoder = await tiff.CreateImageDecoderAsync(ifd, _options, cancellationToken).ConfigureAwait(false);

            Image<TPixel>? image = new Image<TPixel>(_configuration, decoder.Width, decoder.Height);
            try
            {
                await decoder.DecodeAsync(image, cancellationToken).ConfigureAwait(false);
                return Interlocked.Exchange<Image<TPixel>?>(ref image, null)!;
            }
            finally
            {
#pragma warning disable CA1508 // Avoid dead conditional code
                image?.Dispose();
#pragma warning restore CA1508
            }
        }
    }
}
