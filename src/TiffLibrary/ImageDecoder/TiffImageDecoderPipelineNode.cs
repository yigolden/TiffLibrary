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

        public ValueTask RunAsync(TiffImageDecoderContext context)
        {
            ITiffImageDecoderMiddleware middleware = Middleware;
            ITiffImageDecoderPipelineNode next = Next;

            context.CancellationToken.ThrowIfCancellationRequested();

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

            public ValueTask RunAsync(TiffImageDecoderContext context)
            {
                return default;
            }
        }

    }

}
