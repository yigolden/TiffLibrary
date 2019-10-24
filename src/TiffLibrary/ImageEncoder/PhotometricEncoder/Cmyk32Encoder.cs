using System;
using System.Buffers;
using System.Threading.Tasks;
using TiffLibrary.PixelFormats;

namespace TiffLibrary.ImageEncoder.PhotometricEncoder
{
    internal class Cmyk32Encoder<TPixel> : ITiffImageEncoderMiddleware<TPixel> where TPixel : unmanaged
    {
        private static readonly ushort[] s_bitsPerSample = new ushort[] { 8, 8, 8, 8 };

        public async ValueTask InvokeAsync(TiffImageEncoderContext<TPixel> context, ITiffImageEncoderPipelineNode<TPixel> next)
        {
            TiffSize imageSize = context.ImageSize;
            int arraySize = 4 * imageSize.Width * imageSize.Height;
            byte[] pixelData = ArrayPool<byte>.Shared.Rent(arraySize);
            try
            {
                using (var writer = new TiffArrayPixelBufferWriter<TiffCmyk32>(pixelData, imageSize.Width, imageSize.Height))
                using (TiffPixelBufferWriter<TPixel> convertedWriter = context.ConvertWriter(writer.AsPixelBufferWriter()))
                {
                    await context.GetReader().ReadAsync(convertedWriter).ConfigureAwait(false);
                }

                context.PhotometricInterpretation = TiffPhotometricInterpretation.Seperated;
                context.BitsPerSample = new TiffValueCollection<ushort>(s_bitsPerSample);
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
                await ifdWriter.WriteTagAsync(TiffTag.SamplesPerPixel, new TiffValueCollection<ushort>(4));
                await ifdWriter.WriteTagAsync(TiffTag.InkSet, new TiffValueCollection<ushort>((ushort)TiffInkSet.CMYK));
            }
        }
    }
}
