using SixLabors.ImageSharp;
using TiffLibrary.Compression;

namespace TiffLibrary.ImageSharpAdapter
{
    internal interface ITiffEncoderOptions
    {
        /// <summary>
        /// Gets the photometric interpretation to use for the input image.
        /// </summary>
        TiffPhotometricInterpretation PhotometricInterpretation { get; }

        /// <summary>
        /// Gets the compression method to use when compressing input image.
        /// </summary>
        TiffCompression Compression { get; }

        /// <summary>
        /// Gets whether the output TIFF file should be a tiled TIFF file.
        /// </summary>
        bool IsTiled { get; }

        /// <summary>
        /// Gets the number of rows when the output TIFF file is a stripped TIFF file.
        /// </summary>
        int RowsPerStrip { get; }

        /// <summary>
        /// Gets the tile size when the output TIFF file is a tiled TIFF file. Both with and height should be multiples of 16.
        /// </summary>
        Size TileSize { get; }

        /// <summary>
        /// Gets the predictor to use on the image.
        /// </summary>
        TiffPredictor Predictor { get; }

        /// <summary>
        /// Gets whether to write alpha channel when write RGB image. Only used when <see cref="PhotometricInterpretation"/> is set to <see cref="TiffPhotometricInterpretation.RGB"/>.
        /// </summary>
        bool EnableTransparencyForRgb { get; }

        /// <summary>
        /// Gets the orientation in the output TIFF file.
        /// </summary>
        TiffOrientation Orientation { get; }

        /// <summary>
        /// Gets the compression level used in Deflate algorithm. A value of 9 is best, and 1 is least compression. The default is 6.
        /// </summary>
        TiffDeflateCompressionLevel DeflateCompressionLevel { get; }

        /// <summary>
        /// Gets the JPEG encoding quality factor when compressing using JPEG. Only used when <see cref="Compression"/> is set to <see cref="TiffCompression.Jpeg"/>.
        /// </summary>
        int JpegQuality { get; }

        /// <summary>
        /// Gets whether to generate optimal Huffman table when encoding. Only used when <see cref="Compression"/> is set to <see cref="TiffCompression.Jpeg"/>.
        /// </summary>
        bool JpegOptimizeCoding { get; }

        /// <summary>
        /// Gets the horizontal chroma subsampling factor for YCbCr image.
        /// </summary>
        public int HorizontalChromaSubSampling { get; }

        /// <summary>
        /// Gets the vertical chroma subsampling factor for YCbCr image.
        /// </summary>
        public int VerticalChromaSubSampling { get; }
    }
}
