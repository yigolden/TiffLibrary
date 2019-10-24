﻿using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TiffLibrary.ImageDecoder;
using TiffLibrary.PixelBuffer;
using TiffLibrary.PixelFormats;

namespace TiffLibrary.PhotometricInterpreters
{
    /// <summary>
    /// A middleware to read 8-bit YCbCr pixels from uncompressed data to destination buffer writer.
    /// </summary>
    public sealed class TiffChunkyYCbCr888Interpreter : ITiffImageDecoderMiddleware
    {
        private readonly TiffYCbCrToRgbConvertionTable _convertionTable;

        /// <summary>
        /// Initialize the middleware with the default YCbCrCoefficients and ReferenceBlackWhite tags.
        /// </summary>
        public TiffChunkyYCbCr888Interpreter() : this(TiffValueCollection<TiffRational>.Empty, TiffValueCollection<TiffRational>.Empty) { }

        /// <summary>
        /// Initialize the middleware.
        /// </summary>
        /// <param name="coefficients">The YCbCrCoefficients tag.</param>
        /// <param name="referenceBlackWhite">The ReferenceBlackWhite tag.</param>
        public TiffChunkyYCbCr888Interpreter(TiffValueCollection<TiffRational> coefficients, TiffValueCollection<TiffRational> referenceBlackWhite)
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

            TiffYCbCrToRgbConvertionTable convertionTable = _convertionTable;

            int bytesPerScanline = 3 * context.SourceImageSize.Width;
            Memory<byte> source = context.UncompressedData.Slice(context.SourceReadOffset.Y * bytesPerScanline);
            ReadOnlySpan<byte> sourceSpan = source.Span;

            using TiffPixelBufferWriter<TiffRgba32> writer = context.GetWriter<TiffRgba32>();

            int rows = context.ReadSize.Height;
            int cols = context.ReadSize.Width;

            for (int row = 0; row < rows; row++)
            {
                using TiffPixelSpanHandle<TiffRgba32> pixelSpanHandle = writer.GetRowSpan(row);
                ReadOnlySpan<byte> rowSourceSpan = sourceSpan.Slice(3 * context.SourceReadOffset.X, 3 * context.ReadSize.Width);
                Span<TiffRgba32> rowDestinationSpan = pixelSpanHandle.GetSpan();

                convertionTable.Convert(rowSourceSpan, rowDestinationSpan, cols);
                sourceSpan = sourceSpan.Slice(bytesPerScanline);
            }

            return next.RunAsync(context);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int Convert(int code, TiffRational referenceBlack, TiffRational referenceWhite, int codingRange)
        {
            return (int)((code - referenceBlack.ToSingle()) * codingRange / (referenceWhite.ToSingle() - referenceBlack.ToSingle()));
        }
    }
}
