using System;
using System.Threading;
using System.Threading.Tasks;

namespace TiffLibrary.ImageEncoder
{
    /// <summary>
    /// Provides methods to lock writer resources using a mutex object.
    /// </summary>
    public interface ITiffParallelMutexService
    {
        /// <summary>
        /// Acquire a mutex lock.
        /// </summary>
        /// The <see cref="CancellationToken"/> that fires if the user has requested to abort the decoding pipeline.
        /// <returns>The lock object.</returns>
        public Task<IDisposable> LockAsync(CancellationToken cancellationToken);
    }
}
