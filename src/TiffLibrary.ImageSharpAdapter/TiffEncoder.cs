using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using TiffLibrary.Compression;

namespace TiffLibrary.ImageSharpAdapter
{
    /// <summary>
    /// Image encoder for writing image data to a stream in TIFF format.
    /// </summary>
    public sealed class TiffEncoder : IImageEncoder, ITiffEncoderOptions
    {

        /// <summary>
        /// Gets or sets the photometric interpretation to use for the input image.
        /// </summary>
        public TiffPhotometricInterpretation PhotometricInterpretation { get; set; } = TiffPhotometricInterpretation.RGB;

        /// <summary>
        /// Gets or sets the compression method to use when compressing input image.
        /// </summary>
        public TiffCompression Compression { get; set; } = TiffCompression.Deflate;

        /// <summary>
        /// Gets or sets whether the output TIFF file should be a tiled TIFF file.
        /// </summary>
        public bool IsTiled { get; set; }

        /// <summary>
        /// Gets or sets the number of rows when the output TIFF file is a stripped TIFF file.
        /// </summary>
        public int RowsPerStrip { get; set; }

        /// <summary>
        /// Gets or sets the tile size when the output TIFF file is a tiled TIFF file. Both with and height should be multiples of 16.
        /// </summary>
        public Size TileSize { get; set; } = new Size(256, 256);

        /// <summary>
        /// Gets or sets the predictor to use on the image.
        /// </summary>
        public TiffPredictor Predictor { get; set; }

        /// <summary>
        /// Gets or sets whether to write alpha channel when write RGB image. Only used when <see cref="PhotometricInterpretation"/> is set to <see cref="TiffPhotometricInterpretation.RGB"/>.
        /// </summary>
        public bool EnableTransparencyForRgb { get; set; } = true;

        /// <summary>
        /// Gets or sets the orientation in the output TIFF file.
        /// </summary>
        public TiffOrientation Orientation { get; set; }

        /// <summary>
        /// Gets or sets the compression level used in Deflate algorithm. A value of 9 is best, and 1 is least compression. The default is 6.
        /// </summary>
        public TiffDeflateCompressionLevel DeflateCompressionLevel { get; set; } = TiffDeflateCompressionLevel.Default;

        /// <summary>
        /// Gets or sets the JPEG encoding quality factor when compressing using JPEG. Only used when <see cref="Compression"/> is set to <see cref="TiffCompression.Jpeg"/>.
        /// </summary>
        public int JpegQuality { get; set; } = 75;

        /// <summary>
        /// Gets or sets whether to generate optimal Huffman table when encoding. Only used when <see cref="Compression"/> is set to <see cref="TiffCompression.Jpeg"/>.
        /// </summary>
        public bool JpegOptimizeCoding { get; set; }

        /// <summary>
        /// Gets or sets the horizontal chroma subsampling factor for YCbCr image.
        /// </summary>
        public int HorizontalChromaSubSampling { get; set; } = 1;

        /// <summary>
        /// Gets or sets the vertical chroma subsampling factor for YCbCr image.
        /// </summary>
        public int VerticalChromaSubSampling { get; set; } = 1;

        /// <inheritdoc />
        public void Encode<TPixel>(Image<TPixel> image, Stream stream) where TPixel : unmanaged, IPixel<TPixel>
        {
            var encoder = new TiffEncoderCore(image.GetConfiguration(), this);
            encoder.Encode(image, stream);
        }

        /// <inheritdoc />
        public Task EncodeAsync<TPixel>(Image<TPixel> image, Stream stream, CancellationToken cancellationToken) where TPixel : unmanaged, IPixel<TPixel>
        {
            var encoder = new TiffEncoderCore(image.GetConfiguration(), this);
            return encoder.EncodeAsync(image, stream, cancellationToken);
        }
    }
}
