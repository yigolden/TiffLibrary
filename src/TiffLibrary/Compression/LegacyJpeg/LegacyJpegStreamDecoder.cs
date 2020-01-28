using System;
using System.Buffers;
using System.Threading.Tasks;
using JpegLibrary;
using TiffLibrary.ImageDecoder;

namespace TiffLibrary.Compression
{
    internal class LegacyJpegStreamDecoder : ITiffImageDecoderMiddleware
    {
        private readonly TiffStreamRegion _streamRegion;

        public LegacyJpegStreamDecoder(TiffStreamRegion streamRegion)
        {
            _streamRegion = streamRegion;
        }

        public async ValueTask InvokeAsync(TiffImageDecoderContext context, ITiffImageDecoderPipelineNode next)
        {
            MemoryPool<byte> memoryPool = context.MemoryPool ?? MemoryPool<byte>.Shared;
            TiffFileContentReader contentReader = context.ContentReader ?? throw new InvalidOperationException();

            IMemoryOwner<byte>? dataHandle = null;
            Memory<byte> data;
            try
            {
                using (IMemoryOwner<byte> bufferHandle = memoryPool.Rent(_streamRegion.Length))
                {
                    // Read JPEG stream
                    Memory<byte> buffer = bufferHandle.Memory.Slice(0, _streamRegion.Length);
                    await contentReader.ReadAsync(_streamRegion.Offset, buffer, context.CancellationToken).ConfigureAwait(false);

                    // Identify the image
                    var decoder = new JpegDecoder();
                    decoder.SetInput(buffer);
                    decoder.Identify();
                    if (decoder.Width != context.SourceImageSize.Width || decoder.Height != context.SourceImageSize.Height)
                    {
                        throw new InvalidOperationException("The image size does not match.");
                    }
                    if (decoder.Precision != 8)
                    {
                        throw new InvalidOperationException("Only 8-bit JPEG is supported.");
                    }

                    // Adjust buffer size and reading region to reduce buffer size
                    int skippedWidth = context.SourceReadOffset.X / 8 * 8;
                    int skippedHeight = context.SourceReadOffset.Y / 8 * 8;
                    context.SourceImageSize = new TiffSize(context.SourceImageSize.Width - skippedWidth, context.SourceImageSize.Height - skippedHeight);
                    context.SourceReadOffset = new TiffPoint(context.SourceReadOffset.X % 8, context.SourceReadOffset.Y % 8);
                    int bufferWidth = context.SourceReadOffset.X + context.ReadSize.Width;
                    int bufferHeight = context.SourceReadOffset.Y + context.ReadSize.Height;

                    // Allocate buffer and decode image
                    int dataSize = bufferWidth * bufferHeight * decoder.NumberOfComponents;
                    dataHandle = memoryPool.Rent(dataSize);
                    data = dataHandle.Memory.Slice(0, dataSize);
                    decoder.SetOutputWriter(new LegacyJpegBufferOutputWriter(skippedWidth, bufferWidth, skippedHeight, bufferHeight, decoder.NumberOfComponents, data));
                    decoder.Decode();
                }

                // Pass the buffer to the next middleware
                context.UncompressedData = data;
                await next.RunAsync(context).ConfigureAwait(false);
                context.UncompressedData = null;
            }
            finally
            {
                dataHandle?.Dispose();
            }
        }
    }
}
