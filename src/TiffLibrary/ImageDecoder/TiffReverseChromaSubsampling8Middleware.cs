using System;
using System.Buffers;
using System.Threading.Tasks;

namespace TiffLibrary.ImageDecoder
{
    /// <summary>
    /// A middleware that reverse chroma subsampling for 8-bit YCbCr image.
    /// </summary>
    public sealed class TiffReverseChromaSubsampling8Middleware : ITiffImageDecoderMiddleware
    {
        private readonly ushort _horizontalSubsampling;
        private readonly ushort _verticalSubsampling;
        private readonly bool _isPlanar;

        /// <summary>
        /// Initialize the middleware.
        /// </summary>
        /// <param name="horizontalSubsampling">The horizontal subsampling factor.</param>
        /// <param name="verticalSubsampling">The vertical subsampling factor.</param>
        /// <param name="isPlanar">Whether thid IFD is planar configuration.</param>
        [CLSCompliant(false)]
        public TiffReverseChromaSubsampling8Middleware(ushort horizontalSubsampling, ushort verticalSubsampling, bool isPlanar)
        {
            if (horizontalSubsampling != 1 && horizontalSubsampling != 2 && horizontalSubsampling != 4)
            {
                throw new ArgumentOutOfRangeException(nameof(horizontalSubsampling), "Unsupported horizontal subsampling.");
            }
            if (verticalSubsampling != 1 && verticalSubsampling != 2 && verticalSubsampling != 4)
            {
                throw new ArgumentOutOfRangeException(nameof(verticalSubsampling), "Unsupported vertical subsampling.");
            }

            _horizontalSubsampling = horizontalSubsampling;
            _verticalSubsampling = verticalSubsampling;
            _isPlanar = isPlanar;
        }

        /// <inheritdoc />
        public ValueTask InvokeAsync(TiffImageDecoderContext context, ITiffImageDecoderPipelineNode next)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (next is null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            int horizontalSubsampling = _horizontalSubsampling;
            int verticalSubsampling = _verticalSubsampling;

            if (horizontalSubsampling == 1 && verticalSubsampling == 1)
            {
                return next.RunAsync(context);
            }

            if (_isPlanar)
            {
                ProcessPlanarData(context);
            }
            else
            {
                ProcessChunkyData(context);
            }

            return next.RunAsync(context);
        }

        private void ProcessChunkyData(TiffImageDecoderContext context)
        {
            Memory<byte> decompressedData = context.UncompressedData;
            if (_horizontalSubsampling >= _verticalSubsampling)
            {
                ProcessChunkyData(context.SourceImageSize, decompressedData.Span, decompressedData.Span);
            }
            else
            {
                using IMemoryOwner<byte> bufferMemory = (context.MemoryPool ?? MemoryPool<byte>.Shared).Rent(decompressedData.Length);

                decompressedData.CopyTo(bufferMemory.Memory);
                ProcessChunkyData(context.SourceImageSize, bufferMemory.Memory.Span.Slice(0, decompressedData.Length), decompressedData.Span);
            }
        }

        private void ProcessChunkyData(TiffSize size, ReadOnlySpan<byte> source, Span<byte> destination)
        {
            int width = size.Width;
            int height = size.Height;
            int horizontalSubsampling = _horizontalSubsampling;
            int verticalSubsampling = _verticalSubsampling;

            int blockWidth = width / horizontalSubsampling;
            int blockHeight = height / verticalSubsampling;
            int cbCrOffsetInBlock = horizontalSubsampling * verticalSubsampling;
            int blockByteCount = cbCrOffsetInBlock + 2;

            for (int blockRow = blockHeight - 1; blockRow >= 0; blockRow--)
            {
                for (int blockCol = blockWidth - 1; blockCol >= 0; blockCol--)
                {
                    int blockOffset = blockRow * blockWidth + blockCol;
                    ReadOnlySpan<byte> blockData = source.Slice(blockOffset * blockByteCount, blockByteCount);
                    byte cr = blockData[cbCrOffsetInBlock + 1];
                    byte cb = blockData[cbCrOffsetInBlock];

                    for (int row = verticalSubsampling - 1; row >= 0; row--)
                    {
                        for (int col = horizontalSubsampling - 1; col >= 0; col--)
                        {
                            int offset = 3 * ((blockRow * verticalSubsampling + row) * width + blockCol * horizontalSubsampling + col);
                            destination[offset + 2] = cr;
                            destination[offset + 1] = cb;
                            destination[offset] = blockData[row * horizontalSubsampling + col];
                        }
                    }
                }
            }
        }

        private void ProcessPlanarData(TiffImageDecoderContext context)
        {
            int width = context.SourceImageSize.Width;
            int height = context.SourceImageSize.Height;
            int horizontalSubsampling = _horizontalSubsampling;
            int verticalSubsampling = _verticalSubsampling;

            int planarByteCount = width * height;
            Span<byte> decompressedData = context.UncompressedData.Span;
            Span<byte> planarCb = decompressedData.Slice(planarByteCount, planarByteCount);
            Span<byte> planarCr = decompressedData.Slice(2 * planarByteCount, planarByteCount);

            for (int row = height - 1; row >= 0; row--)
            {
                for (int col = width - 1; col >= 0; col--)
                {
                    int offset = row * width + col;
                    int subsampleOffset = (row / verticalSubsampling) * (width / horizontalSubsampling) + col / horizontalSubsampling;
                    planarCb[offset] = planarCb[subsampleOffset];
                    planarCr[offset] = planarCr[subsampleOffset];
                }
            }
        }
    }
}
