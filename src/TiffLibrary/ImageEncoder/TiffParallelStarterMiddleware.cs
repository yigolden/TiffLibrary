using System;
using System.Threading;
using System.Threading.Tasks;

namespace TiffLibrary.ImageEncoder
{
    internal class TiffParallelStarterMiddleware<TPixel> : ITiffImageEncoderMiddleware<TPixel> where TPixel : unmanaged
    {
        private readonly int _maxDegreeOfParallelism;

        public TiffParallelStarterMiddleware(int maxDegreeOfParallelism)
        {
            if (maxDegreeOfParallelism <= 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(maxDegreeOfParallelism));
            }
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        public async ValueTask InvokeAsync(TiffImageEncoderContext<TPixel> context, ITiffImageEncoderPipelineNode<TPixel> next)
        {
            using var state = new TiffParallelEncodingState(_maxDegreeOfParallelism);

            using var mutexService = new ParallelMutexService();

            context.RegisterService(typeof(TiffParallelEncodingState), state);
            context.RegisterService(typeof(ITiffParallelMutexService), mutexService);

            await next.RunAsync(context).ConfigureAwait(false);

            await state.Complete!.Task.ConfigureAwait(false);

            context.RegisterService(typeof(TiffParallelEncodingState), null);
            context.RegisterService(typeof(ITiffParallelMutexService), null);
        }

        internal class ParallelMutexService : ITiffParallelMutexService, IDisposable
        {
            private readonly SemaphoreSlim _semaphore;
            private readonly ParallelMutexLock _lock;

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
