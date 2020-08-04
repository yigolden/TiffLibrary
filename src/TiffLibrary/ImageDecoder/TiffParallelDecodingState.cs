using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TiffLibrary.ImageDecoder
{
    internal class TiffParallelDecodingState
    {
        private int _workItemCount;
        private List<Exception>? _exceptions;
        private SpinLock _lock;

        public TaskCompletionSource<object?>? Complete { get; set; }
        public SemaphoreSlim? Semaphore { get; set; }

        public int IncrementWorkItemCount()
        {
            return Interlocked.Increment(ref _workItemCount);
        }

        public int DecrementWorkItemCount()
        {
            return Interlocked.Decrement(ref _workItemCount);
        }

        public void AddException(Exception e)
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
    }
}
