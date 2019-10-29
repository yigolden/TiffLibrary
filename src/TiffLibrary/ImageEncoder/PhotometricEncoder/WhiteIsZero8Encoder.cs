using System.Buffers;
using System.Threading.Tasks;
using TiffLibrary.PixelFormats;
using TiffLibrary.PixelFormats.Converters;

namespace TiffLibrary.ImageEncoder.PhotometricEncoder
{
    internal class WhiteIsZero8Encoder<TPixel> : ITiffImageEncoderMiddleware<TPixel> where TPixel : unmanaged
    {
        public async ValueTask InvokeAsync(TiffImageEncoderContext<TPixel> context, ITiffImageEncoderPipelineNode<TPixel> next)
        {
            TiffSize imageSize = context.ImageSize;
            int arraySize = imageSize.Width * imageSize.Height;
            using (IMemoryOwner<byte> pixelData = context.MemoryPool.Rent(arraySize))
            {
                using (var writer = new TiffMemoryPixelBufferWriter<TiffGray8Reversed>(context.MemoryPool, pixelData.Memory, imageSize.Width, imageSize.Height))
                using (var gray8Writer = new TiffGray8ToGray8ReversedPixelConverter(writer))
                using (TiffPixelBufferWriter<TPixel> convertedWriter = context.ConvertWriter(gray8Writer.AsPixelBufferWriter()))
                {
                    await context.GetReader().ReadAsync(convertedWriter, context.CancellationToken).ConfigureAwait(false);
                }

                context.PhotometricInterpretation = TiffPhotometricInterpretation.WhiteIsZero;
                context.BitsPerSample = TiffValueCollection.Single<ushort>(8);
                context.UncompressedData = pixelData.Memory.Slice(0, arraySize);

                await next.RunAsync(context).ConfigureAwait(false);

                context.UncompressedData = default;
            }

            TiffImageFileDirectoryWriter ifdWriter = context.IfdWriter;
            if (!(ifdWriter is null))
            {
                await ifdWriter.WriteTagAsync(TiffTag.PhotometricInterpretation, TiffValueCollection.Single((ushort)context.PhotometricInterpretation)).ConfigureAwait(false);
                await ifdWriter.WriteTagAsync(TiffTag.BitsPerSample, context.BitsPerSample).ConfigureAwait(false);
            }
        }
    }
}
