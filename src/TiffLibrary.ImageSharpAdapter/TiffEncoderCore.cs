using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TiffLibrary.ImageEncoder;
using TiffLibrary.PixelFormats;

namespace TiffLibrary.ImageSharpAdapter
{
    internal sealed class TiffEncoderCore
    {
        private readonly Configuration _configuration;
        private readonly ITiffEncoderOptions _options;
        private readonly MemoryPool<byte> _memoryPool;

        public TiffEncoderCore(Configuration configuration, ITiffEncoderOptions options)
        {
            _configuration = configuration;
            _options = options;
            _memoryPool = new ImageSharpMemoryPool(configuration.MemoryAllocator);
        }

        public void Encode<TPixel>(Image<TPixel> image, Stream stream) where TPixel : unmanaged, IPixel<TPixel>
        {
            EncodeCoreAsync(image, stream, false, CancellationToken.None).GetAwaiter().GetResult();
        }

        public Task EncodeAsync<TPixel>(Image<TPixel> image, Stream stream, CancellationToken cancellationToken) where TPixel : unmanaged, IPixel<TPixel>
        {
            return EncodeCoreAsync(image, stream, true, cancellationToken);
        }


        private async Task EncodeCoreAsync<TPixel>(Image<TPixel> image, Stream stream, bool isAsync, CancellationToken cancellationToken) where TPixel : unmanaged, IPixel<TPixel>
        {
            TiffFileWriter writer;
            if (isAsync)
            {
                writer = await TiffFileWriter.OpenAsync(stream, leaveOpen: true, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            else
            {
                writer = await TiffFileWriter.OpenAsync(new ImageSharpContentReaderWriter(stream), leaveOpen: true, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            try
            {
                using (TiffImageFileDirectoryWriter ifdWriter = writer.CreateImageFileDirectory())
                {
                    await EncodeImageAsync(image, ifdWriter, cancellationToken).ConfigureAwait(false);

                    writer.SetFirstImageFileDirectoryOffset(await ifdWriter.FlushAsync(cancellationToken).ConfigureAwait(false));
                }

                await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                await writer.DisposeAsync().ConfigureAwait(false);
            }
        }

        private Task EncodeImageAsync<TPixel>(Image<TPixel> image, TiffImageFileDirectoryWriter ifdWriter, CancellationToken cancellationToken) where TPixel : unmanaged, IPixel<TPixel>
        {
            ITiffEncoderOptions options = _options;

            var builder = new TiffImageEncoderBuilder();
            builder.MemoryPool = _memoryPool;
            builder.PhotometricInterpretation = options.PhotometricInterpretation;
            builder.Compression = options.Compression;
            builder.IsTiled = options.IsTiled;
            builder.RowsPerStrip = options.RowsPerStrip;
            builder.TileSize = new TiffSize(options.TileSize.Width, options.TileSize.Height);
            builder.Predictor = options.Predictor;
            builder.EnableTransparencyForRgb = options.EnableTransparencyForRgb;
            builder.Orientation = options.Orientation;
            builder.DeflateCompressionLevel = options.DeflateCompressionLevel;
            builder.JpegOptions = new TiffJpegEncodingOptions { Quality = options.JpegQuality, OptimizeCoding = options.JpegOptimizeCoding };
            builder.HorizontalChromaSubSampling = options.HorizontalChromaSubSampling;
            builder.VerticalChromaSubSampling = options.VerticalChromaSubSampling;

            // Fast path for TiffLibrary-supported pixel formats
            if (typeof(TPixel) == typeof(L8))
            {
                return BuildAndEncodeAsync<L8, TiffGray8>(builder, Unsafe.As<Image<L8>>(image), ifdWriter, cancellationToken);
            }
            if (typeof(TPixel) == typeof(L16))
            {
                return BuildAndEncodeAsync<L16, TiffGray16>(builder, Unsafe.As<Image<L16>>(image), ifdWriter, cancellationToken);
            }
            if (typeof(TPixel) == typeof(Rgb24))
            {
                return BuildAndEncodeAsync<Rgb24, TiffRgb24>(builder, Unsafe.As<Image<Rgb24>>(image), ifdWriter, cancellationToken);
            }
            if (typeof(TPixel) == typeof(Rgba32))
            {
                return BuildAndEncodeAsync<Rgba32, TiffRgba32>(builder, Unsafe.As<Image<Rgba32>>(image), ifdWriter, cancellationToken);
            }
            if (typeof(TPixel) == typeof(Bgr24))
            {
                return BuildAndEncodeAsync<Bgr24, TiffBgr24>(builder, Unsafe.As<Image<Bgr24>>(image), ifdWriter, cancellationToken);
            }
            if (typeof(TPixel) == typeof(Bgra32))
            {
                return BuildAndEncodeAsync<Bgra32, TiffBgra32>(builder, Unsafe.As<Image<Bgra32>>(image), ifdWriter, cancellationToken);
            }
            if (typeof(TPixel) == typeof(Rgba64))
            {
                return BuildAndEncodeAsync<Rgba64, TiffRgba64>(builder, Unsafe.As<Image<Rgba64>>(image), ifdWriter, cancellationToken);
            }

            // Slow path
            return EncodeImageSlowAsync(builder, image, ifdWriter, cancellationToken);
        }

        private static async Task BuildAndEncodeAsync<TImageSharpPixel, TTiffPixel>(TiffImageEncoderBuilder builder, Image<TImageSharpPixel> image, TiffImageFileDirectoryWriter ifdWriter, CancellationToken cancellationToken) where TImageSharpPixel : unmanaged, IPixel<TImageSharpPixel> where TTiffPixel : unmanaged
        {
            TiffImageEncoder<TTiffPixel> encoder = builder.Build<TTiffPixel>();
            await encoder.EncodeAsync(ifdWriter, new ImageSharpPixelBufferReader<TImageSharpPixel, TTiffPixel>(image), cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        private async Task EncodeImageSlowAsync<TPixel>(TiffImageEncoderBuilder builder, Image<TPixel> image, TiffImageFileDirectoryWriter ifdWriter, CancellationToken cancellationToken) where TPixel : unmanaged, IPixel<TPixel>
        {
            using Image<Rgba32> img = image.CloneAs<Rgba32>(_configuration);
            TiffImageEncoder<TiffRgba32> encoder = builder.Build<TiffRgba32>();
            await encoder.EncodeAsync(ifdWriter, new ImageSharpPixelBufferReader<Rgba32, TiffRgba32>(img), cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }
}
