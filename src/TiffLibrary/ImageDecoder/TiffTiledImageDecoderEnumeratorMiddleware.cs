using System;
using System.Threading;
using System.Threading.Tasks;
using TiffLibrary.Utils;

namespace TiffLibrary.ImageDecoder
{
    /// <summary>
    /// A middleware that reads all the tiles from the IFD and runs the following middlewares in the pipeline for each tiles.
    /// </summary>
    public sealed class TiffTiledImageDecoderEnumeratorMiddleware : ITiffImageDecoderMiddleware
    {
        private const int CacheSize = 256; // 4K

        private readonly int _tileWidth;
        private readonly int _tileHeight;
        private readonly int _planaCount;
        private readonly bool _lazyLoad;

        private readonly TiffValueCollection<ulong> _tileOffsets;
        private readonly TiffValueCollection<ulong> _tileByteCounts;

        private readonly TiffImageFileDirectoryEntry _tileOffsetsEntry;
        private readonly TiffImageFileDirectoryEntry _tileByteCountsEntry;

        /// <summary>
        /// Initialize the middleware.
        /// </summary>
        /// <param name="tileWidth">The TileWidth tag.</param>
        /// <param name="tileHeight">The TileLength tag.</param>
        /// <param name="tileOffsets">The TileOffsets tag.</param>
        /// <param name="tileByteCounts">The TileByteCounts tag.</param>
        /// <param name="planeCount">The plane count.</param>
        [CLSCompliant(false)]
        public TiffTiledImageDecoderEnumeratorMiddleware(int tileWidth, int tileHeight, TiffValueCollection<ulong> tileOffsets, TiffValueCollection<ulong> tileByteCounts, int planeCount)
        {
            _tileWidth = tileWidth;
            _tileHeight = tileHeight;
            _planaCount = planeCount;
            _lazyLoad = false;

            _tileOffsets = tileOffsets;
            _tileByteCounts = tileByteCounts;

            if (tileOffsets.Count != tileByteCounts.Count)
            {
                ThrowHelper.ThrowArgumentException("tileOffsets does not have the same element count as tileByteCounts.", nameof(tileByteCounts));
            }
        }

        internal TiffTiledImageDecoderEnumeratorMiddleware(int tileWidth, int tileHeight, TiffImageFileDirectoryEntry tileOffsetsEntry, TiffImageFileDirectoryEntry tileByteCountsEntry, int planeCount)
        {
            _tileWidth = tileWidth;
            _tileHeight = tileHeight;
            _planaCount = planeCount;
            _lazyLoad = true;

            _tileOffsetsEntry = tileOffsetsEntry;
            _tileByteCountsEntry = tileByteCountsEntry;

            if (tileOffsetsEntry.ValueCount != tileByteCountsEntry.ValueCount)
            {
                ThrowHelper.ThrowArgumentException("tileOffsetsEntry does not have the same element count as tileByteCountsEntry.", nameof(tileByteCountsEntry));
            }
        }

