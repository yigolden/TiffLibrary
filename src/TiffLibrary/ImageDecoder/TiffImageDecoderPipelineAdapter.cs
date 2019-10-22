using System;
using System.Threading.Tasks;
using TiffLibrary.PixelBuffer;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.ImageDecoder
{
    /// <summary>
    /// Wraps over middleware list to provide <see cref="TiffImageDecoder"/> functionality.
    /// </summary>
    public sealed class TiffImageDecoderPipelineAdapter : TiffImageDecoder
    {
        private readonly TiffImageDecoderParameters _parameters;
        private readonly ITiffImageDecoderPipelineNode _pipeline;

        private int _orientedWidth;
        private int _orientedHeight;

        /// <summary>
        /// Initialize the adapterwith the specified pipelines.
        /// </summary>
        /// <param name="parameters">Parameters of this TIFF file and IFD.</param>
        /// <param name="pipeline">The pipeline to use for decoding the IFD.</param>
        public TiffImageDecoderPipelineAdapter(TiffImageDecoderParameters parameters, ITiffImageDecoderPipelineNode pipeline)
        {
            _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
            CalculateOrientedSize();
        }

        /// <summary>
        /// The image width after orientation.
        /// </summary>
        public override int Width => _orientedWidth;

        /// <summary>
        /// The image height after orientation.
        /// </summary>
        public override int Height => _orientedHeight;

        private void CalculateOrientedSize()
        {
            switch (_parameters.Orientation)
            {
                case TiffOrientation.LeftTop:
                case TiffOrientation.RightTop:
                case TiffOrientation.RightBottom:
                case TiffOrientation.LeftBottom:
                    _orientedWidth = _parameters.ImageSize.Height;
                    _orientedHeight = _parameters.ImageSize.Width;
                    break;
                default:
                    _orientedWidth = _parameters.ImageSize.Width;
                    _orientedHeight = _parameters.ImageSize.Height;
                    break;
            }
        }

        /// <summary>
        /// Decode the image into the specified pixel buffer.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="offset">Number of columns and rows to skip in the source image.</param>
        /// <param name="readSize">Number of columns and rows to read from the source image.</param>
        /// <param name="destinationOffset">Number of columns and rows to skip in the destination writer.</param>
        /// <param name="writer">The pixel buffer writer to write pixels into.</param>
        /// <returns>A <see cref="Task"/> that completes when the image has been decoded.</returns>
        public override async Task DecodeAsync<TPixel>(TiffPoint offset, TiffSize readSize, TiffPoint destinationOffset, ITiffPixelBufferWriter<TPixel> writer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }
            readSize = new TiffSize(Math.Max(0, Math.Min(writer.Width - destinationOffset.X, readSize.Width)), Math.Max(0, Math.Min(writer.Height - destinationOffset.Y, readSize.Height)));
            readSize = new TiffSize(Math.Max(0, Math.Min(Width - offset.X, readSize.Width)), Math.Max(0, Math.Min(Height - offset.Y, readSize.Height)));
            if (readSize.IsAreaEmpty)
            {
                return;
            }

            TiffFileContentReader reader = await _parameters.ContentSource.OpenReaderAsync().ConfigureAwait(false);
            try
            {
                var context = new TiffDefaultImageDecoderContext<TPixel>()
                {
                    OperationContext = _parameters.OperationContext,
                    ContentReader = reader,
                    SourceImageSize = _parameters.ImageSize,
                    SourceReadOffset = offset,
                    ReadSize = readSize,
                    PixelConverterFactory = _parameters.PixelConverterFactory ?? TiffDefaultPixelConverterFactory.Instance,
                    DestinationWriter = new TiffPixelBufferWriter<TPixel>(TiffNoopDisposablePixelBufferWriter.Wrap(writer)).Crop(destinationOffset, readSize)
                };

                await _pipeline.RunAsync(context).ConfigureAwait(false);
            }
            finally
            {
                await reader.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}
