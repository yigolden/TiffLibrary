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
            return IdentifyCoreAsync(new ImageSharpContentSource(stream)).GetAwaiter().GetResult();
        }

        public Task<IImageInfo> IdentifyAsync(Stream stream)
        {
            if (!stream.CanSeek)
            {
                throw new InvalidOperationException("TIFF stream must be seekable.");
            }
            return IdentifyCoreAsync(TiffFileContentSource.Create(stream, true));
        }

        private async Task<IImageInfo> IdentifyCoreAsync(TiffFileContentSource contentSource)
        {
            using TiffFileReader tiff = await TiffFileReader.OpenAsync(contentSource).ConfigureAwait(false);
            TiffImageFileDirectory ifd = await tiff.ReadImageFileDirectoryAsync().ConfigureAwait(false);
            TiffImageDecoder decoder = await tiff.CreateImageDecoderAsync(ifd, _options).ConfigureAwait(false);

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
            return DecodeCoreAsync<TPixel>(new ImageSharpContentSource(stream)).GetAwaiter().GetResult();
        }

        public Task<Image<TPixel>> DecodeAsync<TPixel>(Stream stream) where TPixel : unmanaged, IPixel<TPixel>
        {
            if (!stream.CanSeek)
            {
                throw new InvalidOperationException("TIFF stream must be seekable.");
            }
            return DecodeCoreAsync<TPixel>(TiffFileContentSource.Create(stream, true));
        }

        private async Task<Image<TPixel>> DecodeCoreAsync<TPixel>(TiffFileContentSource contentSource)
           where TPixel : unmanaged, IPixel<TPixel>
        {
            using TiffFileReader tiff = await TiffFileReader.OpenAsync(contentSource).ConfigureAwait(false);
            TiffImageFileDirectory ifd = await tiff.ReadImageFileDirectoryAsync().ConfigureAwait(false);
            TiffImageDecoder decoder = await tiff.CreateImageDecoderAsync(ifd, _options).ConfigureAwait(false);

            // Fast path for TiffLibrary-supported pixel formats
            if (typeof(TPixel) == typeof(L8))
            {
                return Unsafe.As<Image<TPixel>>(await DecodeImageAsync<L8, TiffGray8>(decoder).ConfigureAwait(false));
            }
            if (typeof(TPixel) == typeof(L16))
            {
                return Unsafe.As<Image<TPixel>>(await DecodeImageAsync<L16, TiffGray16>(decoder).ConfigureAwait(false));
            }
            if (typeof(TPixel) == typeof(Rgb24))
            {
                return Unsafe.As<Image<TPixel>>(await DecodeImageAsync<Rgb24, TiffRgb24>(decoder).ConfigureAwait(false));
            }
            if (typeof(TPixel) == typeof(Rgba32))
            {
                return Unsafe.As<Image<TPixel>>(await DecodeImageAsync<Rgba32, TiffRgba32>(decoder).ConfigureAwait(false));
            }
            if (typeof(TPixel) == typeof(Bgr24))
            {
                return Unsafe.As<Image<TPixel>>(await DecodeImageAsync<Bgr24, TiffBgr24>(decoder).ConfigureAwait(false));
            }
            if (typeof(TPixel) == typeof(Bgra32))
            {
                return Unsafe.As<Image<TPixel>>(await DecodeImageAsync<Bgra32, TiffBgra32>(decoder).ConfigureAwait(false));
            }
            if (typeof(TPixel) == typeof(Rgba64))
            {
                return Unsafe.As<Image<TPixel>>(await DecodeImageAsync<Rgba64, TiffRgba64>(decoder).ConfigureAwait(false));
            }

            // Slow path
            if (typeof(TPixel) == typeof(Rgb48))
            {
                return await DecodeImageSlowAsync<TiffRgba64, Rgba64, TPixel>(decoder).ConfigureAwait(false);
            }
            return await DecodeImageSlowAsync<TiffRgba32, Rgba32, TPixel>(decoder).ConfigureAwait(false);
        }

        private async Task<Image<TImageSharpPixel>> DecodeImageAsync<TImageSharpPixel, TTiffPixel>(TiffImageDecoder decoder) where TImageSharpPixel : unmanaged, IPixel<TImageSharpPixel> where TTiffPixel : unmanaged
        {
            Image<TImageSharpPixel>? image = new Image<TImageSharpPixel>(_configuration, decoder.Width, decoder.Height);
            try
            {
                await decoder.DecodeAsync(new ImageSharpPixelBufferWriter<TImageSharpPixel, TTiffPixel>(image)).ConfigureAwait(false);
                return Interlocked.Exchange(ref image, null)!;
            }
            finally
            {
                image?.Dispose();
            }
        }

        private async Task<Image<TImageSharpPixel>> DecodeImageSlowAsync<TTiffPixel, TIntermediate, TImageSharpPixel>(TiffImageDecoder decoder)
            where TTiffPixel : unmanaged
            where TIntermediate : unmanaged, IPixel<TIntermediate>
            where TImageSharpPixel : unmanaged, IPixel<TImageSharpPixel>
        {
            var image = new Image<TImageSharpPixel>(_configuration, decoder.Width, decoder.Height);
            try
            {
                var writer = new ImageSharpPixelBufferWriter<TImageSharpPixel, TImageSharpPixel>(image);
                await decoder.DecodeAsync(new ImageSharpConversionPixelBufferWriter2<TiffRgba32, Rgba32, TImageSharpPixel>(writer)).ConfigureAwait(false);
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