        /// <inheritdoc />
        public async ValueTask InvokeAsync(TiffImageDecoderContext context, ITiffImageDecoderPipelineNode next)
        {
            ThrowHelper.ThrowIfNull(context);
            ThrowHelper.ThrowIfNull(next);

            if (context.ContentReader is null)
            {
                ThrowHelper.ThrowArgumentException("ContentReader is not provided in the TiffImageDecoderContext instance.");
            }
            if (context.OperationContext is null)
            {
                ThrowHelper.ThrowArgumentException("OperationContext is not provided in the TiffImageDecoderContext instance.");
            }

            bool isParallel = context.GetService(typeof(TiffParallelDecodingState)) is not null;

            // Initialize the cache
            TiffFieldReader? reader = null;
            TiffStrileOffsetCache cache;
            if (_lazyLoad)
            {
                reader = new TiffFieldReader(context.ContentReader, context.OperationContext, leaveOpen: true);
                cache = new TiffStrileOffsetCache(reader, _tileOffsetsEntry, _tileByteCountsEntry, CacheSize);
            }
            else
            {
                cache = new TiffStrileOffsetCache(_tileOffsets, _tileByteCounts);
            }

            int tileWidth = _tileWidth;
            int tileHeight = _tileHeight;

            try
            {
                int tileAcross = (context.SourceImageSize.Width + tileWidth - 1) / tileWidth;
                int tileDown = (context.SourceImageSize.Height + tileHeight - 1) / tileHeight;
                int tileCount = tileAcross * tileDown;
                CancellationToken cancellationToken = context.CancellationToken;

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

                int planeCount = _planaCount;
                TiffImageEnumeratorDecoderContext? wrapperContext = null;
                TiffMutableValueCollection<TiffStreamRegion> planarRegions = default;
                if (!isParallel)
                {
                    wrapperContext = new TiffImageEnumeratorDecoderContext(context);
                    planarRegions = new TiffMutableValueCollection<TiffStreamRegion>(planeCount);
                }

                // loop through all the tiles overlapping with the region to read.
                int colStart = context.SourceReadOffset.X / tileWidth;
                int colEnd = (context.SourceReadOffset.X + context.ReadSize.Width + tileWidth - 1) / tileWidth;
                int rowStart = context.SourceReadOffset.Y / tileHeight;
                int rowEnd = (context.SourceReadOffset.Y + context.ReadSize.Height + tileHeight - 1) / tileHeight;

                for (int row = rowStart; row < rowEnd; row++)
                {
                    // Calculate coordinates on the y direction for the tiles on this row.
                    int currentYOffset = row * tileHeight;
                    int skippedScanlines = Math.Max(0, context.SourceReadOffset.Y - currentYOffset);
                    int requestedScanlines = Math.Min(tileHeight - skippedScanlines, context.SourceReadOffset.Y + context.ReadSize.Height - currentYOffset - skippedScanlines);

                    for (int col = colStart; col < colEnd; col++)
                    {
                        if (isParallel)
                        {
                            wrapperContext = new TiffImageEnumeratorDecoderContext(context);
                            planarRegions = new TiffMutableValueCollection<TiffStreamRegion>(planeCount);
                        }

                        wrapperContext!.SourceImageSize = new TiffSize(tileWidth, tileHeight);

                        // Calculate coordinates on the y direction for this tile.
                        int currentXOffset = col * tileWidth;
                        int skippedXOffset = Math.Max(0, context.SourceReadOffset.X - currentXOffset);
                        int requestedWidth = Math.Min(tileWidth - skippedXOffset, context.SourceReadOffset.X + context.ReadSize.Width - currentXOffset - skippedXOffset);

                        // Update size info of this tile
                        wrapperContext.SourceReadOffset = new TiffPoint(skippedXOffset, skippedScanlines);
                        wrapperContext.ReadSize = new TiffSize(requestedWidth, requestedScanlines);

                        // Update size info of the destination buffer
                        wrapperContext.SetCropSize(new TiffPoint(Math.Max(0, currentXOffset - context.SourceReadOffset.X), Math.Max(0, currentYOffset - context.SourceReadOffset.Y)), context.ReadSize);

                        // Check to see if there is any region area to be read
                        if (wrapperContext.ReadSize.IsAreaEmpty)
                        {
                            continue;
                        }

                        // Prepare stream regions of this tile
                        for (int planarIndex = 0; planarIndex < planeCount; planarIndex++)
                        {
                            int tileIndex = planarIndex * tileCount + row * tileAcross + col;
                            planarRegions[planarIndex] = await cache.GetOffsetAndCountAsync(tileIndex, cancellationToken).ConfigureAwait(false);
                        }
                        wrapperContext.PlanarRegions = planarRegions.GetReadOnlyView();

                        cancellationToken.ThrowIfCancellationRequested();

                        // Pass down the data
                        await next.RunAsync(wrapperContext).ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                ((IDisposable)cache).Dispose();
                if (reader is not null)
                {
                    await reader.DisposeAsync().ConfigureAwait(false);
                }
            }

        }
    }
}
