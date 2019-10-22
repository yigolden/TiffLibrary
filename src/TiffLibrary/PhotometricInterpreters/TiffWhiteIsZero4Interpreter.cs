using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TiffLibrary.ImageDecoder;
using TiffLibrary.PixelBuffer;
using TiffLibrary.PixelFormats;

namespace TiffLibrary.PhotometricInterpreters
{
    /// <summary>
    /// A middleware to read 4-bit WhiteIsZero pixels from uncompressed data to destination buffer writer.
    /// </summary>
    public sealed class TiffWhiteIsZero4Interpreter : ITiffImageDecoderMiddleware
    {
        /// <summary>
        /// A shared instance of <see cref="TiffWhiteIsZero4Interpreter"/>.
        /// </summary>
        public static TiffWhiteIsZero4Interpreter Instance { get; } = new TiffWhiteIsZero4Interpreter();

        /// <summary>
        /// Run this middleware.
        /// </summary>
        /// <param name="context">Information of the current decoding process.</param>
        /// <param name="next">The next middleware in the decoder pipeline.</param>
        /// <returns>A <see cref="Task"/> that completes when this middleware completes running.</returns>
        public Task InvokeAsync(TiffImageDecoderContext context, ITiffImageDecoderPipelineNode next)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (next is null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            int bytesPerScanline = (context.SourceImageSize.Width + 1) / 2;
            Memory<byte> source = context.UncompressedData.Slice(context.SourceReadOffset.Y * bytesPerScanline);
            ReadOnlySpan<byte> sourceSpan = source.Span;

            using TiffPixelBufferWriter<TiffGray8> writer = context.GetWriter<TiffGray8>();

            int xOffset = context.SourceReadOffset.X;
            int rows = context.ReadSize.Height;
            for (int row = 0; row < rows; row++)
            {
                using TiffPixelSpanHandle<TiffGray8> pixelSpanHandle = writer.GetRowSpan(row);
                ReadOnlySpan<byte> bitsSpan = sourceSpan.Slice(xOffset >> 1); // xOffset / 2
                Span<byte> rowDestinationSpan = MemoryMarshal.AsBytes(pixelSpanHandle.GetSpan());
                int bitsOffset = xOffset & 1; // xOffset % 2
                int sourceIndex = 0;
                int destinationIndex = 0;
                int remainingWidth = context.ReadSize.Width;
                byte bits;

                if (bitsOffset > 0)
                {
                    remainingWidth--;
                    bits = bitsSpan[sourceIndex++];
                    bits = (byte)(~bits & 0xf);
                    rowDestinationSpan[destinationIndex++] = (byte)(bits << 4 | bits);
                }

                // manual loop unrolling
                for (; (remainingWidth >> 4) > 0; remainingWidth -= 16) // for (; remainingWidth >= 16; remainingWidth -= 16)
                {
                    bits = (byte)~bitsSpan[sourceIndex++];
                    rowDestinationSpan[destinationIndex++] = (byte)(bits & 0xf0 | bits >> 4);
                    rowDestinationSpan[destinationIndex++] = (byte)(bits << 4 | bits & 0xf);
                    bits = (byte)~bitsSpan[sourceIndex++];
                    rowDestinationSpan[destinationIndex++] = (byte)(bits & 0xf0 | bits >> 4);
                    rowDestinationSpan[destinationIndex++] = (byte)(bits << 4 | bits & 0xf);
                    bits = (byte)~bitsSpan[sourceIndex++];
                    rowDestinationSpan[destinationIndex++] = (byte)(bits & 0xf0 | bits >> 4);
                    rowDestinationSpan[destinationIndex++] = (byte)(bits << 4 | bits & 0xf);
                    bits = (byte)~bitsSpan[sourceIndex++];
                    rowDestinationSpan[destinationIndex++] = (byte)(bits & 0xf0 | bits >> 4);
                    rowDestinationSpan[destinationIndex++] = (byte)(bits << 4 | bits & 0xf);
                    bits = (byte)~bitsSpan[sourceIndex++];
                    rowDestinationSpan[destinationIndex++] = (byte)(bits & 0xf0 | bits >> 4);
                    rowDestinationSpan[destinationIndex++] = (byte)(bits << 4 | bits & 0xf);
                    bits = (byte)~bitsSpan[sourceIndex++];
                    rowDestinationSpan[destinationIndex++] = (byte)(bits & 0xf0 | bits >> 4);
                    rowDestinationSpan[destinationIndex++] = (byte)(bits << 4 | bits & 0xf);
                    bits = (byte)~bitsSpan[sourceIndex++];
                    rowDestinationSpan[destinationIndex++] = (byte)(bits & 0xf0 | bits >> 4);
                    rowDestinationSpan[destinationIndex++] = (byte)(bits << 4 | bits & 0xf);
                    bits = (byte)~bitsSpan[sourceIndex++];
                    rowDestinationSpan[destinationIndex++] = (byte)(bits & 0xf0 | bits >> 4);
                    rowDestinationSpan[destinationIndex++] = (byte)(bits << 4 | bits & 0xf);
                }

                for (; (remainingWidth >> 1) > 0; remainingWidth -= 2) // for (; remainingWidth >= 2; remainingWidth -= 2)
                {
                    bits = (byte)~bitsSpan[sourceIndex++];
                    rowDestinationSpan[destinationIndex++] = (byte)(bits & 0xf0 | bits >> 4);
                    rowDestinationSpan[destinationIndex++] = (byte)(bits << 4 | bits & 0xf);
                }

                if (remainingWidth != 0)
                {
                    bits = (byte)~bitsSpan[sourceIndex++];
                    rowDestinationSpan[destinationIndex++] = (byte)(bits & 0xf0 | bits >> 4);
                }

                sourceSpan = sourceSpan.Slice(bytesPerScanline);
            }

            return next.RunAsync(context);
        }
    }
}
