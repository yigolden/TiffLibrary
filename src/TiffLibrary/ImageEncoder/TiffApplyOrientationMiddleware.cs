using System;
using System.Threading.Tasks;

namespace TiffLibrary.ImageEncoder
{
    /// <summary>
    /// A middleware that handles orientation of the input image.
    /// </summary>
    /// <typeparam name="TPixel">The pixel type.</typeparam>
    public sealed class TiffApplyOrientationMiddleware<TPixel> : ITiffImageEncoderMiddleware<TPixel> where TPixel : unmanaged
    {
        private readonly TiffOrientation _orientation;

        /// <summary>
        /// Initialize the middleware with the specified orientation.
        /// </summary>
        /// <param name="orientation">The orientation tag.</param>
        public TiffApplyOrientationMiddleware(TiffOrientation orientation)
        {
            _orientation = orientation;
        }

        /// <summary>
        /// Wraps <paramref name="context"/> in a new context with the updated <see cref="TiffImageEncoderContext{TPixel}.ImageSize"/> as well as a wrapped reader that handles orientation. Then runs the <paramref name="next"/> middleware with the replaced context. Writes the <see cref="TiffTag.Orientation"/> tag to the IFD writer.
        /// </summary>
        /// <param name="context">The encoder context.</param>
        /// <param name="next">The next middleware.</param>
        /// <returns>A <see cref="Task"/> that completes when the image has been encoded.</returns>
        public ValueTask InvokeAsync(TiffImageEncoderContext<TPixel> context, ITiffImageEncoderPipelineNode<TPixel> next)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (next is null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (_orientation == 0)
            {
                return next.RunAsync(context);
            }

            return WrapContextAndRunAsync(context, next);
        }

        private async ValueTask WrapContextAndRunAsync(TiffImageEncoderContext<TPixel> context, ITiffImageEncoderPipelineNode<TPixel> next)
        {
            await next.RunAsync(new TiffOrientatedImageEncoderContext<TPixel>(context, _orientation)).ConfigureAwait(false);

            TiffImageFileDirectoryWriter ifdWriter = context.IfdWriter;
            if (!(ifdWriter is null))
            {
                await ifdWriter.WriteTagAsync(TiffTag.Orientation, TiffValueCollection.Single((ushort)_orientation)).ConfigureAwait(false);
            }
        }
    }
}
