using System;
using System.Threading.Tasks;

namespace TiffLibrary.ImageEncoder
{
    internal static class TiffImageEncoderContextMutexHelper
    {
        public static async Task<IDisposable> LockAsync<T>(this TiffImageEncoderContext<T> context) where T : unmanaged
        {
            ThrowHelper.ThrowIfNull(context);

            ITiffParallelMutexService? mutexService = context.GetService(typeof(ITiffParallelMutexService)) as ITiffParallelMutexService;
            if (mutexService is null)
            {
                return NullDisposable.Instance;
            }
            return await mutexService.LockAsync(context.CancellationToken).ConfigureAwait(false);
        }


        class NullDisposable : IDisposable
        {
            public static IDisposable Instance { get; } = new NullDisposable();
            public void Dispose() { }
        }
    }
}
