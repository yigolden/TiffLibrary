using System;
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
        public async ValueTask InvokeAsync(TiffImageEncoderContext<TPixel> context, ITiffImageEncoderPipelineNode<TPixel> next)
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
            int rowsPerStrip = _rowsPerStrip <= 0 ? height : _rowsPerStrip;
            int stripCount = (height + rowsPerStrip - 1) / rowsPerStrip;
            
            ulong[] stripOffsets = new ulong[stripCount];
            ulong[] stripByteCounts = new ulong[stripCount];

            for (int i = 0; i < stripCount; i++)
            {
                int offsetY = i * rowsPerStrip;
                int stripHeight = Math.Min(height - offsetY, rowsPerStrip);

                wrappedContext.ExposeIfdWriter = i == 0;
                wrappedContext.OutputRegion = default;
                wrappedContext.Crop(new TiffPoint(0, offsetY), new TiffSize(width, stripHeight));
                await next.RunAsync(wrappedContext).ConfigureAwait(false);

                stripOffsets[i] = (ulong)(long)wrappedContext.OutputRegion.Offset;
                stripByteCounts[i] = (ulong)wrappedContext.OutputRegion.Length;
            }

            TiffImageFileDirectoryWriter? ifdWriter = context.IfdWriter;
            if (!(ifdWriter is null))
            {
                await ifdWriter.WriteTagAsync(TiffTag.ImageWidth, TiffValueCollection.Single((uint)width)).ConfigureAwait(false);
                await ifdWriter.WriteTagAsync(TiffTag.ImageLength, TiffValueCollection.Single((uint)height)).ConfigureAwait(false);
                await ifdWriter.WriteTagAsync(TiffTag.RowsPerStrip, TiffValueCollection.Single((ushort)rowsPerStrip)).ConfigureAwait(false);

                if (context.FileWriter?.UseBigTiff ?? false)
                {
                    await ifdWriter.WriteTagAsync(TiffTag.StripOffsets, TiffValueCollection.UnsafeWrap(stripOffsets)).ConfigureAwait(false);
                    await ifdWriter.WriteTagAsync(TiffTag.StripByteCounts, TiffValueCollection.UnsafeWrap(stripByteCounts)).ConfigureAwait(false);
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

                    await ifdWriter.WriteTagAsync(TiffTag.StripOffsets, TiffValueCollection.UnsafeWrap(stripOffsets32)).ConfigureAwait(false);
                    await ifdWriter.WriteTagAsync(TiffTag.StripByteCounts, TiffValueCollection.UnsafeWrap(stripByteCounts32)).ConfigureAwait(false);
                }
            }
        }
    }
}
