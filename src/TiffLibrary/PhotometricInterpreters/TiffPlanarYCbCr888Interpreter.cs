using System;
using System.Threading.Tasks;
using TiffLibrary.ImageDecoder;
using TiffLibrary.PixelBuffer;
using TiffLibrary.PixelFormats;

namespace TiffLibrary.PhotometricInterpreters
{
    /// <summary>
    /// A middleware to read 8-bit YCbCr planar pixels from uncompressed data to destination buffer writer.
    /// </summary>
    public sealed class TiffPlanarYCbCr888Interpreter : ITiffImageDecoderMiddleware
    {
        private readonly TiffYCbCrToRgbConvertionTable _convertionTable;

        /// <summary>
        /// Initialize the middleware with the default YCbCrCoefficients and ReferenceBlackWhite tags.
        /// </summary>
        public TiffPlanarYCbCr888Interpreter() : this(TiffValueCollection<TiffRational>.Empty, TiffValueCollection<TiffRational>.Empty) { }

        /// <summary>
        /// Initialize the middleware.
        /// </summary>
        /// <param name="coefficients">The YCbCrCoefficients tag.</param>
        /// <param name="referenceBlackWhite">The ReferenceBlackWhite tag.</param>
        public TiffPlanarYCbCr888Interpreter(TiffValueCollection<TiffRational> coefficients, TiffValueCollection<TiffRational> referenceBlackWhite)
        {
            if (!coefficients.IsEmpty && coefficients.Count != 3)
            {
                throw new ArgumentException("coefficient should have 3 none-zero elements.");
            }
            if (!referenceBlackWhite.IsEmpty && referenceBlackWhite.Count != 6)
            {
                throw new ArgumentException("referenceWhiteBlack should have 6 elements.");
            }
            _convertionTable = TiffYCbCrToRgbConvertionTable.Create(coefficients.GetOrCreateArray(), referenceBlackWhite.GetOrCreateArray());
        }

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

            TiffYCbCrToRgbConvertionTable convertionTable = _convertionTable;

            int skippedRowOffset = context.SourceImageSize.Width * context.SourceReadOffset.Y;
            int planarByteCount = context.SourceImageSize.Width * context.SourceImageSize.Height;

            ReadOnlySpan<byte> sourceSpan = context.UncompressedData.Span;
            ReadOnlySpan<byte> sourceY = sourceSpan.Slice(0, planarByteCount);
            ReadOnlySpan<byte> sourceCb = sourceSpan.Slice(planarByteCount, planarByteCount);
            ReadOnlySpan<byte> sourceCr = sourceSpan.Slice(2 * planarByteCount, planarByteCount);

            using TiffPixelBufferWriter<TiffRgba32> writer = context.GetWriter<TiffRgba32>();

            int rows = context.ReadSize.Height;
            int cols = context.ReadSize.Width;

            for (int row = 0; row < rows; row++)
            {
                using TiffPixelSpanHandle<TiffRgba32> pixelSpanHandle = writer.GetRowSpan(row);
                Span<TiffRgba32> rowDestinationSpan = pixelSpanHandle.GetSpan();
                int rowOffset = skippedRowOffset + row * context.SourceImageSize.Width + context.SourceReadOffset.X;
                for (int col = 0; col < cols; col++)
                {
                    int componentOffset = rowOffset + col;
                    rowDestinationSpan[col] = convertionTable.Convert(sourceY[componentOffset], sourceCb[componentOffset], sourceCr[componentOffset]);
                }
            }

            return next.RunAsync(context);
        }
    }
}
