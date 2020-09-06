using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TiffLibrary.ImageEncoder;
using TiffLibrary.PixelFormats;
using TiffLibrary;

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
                    await EncodeImageAsync(image.Frames.RootFrame, ifdWriter, cancellationToken).ConfigureAwait(false);

                    writer.SetFirstImageFileDirectoryOffset(await ifdWriter.FlushAsync(cancellationToken).ConfigureAwait(false));
                }

                await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                await writer.DisposeAsync().ConfigureAwait(false);
            }
        }

        private Task EncodeImageAsync<TPixel>(ImageFrame<TPixel> image, TiffImageFileDirectoryWriter ifdWriter, CancellationToken cancellationToken) where TPixel : unmanaged, IPixel<TPixel>
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

            TiffImageEncoder<TPixel> encoder = builder.BuildForImageSharp<TPixel>();
            return encoder.EncodeAsync(ifdWriter, image, cancellationToken);
        }

    }
}
