﻿using System;
using System.Threading.Tasks;

namespace TiffLibrary.ImageEncoder
{
    /// <summary>
    /// A middleware that crops the input image into strips.
    /// </summary>
    /// <typeparam name="TPixel">The pixel type.</typeparam>
    public sealed class TiffStrippedImageEncoderEnumeratorMiddleware<TPixel> : ITiffImageEncoderMiddleware<TPixel> where TPixel : unmanaged
    {
        private readonly int _rowsPerStrip;

        /// <summary>
        /// Initialize the middleware with the specified rows per strip.
        /// </summary>
        /// <param name="rowsPerStrip">Number of rows per strip.</param>
        public TiffStrippedImageEncoderEnumeratorMiddleware(int rowsPerStrip)
        {
            _rowsPerStrip = rowsPerStrip;
        }

        /// <summary>
        /// Crops the input image into multiple strips and runs the next middleware for each strip.
        /// </summary>
        /// <param name="context">The encoder context.</param>
        /// <param name="next">The next middleware.</param>
        /// <returns>A <see cref="Task"/> that completes when the image has been encoded.</returns>
        public async Task InvokeAsync(TiffImageEncoderContext<TPixel> context, ITiffImageEncoderPipelineNode<TPixel> next)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (next is null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            var wrappedContext = new TiffCroppedImageEncoderContext<TPixel>(context);

            int width = context.ImageSize.Width, height = context.ImageSize.Height;
            int stripCount = (height + _rowsPerStrip - 1) / _rowsPerStrip;

            ulong[] stripOffsets = new ulong[stripCount];
            ulong[] stripByteCounts = new ulong[stripCount];

            for (int i = 0; i < stripCount; i++)
            {
                int offsetY = i * _rowsPerStrip;
                int stripHeight = Math.Min(height - offsetY, _rowsPerStrip);

                wrappedContext.ExposeIfdWriter = i == 0;
                wrappedContext.OutputRegion = default;
                wrappedContext.Crop(new TiffPoint(0, offsetY), new TiffSize(width, stripHeight));
                await next.RunAsync(wrappedContext).ConfigureAwait(false);

                stripOffsets[i] = (ulong)(long)wrappedContext.OutputRegion.Offset;
                stripByteCounts[i] = (ulong)wrappedContext.OutputRegion.Length;
            }

            TiffImageFileDirectoryWriter ifdWriter = context.IfdWriter;
            if (!(ifdWriter is null))
            {
                await ifdWriter.WriteTagAsync(TiffTag.ImageWidth, new TiffValueCollection<uint>((uint)width)).ConfigureAwait(false);
                await ifdWriter.WriteTagAsync(TiffTag.ImageLength, new TiffValueCollection<uint>((uint)height)).ConfigureAwait(false);
                await ifdWriter.WriteTagAsync(TiffTag.RowsPerStrip, new TiffValueCollection<ushort>((ushort)_rowsPerStrip)).ConfigureAwait(false);

                if (context.FileWriter.UseBigTiff)
                {
                    await ifdWriter.WriteTagAsync(TiffTag.StripOffsets, new TiffValueCollection<ulong>(stripOffsets)).ConfigureAwait(false);
                    await ifdWriter.WriteTagAsync(TiffTag.StripByteCounts, new TiffValueCollection<ulong>(stripByteCounts)).ConfigureAwait(false);
                }
                else
                {
                    uint[] stripOffsets32 = new uint[stripCount];
                    uint[] stripByteCounts32 = new uint[stripCount];

                    for (int i = 0; i < stripCount; i++)
                    {
                        stripOffsets32[i] = (uint)stripOffsets[i];
                        stripByteCounts32[i] = (uint)stripByteCounts[i];
                    }

                    await ifdWriter.WriteTagAsync(TiffTag.StripOffsets, new TiffValueCollection<uint>(stripOffsets32)).ConfigureAwait(false);
                    await ifdWriter.WriteTagAsync(TiffTag.StripByteCounts, new TiffValueCollection<uint>(stripByteCounts32)).ConfigureAwait(false);
                }
            }
        }
    }
}
