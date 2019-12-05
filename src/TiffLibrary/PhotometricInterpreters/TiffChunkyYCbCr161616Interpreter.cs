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
    /// A middleware to read 16-bit YCbCr pixels from uncompressed data to destination buffer writer.
    /// </summary>
    public sealed class TiffChunkyYCbCr161616Interpreter : ITiffImageDecoderMiddleware
    {
        private readonly TiffYCbCrConverter16 _converter;

        /// <summary>
        /// Initialize the middleware with the default YCbCrCoefficients and ReferenceBlackWhite tags.
        /// </summary>
        public TiffChunkyYCbCr161616Interpreter() : this(TiffValueCollection.Empty<TiffRational>(), TiffValueCollection.Empty<TiffRational>()) { }

        /// <summary>
        /// Initialize the middleware.
        /// </summary>
        /// <param name="coefficients">The YCbCrCoefficients tag.</param>
        /// <param name="referenceBlackWhite">The ReferenceBlackWhite tag.</param>
        public TiffChunkyYCbCr161616Interpreter(TiffValueCollection<TiffRational> coefficients, TiffValueCollection<TiffRational> referenceBlackWhite)
        {
            if (!coefficients.IsEmpty && coefficients.Count != 3)
            {
                throw new ArgumentException("coefficient should have 3 none-zero elements.");
            }
            if (!referenceBlackWhite.IsEmpty && referenceBlackWhite.Count != 6)
            {
                throw new ArgumentException("referenceWhiteBlack should have 6 elements.");
            }
            _converter = TiffYCbCrConverter16.Create(coefficients.GetOrCreateArray(), referenceBlackWhite.GetOrCreateArray());
        }

        /// <summary>
        /// Run this middleware.
        /// </summary>
        /// <param name="context">Information of the current decoding process.</param>
        /// <param name="next">The next middleware in the decoder pipeline.</param>
        /// <returns>A <see cref="Task"/> that completes when this middleware completes running.</returns>
        public ValueTask InvokeAsync(TiffImageDecoderContext context, ITiffImageDecoderPipelineNode next)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (next is null)
            {
                throw new ArgumentNullException(nameof(next));
            }
            if (context.OperationContext is null)
            {
                throw new InvalidOperationException("Failed to acquire OperationContext.");
            }

            TiffYCbCrConverter16 converter = _converter;

            int elementsPerScanline = 3 * context.SourceImageSize.Width;
            Memory<byte> source = context.UncompressedData.Slice(context.SourceReadOffset.Y * elementsPerScanline * sizeof(ushort));
            Span<ushort> sourceSpan = MemoryMarshal.Cast<byte, ushort>(source.Span);

            using TiffPixelBufferWriter<TiffRgba64> writer = context.GetWriter<TiffRgba64>();

            int rows = context.ReadSize.Height;
            int cols = context.ReadSize.Width;
            bool reverseEndiannessNeeded = context.OperationContext.IsLittleEndian != BitConverter.IsLittleEndian;

            if (reverseEndiannessNeeded)
            {
                for (int row = 0; row < rows; row++)
                {
                    using TiffPixelSpanHandle<TiffRgba64> pixelSpanHandle = writer.GetRowSpan(row);
                    Span<ushort> rowSourceSpan = sourceSpan.Slice(3 * context.SourceReadOffset.X, 3 * context.ReadSize.Width);
                    Span<TiffRgba64> rowDestinationSpan = pixelSpanHandle.GetSpan();

                    for (int i = 0; i < rowSourceSpan.Length; i++)
                    {
                        rowSourceSpan[i] = BinaryPrimitives.ReverseEndianness(rowSourceSpan[i]);
                    }

                    converter.ConvertToRgba64(rowSourceSpan, rowDestinationSpan, cols);
                    sourceSpan = sourceSpan.Slice(elementsPerScanline);
                }
            }
            else
            {
                for (int row = 0; row < rows; row++)
                {
                    using TiffPixelSpanHandle<TiffRgba64> pixelSpanHandle = writer.GetRowSpan(row);
                    Span<ushort> rowSourceSpan = sourceSpan.Slice(3 * context.SourceReadOffset.X, 3 * context.ReadSize.Width);
                    Span<TiffRgba64> rowDestinationSpan = pixelSpanHandle.GetSpan();

                    converter.ConvertToRgba64(rowSourceSpan, rowDestinationSpan, cols);
                    sourceSpan = sourceSpan.Slice(elementsPerScanline);
                }
            }


            return next.RunAsync(context);
        }
    }
}
