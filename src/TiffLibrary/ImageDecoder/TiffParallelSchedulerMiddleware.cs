using System.Threading.Tasks;

namespace TiffLibrary.ImageDecoder
{
    internal class TiffParallelSchedulerMiddleware : ITiffImageDecoderMiddleware
    {
        public ValueTask InvokeAsync(TiffImageDecoderContext context, ITiffImageDecoderPipelineNode next)
        {
            TiffParallelDecodingState? state = context.GetService(typeof(TiffParallelDecodingState)) as TiffParallelDecodingState;
            if (state is null)
            {
                return next.RunAsync(context);
            }

            return new ValueTask(state.DispatchAsync(() => next.RunAsync(context), context.CancellationToken));
        }

    }
}
