using System;
using TiffLibrary.Compression;
using TiffLibrary.ImageEncoder;
using TiffLibrary.ImageEncoder.PhotometricEncoder;

namespace TiffLibrary
{
    /// <summary>
    /// A builder object to build a <see cref="TiffImageEncoder{TPixel}"/> instance.
    /// </summary>
    public sealed class TiffImageEncoderBuilder
    {
        /// <summary>
        /// Gets or sets the photometric interpretation to use for the input image.
        /// </summary>
        public TiffPhotometricInterpretation PhotometricInterpretation { get; set; }

        /// <summary>
        /// Gets or sets the compression method to use when compressing input image.
        /// </summary>
        public TiffCompression Compression { get; set; }

        /// <summary>
        /// Gets or sets whether the output TIFF file should be a tiled TIFF file.
        /// </summary>
        public bool IsTiled { get; set; }

        /// <summary>
        /// Gets or sets the number of rows when the output TIFF file is a stripped TIFF file.
        /// </summary>
        public int RowsPerStrip { get; set; } = 256;

        /// <summary>
        /// Gets or sets the tile size when the output TIFF file is a tiled TIFF file. Both with and height should be a multiple of 16.
        /// </summary>
        public TiffSize TileSize { get; set; } = new TiffSize(512, 512);

        /// <summary>
        /// Gets or sets the predictor to use on the image.
        /// </summary>
        public TiffPredictor ApplyPredictor { get; set; }

        /// <summary>
        /// Gets or sets whether to write alpha channel when write RGB image. Only used when <see cref="PhotometricInterpretation"/> is set to <see cref="TiffPhotometricInterpretation.RGB"/>.
        /// </summary>
        public bool EnableTransparencyForRgb { get; set; }

        /// <summary>
        /// Gets or sets the orientation in the output TIFF file.
        /// </summary>
        public TiffOrientation Orientation { get; set; } = TiffOrientation.TopLeft;

        /// <summary>
        /// Gets or sets the JPEG encoding quality factor when compressing using JPEG. Only used when <see cref="Compression"/> is set to <see cref="TiffCompression.Jpeg"/>.
        /// </summary>
        public int JpegQuality { get; set; } = 75;

        /// <summary>
        /// Build the <see cref="TiffImageEncoder{TPixel}"/> instance with the specified pixel format of input image.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type of the input image.</typeparam>
        /// <returns>The <see cref="TiffImageEncoder{TPixel}"/> instance.</returns>
        public TiffImageEncoder<TPixel> Build<TPixel>() where TPixel : unmanaged
        {
            var pipelineBuilder = new TiffImageEncoderPipelineBuilder<TPixel>();

            bool useHorizontalDifferencingPredictor = ApplyPredictor == TiffPredictor.HorizontalDifferencing;
            switch (PhotometricInterpretation)
            {
                case TiffPhotometricInterpretation.WhiteIsZero:
                    pipelineBuilder.Add(new WhiteIsZero8Encoder<TPixel>());
                    break;
                case TiffPhotometricInterpretation.BlackIsZero:
                    pipelineBuilder.Add(new BlackIsZero8Encoder<TPixel>());
                    break;
                case TiffPhotometricInterpretation.RGB:
                    if (EnableTransparencyForRgb)
                    {
                        pipelineBuilder.Add(new Rgba32Encoder<TPixel>());
                    }
                    else
                    {
                        pipelineBuilder.Add(new Rgb24Encoder<TPixel>());
                    }
                    break;
                case TiffPhotometricInterpretation.TransparencyMask:
                    pipelineBuilder.Add(new TransparencyMaskEncoder<TPixel>(127));
                    break;
                case TiffPhotometricInterpretation.Seperated:
                    pipelineBuilder.Add(new Cmyk32Encoder<TPixel>());
                    break;
                case TiffPhotometricInterpretation.YCbCr:
                    pipelineBuilder.Add(new YCbCr24Encoder<TPixel>());
                    break;
                default:
                    throw new NotSupportedException("The selected photometric interpretation is not supported.");
            }

            if (useHorizontalDifferencingPredictor)
            {
                pipelineBuilder.Add(new TiffApplyPredictorMiddleware<TPixel>(TiffPredictor.HorizontalDifferencing));
            }

            switch (Compression)
            {
                case 0:
                case TiffCompression.NoCompression:
                    pipelineBuilder.Add(new TiffImageCompressionMiddleware<TPixel>(Compression, NoneCompressionAlgorithm.Instance));
                    break;
                case TiffCompression.Lzw:
                    pipelineBuilder.Add(new TiffImageCompressionMiddleware<TPixel>(Compression, LzwCompressionAlgorithm.Instance));
                    break;
                case TiffCompression.Deflate:
                case TiffCompression.OldDeflate:
                    pipelineBuilder.Add(new TiffImageCompressionMiddleware<TPixel>(Compression, DeflateCompressionAlgorithm.Instance));
                    break;
                case TiffCompression.Jpeg:
                    if (JpegQuality < 0 || JpegQuality > 100)
                    {
                        throw new InvalidOperationException("JpegQuality should be set between 0 and 100.");
                    }
                    var jpegCompressionAlgorithm = new JpegCompressionAlgorithm(PhotometricInterpretation, JpegQuality, useSharedJpegTables: true);
                    pipelineBuilder.Add(jpegCompressionAlgorithm.GetTableWriter<TPixel>());
                    pipelineBuilder.Add(new TiffImageCompressionMiddleware<TPixel>(Compression, jpegCompressionAlgorithm));
                    break;
                case TiffCompression.PackBits:
                    pipelineBuilder.Add(new TiffImageCompressionMiddleware<TPixel>(Compression, PackBitsCompressionAlgorithm.Instance));
                    break;
                default:
                    throw new NotSupportedException("The selected compression algorithm is not supported.");
            }

            ITiffImageEncoderPipelineNode<TPixel> imageEncoder = pipelineBuilder.Build();

            if (IsTiled)
            {
                pipelineBuilder.InsertFirst(new TiffImageEncoderPaddingMiddleware<TPixel>(TileSize));
                pipelineBuilder.InsertFirst(new TiffTiledImageEncoderEnumeratorMiddleware<TPixel>(TileSize));
            }
            else
            {
                pipelineBuilder.InsertFirst(new TiffStrippedImageEncoderEnumeratorMiddleware<TPixel>(RowsPerStrip));
            }

            ITiffImageEncoderPipelineNode<TPixel> ifdEncoder = pipelineBuilder.Build();

            if (Orientation != 0)
            {
                imageEncoder = PrependOrientationMiddleware(imageEncoder);
                ifdEncoder = PrependOrientationMiddleware(ifdEncoder);
            }

            return new TiffImageEncoderPipelineAdapter<TPixel>(imageEncoder, ifdEncoder);
        }

        private ITiffImageEncoderPipelineNode<TPixel> PrependOrientationMiddleware<TPixel>(ITiffImageEncoderPipelineNode<TPixel> node) where TPixel : unmanaged
        {
            return new TiffImageEncoderPipelineNode<TPixel>(new TiffApplyOrientationMiddleware<TPixel>(Orientation))
            {
                Next = node
            };
        }
    }
}
