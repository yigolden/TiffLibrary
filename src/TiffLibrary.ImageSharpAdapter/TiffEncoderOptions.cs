using SixLabors.Primitives;

namespace TiffLibrary.ImageSharpAdapter
{
    public sealed class TiffEncoderOptions : ITiffEncoderOptions
    {
        public TiffPhotometricInterpretation PhotometricInterpretation { get; set; } = TiffPhotometricInterpretation.RGB;

        public TiffCompression Compression { get; set; } = TiffCompression.Lzw;

        public bool IsTiled { get; set; }

        public int RowsPerStrip { get; set; } = 0;

        public Size TileSize { get; set; } = new Size(512, 512);

        public TiffPredictor Predictor { get; set; }

        public bool EnableTransparencyForRgb { get; set; } = true;

        public TiffOrientation Orientation { get; set; }

        public int JpegQuality { get; set; } = 75;

        public int HorizontalChromaSubSampling { get; set; } = 1;

        public int VerticalChromaSubSampling { get; set; } = 1;

    }
}
