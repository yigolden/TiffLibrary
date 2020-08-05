using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace TiffLibrary.ImageEncoder
{
    /// <summary>
    /// A middleware that apply subsampling to the input image.
    /// </summary>
    /// <typeparam name="TPixel">The pixel type.</typeparam>
    public sealed class TiffApplyChromaSubsamplingMiddleware<TPixel> : ITiffImageEncoderMiddleware<TPixel> where TPixel : unmanaged
    {
        private readonly int _horizontalSubsampling;
        private readonly int _verticalSubsampling;

        /// <summary>
        /// Initialize the middleware.
        /// </summary>
        /// <param name="horizontalSubsampling">The horizontal subsampling factor.</param>
        /// <param name="verticalSubsampling">The vertical subsampling factor.</param>
        public TiffApplyChromaSubsamplingMiddleware(int horizontalSubsampling, int verticalSubsampling)
        {
            _horizontalSubsampling = horizontalSubsampling;
            _verticalSubsampling = verticalSubsampling;
        }

        /// <summary>
        /// Apply chroma subsampling to <see cref="TiffImageEncoderContext{TPixel}.UncompressedData"/>, and runs the next middleware.
        /// </summary>
        /// <param name="context">The encoder context.</param>
        /// <param name="next">The next middleware.</param>
        /// <returns>A <see cref="Task"/> that completes when the image has been encoded.</returns>
        public ValueTask InvokeAsync(TiffImageEncoderContext<TPixel> context, ITiffImageEncoderPipelineNode<TPixel> next)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (next is null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            // Make sure we are operating on YCbCr data.
            TiffValueCollection<ushort> bitsPerSample = context.BitsPerSample;
            if (bitsPerSample.Count != 3)
            {
                throw new InvalidOperationException("Chroma subsampling can not be applied to this image.");
            }
            for (int i = 0; i < bitsPerSample.Count; i++)
            {
                if (bitsPerSample[i] != 8)
                {
                    throw new InvalidOperationException("Chroma subsampling can not be applied to this image.");
                }
            }

            if (_horizontalSubsampling > 0 && _verticalSubsampling > 0)
            {
                return ProcessAndContinueAsync(context, next);
            }

            return next.RunAsync(context);
        }

        private async ValueTask ProcessAndContinueAsync(TiffImageEncoderContext<TPixel> context, ITiffImageEncoderPipelineNode<TPixel> next)
        {
            int width = context.ImageSize.Width;
            int height = context.ImageSize.Height;

            int blockCols = (width + _horizontalSubsampling - 1) / _horizontalSubsampling;
            int blockRows = (height + _verticalSubsampling - 1) / _verticalSubsampling;
            int subsampledDataLength = width * height + blockCols * blockRows * 2;

            using IMemoryOwner<byte> subsampledDataHandle = (context.MemoryPool ?? MemoryPool<byte>.Shared).Rent(subsampledDataLength);
            Memory<byte> subsampledData = subsampledDataHandle.Memory.Slice(0, subsampledDataLength);
            ProcessChunkyData(context.ImageSize, context.UncompressedData.Span, subsampledData.Span);
            context.UncompressedData = subsampledData;
            await next.RunAsync(context).ConfigureAwait(false);
        }

        private void ProcessChunkyData(TiffSize size, ReadOnlySpan<byte> source, Span<byte> destination)
        {
            int width = size.Width;
            int height = size.Height;
            int horizontalSubsampling = _horizontalSubsampling;
            int verticalSubsampling = _verticalSubsampling;

            int blockCols = (width + horizontalSubsampling - 1) / horizontalSubsampling;
            int blockRows = (height + verticalSubsampling - 1) / verticalSubsampling;
            int blockByteCount = horizontalSubsampling * verticalSubsampling + 2;

            Debug.Assert(source.Length >= width * height * 3);
            Debug.Assert(destination.Length == width * height + blockCols * blockRows * 2);

            ref byte sourceRef = ref MemoryMarshal.GetReference(source);
            ref byte destinationRef = ref MemoryMarshal.GetReference(destination);

            for (int blockRow = 0; blockRow < blockRows; blockRow++)
            {
                for (int blockCol = 0; blockCol < blockCols; blockCol++)
                {
                    int cb = 0;
                    int cr = 0;
                    int pixelCount = 0;
                    int index = 0;

                    ref byte destBlockRef = ref Unsafe.Add(ref destinationRef, blockByteCount * (blockRow * blockCols + blockCol));

                    // Loop through all the pixels in this block
                    for (int row = 0; row < verticalSubsampling; row++)
                    {
                        int actualRow = blockRow * verticalSubsampling + row;
                        for (int col = 0; col < horizontalSubsampling; col++)
                        {
                            int actualCol = blockCol * horizontalSubsampling + col;

                            ref byte pixelRef = ref Unsafe.Add(ref sourceRef, 3 * (actualRow * width + actualCol));

                            // Make sure we are in the bound of the image
                            if (actualRow < height && actualCol < width)
                            {
                                // Copy luminance component
                                Unsafe.Add(ref destBlockRef, index++) = pixelRef;
                                cb += Unsafe.Add(ref pixelRef, 1);
                                cr += Unsafe.Add(ref pixelRef, 2);
                                pixelCount++;
                            }
                            else
                            {
                                Unsafe.Add(ref destBlockRef, index++) = 0;
                            }
                        }
                    }

                    // Copy chrominance component
                    cb = (cb + (pixelCount / 2)) / pixelCount;
                    cr = (cr + (pixelCount / 2)) / pixelCount;
                    Unsafe.Add(ref destBlockRef, index++) = (byte)cb;
                    Unsafe.Add(ref destBlockRef, index++) = (byte)cr;
                }
            }
        }

        /// <summary>
        /// The middleware that can be used to write YCbCrSubSampling field.
        /// </summary>
        /// <returns>The middleware to write YCbCrSubSampling field</returns>
        public ITiffImageEncoderMiddleware<TPixel> GetFieldWriter()
        {
            return new FieldWriter(_horizontalSubsampling, _verticalSubsampling);
        }

        class FieldWriter : ITiffImageEncoderMiddleware<TPixel>
        {
            private ushort[] _subsampling;

            public FieldWriter(int horizontalSubsampling, int verticalSubsampling)
            {
                _subsampling = new ushort[] { (ushort)horizontalSubsampling, (ushort)verticalSubsampling };
            }

            public ValueTask InvokeAsync(TiffImageEncoderContext<TPixel> context, ITiffImageEncoderPipelineNode<TPixel> next)
            {
                TiffImageFileDirectoryWriter? ifdWriter = context.IfdWriter;
                if (ifdWriter is null)
                {
                    return next.RunAsync(context);
                }
                else
                {
                    return new ValueTask(WriteFieldAndContinueAsync(context, next));
                }
            }

            public async Task WriteFieldAndContinueAsync(TiffImageEncoderContext<TPixel> context, ITiffImageEncoderPipelineNode<TPixel> next)
            {
                await next.RunAsync(context).ConfigureAwait(false);

                TiffImageFileDirectoryWriter? ifdWriter = context.IfdWriter;
                Debug.Assert(ifdWriter != null);
                using (await context.LockAsync().ConfigureAwait(false))
                {
                    CancellationToken cancellationToken = context.CancellationToken;
                    await ifdWriter!.WriteTagAsync(TiffTag.YCbCrSubSampling, TiffValueCollection.UnsafeWrap(_subsampling), cancellationToken).ConfigureAwait(false);
                    await ifdWriter!.WriteTagAsync(TiffTag.YCbCrPositioning, TiffValueCollection.Single((ushort)TiffYCbCrPositioning.Centered), cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}
