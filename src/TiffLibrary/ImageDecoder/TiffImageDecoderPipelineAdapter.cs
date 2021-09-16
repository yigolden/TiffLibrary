using System;
using System.Buffers;
using System.Threading;
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
            ThrowHelper.ThrowIfNull(parameters);
            ThrowHelper.ThrowIfNull(pipeline);

            _parameters = parameters;
            _pipeline = pipeline;
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

        /// <inheritdoc />
        public override void Decode<TPixel>(TiffPoint offset, TiffSize readSize, TiffPoint destinationOffset, ITiffPixelBufferWriter<TPixel> writer)
        {
            ThrowHelper.ThrowIfNull(writer);

            readSize = new TiffSize(Math.Max(0, Math.Min(writer.Width - destinationOffset.X, readSize.Width)), Math.Max(0, Math.Min(writer.Height - destinationOffset.Y, readSize.Height)));
            readSize = new TiffSize(Math.Max(0, Math.Min(Width - offset.X, readSize.Width)), Math.Max(0, Math.Min(Height - offset.Y, readSize.Height)));
            if (readSize.IsAreaEmpty)
            {
                return;
            }

            if (_parameters.ContentSource is null)
            {
                ThrowHelper.ThrowInvalidOperationException("Failed to acquire ContentSource.");
            }

            using TiffFileContentReader reader = TiffSyncFileContentSource.WrapReader(_parameters.ContentSource.OpenReader());
            var context = new TiffDefaultImageDecoderContext<TPixel>()
            {
                MemoryPool = _parameters.MemoryPool ?? MemoryPool<byte>.Shared,
                CancellationToken = CancellationToken.None,
                OperationContext = _parameters.OperationContext,
                ContentReader = reader,
                SourceImageSize = _parameters.ImageSize,
                SourceReadOffset = offset,
                ReadSize = readSize,
                PixelConverterFactory = _parameters.PixelConverterFactory ?? TiffDefaultPixelConverterFactory.Instance,
                DestinationWriter = new TiffPixelBufferWriter<TPixel>(TiffNoopDisposablePixelBufferWriter.Wrap(writer)).Crop(destinationOffset, readSize)
            };

            _pipeline.RunAsync(context).AsTask().GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public override async Task DecodeAsync<TPixel>(TiffPoint offset, TiffSize readSize, TiffPoint destinationOffset, ITiffPixelBufferWriter<TPixel> writer, CancellationToken cancellationToken = default)
        {
            ThrowHelper.ThrowIfNull(writer);

            readSize = new TiffSize(Math.Max(0, Math.Min(writer.Width - destinationOffset.X, readSize.Width)), Math.Max(0, Math.Min(writer.Height - destinationOffset.Y, readSize.Height)));
            readSize = new TiffSize(Math.Max(0, Math.Min(Width - offset.X, readSize.Width)), Math.Max(0, Math.Min(Height - offset.Y, readSize.Height)));
            if (readSize.IsAreaEmpty)
            {
                return;
            }

            if (_parameters.ContentSource is null)
            {
                ThrowHelper.ThrowInvalidOperationException("Failed to acquire ContentSource.");
            }

            TiffFileContentReader reader = await _parameters.ContentSource.OpenReaderAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var context = new TiffDefaultImageDecoderContext<TPixel>()
                {
                    MemoryPool = _parameters.MemoryPool ?? MemoryPool<byte>.Shared,
                    CancellationToken = cancellationToken,
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
