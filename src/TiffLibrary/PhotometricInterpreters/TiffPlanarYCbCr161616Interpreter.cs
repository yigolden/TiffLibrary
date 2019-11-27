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
    /// A middleware to read 16-bit YCbCr planar pixels from uncompressed data to destination buffer writer.
    /// </summary>
    public sealed class TiffPlanarYCbCr161616Interpreter : ITiffImageDecoderMiddleware
    {
        private readonly TiffYCbCrConverter16 _converter;

        /// <summary>
        /// Initialize the middleware with the default YCbCrCoefficients and ReferenceBlackWhite tags.
        /// </summary>
        public TiffPlanarYCbCr161616Interpreter() : this(TiffValueCollection.Empty<TiffRational>(), TiffValueCollection.Empty<TiffRational>()) { }

        /// <summary>
        /// Initialize the middleware.
        /// </summary>
        /// <param name="coefficients">The YCbCrCoefficients tag.</param>
        /// <param name="referenceBlackWhite">The ReferenceBlackWhite tag.</param>
        public TiffPlanarYCbCr161616Interpreter(TiffValueCollection<TiffRational> coefficients, TiffValueCollection<TiffRational> referenceBlackWhite)
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

            TiffYCbCrConverter16 converter = _converter;

            int skippedRowOffset = context.SourceImageSize.Width * context.SourceReadOffset.Y;
            int planarByteCount = sizeof(ushort) * context.SourceImageSize.Width * context.SourceImageSize.Height;

            ReadOnlySpan<byte> sourceSpan = context.UncompressedData.Span;
            ReadOnlySpan<ushort> sourceY = MemoryMarshal.Cast<byte, ushort>(sourceSpan.Slice(0, planarByteCount));
            ReadOnlySpan<ushort> sourceCb = MemoryMarshal.Cast<byte, ushort>(sourceSpan.Slice(planarByteCount, planarByteCount));
            ReadOnlySpan<ushort> sourceCr = MemoryMarshal.Cast<byte, ushort>(sourceSpan.Slice(2 * planarByteCount, planarByteCount));

            using TiffPixelBufferWriter<TiffRgba64> writer = context.GetWriter<TiffRgba64>();

            int rows = context.ReadSize.Height;
            int cols = context.ReadSize.Width;
            bool reverseEndiannessNeeded = context.OperationContext.IsLittleEndian != BitConverter.IsLittleEndian;

            if (reverseEndiannessNeeded)
            {
                for (int row = 0; row < rows; row++)
                {
                    using TiffPixelSpanHandle<TiffRgba64> pixelSpanHandle = writer.GetRowSpan(row);
                    Span<TiffRgba64> rowDestinationSpan = pixelSpanHandle.GetSpan();
                    int rowOffset = skippedRowOffset + row * context.SourceImageSize.Width + context.SourceReadOffset.X;
                    for (int col = 0; col < cols; col++)
                    {
                        int componentOffset = rowOffset + col;
                        rowDestinationSpan[col] = converter.ConvertToRgba64(BinaryPrimitives.ReverseEndianness(sourceY[componentOffset]), BinaryPrimitives.ReverseEndianness(sourceCb[componentOffset]), BinaryPrimitives.ReverseEndianness(sourceCr[componentOffset]));
                    }
                }
            }
            else
            {
                for (int row = 0; row < rows; row++)
                {
                    using TiffPixelSpanHandle<TiffRgba64> pixelSpanHandle = writer.GetRowSpan(row);
                    Span<TiffRgba64> rowDestinationSpan = pixelSpanHandle.GetSpan();
                    int rowOffset = skippedRowOffset + row * context.SourceImageSize.Width + context.SourceReadOffset.X;
                    for (int col = 0; col < cols; col++)
                    {
                        int componentOffset = rowOffset + col;
                        rowDestinationSpan[col] = converter.ConvertToRgba64(sourceY[componentOffset], sourceCb[componentOffset], sourceCr[componentOffset]);
                    }
                }
            }

            return next.RunAsync(context);
        }
    }
}
