using System;
using System.Threading;
using System.Threading.Tasks;

namespace TiffLibrary.ImageDecoder
{
    internal class TiffParallelBlockerMiddleware : ITiffImageDecoderMiddleware
    {
        public async ValueTask InvokeAsync(TiffImageDecoderContext context, ITiffImageDecoderPipelineNode next)
        {
            var state = new TiffParallelDecodingState();
            var tcs = new TaskCompletionSource<object?>();
            state.Complete = tcs;

            using var mutexService = new ParallelMutexService();

            context.RegisterService(typeof(TiffParallelDecodingState), state);
            context.RegisterService(typeof(ITiffParallelMutexService), mutexService);

            await next.RunAsync(context).ConfigureAwait(false);

            await tcs.Task.ConfigureAwait(false);

            context.RegisterService(typeof(ITiffParallelMutexService), null);

            state.ThrowAggregateException();
        }

        internal class ParallelMutexService : ITiffParallelMutexService, IDisposable
        {
            private readonly SemaphoreSlim _semaphore ;
            private readonly ParallelMutexLock _lock ;

            public ParallelMutexService()
            {
                _semaphore = new SemaphoreSlim(1);
                _lock = new ParallelMutexLock(_semaphore);
            }

            public async Task<IDisposable> LockAsync(CancellationToken cancellationToken)
            {
                await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                return _lock;
            }

            public void Dispose()
            {
                _semaphore.Dispose();
            }
        }

        internal class ParallelMutexLock : IDisposable
        {
            private readonly SemaphoreSlim _semaphore;

            public ParallelMutexLock(SemaphoreSlim semaphore)
            {
                _semaphore = semaphore;
            }

            public void Dispose()
            {
                _semaphore.Release();
            }

        }
    }
}
