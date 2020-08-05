using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TiffLibrary.ImageDecoder
{
    internal class TiffParallelDecodingState : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly TaskCompletionSource<object?> _tcs;

        private int _workItemCount;
        private List<Exception>? _exceptions;
        private SpinLock _lock;

        public TiffParallelDecodingState(int maxDegreeOfParallelism)
        {
            if (maxDegreeOfParallelism < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxDegreeOfParallelism));
            }
            _semaphore = new SemaphoreSlim(maxDegreeOfParallelism);
            _tcs = new TaskCompletionSource<object?>();
        }


        public TaskCompletionSource<object?> Complete => _tcs;
        public SemaphoreSlim? Semaphore { get; set; }

        public void LockTaskCompletion()
        {
            Interlocked.Increment(ref _workItemCount);
        }

        public void ReleaseTaskCompletion()
        {
            int count = Interlocked.Decrement(ref _workItemCount);
            if (count == 0)
            {
                _tcs.TrySetResult(null);
            }
        }

        public async Task DispatchAsync(Func<ValueTask> action, CancellationToken cancellationToken)
        {
            // Add current task to the work item
            Interlocked.Increment(ref _workItemCount);

            // Wait until we have the signal
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            // Dispatch the remaining pipeline to another thread.
            _ = Task.Run(() => RunAsync(action));
        }

        private async Task RunAsync(Func<ValueTask> action)
        {
            try
            {
                await action.Invoke().ConfigureAwait(false);
            }
            catch (OperationCanceledException e)
            {
                _tcs.TrySetCanceled(e.CancellationToken);
            }
#pragma warning disable CA1031 // CA1031: Do not catch general exception types
            catch (Exception e)
#pragma warning restore CA1031 // CA1031: Do not catch general exception types
            {
                AddException(e);
            }
            finally
            {
                _semaphore.Release();
                int count = Interlocked.Decrement(ref _workItemCount);
                if (count == 0)
                {
                    _tcs.TrySetResult(null);
                }
            }
        }

        private void AddException(Exception e)
        {
            bool lockTaken = false;
            try
            {
                _lock.Enter(ref lockTaken);

                List<Exception>? exceptions = _exceptions;
                if (exceptions is null)
                {
                    exceptions = _exceptions = new List<Exception>();
                }

                exceptions.Add(e);
            }
            finally
            {
                if (lockTaken)
                {
                    _lock.Exit();
                }
            }
        }

        public void ThrowAggregateException()
        {
            bool lockTaken = false;
            try
            {
                _lock.Enter(ref lockTaken);

                List<Exception>? exceptions = _exceptions;
                if (exceptions is null || exceptions.Count == 0)
                {
                    return;
                }

                throw new AggregateException(exceptions);
            }
            finally
            {
                if (lockTaken)
                {
                    _lock.Exit();
                }
            }
        }

        public void Dispose()
        {
            _semaphore.Dispose();
        }
    }
}
