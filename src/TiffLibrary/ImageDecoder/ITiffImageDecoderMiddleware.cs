using System.Threading.Tasks;

namespace TiffLibrary.ImageDecoder
{
    /// <summary>
    /// Represents a middleware in the image decoder pipeline.
    /// </summary>
    public interface ITiffImageDecoderMiddleware
    {
        /// <summary>
        /// Run this middleware.
        /// </summary>
        /// <param name="context">Information of the current decoding process.</param>
        /// <param name="next">The next middleware in the decoder pipeline.</param>
        /// <returns>A <see cref="Task"/> that completes when this middleware completes running.</returns>
        ValueTask InvokeAsync(TiffImageDecoderContext context, ITiffImageDecoderPipelineNode next);
    }
}
