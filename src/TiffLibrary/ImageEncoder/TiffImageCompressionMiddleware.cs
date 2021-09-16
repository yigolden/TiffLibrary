using System;
using System.Threading;
using System.Threading.Tasks;
using TiffLibrary.Compression;

namespace TiffLibrary.ImageEncoder
{
    /// <summary>
    /// A middleware that handles compression of the input image.
    /// </summary>
    /// <typeparam name="TPixel">The pixel type.</typeparam>
    public sealed class TiffImageCompressionMiddleware<TPixel> : ITiffImageEncoderMiddleware<TPixel> where TPixel : unmanaged
    {
        private readonly TiffCompression _compression;
        private readonly ITiffCompressionAlgorithm _compressionAlgorithm;

        /// <summary>
        /// Initialize the middleware with the specified compression.
        /// </summary>
        /// <param name="compression">The compression method.</param>
        /// <param name="compressionAlgorithm">A instance the handles the actual compression.</param>
        [CLSCompliant(false)]
        public TiffImageCompressionMiddleware(TiffCompression compression, ITiffCompressionAlgorithm compressionAlgorithm)
        {
            _compression = compression;
            _compressionAlgorithm = compressionAlgorithm;
        }

        /// <summary>
        /// Apply compression to <see cref="TiffImageEncoderContext{TPixel}.UncompressedData"/> and writes the compressed image to <see cref="TiffImageEncoderContext{TPixel}.FileWriter"/>. Writes <see cref="TiffTag.Compression"/> to thhe IFD writer and runs the next middleware.
        /// </summary>
        /// <param name="context">The encoder context.</param>
        /// <param name="next">The next middleware.</param>
        /// <returns>A <see cref="Task"/> that completes when the image has been encoded.</returns>
        [CLSCompliant(false)]
        public async ValueTask InvokeAsync(TiffImageEncoderContext<TPixel> context, ITiffImageEncoderPipelineNode<TPixel> next)
        {
            ThrowHelper.ThrowIfNull(context);
            ThrowHelper.ThrowIfNull(next);
            if (context.FileWriter is null)
            {
                ThrowHelper.ThrowInvalidOperationException("Failed to acquire FileWriter");
            }

            TiffValueCollection<ushort> bitsPerSample = context.BitsPerSample;
            int totalBits = 0;
            for (int i = 0; i < bitsPerSample.Count; i++)
            {
                totalBits += bitsPerSample[i];
            }

            int width = context.ImageSize.Width;
            int bytesPerScanlines = (totalBits * width + 7) / 8;

            var compressionContext = new TiffCompressionContext
            {
                MemoryPool = context.MemoryPool,
                PhotometricInterpretation = context.PhotometricInterpretation,
                ImageSize = context.ImageSize,
                BitsPerSample = context.BitsPerSample,
                BytesPerScanline = bytesPerScanlines
            };

            using (var bufferWriter = new MemoryPoolBufferWriter(context.MemoryPool))
            {
                _compressionAlgorithm.Compress(compressionContext, context.UncompressedData, bufferWriter);
                int length = bufferWriter.Length;

                using (await context.LockAsync().ConfigureAwait(false))
                {
                    TiffStreamOffset offset = await context.FileWriter!.WriteAlignedBytesAsync(bufferWriter.GetReadOnlySequence(), context.CancellationToken).ConfigureAwait(false);
                    context.BitsPerSample = compressionContext.BitsPerSample;
                    context.OutputRegion = new TiffStreamRegion(offset, length);
                }
            }

            TiffImageFileDirectoryWriter? ifdWriter = context.IfdWriter;
            if (ifdWriter is not null)
            {
                using (await context.LockAsync().ConfigureAwait(false))
                {
                    CancellationToken cancellationToken = context.CancellationToken;
                    await ifdWriter.WriteTagAsync(TiffTag.Compression, TiffValueCollection.Single((ushort)_compression), cancellationToken).ConfigureAwait(false);
                }
            }

            await next.RunAsync(context).ConfigureAwait(false);
        }
    }
}
