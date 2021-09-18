#if NO_ASYNC_DISPOSABLE_ON_STREAM

using System.IO;
using System.Threading.Tasks;

namespace System
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal static class AsyncDisposableShim
    {
        public static ValueTask DisposeAsync(this Stream stream)
        {
            stream.Dispose();
            return default;
        }
    }
}

#endif
