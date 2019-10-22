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
        public TiffReverseOrientationMiddleware(TiffOrientation orientation)
        {
            _orientation = orientation;
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

            if (_orientation == 0 || _orientation == TiffOrientation.TopLeft)
            {
                return next.RunAsync(context);
            }

            return next.RunAsync(new TiffOrientatedImageDecoderContext(context, _orientation));
        }

    }
}
