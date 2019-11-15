using SixLabors.Primitives;

namespace TiffLibrary.ImageSharpAdapter
{
    public sealed class TiffEncoderOptions : ITiffEncoderOptions
    {
        public TiffPhotometricInterpretation PhotometricInterpretation { get; set; } = TiffPhotometricInterpretation.RGB;

        public TiffCompression Compression { get; set; } = TiffCompression.Lzw;

        public bool IsTiled { get; set; }

        public int RowsPerStrip { get; set; } = 128;

        public Size TileSize { get; set; } = new Size(512, 512);

        public TiffPredictor ApplyPredictor { get; set; }

        public bool EnableTransparencyForRgb { get; set; } = true;

        public TiffOrientation Orientation { get; set; }

        public int JpegQuality { get; set; } = 75;

        public int YCbCrHorizontalSubSampling { get; set; } = 1;

        public int YCbCrVerticalSubSampling { get; set; } = 1;

    }
}
