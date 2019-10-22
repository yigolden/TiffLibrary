using System.IO;
using System.Threading.Tasks;

namespace System
{

#if NO_ASYNC_DISPOSABLE_INTERFACE
    internal interface IAsyncDisposable
    {
        ValueTask DisposeAsync();
    }

#endif

#if NO_ASYNC_DISPOSABLE_ON_STREAM
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
