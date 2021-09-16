using System;
using System.Threading.Tasks;

namespace TiffLibrary.ImageDecoder
{
    /// <summary>
    /// A middleware that handles orientation of the source image.
    /// </summary>
    public sealed class TiffReverseOrientationMiddleware : ITiffImageDecoderMiddleware
    {
        private readonly TiffOrientation _orientation;

        /// <summary>
        /// Initialize the middleware with the specified orientation.
        /// </summary>
        /// <param name="orientation">The orientation tag.</param>
        [CLSCompliant(false)]
        public TiffReverseOrientationMiddleware(TiffOrientation orientation)
        {
            _orientation = orientation;
        }

        /// <inheritdoc />
        public ValueTask InvokeAsync(TiffImageDecoderContext context, ITiffImageDecoderPipelineNode next)
        {
            ThrowHelper.ThrowIfNull(context);
            ThrowHelper.ThrowIfNull(next);

            if (_orientation == 0 || _orientation == TiffOrientation.TopLeft)
            {
                return next.RunAsync(context);
            }

            return next.RunAsync(new TiffOrientatedImageDecoderContext(context, _orientation));
        }

    }
}
