using System;
using System.Buffers;
using System.Threading.Tasks;
using TiffLibrary.PixelFormats;

namespace TiffLibrary.ImageEncoder.PhotometricEncoder
{
    internal class TransparencyMaskEncoder<TPixel> : ITiffImageEncoderMiddleware<TPixel> where TPixel : unmanaged
    {
        private readonly byte _threshold;

        public TransparencyMaskEncoder(byte threshold)
        {
            _threshold = threshold;
        }

        public async ValueTask InvokeAsync(TiffImageEncoderContext<TPixel> context, ITiffImageEncoderPipelineNode<TPixel> next)
        {
            TiffSize imageSize = context.ImageSize;
            using (IMemoryOwner<byte> pixelData = context.MemoryPool.Rent(imageSize.Width * imageSize.Height))
            {
                Memory<byte> pixelDataMemory = pixelData.Memory;
                using (var writer = new TiffMemoryPixelBufferWriter<TiffMask>(context.MemoryPool, pixelDataMemory, imageSize.Width, imageSize.Height))
                using (TiffPixelBufferWriter<TPixel> convertedWriter = context.ConvertWriter(writer.AsPixelBufferWriter()))
                {
                    await context.GetReader().ReadAsync(convertedWriter, context.CancellationToken).ConfigureAwait(false);
                }

                int count = PackBytesIntoBits(pixelDataMemory.Span, imageSize, _threshold);

                context.PhotometricInterpretation = TiffPhotometricInterpretation.TransparencyMask;
                context.BitsPerSample = TiffValueCollection.Single<ushort>(1);
                context.UncompressedData = pixelData.Memory.Slice(0, count);

                await next.RunAsync(context).ConfigureAwait(false);

                context.UncompressedData = default;
            }

            TiffImageFileDirectoryWriter ifdWriter = context.IfdWriter;
            if (!(ifdWriter is null))
            {
                await ifdWriter.WriteTagAsync(TiffTag.PhotometricInterpretation, TiffValueCollection.Single((ushort)context.PhotometricInterpretation)).ConfigureAwait(false);
                await ifdWriter.WriteTagAsync(TiffTag.BitsPerSample, context.BitsPerSample).ConfigureAwait(false);
                await ifdWriter.WriteTagAsync(TiffTag.FillOrder, TiffValueCollection.Single((ushort)TiffFillOrder.HigherOrderBitsFirst));
            }
        }

        private int PackBytesIntoBits(Span<byte> pixelData, TiffSize imageSize, int threshold)
        {
            // Calculate bytes per scanline
            int width = imageSize.Width;
            int height = imageSize.Height;

            // Pack each byte into 1 bit
            int pixelIndex = 0;
            int bufferIndex = 0;
            for (int row = 0; row < height; row++)
            {
                int buffer = 0;
                int bitsInBuffer = 0;
                for (int col = 0; col < width; col++)
                {
                    int fillByte = pixelData[pixelIndex++] > threshold ? 1 : 0;
                    buffer = (buffer << 1) | fillByte;
                    bitsInBuffer++;
                    if (bitsInBuffer == 8)
                    {
                        pixelData[bufferIndex++] = (byte)buffer;
                        buffer = 0;
                        bitsInBuffer = 0;
                    }
                }
                if (bitsInBuffer != 0)
                {
                    buffer <<= (8 - bitsInBuffer);
                    pixelData[bufferIndex++] = (byte)buffer;
                }
            }

            return bufferIndex;
        }
    }
}
