using System;
using System.Threading;
using System.Threading.Tasks;
using TiffLibrary.Utils;

namespace TiffLibrary.ImageDecoder
{
    /// <summary>
    /// A middleware that reads all the strips from the IFD and runs the following middlewares in the pipeline for each strip.
    /// </summary>
    public sealed class TiffStrippedImageDecoderEnumeratorMiddleware : ITiffImageDecoderMiddleware
    {
        private const int CacheSize = 256; // 4K

        private readonly int _rowsPerStrip;
        private readonly int _planeCount;
        private readonly bool _lazyLoad;
        private readonly int _stripCount;

        private readonly TiffValueCollection<ulong> _stripOffsets;
        private readonly TiffValueCollection<ulong> _stripsByteCount;

        private readonly TiffImageFileDirectoryEntry _stripOffsetsEntry;
        private readonly TiffImageFileDirectoryEntry _stripsByteCountEntry;

        /// <summary>
        /// Initialize the middleware.
        /// </summary>
        /// <param name="rowsPerStrip">Rows per strip.</param>
        /// <param name="stripOffsets">The StripOffsets tag.</param>
        /// <param name="stripsByteCount">The StripsByteCount tag.</param>
        /// <param name="planeCount">The number of planes.</param>
        [CLSCompliant(false)]
        public TiffStrippedImageDecoderEnumeratorMiddleware(int rowsPerStrip, TiffValueCollection<ulong> stripOffsets, TiffValueCollection<ulong> stripsByteCount, int planeCount)
        {
            _rowsPerStrip = rowsPerStrip;
            _planeCount = planeCount;
            _lazyLoad = false;

            _stripOffsets = stripOffsets;
            _stripsByteCount = stripsByteCount;

            if (stripOffsets.Count != stripsByteCount.Count)
            {
                throw new ArgumentException("stripsByteCount does not have the same element count as stripsOffsets.", nameof(stripsByteCount));
            }
            _stripCount = stripOffsets.Count;
        }

        internal TiffStrippedImageDecoderEnumeratorMiddleware(int rowsPerStrip, TiffImageFileDirectoryEntry stripOffsetsEntry, TiffImageFileDirectoryEntry stripsByteCountEntry, int planeCount)
        {
            _rowsPerStrip = rowsPerStrip;
            _planeCount = planeCount;
            _lazyLoad = true;

            _stripOffsetsEntry = stripOffsetsEntry;
            _stripsByteCountEntry = stripsByteCountEntry;

            if (stripOffsetsEntry.ValueCount != stripsByteCountEntry.ValueCount)
            {
                throw new ArgumentException("stripOffsetsEntry does not have the same element count as stripsByteCountEntry.", nameof(stripsByteCountEntry));
            }
            _stripCount = (int)_stripOffsetsEntry.ValueCount;
        }

        /// <inheritdoc />
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

            if (context.ContentReader is null)
            {
                throw new ArgumentException("ContentReader is not provided in the TiffImageDecoderContext instance.");
            }
            if (context.OperationContext is null)
            {
                throw new ArgumentException("OperationContext is not provided in the TiffImageDecoderContext instance.");
            }

            bool isParallel = context.GetService(typeof(TiffParallelDecodingState)) is not null;

            // Initialize the cache
            TiffFieldReader? reader = null;
            TiffStrileOffsetCache cache;
            if (_lazyLoad)
            {
                reader = new TiffFieldReader(context.ContentReader, context.OperationContext, leaveOpen: true);
                cache = new TiffStrileOffsetCache(reader, _stripOffsetsEntry, _stripsByteCountEntry, CacheSize);
            }
            else
            {
                cache = new TiffStrileOffsetCache(_stripOffsets, _stripsByteCount);
            }

            try
            {
                int rowsPerStrip = _rowsPerStrip;
                CancellationToken cancellationToken = context.CancellationToken;

                // Special case for mailformed file.
                if (rowsPerStrip <= 0 && _stripCount == 1)
                {
                    rowsPerStrip = context.SourceImageSize.Height;
                }

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
                int planeCount = _planeCount;
                TiffImageEnumeratorDecoderContext? wrapperContext = null;
                TiffMutableValueCollection<TiffStreamRegion> planarRegions = default;
                if (!isParallel)
                {
                    wrapperContext = new TiffImageEnumeratorDecoderContext(context);
                    planarRegions = new TiffMutableValueCollection<TiffStreamRegion>(planeCount);
                }

                // loop through all the strips overlapping with the region to read
                int stripStart = context.SourceReadOffset.Y / rowsPerStrip;
                int stripEnd = (context.SourceReadOffset.Y + context.ReadSize.Height + rowsPerStrip - 1) / rowsPerStrip;
                int actualStripCount = _stripCount / _planeCount;
                for (int stripIndex = stripStart; stripIndex < stripEnd; stripIndex++)
                {
                    if (isParallel)
                    {
                        wrapperContext = new TiffImageEnumeratorDecoderContext(context);
                        planarRegions = new TiffMutableValueCollection<TiffStreamRegion>(planeCount);
                    }

                    // Calculate size info of this strip
                    int currentYOffset = stripIndex * rowsPerStrip;
                    int stripImageHeight = Math.Min(rowsPerStrip, context.SourceImageSize.Height - currentYOffset);
                    int skippedScanlines = Math.Max(0, context.SourceReadOffset.Y - currentYOffset);
                    int requestedScanlines = Math.Min(stripImageHeight - skippedScanlines, context.SourceReadOffset.Y + context.ReadSize.Height - currentYOffset - skippedScanlines);
                    wrapperContext!.SourceImageSize = new TiffSize(context.SourceImageSize.Width, stripImageHeight);
                    wrapperContext.SourceReadOffset = new TiffPoint(context.SourceReadOffset.X, skippedScanlines);
                    wrapperContext.ReadSize = new TiffSize(context.ReadSize.Width, requestedScanlines);

                    // Update size info of the destination buffer
                    wrapperContext.SetCropSize(new TiffPoint(0, Math.Max(0, currentYOffset - context.SourceReadOffset.Y)), context.ReadSize);

                    // Check to see if there is any region area to be read
                    if (wrapperContext.ReadSize.IsAreaEmpty)
                    {
                        continue;
                    }

                    // Prepare stream regions of this strip
                    for (int planarIndex = 0; planarIndex < planeCount; planarIndex++)
                    {
                        int accessIndex = planarIndex * actualStripCount + stripIndex;
                        planarRegions[planarIndex] = await cache.GetOffsetAndCountAsync(accessIndex, cancellationToken).ConfigureAwait(false);
                    }
                    wrapperContext.PlanarRegions = planarRegions.GetReadOnlyView();

                    cancellationToken.ThrowIfCancellationRequested();

                    // Pass down the data
                    await next.RunAsync(wrapperContext).ConfigureAwait(false);
                }
            }
            finally
            {
                ((IDisposable)cache).Dispose();
                reader?.Dispose();
            }


        }
    }

}
