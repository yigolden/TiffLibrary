using System;
using System.Threading;
using System.Threading.Tasks;

namespace TiffLibrary.ImageDecoder
{
    internal class TiffParallelDispatcherMiddleware : ITiffImageDecoderMiddleware
    {
        private readonly int _maxDegreeOfParallelism;

        public TiffParallelDispatcherMiddleware(int maxDegreeOfParallelism)
        {
            if (maxDegreeOfParallelism <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxDegreeOfParallelism));
            }
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        public async ValueTask InvokeAsync(TiffImageDecoderContext context, ITiffImageDecoderPipelineNode next)
        {
            TiffParallelDecodingState? state = context.GetService(typeof(TiffParallelDecodingState)) as TiffParallelDecodingState;
            if (state is null)
            {
                await next.RunAsync(context).ConfigureAwait(false);
                return;
            }

            // Make sure state is initialized
            SemaphoreSlim? semaphore = state.Semaphore;
            if (semaphore is null)
            {
                semaphore = state.Semaphore = new SemaphoreSlim(_maxDegreeOfParallelism);
            }

            // Add current task to the work item
            state.IncrementWorkItemCount();
            await semaphore.WaitAsync(context.CancellationToken).ConfigureAwait(false);

            // Asynchronously run this task
            _ = Task.Run(() => RunAsync(context, next, state, semaphore));
        }

        private static async Task RunAsync(TiffImageDecoderContext context, ITiffImageDecoderPipelineNode next, TiffParallelDecodingState state, SemaphoreSlim semaphore)
        {
            try
            {
                await next.RunAsync(context).ConfigureAwait(false);
            }
            catch (OperationCanceledException e)
            {
                state.Complete?.TrySetCanceled(e.CancellationToken);
            }
#pragma warning disable CA1031 // CA1031: Do not catch general exception types
            catch (Exception e)
#pragma warning restore CA1031 // CA1031: Do not catch general exception types
            {
                state.AddException(e);
            }
            finally
            {
                semaphore.Release();
                int count = state.DecrementWorkItemCount();
                if (count == 0)
                {
                    state.Complete?.TrySetResult(null);
                }
            }
        }



    }
}
