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

        public async Task InvokeAsync(TiffImageEncoderContext<TPixel> context, ITiffImageEncoderPipelineNode<TPixel> next)
        {
            TiffSize imageSize = context.ImageSize;
            byte[] pixelData = ArrayPool<byte>.Shared.Rent(imageSize.Width * imageSize.Height);
            try
            {
                using (var writer = new TiffArrayPixelBufferWriter<TiffMask>(pixelData, imageSize.Width, imageSize.Height))
                using (TiffPixelBufferWriter<TPixel> convertedWriter = context.ConvertWriter(writer.AsPixelBufferWriter()))
                {
                    await context.GetReader().ReadAsync(convertedWriter).ConfigureAwait(false);
                }

                int count = PackBytesIntoBits(pixelData, imageSize, _threshold);

                context.PhotometricInterpretation = TiffPhotometricInterpretation.TransparencyMask;
                context.BitsPerSample = new TiffValueCollection<ushort>(1);
                context.UncompressedData = pixelData.AsMemory(0, count);

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
                await ifdWriter.WriteTagAsync(TiffTag.FillOrder, new TiffValueCollection<ushort>((ushort)TiffFillOrder.HigherOrderBitsFirst));
            }
        }

        private int PackBytesIntoBits(byte[] pixelData, TiffSize imageSize, int threshold)
        {
            // Calculate bytes per scanline
            int width = imageSize.Width;
            int height = imageSize.Height;
            int bytesPerScanlines = (width + 7) / 8;

            // Pack each byte into 1 bit
            int pixelIndex = 0;
            int bufferIndex = 0;
            int buffer = 0;
            int bitsInBuffer = 0;
            for (int row = 0; row < height; row++)
            {
                buffer = 0;
                bitsInBuffer = 0;
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
