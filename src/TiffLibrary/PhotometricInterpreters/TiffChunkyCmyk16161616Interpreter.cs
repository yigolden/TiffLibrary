using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TiffLibrary.ImageDecoder;
using TiffLibrary.PixelBuffer;
using TiffLibrary.PixelFormats;

namespace TiffLibrary.PhotometricInterpreters
{
    /// <summary>
    /// A middleware to read 16-bit CMYK pixels from uncompressed data to destination buffer writer.
    /// </summary>
    public sealed class TiffChunkyCmyk16161616Interpreter : ITiffImageDecoderMiddleware
    {
        /// <summary>
        /// A shared instance of <see cref="TiffChunkyCmyk16161616Interpreter"/>.
        /// </summary>
        public static TiffChunkyCmyk16161616Interpreter Instance { get; } = new TiffChunkyCmyk16161616Interpreter();

        /// <inheritdoc />
        public ValueTask InvokeAsync(TiffImageDecoderContext context, ITiffImageDecoderPipelineNode next)
        {
            ThrowHelper.ThrowIfNull(context);
            ThrowHelper.ThrowIfNull(next);

            int bytesPerScanline = 8 * context.SourceImageSize.Width;
            Memory<byte> source = context.UncompressedData.Slice(context.SourceReadOffset.Y * bytesPerScanline);
            ReadOnlySpan<TiffCmyk64> cmykSourceSpan = MemoryMarshal.Cast<byte, TiffCmyk64>(source.Span);

            using TiffPixelBufferWriter<TiffCmyk64> writer = context.GetWriter<TiffCmyk64>();

            int rows = context.ReadSize.Height;

            if (context.IsLittleEndian == BitConverter.IsLittleEndian)
            {
                for (int row = 0; row < rows; row++)
                {
                    using TiffPixelSpanHandle<TiffCmyk64> pixelSpanHandle = writer.GetRowSpan(row);
                    cmykSourceSpan.Slice(context.SourceReadOffset.X, context.ReadSize.Width).CopyTo(pixelSpanHandle.GetSpan());
                    cmykSourceSpan = cmykSourceSpan.Slice(context.SourceImageSize.Width);
                }
            }
            else
            {
                int cols = context.ReadSize.Width;
                for (int row = 0; row < rows; row++)
                {
                    using TiffPixelSpanHandle<TiffCmyk64> pixelSpanHandle = writer.GetRowSpan(row);
                    Span<TiffCmyk64> rowDestinationSpan = pixelSpanHandle.GetSpan();

                    for (int col = 0; col < cols; col++)
                    {
                        TiffCmyk64 cmyk = cmykSourceSpan[col];

                        cmyk.C = BinaryPrimitives.ReverseEndianness(cmyk.C);
                        cmyk.M = BinaryPrimitives.ReverseEndianness(cmyk.M);
                        cmyk.Y = BinaryPrimitives.ReverseEndianness(cmyk.Y);
                        cmyk.K = BinaryPrimitives.ReverseEndianness(cmyk.K);

                        rowDestinationSpan[col] = cmyk;
                    }

                    cmykSourceSpan = cmykSourceSpan.Slice(context.SourceImageSize.Width);
                }
            }

            return next.RunAsync(context);
        }
    }
}
