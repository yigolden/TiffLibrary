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
            _options = new TiffImageDecoderOptions { IgnoreOrientation = options.IgnoreOrientation };
        }

        public IImageInfo Identify(Stream stream)
        {
            return IdentifyCoreAsync(new ImageSharpContentSource(stream)).GetAwaiter().GetResult();
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

        public Image<TPixel> Decode<TPixel>(Stream stream) where TPixel : struct, IPixel<TPixel>
        {
            return DecodeCoreAsync<TPixel>(new ImageSharpContentSource(stream)).GetAwaiter().GetResult();
        }

        private async Task<Image<TPixel>> DecodeCoreAsync<TPixel>(TiffFileContentSource contentSource)
           where TPixel : struct, IPixel<TPixel>
        {
            using TiffFileReader tiff = await TiffFileReader.OpenAsync(contentSource).ConfigureAwait(false);
            TiffImageFileDirectory ifd = await tiff.ReadImageFileDirectoryAsync().ConfigureAwait(false);
            TiffImageDecoder decoder = await tiff.CreateImageDecoderAsync(ifd, _options).ConfigureAwait(false);

            // Fast path for TiffLibrary-supported pixel formats
            if (typeof(TPixel) == typeof(Gray8))
            {
                return Unsafe.As<Image<TPixel>>(await DecodeImageAsync<Gray8, TiffGray8>(decoder).ConfigureAwait(false));
            }
            if (typeof(TPixel) == typeof(Gray16))
            {
                return Unsafe.As<Image<TPixel>>(await DecodeImageAsync<Gray16, TiffGray16>(decoder).ConfigureAwait(false));
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
            return await DecodeImageSlowAsync<TPixel>(decoder).ConfigureAwait(false);
        }

        private async Task<Image<TImageSharpPixel>> DecodeImageAsync<TImageSharpPixel, TTiffPixel>(TiffImageDecoder decoder) where TImageSharpPixel : struct, IPixel<TImageSharpPixel> where TTiffPixel : unmanaged
        {
            var image = new Image<TImageSharpPixel>(_configuration, decoder.Width, decoder.Height);
            try
            {
                await decoder.DecodeAsync(new ImageSharpPixelBuffer<TImageSharpPixel, TTiffPixel>(image)).ConfigureAwait(false);
                return Interlocked.Exchange(ref image, null);
            }
            finally
            {
                image?.Dispose();
            }
        }

        private async Task<Image<TImageSharpPixel>> DecodeImageSlowAsync<TImageSharpPixel>(TiffImageDecoder decoder) where TImageSharpPixel : struct, IPixel<TImageSharpPixel>
        {
            using var image = new Image<Rgba32>(_configuration, decoder.Width, decoder.Height);
            await decoder.DecodeAsync(new ImageSharpPixelBuffer<Rgba32, TiffRgba32>(image)).ConfigureAwait(false);
            return image.CloneAs<TImageSharpPixel>();
        }
    }
}
