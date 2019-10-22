using System;
using System.Buffers;
using System.Threading.Tasks;
using TiffLibrary.PixelFormats;
using TiffLibrary.PixelFormats.Converters;

namespace TiffLibrary.ImageEncoder.PhotometricEncoder
{
    internal class WhiteIsZero8Encoder<TPixel> : ITiffImageEncoderMiddleware<TPixel> where TPixel : unmanaged
    {
        public async Task InvokeAsync(TiffImageEncoderContext<TPixel> context, ITiffImageEncoderPipelineNode<TPixel> next)
        {
            TiffSize imageSize = context.ImageSize;
            int arraySize = imageSize.Width * imageSize.Height;
            byte[] pixelData = ArrayPool<byte>.Shared.Rent(arraySize);
            try
            {
                using (var white8Writer = new TiffArrayPixelBufferWriter<TiffGray8Reversed>(pixelData, imageSize.Width, imageSize.Height))
                using (var gray8Writer = new TiffGray8ToGray8ReversedPixelConverter(white8Writer))
                using (TiffPixelBufferWriter<TPixel> convertedWriter = context.ConvertWriter(gray8Writer.AsPixelBufferWriter()))
                {
                    await context.GetReader().ReadAsync(convertedWriter).ConfigureAwait(false);
                }

                context.PhotometricInterpretation = TiffPhotometricInterpretation.WhiteIsZero;
                context.BitsPerSample = new TiffValueCollection<ushort>(8);
                context.UncompressedData = pixelData.AsMemory(0, arraySize);

                await next.RunAsync(context).ConfigureAwait(false);
            }
            finally
            {
                context.UncompressedData = default;
                ArrayPool<byte>.Shared.Return(pixelData);
            }

            TiffImageFileDirectoryWriter ifdWriter = context.IfdWriter;
            if (!(ifdWriter is null))
            {
                await ifdWriter.WriteTagAsync(TiffTag.PhotometricInterpretation, new TiffValueCollection<ushort>((ushort)context.PhotometricInterpretation)).ConfigureAwait(false);
                await ifdWriter.WriteTagAsync(TiffTag.BitsPerSample, context.BitsPerSample).ConfigureAwait(false);
            }
        }
    }
}
