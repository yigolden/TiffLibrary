using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TiffLibrary.Utils;

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
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(horizontalSubsampling), "Unsupported horizontal subsampling.");
            }
            if (verticalSubsampling != 1 && verticalSubsampling != 2 && verticalSubsampling != 4)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(verticalSubsampling), "Unsupported vertical subsampling.");
            }

            _horizontalSubsampling = horizontalSubsampling;
            _verticalSubsampling = verticalSubsampling;
            _isPlanar = isPlanar;
        }

        /// <inheritdoc />
        public ValueTask InvokeAsync(TiffImageDecoderContext context, ITiffImageDecoderPipelineNode next)
        {
            ThrowHelper.ThrowIfNull(context);
            ThrowHelper.ThrowIfNull(next);

            int horizontalSubsampling = _horizontalSubsampling;
            int verticalSubsampling = _verticalSubsampling;

            if (horizontalSubsampling == 1 && verticalSubsampling == 1)
            {
                return next.RunAsync(context);
            }

            if (_isPlanar)
            {
                return ProcessPlanarData(context, horizontalSubsampling, verticalSubsampling, next);
            }
            else
            {
                return ProcessChunkyData(context, next);
            }
        }

        private ValueTask ProcessChunkyData(TiffImageDecoderContext context, ITiffImageDecoderPipelineNode next)
        {
            Memory<byte> uncompressedData = context.UncompressedData;
            if (_horizontalSubsampling >= _verticalSubsampling && _verticalSubsampling <= 2)
            {
                ProcessChunkyData(context.SourceImageSize, _horizontalSubsampling, _verticalSubsampling, uncompressedData.Span, uncompressedData.Span);
                return next.RunAsync(context);
            }
            else
            {
                return ProcessCoreAsync(context, _horizontalSubsampling, _verticalSubsampling, next);
            }

            static async ValueTask ProcessCoreAsync(TiffImageDecoderContext context, int horizontalSubsampling, int verticalSubsampling, ITiffImageDecoderPipelineNode next)
            {
                Memory<byte> uncompressedData = context.UncompressedData;
                using IMemoryOwner<byte> bufferMemory = (context.MemoryPool ?? MemoryPool<byte>.Shared).Rent(uncompressedData.Length);

                uncompressedData.CopyTo(bufferMemory.Memory);
                ProcessChunkyData(context.SourceImageSize, horizontalSubsampling, verticalSubsampling, bufferMemory.Memory.Span.Slice(0, uncompressedData.Length), uncompressedData.Span);

                await next.RunAsync(context).ConfigureAwait(false);
            }
        }

        private static void ProcessChunkyData(TiffSize size, int horizontalSubsampling, int verticalSubsampling, ReadOnlySpan<byte> source, Span<byte> destination)
        {
            int width = size.Width;
            int height = size.Height;

            int blockWidth = (width + horizontalSubsampling - 1) / horizontalSubsampling;
            int blockHeight = (height + verticalSubsampling - 1) / verticalSubsampling;
            int horizontalSubsamplingShift = TiffMathHelper.Log2((uint)horizontalSubsampling);
            int verticalSubsamplingShift = TiffMathHelper.Log2((uint)verticalSubsampling);
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
                            int imageRow = (blockRow << verticalSubsamplingShift) + row;
                            int imageCol = (blockCol << horizontalSubsamplingShift) + col;
                            if (imageRow < height && imageCol < width)
                            {
                                int offset = 3 * (imageRow * width + imageCol);
                                destination[offset + 2] = cr;
                                destination[offset + 1] = cb;
                                destination[offset] = blockData[(row << horizontalSubsamplingShift) + col];
                            }
                        }
                    }
                }
            }
        }

        private static async ValueTask ProcessPlanarData(TiffImageDecoderContext context, int horizontalSubsampling, int verticalSubsampling, ITiffImageDecoderPipelineNode next)
        {
            using IMemoryOwner<byte> bufferOwner = ProcessCore(context, horizontalSubsampling, verticalSubsampling);
            context.UncompressedData = bufferOwner.Memory;
            await next.RunAsync(context).ConfigureAwait(false);

            static IMemoryOwner<byte> ProcessCore(TiffImageDecoderContext context, int horizontalSubsampling, int verticalSubsampling)
            {
                int imageWidth = context.SourceImageSize.Width;
                int imageHeight = context.SourceImageSize.Height;
                int blockCols = (imageWidth + horizontalSubsampling - 1) / horizontalSubsampling;
                int blockRows = (imageHeight + verticalSubsampling - 1) / verticalSubsampling;
                ReadOnlySpan<byte> uncompressedData = context.UncompressedData.Span;
                int luminanceDataLength = blockCols * blockRows * horizontalSubsampling * verticalSubsampling;
                int chrominanceLength = blockCols * blockRows;
                if (uncompressedData.Length < (luminanceDataLength + chrominanceLength * 2))
                {
                    ThrowHelper.ThrowInvalidDataException();
                }

                ReadOnlySpan<byte> planarY = uncompressedData.Slice(0, luminanceDataLength);
                ReadOnlySpan<byte> planarCb = uncompressedData.Slice(luminanceDataLength, chrominanceLength);
                ReadOnlySpan<byte> planarCr = uncompressedData.Slice(luminanceDataLength + chrominanceLength, chrominanceLength);

                int horizontalLuminanceDataLength = blockCols * horizontalSubsampling;
                int horizontalSubsamplingShift = TiffMathHelper.Log2((uint)horizontalSubsampling);
                int verticalSubsamplingShift = TiffMathHelper.Log2((uint)verticalSubsampling);

                IMemoryOwner<byte>? bufferMemory = null;
                try
                {
                    bufferMemory = (context.MemoryPool ?? MemoryPool<byte>.Shared).Rent(imageWidth * imageHeight * 3);
                    Span<byte> destinationSpan = bufferMemory.Memory.Span;

                    int planarSize = imageWidth * imageHeight;
                    for (int row = 0; row < imageHeight; row++)
                    {
                        int destinationRowOffset = row * imageWidth;
                        for (int col = 0; col < imageWidth; col++)
                        {
                            destinationSpan[destinationRowOffset + col] = planarY[row * horizontalLuminanceDataLength + col];
                            destinationSpan[planarSize + destinationRowOffset + col] = planarCb[(row >> verticalSubsamplingShift) * blockCols + (col >> horizontalSubsamplingShift)];
                            destinationSpan[planarSize * 2 + destinationRowOffset + col] = planarCr[(row >> verticalSubsamplingShift) * blockCols + (col >> horizontalSubsamplingShift)];
                        }
                    }
                    return Interlocked.Exchange<IMemoryOwner<byte>?>(ref bufferMemory, null)!;
                }
                finally
                {
                    bufferMemory?.Dispose();
                }
            }
        }
    }
}
