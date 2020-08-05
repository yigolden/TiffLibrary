using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TiffLibrary.PixelFormats;

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

            // Fast path for TiffLibrary-supported pixel formats
            if (typeof(TPixel) == typeof(L8))
            {
                return Unsafe.As<Image<TPixel>>(await DecodeImageAsync<L8, TiffGray8>(decoder, cancellationToken).ConfigureAwait(false));
            }
            if (typeof(TPixel) == typeof(L16))
            {
                return Unsafe.As<Image<TPixel>>(await DecodeImageAsync<L16, TiffGray16>(decoder, cancellationToken).ConfigureAwait(false));
            }
            if (typeof(TPixel) == typeof(Rgb24))
            {
                return Unsafe.As<Image<TPixel>>(await DecodeImageAsync<Rgb24, TiffRgb24>(decoder, cancellationToken).ConfigureAwait(false));
            }
            if (typeof(TPixel) == typeof(Rgba32))
            {
                return Unsafe.As<Image<TPixel>>(await DecodeImageAsync<Rgba32, TiffRgba32>(decoder, cancellationToken).ConfigureAwait(false));
            }
            if (typeof(TPixel) == typeof(Bgr24))
            {
                return Unsafe.As<Image<TPixel>>(await DecodeImageAsync<Bgr24, TiffBgr24>(decoder, cancellationToken).ConfigureAwait(false));
            }
            if (typeof(TPixel) == typeof(Bgra32))
            {
                return Unsafe.As<Image<TPixel>>(await DecodeImageAsync<Bgra32, TiffBgra32>(decoder, cancellationToken).ConfigureAwait(false));
            }
            if (typeof(TPixel) == typeof(Rgba64))
            {
                return Unsafe.As<Image<TPixel>>(await DecodeImageAsync<Rgba64, TiffRgba64>(decoder, cancellationToken).ConfigureAwait(false));
            }

            // Slow path
            if (typeof(TPixel) == typeof(Rgb48))
            {
                return await DecodeImageSlowAsync<TiffRgba64, Rgba64, TPixel>(decoder, cancellationToken).ConfigureAwait(false);
            }

            return await DecodeImageSlowAsync<TiffRgba32, Rgba32, TPixel>(decoder, cancellationToken).ConfigureAwait(false);
        }

        private async Task<Image<TImageSharpPixel>> DecodeImageAsync<TImageSharpPixel, TTiffPixel>(TiffImageDecoder decoder, CancellationToken cancellationToken) where TImageSharpPixel : unmanaged, IPixel<TImageSharpPixel> where TTiffPixel : unmanaged
        {
            Image<TImageSharpPixel>? image = new Image<TImageSharpPixel>(_configuration, decoder.Width, decoder.Height);
            try
            {
                await decoder.DecodeAsync(new ImageSharpPixelBufferWriter<TImageSharpPixel, TTiffPixel>(image), cancellationToken).ConfigureAwait(false);
                return Interlocked.Exchange(ref image, null)!;
            }
            finally
            {
                image?.Dispose();
            }
        }

        private async Task<Image<TImageSharpPixel>> DecodeImageSlowAsync<TTiffPixel, TIntermediate, TImageSharpPixel>(TiffImageDecoder decoder, CancellationToken cancellationToken)
            where TTiffPixel : unmanaged
            where TIntermediate : unmanaged, IPixel<TIntermediate>
            where TImageSharpPixel : unmanaged, IPixel<TImageSharpPixel>
        {
            var image = new Image<TImageSharpPixel>(_configuration, decoder.Width, decoder.Height);
            try
            {
                var writer = new ImageSharpPixelBufferWriter<TImageSharpPixel, TImageSharpPixel>(image);
                await decoder.DecodeAsync(new ImageSharpConversionPixelBufferWriter2<TiffRgba32, Rgba32, TImageSharpPixel>(writer), cancellationToken).ConfigureAwait(false);
                return Interlocked.Exchange(ref image, null)!;
            }
            finally
            {
                if (!(image is null))
                {
                    image.Dispose();
                }
            }

        }
    }
}
