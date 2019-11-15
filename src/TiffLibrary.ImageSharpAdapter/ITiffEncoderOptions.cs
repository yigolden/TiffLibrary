using SixLabors.Primitives;

namespace TiffLibrary.ImageSharpAdapter
{
    internal interface ITiffEncoderOptions
    {
        TiffPhotometricInterpretation PhotometricInterpretation { get; }
        TiffCompression Compression { get; }

        bool IsTiled { get; }

        int RowsPerStrip { get; }

        Size TileSize { get; }

        TiffPredictor ApplyPredictor { get; }

        bool EnableTransparencyForRgb { get; }

        TiffOrientation Orientation { get; }

        int JpegQuality { get; }

        public int YCbCrHorizontalSubSampling { get; }

        public int YCbCrVerticalSubSampling { get; }
    }
}
