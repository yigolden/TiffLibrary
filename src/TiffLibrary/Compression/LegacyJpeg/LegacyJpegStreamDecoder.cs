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
                const int BufferSize = 65536;
                using (var bufferWriter = new MemoryPoolBufferWriter(memoryPool))
                {
                    // Read JPEG stream
                    TiffStreamRegion streamRegion = _streamRegion;
                    do
                    {
                        int readSize = Math.Min(streamRegion.Length, BufferSize);
                        Memory<byte> memory = bufferWriter.GetMemory(readSize);
                        memory = memory.Slice(0, Math.Min(streamRegion.Length, memory.Length));
                        readSize = await contentReader.ReadAsync(streamRegion.Offset, memory, context.CancellationToken).ConfigureAwait(false);
                        bufferWriter.Advance(readSize);
                        streamRegion = new TiffStreamRegion(streamRegion.Offset + readSize, streamRegion.Length - readSize);
                    } while (streamRegion.Length > 0);

                    // Identify the image
                    var decoder = new JpegDecoder();
                    decoder.MemoryPool = memoryPool;
                    decoder.SetInput(bufferWriter.GetReadOnlySequence());
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
                    context.SourceReadOffset = new TiffPoint(context.SourceReadOffset.X % 8, context.SourceReadOffset.Y % 8);
                    int bufferWidth = context.SourceReadOffset.X + context.ReadSize.Width;
                    int bufferHeight = context.SourceReadOffset.Y + context.ReadSize.Height;
                    context.SourceImageSize = new TiffSize(bufferWidth, bufferHeight);

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
