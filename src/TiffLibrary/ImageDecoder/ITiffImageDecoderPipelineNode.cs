using System.Threading.Tasks;

namespace TiffLibrary.ImageDecoder
{
    /// <summary>
    /// A node in the decoder pipeline.
    /// </summary>
    public interface ITiffImageDecoderPipelineNode
    {
        /// <summary>
        /// Run the middleware of this node as well as the following middlewares in the pipeline.
        /// </summary>
        /// <param name="context">Information of the current decoding process.</param>
        /// <returns>A <see cref="Task"/> that completes when this middlware completes running.</returns>
        Task RunAsync(TiffImageDecoderContext context);
    }
}
