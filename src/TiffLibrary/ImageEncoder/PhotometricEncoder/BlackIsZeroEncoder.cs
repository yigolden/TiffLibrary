using System.Buffers;
using System.Threading.Tasks;
using TiffLibrary.PixelFormats;

namespace TiffLibrary.ImageEncoder.PhotometricEncoder
{
    internal class BlackIsZeroEncoder<TPixel> : ITiffImageEncoderMiddleware<TPixel> where TPixel : unmanaged
    {
        public async ValueTask InvokeAsync(TiffImageEncoderContext<TPixel> context, ITiffImageEncoderPipelineNode<TPixel> next)
        {
            bool is16Bit = typeof(TPixel) == typeof(TiffGray16);
            int bytesPerSample = (is16Bit ? 2 : 1);
            ushort bitsPerSample = (ushort) (is16Bit ? 16 : 8);

            MemoryPool<byte> memoryPool = context.MemoryPool ?? MemoryPool<byte>.Shared;
            TiffSize imageSize = context.ImageSize;
            int arraySize = imageSize.Width * imageSize.Height * bytesPerSample;
            using (IMemoryOwner<byte> pixelData = memoryPool.Rent(arraySize))
            {
                using (var writer = new TiffMemoryPixelBufferWriter<TPixel>(memoryPool, pixelData.Memory, imageSize.Width, imageSize.Height))
                using (TiffPixelBufferWriter<TPixel> convertedWriter = context.ConvertWriter(writer.AsPixelBufferWriter()))
                {
                    await context.GetReader().ReadAsync(convertedWriter, context.CancellationToken).ConfigureAwait(false);
                }

                context.PhotometricInterpretation = TiffPhotometricInterpretation.BlackIsZero;
                context.BitsPerSample = TiffValueCollection.Single<ushort>(bitsPerSample);
                context.UncompressedData = pixelData.Memory.Slice(0, arraySize);

                await next.RunAsync(context).ConfigureAwait(false);

                context.UncompressedData = default;
            }

            TiffImageFileDirectoryWriter? ifdWriter = context.IfdWriter;
            if (ifdWriter is not null)
            {
                using (await context.LockAsync().ConfigureAwait(false))
                {
                    await ifdWriter.WriteTagAsync(TiffTag.PhotometricInterpretation, TiffValueCollection.Single((ushort)context.PhotometricInterpretation)).ConfigureAwait(false);
                    await ifdWriter.WriteTagAsync(TiffTag.BitsPerSample, context.BitsPerSample).ConfigureAwait(false);
                }
            }
        }
    }
}
