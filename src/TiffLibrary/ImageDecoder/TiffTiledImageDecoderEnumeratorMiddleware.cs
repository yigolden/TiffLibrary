using System;
using System.Threading.Tasks;

namespace TiffLibrary.ImageDecoder
{
    /// <summary>
    /// A middleware that reads all the tiles from the IFD and runs the following middlewares in the pipeline for each tiles.
    /// </summary>
    public sealed class TiffTiledImageDecoderEnumeratorMiddleware : ITiffImageDecoderMiddleware
    {
        private readonly int _tileWidth;
        private readonly int _tileHeight;
        private readonly TiffValueCollection<long> _tileOffsets;
        private readonly TiffValueCollection<int> _tileByteCounts;
        private readonly int _planarCount;

        /// <summary>
        /// Initialize the middleware.
        /// </summary>
        /// <param name="tileWidth">The TileWidth tag.</param>
        /// <param name="tileHeight">The TileLength tag.</param>
        /// <param name="tileOffsets">The TileOffsets tag.</param>
        /// <param name="tileByteCounts">The TileByteCounts tag.</param>
        /// <param name="planeCount">The plane count.</param>
        public TiffTiledImageDecoderEnumeratorMiddleware(int tileWidth, int tileHeight, TiffValueCollection<long> tileOffsets, TiffValueCollection<int> tileByteCounts, int planeCount)
        {
            _tileWidth = tileWidth;
            _tileHeight = tileHeight;
            _tileOffsets = tileOffsets;
            _tileByteCounts = tileByteCounts;
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

            int tileAcross = (context.SourceImageSize.Width + _tileWidth - 1) / _tileWidth;
            int tileDown = (context.SourceImageSize.Height + _tileHeight - 1) / _tileHeight;
            int tileCount = tileAcross * tileDown;

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
            wrapperContext.SourceImageSize = new TiffSize(_tileWidth, _tileHeight);

            // loop through all the tiles overlapping with the region to read.
            int colStart = context.SourceReadOffset.X / _tileWidth;
            int colEnd = (context.SourceReadOffset.X + context.ReadSize.Width + _tileWidth - 1) / _tileWidth;
            int rowStart = context.SourceReadOffset.Y / _tileHeight;
            int rowEnd = (context.SourceReadOffset.Y + context.ReadSize.Height + _tileHeight - 1) / _tileHeight;

            for (int row = rowStart; row < rowEnd; row++)
            {
                // Calculate coordinates on the y direction for the tiles on this row.
                int currentYOffset = row * _tileHeight;
                int skippedScanlines = Math.Max(0, context.SourceReadOffset.Y - currentYOffset);
                int requestedScanlines = Math.Min(_tileHeight - skippedScanlines, context.SourceReadOffset.Y + context.ReadSize.Height - currentYOffset - skippedScanlines);

                for (int col = colStart; col < colEnd; col++)
                {
                    // Calculate coordinates on the y direction for this tile.
                    int currentXOffset = col * _tileWidth;
                    int skippedXOffset = Math.Max(0, context.SourceReadOffset.X - currentXOffset);
                    int requestedWidth = Math.Min(_tileWidth - skippedXOffset, context.SourceReadOffset.X + context.ReadSize.Width - currentXOffset - skippedXOffset);

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
                    for (int planarIndex = 0; planarIndex < _planarCount; planarIndex++)
                    {
                        int tileIndex = planarIndex * tileCount + row * tileAcross + col;
                        long tileOffset = _tileOffsets[tileIndex];
                        int tileByteCount = _tileByteCounts[tileIndex];

                        planarRegions[planarIndex] = new TiffStreamRegion(tileOffset, tileByteCount);
                    }
                    wrapperContext.PlanarRegions = planarRegions.GetReadOnlyView();

                    context.CancellationToken.ThrowIfCancellationRequested();

                    // Pass down the data
                    await next.RunAsync(wrapperContext).ConfigureAwait(false);
                }
            }

        }
    }
}
