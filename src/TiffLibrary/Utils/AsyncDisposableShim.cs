using System.IO;
using System.Threading.Tasks;

namespace System
{

#if NO_ASYNC_DISPOSABLE_INTERFACE
    internal interface IAsyncDisposable
    {
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        ValueTask DisposeAsync();
    }

#endif

#if NO_ASYNC_DISPOSABLE_ON_STREAM
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal static class AsyncDisposableShim
    {
        public static ValueTask DisposeAsync(this Stream stream)
        {
            stream.Dispose();
            return default;
        }
    }
#endif

}
