using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TiffLibrary.PixelFormats;

namespace TiffLibrary.ImageSharpAdapter
{
    internal sealed class TiffEncoderCore
    {
        private readonly Configuration _configuration;
        private readonly ITiffEncoderOptions _options;

        public TiffEncoderCore(Configuration configuration, ITiffEncoderOptions options)
        {
            _configuration = configuration;
            _options = options;
        }

        public void Encode<TPixel>(Image<TPixel> image, Stream stream) where TPixel : struct, IPixel<TPixel>
        {
            EncodeCoreAsync(image, stream).GetAwaiter().GetResult();
        }

        private async Task EncodeCoreAsync<TPixel>(Image<TPixel> image, Stream stream) where TPixel : struct, IPixel<TPixel>
        {
            using (TiffFileWriter writer = await TiffFileWriter.OpenAsync(stream, leaveOpen: true).ConfigureAwait(false))
            {
                TiffStreamOffset ifdOffset;

                using (TiffImageFileDirectoryWriter ifdWriter = writer.CreateImageFileDirectory())
                {
                    await EncodeImageAsync(image, ifdWriter).ConfigureAwait(false);

                    ifdOffset = await ifdWriter.FlushAsync().ConfigureAwait(false);
                }

                writer.SetFirstImageFileDirectoryOffset(ifdOffset);

                await writer.FlushAsync().ConfigureAwait(false);
            }
        }

        private Task EncodeImageAsync<TPixel>(Image<TPixel> image, TiffImageFileDirectoryWriter ifdWriter) where TPixel : struct, IPixel<TPixel>
        {
            ITiffEncoderOptions options = _options;

            var builder = new TiffImageEncoderBuilder();
            builder.PhotometricInterpretation = options.PhotometricInterpretation;
            builder.Compression = options.Compression;
            builder.IsTiled = options.IsTiled;
            builder.RowsPerStrip = options.RowsPerStrip;
            builder.TileSize = new TiffSize(options.TileSize.Width, options.TileSize.Height);
            builder.ApplyPredictor = options.ApplyPredictor;
            builder.EnableTransparencyForRgb = options.EnableTransparencyForRgb;
            builder.Orientation = options.Orientation;
            builder.JpegQuality = options.JpegQuality;

            // Fast path for TiffLibrary-supported pixel formats
            if (typeof(TPixel) == typeof(Gray8))
            {
                return BuildAndEncodeAsync<Gray8, TiffGray8>(builder, Unsafe.As<Image<Gray8>>(image), ifdWriter);
            }
            if (typeof(TPixel) == typeof(Gray16))
            {
                return BuildAndEncodeAsync<Gray16, TiffGray16>(builder, Unsafe.As<Image<Gray16>>(image), ifdWriter);
            }
            if (typeof(TPixel) == typeof(Rgb24))
            {
                return BuildAndEncodeAsync<Rgb24, TiffRgb24>(builder, Unsafe.As<Image<Rgb24>>(image), ifdWriter);
            }
            if (typeof(TPixel) == typeof(Rgba32))
            {
                return BuildAndEncodeAsync<Rgba32, TiffRgba32>(builder, Unsafe.As<Image<Rgba32>>(image), ifdWriter);
            }
            if (typeof(TPixel) == typeof(Bgr24))
            {
                return BuildAndEncodeAsync<Bgr24, TiffBgr24>(builder, Unsafe.As<Image<Bgr24>>(image), ifdWriter);
            }
            if (typeof(TPixel) == typeof(Bgra32))
            {
                return BuildAndEncodeAsync<Bgra32, TiffBgra32>(builder, Unsafe.As<Image<Bgra32>>(image), ifdWriter);
            }
            if (typeof(TPixel) == typeof(Rgba64))
            {
                return BuildAndEncodeAsync<Rgba64, TiffRgba64>(builder, Unsafe.As<Image<Rgba64>>(image), ifdWriter);
            }

            // Slow path
            return EncodeImageSlowAsync(builder, image, ifdWriter);
        }

        private async Task BuildAndEncodeAsync<TImageSharpPixel, TTiffPixel>(TiffImageEncoderBuilder builder, Image<TImageSharpPixel> image, TiffImageFileDirectoryWriter ifdWriter) where TImageSharpPixel : struct, IPixel<TImageSharpPixel> where TTiffPixel : unmanaged
        {
            TiffImageEncoder<TTiffPixel> encoder = builder.Build<TTiffPixel>();
            await encoder.EncodeAsync(ifdWriter, new ImageSharpPixelBuffer<TImageSharpPixel, TTiffPixel>(image)).ConfigureAwait(false);
        }

        private async Task EncodeImageSlowAsync<TPixel>(TiffImageEncoderBuilder builder, Image<TPixel> image, TiffImageFileDirectoryWriter ifdWriter) where TPixel : struct, IPixel<TPixel>
        {
            using Image<Rgba32> img = image.CloneAs<Rgba32>(_configuration);
            TiffImageEncoder<TiffRgba32> encoder = builder.Build<TiffRgba32>();
            await encoder.EncodeAsync(ifdWriter, new ImageSharpPixelBuffer<Rgba32, TiffRgba32>(img)).ConfigureAwait(false);
        }
    }
}
