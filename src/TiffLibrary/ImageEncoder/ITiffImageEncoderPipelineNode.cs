using System.Threading.Tasks;

namespace TiffLibrary.ImageEncoder
{
    /// <summary>
    /// A node in the encoder pipeline.
    /// </summary>
    /// <typeparam name="TPixel"></typeparam>
    public interface ITiffImageEncoderPipelineNode<TPixel> where TPixel : unmanaged
    {
        /// <summary>
        /// Run the middleware of this node as well as the following middlewares in the pipeline.
        /// </summary>
        /// <param name="context">Information of the current encoding process.</param>
        /// <returns>A <see cref="Task"/> that completes when this middleware completes running.</returns>
        ValueTask RunAsync(TiffImageEncoderContext<TPixel> context);
    }
}
