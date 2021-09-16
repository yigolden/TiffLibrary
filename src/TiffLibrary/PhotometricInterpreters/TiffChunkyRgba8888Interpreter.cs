using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TiffLibrary.ImageDecoder;
using TiffLibrary.PixelBuffer;
using TiffLibrary.PixelFormats;

namespace TiffLibrary.PhotometricInterpreters
{
    /// <summary>
    /// A middleware to read 8-bit RGBA pixels from uncompressed data to destination buffer writer.
    /// </summary>
    public sealed class TiffChunkyRgba8888Interpreter : ITiffImageDecoderMiddleware
    {
        private readonly bool _isAlphaAssociated;
        private readonly bool _undoColorPreMultiplying;

        /// <summary>
        /// Initialize the middleware.
        /// </summary>
        /// <param name="isAlphaAssociated">Whether the alpha channel is associated.</param>
        /// <param name="undoColorPreMultiplying">Whether to undo color pre-multiplying.</param>
        public TiffChunkyRgba8888Interpreter(bool isAlphaAssociated, bool undoColorPreMultiplying)
        {
            _isAlphaAssociated = isAlphaAssociated;
            _undoColorPreMultiplying = undoColorPreMultiplying;
        }

        /// <inheritdoc />
        public ValueTask InvokeAsync(TiffImageDecoderContext context, ITiffImageDecoderPipelineNode next)
        {
            ThrowHelper.ThrowIfNull(context);
            ThrowHelper.ThrowIfNull(next);

            if (!_isAlphaAssociated)
            {
                ProcessUnassociated(context);
            }
            else if (_undoColorPreMultiplying)
            {
                ProcessAssociatedWithUndoColorPreMultiplying(context);
            }
            else
            {
                ProcessAssociatedPreservingColorPreMultiplying(context);
            }

            return next.RunAsync(context);
        }

        private static void ProcessUnassociated(TiffImageDecoderContext context)
        {
            int bytesPerScanline = 4 * context.SourceImageSize.Width;
            Memory<byte> source = context.UncompressedData.Slice(context.SourceReadOffset.Y * bytesPerScanline);
            ReadOnlySpan<byte> sourceSpan = source.Span;

            using TiffPixelBufferWriter<TiffRgba32> writer = context.GetWriter<TiffRgba32>();

            int rows = context.ReadSize.Height;
            for (int row = 0; row < rows; row++)
            {
                using TiffPixelSpanHandle<TiffRgba32> pixelSpanHandle = writer.GetRowSpan(row);
                Span<byte> rowDestinationSpan = MemoryMarshal.Cast<TiffRgba32, byte>(pixelSpanHandle.GetSpan());
                sourceSpan.Slice(4 * context.SourceReadOffset.X, 4 * context.ReadSize.Width).CopyTo(rowDestinationSpan);
                sourceSpan = sourceSpan.Slice(bytesPerScanline);
            }
        }

        private static void ProcessAssociatedWithUndoColorPreMultiplying(TiffImageDecoderContext context)
        {
            int bytesPerScanline = 4 * context.SourceImageSize.Width;
            Memory<byte> source = context.UncompressedData.Slice(context.SourceReadOffset.Y * bytesPerScanline);
            ReadOnlySpan<byte> sourceSpan = source.Span;

            using TiffPixelBufferWriter<TiffRgba32> writer = context.GetWriter<TiffRgba32>();

            int rows = context.ReadSize.Height;
            int cols = context.ReadSize.Width;
            for (int row = 0; row < rows; row++)
            {
                using TiffPixelSpanHandle<TiffRgba32> pixelSpanHandle = writer.GetRowSpan(row);
                ReadOnlySpan<TiffRgba32> rowSourceSpan = MemoryMarshal.Cast<byte, TiffRgba32>(sourceSpan.Slice(4 * context.SourceReadOffset.X, 4 * context.ReadSize.Width));
                Span<TiffRgba32> rowDestinationSpan = pixelSpanHandle.GetSpan();

                for (int col = 0; col < cols; col++)
                {
                    TiffRgba32 pixel = rowSourceSpan[col];
                    byte a = pixel.A;
                    if (a != 0)
                    {
                        pixel.R = (byte)(pixel.R * 255 / a);
                        pixel.G = (byte)(pixel.G * 255 / a);
                        pixel.B = (byte)(pixel.B * 255 / a);
                    }
                    rowDestinationSpan[col] = pixel;
                }

                sourceSpan = sourceSpan.Slice(bytesPerScanline);
            }

        }

        private static void ProcessAssociatedPreservingColorPreMultiplying(TiffImageDecoderContext context)
        {
            int bytesPerScanline = 4 * context.SourceImageSize.Width;
            Memory<byte> source = context.UncompressedData.Slice(context.SourceReadOffset.Y * bytesPerScanline);
            ReadOnlySpan<byte> sourceSpan = source.Span;

            using TiffPixelBufferWriter<TiffRgb24> writer = context.GetWriter<TiffRgb24>();

            int rows = context.ReadSize.Height;
            int cols = context.ReadSize.Width;
            for (int row = 0; row < rows; row++)
            {
                using TiffPixelSpanHandle<TiffRgb24> pixelSpanHandle = writer.GetRowSpan(row);
                ReadOnlySpan<TiffRgba32> rowSourceSpan = MemoryMarshal.Cast<byte, TiffRgba32>(sourceSpan.Slice(4 * context.SourceReadOffset.X, 4 * context.ReadSize.Width));
                Span<TiffRgb24> rowDestinationSpan = pixelSpanHandle.GetSpan();

                for (int col = 0; col < cols; col++)
                {
                    TiffRgba32 pixel = rowSourceSpan[col];
                    TiffRgb24 pixel24;
                    byte a = pixel.A;
                    if (a == 0)
                    {
                        pixel24 = default;
                    }
                    else
                    {
                        pixel24 = new TiffRgb24((byte)(pixel.R * 255 / a), (byte)(pixel.G * 255 / a), (byte)(pixel.B * 255 / a));
                    }
                    rowDestinationSpan[col] = pixel24;
                }

                sourceSpan = sourceSpan.Slice(bytesPerScanline);
            }
        }
    }
}
