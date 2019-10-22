using System.Threading.Tasks;

namespace TiffLibrary.ImageDecoder
{
    internal sealed class TiffImageDecoderPipelineNode : ITiffImageDecoderPipelineNode
    {
        public ITiffImageDecoderMiddleware Middleware { get; set; }
        public ITiffImageDecoderPipelineNode Next { get; set; }

        public TiffImageDecoderPipelineNode(ITiffImageDecoderMiddleware middleware)
        {
            Middleware = middleware;
        }

        public Task RunAsync(TiffImageDecoderContext context)
        {
            ITiffImageDecoderMiddleware middleware = Middleware;
            ITiffImageDecoderPipelineNode next = Next;

            if (next is null)
            {
                return middleware.InvokeAsync(context, EmptyImplementation.Instance);
            }
            else
            {
                return middleware.InvokeAsync(context, next);
            }
        }

        sealed class EmptyImplementation : ITiffImageDecoderPipelineNode
        {
            public static ITiffImageDecoderPipelineNode Instance { get; } = new EmptyImplementation();

            public Task RunAsync(TiffImageDecoderContext context)
            {
                return Task.CompletedTask;
            }
        }

    }

}
