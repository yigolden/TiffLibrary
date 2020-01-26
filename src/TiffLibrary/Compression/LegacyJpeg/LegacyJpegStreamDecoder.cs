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
                    Memory<byte> buffer = bufferHandle.Memory.Slice(0, _streamRegion.Length);
                    await contentReader.ReadAsync(_streamRegion.Offset, buffer, context.CancellationToken).ConfigureAwait(false);

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

                    int dataSize = decoder.Width * decoder.Height * decoder.NumberOfComponents;
                    dataHandle = memoryPool.Rent(dataSize);
                    data = dataHandle.Memory.Slice(0, dataSize);
                    decoder.SetOutputWriter(new JpegBuffer8BitOutputWriter(decoder.Width, context.SourceReadOffset.Y, context.ReadSize.Height, decoder.NumberOfComponents, data));
                    decoder.Decode();
                }

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
