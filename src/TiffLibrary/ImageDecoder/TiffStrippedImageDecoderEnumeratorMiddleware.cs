using System;
using System.Threading.Tasks;

namespace TiffLibrary.ImageDecoder
{
    /// <summary>
    /// A middleware that reads all the strips from the IFD and runs the following middlewares in the pipeline for each strip.
    /// </summary>
    public sealed class TiffStrippedImageDecoderEnumeratorMiddleware : ITiffImageDecoderMiddleware
    {
        private readonly int _rowsPerStrip;
        private readonly TiffValueCollection<long> _stripOffsets;
        private readonly TiffValueCollection<int> _stripsByteCount;
        private readonly int _planarCount;

        /// <summary>
        /// Initialize the middleware.
        /// </summary>
        /// <param name="rowsPerStrip">Rows per strip.</param>
        /// <param name="stripOffsets">The StripOffsets tag.</param>
        /// <param name="stripsByteCount">The StripsByteCount tag.</param>
        /// <param name="planeCount">The number of planes.</param>
        public TiffStrippedImageDecoderEnumeratorMiddleware(int rowsPerStrip, TiffValueCollection<long> stripOffsets, TiffValueCollection<int> stripsByteCount, int planeCount)
        {
            _rowsPerStrip = rowsPerStrip;
            _stripOffsets = stripOffsets;
            _stripsByteCount = stripsByteCount;
            _planarCount = planeCount;
        }

        /// <summary>
        /// Run this middleware.
        /// </summary>
        /// <param name="context">Information of the current decoding process.</param>
        /// <param name="next">The next middleware in the decoder pipeline.</param>
        /// <returns>A <see cref="Task"/> that completes when this middleware completes running.</returns>
        public async ValueTask InvokeAsync(TiffImageDecoderContext context, ITiffImageDecoderPipelineNode next)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (next is null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            int rowsPerStrip = _rowsPerStrip;

            // Make sure the region to read is in the image boundary.
            if (context.SourceReadOffset.X >= context.SourceImageSize.Width || context.SourceReadOffset.Y >= context.SourceImageSize.Height)
            {
                return;
            }
            context.ReadSize = new TiffSize(Math.Min(context.ReadSize.Width, context.SourceImageSize.Width - context.SourceReadOffset.X), Math.Min(context.ReadSize.Height, context.SourceImageSize.Height - context.SourceReadOffset.Y));
            if (context.ReadSize.IsAreaEmpty)
            {
                return;
            }

            // Create a wrapped context
            var wrapperContext = new TiffImageEnumeratorDecoderContext(context);
            var planarRegions = new TiffMutableValueCollection<TiffStreamRegion>(_planarCount);

            // loop through all the strips overlapping with the region to read
            int stripStart = context.SourceReadOffset.Y / rowsPerStrip;
            int stripEnd = (context.SourceReadOffset.Y + context.ReadSize.Height + rowsPerStrip - 1) / rowsPerStrip;
            int actualStripCount = _stripOffsets.Count / _planarCount;
            for (int stripIndex = stripStart; stripIndex < stripEnd; stripIndex++)
            {
                // Calculate size info of this strip
                int currentYOffset = stripIndex * rowsPerStrip;
                int stripImageHeight = Math.Min(rowsPerStrip, context.SourceImageSize.Height - currentYOffset);
                int skippedScanlines = Math.Max(0, context.SourceReadOffset.Y - currentYOffset);
                int requestedScanlines = Math.Min(stripImageHeight - skippedScanlines, context.SourceReadOffset.Y + context.ReadSize.Height - currentYOffset - skippedScanlines);
                wrapperContext.SourceImageSize = new TiffSize(context.SourceImageSize.Width, stripImageHeight);
                wrapperContext.SourceReadOffset = new TiffPoint(context.SourceReadOffset.X, skippedScanlines);
                wrapperContext.ReadSize = new TiffSize(context.ReadSize.Width, requestedScanlines);

                // Updaet size info of the destination buffer
                wrapperContext.SetCropSize(new TiffPoint(0, Math.Max(0, currentYOffset - context.SourceReadOffset.Y)), context.ReadSize);

                // Check to see if there is any region area to be read
                if (wrapperContext.ReadSize.IsAreaEmpty)
                {
                    continue;
                }

                // Prepare stream regions of this strip
                for (int planarIndex = 0; planarIndex < _planarCount; planarIndex++)
                {
                    int accessIndex = planarIndex * actualStripCount + stripIndex;
                    long substripOffset = _stripOffsets[accessIndex];
                    int substripByteCount = _stripsByteCount[accessIndex];

                    planarRegions[planarIndex] = new TiffStreamRegion(substripOffset, substripByteCount);
                }
                wrapperContext.PlanarRegions = planarRegions.GetReadOnlyView();

                context.CancellationToken.ThrowIfCancellationRequested();

                // Pass down the data
                await next.RunAsync(wrapperContext).ConfigureAwait(false);
            }
        }
    }

}
