using System.Threading.Tasks;

namespace TiffLibrary.ImageEncoder
{
    /// <summary>
    /// Represents a middleware in the image encoder pipeline.
    /// </summary>
    /// <typeparam name="TPixel">The pixel type.</typeparam>
    public interface ITiffImageEncoderMiddleware<TPixel> where TPixel : unmanaged
    {
        /// <summary>
        /// Run this middleware.
        /// </summary>
        /// <param name="context">Information of the current encoding process.</param>
        /// <param name="next">The next middleware in the encoder pipeline.</param>
        /// <returns>A <see cref="Task"/> that completes when this middleware completes running.</returns>
        ValueTask InvokeAsync(TiffImageEncoderContext<TPixel> context, ITiffImageEncoderPipelineNode<TPixel> next);
    }
}
