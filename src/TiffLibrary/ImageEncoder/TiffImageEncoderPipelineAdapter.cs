﻿using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.ImageEncoder
{
    /// <summary>
    /// Wraps over middleware list to provide <see cref="TiffImageEncoder{TPixel}"/> functionality.
    /// </summary>
    /// <typeparam name="TPixel">The pixel type.</typeparam>
    public sealed class TiffImageEncoderPipelineAdapter<TPixel> : TiffImageEncoder<TPixel> where TPixel : unmanaged
    {
        private readonly MemoryPool<byte>? _memoryPool;
        private readonly ITiffImageEncoderPipelineNode<TPixel> _imageEncoder;
        private readonly ITiffImageEncoderPipelineNode<TPixel> _ifdEncoder;

        /// <summary>
        /// Initialize the adapter with the specified pipelines.
        /// </summary>
        /// <param name="memoryPool">The memory pool to use when allocating large chunk of memory.</param>
        /// <param name="imageEncoder">The pipeline to use for encoding a single image.</param>
        /// <param name="ifdEncoder">The pipeline to use for encoding an IFD.</param>
        [CLSCompliant(false)]
        public TiffImageEncoderPipelineAdapter(MemoryPool<byte>? memoryPool, ITiffImageEncoderPipelineNode<TPixel> imageEncoder, ITiffImageEncoderPipelineNode<TPixel> ifdEncoder)
        {
            _memoryPool = memoryPool;
            _imageEncoder = imageEncoder;
            _ifdEncoder = ifdEncoder;
        }

        /// <inheritdoc />
        public override async Task<TiffStreamRegion> EncodeAsync(TiffFileWriter writer, TiffPoint offset, TiffSize size, ITiffPixelBufferReader<TPixel> reader, CancellationToken cancellationToken)
        {
            ThrowHelper.ThrowIfNull(writer);
            ThrowHelper.ThrowIfNull(reader);

            if (_imageEncoder is null)
            {
                ThrowHelper.ThrowInvalidOperationException("Image encoder is not configured.");
            }
            size = new TiffSize(Math.Max(0, Math.Min(reader.Width - offset.X, size.Width)), Math.Max(0, Math.Min(reader.Height - offset.Y, size.Height)));
            if (size.IsAreaEmpty)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(size), "The image size is zero.");
            }

            var context = new TiffDefaultImageEncoderContext<TPixel>
            {
                MemoryPool = _memoryPool ?? MemoryPool<byte>.Shared,
                CancellationToken = cancellationToken,
                FileWriter = writer,
                ImageSize = size,
                PixelConverterFactory = TiffDefaultPixelConverterFactory.Instance,
                PixelBufferReader = reader
            };

            await _imageEncoder.RunAsync(context).ConfigureAwait(false);

            return context.OutputRegion;
        }

        /// <inheritdoc />
        public override async Task EncodeAsync(TiffImageFileDirectoryWriter writer, TiffPoint offset, TiffSize size, ITiffPixelBufferReader<TPixel> reader, CancellationToken cancellationToken)
        {
            ThrowHelper.ThrowIfNull(writer);
            ThrowHelper.ThrowIfNull(reader);

            if (_ifdEncoder is null)
            {
                ThrowHelper.ThrowInvalidOperationException("IFD encoder is not configured.");
            }

            size = new TiffSize(Math.Max(0, Math.Min(reader.Width - offset.X, reader.Width)), Math.Max(0, Math.Min(reader.Height - offset.Y, reader.Height)));
            if (size.IsAreaEmpty)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(size), "The image size is zero.");
            }

            var context = new TiffDefaultImageEncoderContext<TPixel>
            {
                MemoryPool = _memoryPool ?? MemoryPool<byte>.Shared,
                CancellationToken = cancellationToken,
                FileWriter = writer.FileWriter,
                IfdWriter = writer,
                ImageSize = size,
                PixelConverterFactory = TiffDefaultPixelConverterFactory.Instance,
                PixelBufferReader = reader
            };

            await _ifdEncoder.RunAsync(context).ConfigureAwait(false);
        }
    }
}
