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
        public async ValueTask InvokeAsync(TiffImageEncoderContext<TPixel> context, ITiffImageEncoderPipelineNode<TPixel> next)
        {
            if (context is null)
            {
                throw new System.ArgumentNullException(nameof(context));
            }

            if (next is null)
            {
                throw new System.ArgumentNullException(nameof(next));
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
                PhotometricInterpretation = context.PhotometricInterpretation,
                ImageSize = context.ImageSize,
                BitsPerSample = context.BitsPerSample,
                BytesPerScanline = bytesPerScanlines
            };

            using (var bufferWriter = new MemoryPoolBufferWriter(context.MemoryPool))
            {
                _compressionAlgorithm.Compress(compressionContext, context.UncompressedData, bufferWriter);
                int length = bufferWriter.Length;
                TiffStreamOffset offset = await context.FileWriter.WriteAlignedBytesAsync(bufferWriter.GetReadOnlySequence()).ConfigureAwait(false);

                context.Compression = _compression;
                context.OutputRegion = new TiffStreamRegion(offset, length);
            }

            TiffImageFileDirectoryWriter ifdWriter = context.IfdWriter;
            if (!(ifdWriter is null))
            {
                await ifdWriter.WriteTagAsync(TiffTag.Compression, TiffValueCollection.Single((ushort)_compression)).ConfigureAwait(false);
            }

            await next.RunAsync(context).ConfigureAwait(false);
        }
    }
}
