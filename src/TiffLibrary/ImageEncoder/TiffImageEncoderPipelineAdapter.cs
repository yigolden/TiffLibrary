﻿using System;
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
        private readonly ITiffImageEncoderPipelineNode<TPixel> _imageEncoder;
        private readonly ITiffImageEncoderPipelineNode<TPixel> _ifdEncoder;

        /// <summary>
        /// Initialize the adapter with the specified pipelines.
        /// </summary>
        /// <param name="imageEncoder">The pipeline to use for encoding a single image.</param>
        /// <param name="ifdEncoder">The pipeline to use for encoding an IFD.</param>
        public TiffImageEncoderPipelineAdapter(ITiffImageEncoderPipelineNode<TPixel> imageEncoder, ITiffImageEncoderPipelineNode<TPixel> ifdEncoder)
        {
            _imageEncoder = imageEncoder;
            _ifdEncoder = ifdEncoder;
        }

        /// <summary>
        /// Encode a single image without writing any IFD fields.
        /// </summary>
        /// <param name="writer">The <see cref="TiffFileWriter"/> object to write encoded image data to.</param>
        /// <param name="offset">The number of columns and rows to skip in <paramref name="reader"/>.</param>
        /// <param name="size">The number of columns and rows to encode in <paramref name="reader"/>.</param>
        /// <param name="reader">The pixel buffer reader object.</param>
        /// <returns>A <see cref="Task{TiffStreamRegion}"/> that completes and return the position and length written into the stream when the image has been encoded.</returns>
        public override async Task<TiffStreamRegion> EncodeAsync(TiffFileWriter writer, TiffPoint offset, TiffSize size, ITiffPixelBufferReader<TPixel> reader)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (reader is null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (_imageEncoder is null)
            {
                throw new InvalidOperationException("Image encoder is not configured.");
            }

            TiffPixelBufferReader<TPixel> structReader = reader.AsPixelBufferReader();
            size = new TiffSize(Math.Max(0, Math.Min(structReader.Width - offset.X, size.Width)), Math.Max(0, Math.Min(structReader.Height - offset.Y, size.Height)));
            if (size.IsAreaEmpty)
            {
                throw new ArgumentOutOfRangeException(nameof(size), "The image size is zero.");
            }

            var context = new TiffDefaultImageEncoderContext<TPixel>
            {
                FileWriter = writer,
                ImageSize = size,
                PixelConverterFactory = TiffDefaultPixelConverterFactory.Instance,
                PixelBufferReader = structReader
            };

            await _imageEncoder.RunAsync(context).ConfigureAwait(false);

            return context.OutputRegion;
        }

        /// <summary>
        /// Encode an image as well as associated IFD fields into TIFF stream.
        /// </summary>
        /// <param name="writer">The <see cref="TiffImageFileDirectoryWriter"/> object to write encoded image data and fields to.</param>
        /// <param name="offset">The number of columns and rows to skip in <paramref name="reader"/>.</param>
        /// <param name="size">The number of columns and rows to encode in <paramref name="reader"/>.</param>
        /// <param name="reader">The pixel buffer reader object.</param>
        /// <returns>A <see cref="Task"/> that completes when the image and fields have been encoded.</returns>
        public override async Task EncodeAsync(TiffImageFileDirectoryWriter writer, TiffPoint offset, TiffSize size, ITiffPixelBufferReader<TPixel> reader)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (reader is null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (_ifdEncoder is null)
            {
                throw new InvalidOperationException("Ifd encoder is not configured.");
            }

            TiffPixelBufferReader<TPixel> structReader = reader.AsPixelBufferReader();
            size = new TiffSize(Math.Max(0, Math.Min(structReader.Width - offset.X, structReader.Width)), Math.Max(0, Math.Min(structReader.Height - offset.Y, structReader.Height)));
            if (size.IsAreaEmpty)
            {
                throw new ArgumentOutOfRangeException(nameof(size), "The image size is zero.");
            }

            var context = new TiffDefaultImageEncoderContext<TPixel>
            {
                FileWriter = writer.FileWriter,
                IfdWriter = writer,
                ImageSize = size,
                PixelConverterFactory = TiffDefaultPixelConverterFactory.Instance,
                PixelBufferReader = structReader
            };

            await _ifdEncoder.RunAsync(context).ConfigureAwait(false);
        }
    }
}
