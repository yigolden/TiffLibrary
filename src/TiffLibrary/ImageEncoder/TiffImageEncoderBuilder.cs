using System;
using System.Buffers;
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
        /// The memory pool to use when allocating large chunk of memory.
        /// </summary>
        public MemoryPool<byte>? MemoryPool { get; set; }

        /// <summary>
        /// Gets or sets the photometric interpretation to use for the input image.
        /// </summary>
        [CLSCompliant(false)]
        public TiffPhotometricInterpretation PhotometricInterpretation { get; set; }

        /// <summary>
        /// Gets or sets the compression method to use when compressing input image.
        /// </summary>
        [CLSCompliant(false)]
        public TiffCompression Compression { get; set; }

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
        public TiffSize TileSize { get; set; } = new TiffSize(512, 512);

        /// <summary>
        /// Gets or sets the predictor to use on the image.
        /// </summary>
        [CLSCompliant(false)]
        public TiffPredictor Predictor { get; set; }

        /// <summary>
        /// Gets or sets whether to write alpha channel when write RGB image. Only used when <see cref="PhotometricInterpretation"/> is set to <see cref="TiffPhotometricInterpretation.RGB"/>.
        /// </summary>
        public bool EnableTransparencyForRgb { get; set; }

        /// <summary>
        /// Gets or sets the orientation in the output TIFF file.
        /// </summary>
        [CLSCompliant(false)]
        public TiffOrientation Orientation { get; set; }

        /// <summary>
        /// The compression level used in Deflate algorithm. A value of 9 is best, and 1 is least compression. The default is 6.
        /// </summary>
        public TiffDeflateCompressionLevel DeflateCompressionLevel { get; set; } = TiffDeflateCompressionLevel.Default;

        /// <summary>
        /// Gets or sets the options to use when encoding with JPEG compression. Only used when <see cref="Compression"/> is set to <see cref="TiffCompression.Jpeg"/>.
        /// </summary>
        public TiffJpegEncodingOptions? JpegOptions { get; set; }

        /// <summary>
        /// Gets or sets the horizontal chroma subsampling factor for YCbCr image.
        /// </summary>
        public int HorizontalChromaSubSampling { get; set; } = 1;

        /// <summary>
        /// Gets or sets the vertical chroma subsampling factor for YCbCr image.
        /// </summary>
        public int VerticalChromaSubSampling { get; set; } = 1;

        /// <summary>
        /// Gets or sets the maximum number of concurrent encoding pipelines enabled by this <see cref="TiffImageEncoderBuilder"/> instance.
        /// </summary>
        public int MaxDegreeOfParallelism { get; set; }

        /// <summary>
        /// Build the <see cref="TiffImageEncoder{TPixel}"/> instance with the specified pixel format of input image.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type of the input image.</typeparam>
        /// <returns>The <see cref="TiffImageEncoder{TPixel}"/> instance.</returns>
        public TiffImageEncoder<TPixel> Build<TPixel>() where TPixel : unmanaged
        {
            var pipelineBuilder = new TiffImageEncoderPipelineBuilder<TPixel>();

            bool useHorizontalDifferencingPredictor = Predictor == TiffPredictor.HorizontalDifferencing;
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
                    pipelineBuilder.Add(new TransparencyMaskEncoder<TPixel>(threshold: 127));
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

            int horizontalSubsampling = HorizontalChromaSubSampling;
            int verticalSubsampling = VerticalChromaSubSampling;
            if (PhotometricInterpretation == TiffPhotometricInterpretation.YCbCr)
            {
                horizontalSubsampling = horizontalSubsampling == 0 ? 1 : horizontalSubsampling;
                verticalSubsampling = verticalSubsampling == 0 ? 1 : verticalSubsampling;

                if (horizontalSubsampling != 1 && horizontalSubsampling != 2 && horizontalSubsampling != 4)
                {
                    throw new InvalidOperationException("HorizontalChromaSubSampling can only be 1, 2, or 4.");
                }
                if (verticalSubsampling != 1 && verticalSubsampling != 2 && verticalSubsampling != 4)
                {
                    throw new InvalidOperationException("VerticalChromaSubSampling can only be 1, 2, or 4.");
                }

                var chromaSubsamplingMiddleware = new TiffApplyChromaSubsamplingMiddleware<TPixel>(horizontalSubsampling, verticalSubsampling);
                if (Compression != TiffCompression.Jpeg && (horizontalSubsampling > 1 || verticalSubsampling > 1))
                {
                    pipelineBuilder.Add(chromaSubsamplingMiddleware);
                }
                pipelineBuilder.Add(chromaSubsamplingMiddleware.GetFieldWriter());
            }

            switch (Compression)
            {
                case 0:
                case TiffCompression.NoCompression:
                    pipelineBuilder.Add(new TiffImageCompressionMiddleware<TPixel>(TiffCompression.NoCompression, NoneCompressionAlgorithm.Instance));
                    break;
                case TiffCompression.ModifiedHuffmanCompression:
                    pipelineBuilder.Add(new TiffImageCompressionMiddleware<TPixel>(Compression, ModifiedHuffmanCompressionAlgorithm.GetSharedInstance(TiffFillOrder.HigherOrderBitsFirst)));
                    break;
                case TiffCompression.T4Encoding:
                    pipelineBuilder.Add(new TiffImageCompressionMiddleware<TPixel>(Compression, CcittGroup3OneDimensionalCompressionAlgorithm.GetSharedInstance(TiffFillOrder.HigherOrderBitsFirst)));
                    break;
                case TiffCompression.T6Encoding:
                    pipelineBuilder.Add(new TiffImageCompressionMiddleware<TPixel>(Compression, CcittGroup4Compression.GetSharedInstance(TiffFillOrder.HigherOrderBitsFirst)));
                    break;
                case TiffCompression.Lzw:
                    pipelineBuilder.Add(new TiffImageCompressionMiddleware<TPixel>(Compression, LzwCompressionAlgorithm.Instance));
                    break;
                case TiffCompression.Deflate:
                    pipelineBuilder.Add(new TiffImageCompressionMiddleware<TPixel>(Compression, DeflateCompressionLevel == TiffDeflateCompressionLevel.Default ? DeflateCompressionAlgorithm.Instance : new DeflateCompressionAlgorithm(DeflateCompressionLevel)));
                    break;
                case TiffCompression.Jpeg:
                    TiffJpegEncodingOptions jpegOptions = JpegOptions ?? TiffJpegEncodingOptions.Default;
                    if ((uint)jpegOptions.Quality > 100)
                    {
                        throw new InvalidOperationException("JpegQuality should be set between 0 and 100.");
                    }
                    var jpegCompressionAlgorithm = new JpegCompressionAlgorithm(PhotometricInterpretation, horizontalSubsampling, verticalSubsampling, jpegOptions);
                    pipelineBuilder.Add(jpegCompressionAlgorithm.GetTableWriter<TPixel>());
                    pipelineBuilder.Add(new TiffImageCompressionMiddleware<TPixel>(Compression, jpegCompressionAlgorithm));
                    break;
                case TiffCompression.PackBits:
                    pipelineBuilder.Add(new TiffImageCompressionMiddleware<TPixel>(Compression, PackBitsCompressionAlgorithm.Instance));
                    break;
                case TiffCompression.ThunderScan:
                    pipelineBuilder.Add(new TiffImageCompressionMiddleware<TPixel>(Compression, ThunderScanCompressionAlgorithm.Instance));
                    break;
                case TiffCompression.NeXT:
                    pipelineBuilder.Add(new TiffImageCompressionMiddleware<TPixel>(Compression, NeXTCompressionAlgorithm.Instance));
                    break;
                case TiffCompression.OldDeflate:
                    throw new NotSupportedException("Legacy Deflate compression is not supported. Use TiffCompression.Deflate instead.");
                case TiffCompression.OldJpeg:
                    throw new NotSupportedException("Legacy JPEG compression is not supported. Use TiffCompression.Jpeg instead.");
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

            if (MaxDegreeOfParallelism > 1)
            {
                pipelineBuilder.InsertFirst(new TiffParallelStarterMiddleware<TPixel>(MaxDegreeOfParallelism));
            }

            ITiffImageEncoderPipelineNode<TPixel> ifdEncoder = pipelineBuilder.Build();

            if (Orientation != 0)
            {
                imageEncoder = PrependOrientationMiddleware(imageEncoder);
                ifdEncoder = PrependOrientationMiddleware(ifdEncoder);
            }

            return new TiffImageEncoderPipelineAdapter<TPixel>(MemoryPool, imageEncoder, ifdEncoder);
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
