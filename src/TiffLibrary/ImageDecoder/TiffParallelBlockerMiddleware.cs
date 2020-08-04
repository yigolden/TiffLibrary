using System;
using System.Threading;
using System.Threading.Tasks;

namespace TiffLibrary.ImageDecoder
{
    internal class TiffParallelBlockerMiddleware : ITiffImageDecoderMiddleware
    {
        private readonly int _maxDegreeOfParallelism;

        public TiffParallelBlockerMiddleware(int maxDegreeOfParallelism)
        {
            if (maxDegreeOfParallelism <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxDegreeOfParallelism));
            }
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        public async ValueTask InvokeAsync(TiffImageDecoderContext context, ITiffImageDecoderPipelineNode next)
        {
            using var state = new TiffParallelDecodingState(_maxDegreeOfParallelism);

            using var mutexService = new ParallelMutexService();

            context.RegisterService(typeof(TiffParallelDecodingState), state);
            context.RegisterService(typeof(ITiffParallelMutexService), mutexService);

            await next.RunAsync(context).ConfigureAwait(false);

            await state.Complete!.Task.ConfigureAwait(false);

            context.RegisterService(typeof(TiffParallelDecodingState), null);
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
