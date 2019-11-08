using System;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TiffLibrary.PhotometricInterpreters;
using TiffLibrary.PixelFormats;

namespace TiffLibrary.ImageEncoder.PhotometricEncoder
{
    internal class YCbCr24Encoder<TPixel> : ITiffImageEncoderMiddleware<TPixel> where TPixel : unmanaged
    {
        private static readonly ushort[] s_bitsPerSample = new ushort[] { 8, 8, 8 };
        private static readonly ushort[] s_yCbCrSubSampling = new ushort[] { 1, 1 };

        public async ValueTask InvokeAsync(TiffImageEncoderContext<TPixel> context, ITiffImageEncoderPipelineNode<TPixel> next)
        {
            TiffSize imageSize = context.ImageSize;
            int arraySize = 3 * imageSize.Width * imageSize.Height;
            using (IMemoryOwner<byte> pixelData = context.MemoryPool.Rent(arraySize))
            {
                Memory<byte> pixelDataMemory = pixelData.Memory;
                using (var writer = new TiffMemoryPixelBufferWriter<TiffRgb24>(context.MemoryPool, pixelDataMemory, imageSize.Width, imageSize.Height))
                using (TiffPixelBufferWriter<TPixel> convertedWriter = context.ConvertWriter(writer.AsPixelBufferWriter()))
                {
                    await context.GetReader().ReadAsync(convertedWriter, context.CancellationToken).ConfigureAwait(false);
                }

                TiffYCbCrConverter.CreateDefault().ConvertFromRgb24(MemoryMarshal.Cast<byte, TiffRgb24>(pixelDataMemory.Span), pixelDataMemory.Span, imageSize.Width * imageSize.Height);

                context.PhotometricInterpretation = TiffPhotometricInterpretation.YCbCr;
                context.BitsPerSample = TiffValueCollection.UnsafeWrap(s_bitsPerSample);
                context.UncompressedData = pixelDataMemory.Slice(0, arraySize);

                await next.RunAsync(context).ConfigureAwait(false);

                context.UncompressedData = default;
            }


            TiffImageFileDirectoryWriter ifdWriter = context.IfdWriter;
            if (!(ifdWriter is null))
            {
                await ifdWriter.WriteTagAsync(TiffTag.PhotometricInterpretation, TiffValueCollection.Single((ushort)context.PhotometricInterpretation)).ConfigureAwait(false);
                await ifdWriter.WriteTagAsync(TiffTag.BitsPerSample, context.BitsPerSample).ConfigureAwait(false);
                await ifdWriter.WriteTagAsync(TiffTag.SamplesPerPixel, TiffValueCollection.Single<ushort>(3));
                await ifdWriter.WriteTagAsync(TiffTag.YCbCrSubSampling, TiffValueCollection.UnsafeWrap(s_yCbCrSubSampling));
                await ifdWriter.WriteTagAsync(TiffTag.YCbCrCoefficients, TiffYCbCrConverter.DefaultLuma);
                await ifdWriter.WriteTagAsync(TiffTag.ReferenceBlackWhite, TiffYCbCrConverter.DefaultReferenceBlackWhite);
            }
        }
    }
}
