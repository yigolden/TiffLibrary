using System.Buffers;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.IO
{
#if NO_FAST_SPAN
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal static class StreamExtensions
    {
        public static ValueTask WriteAsync(this Stream stream, ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (MemoryMarshal.TryGetArray(buffer, out ArraySegment<byte> array))
            {
                return new ValueTask(stream.WriteAsync(array.Array, array.Offset, array.Count, cancellationToken));
            }
            else
            {
                byte[] sharedBuffer = ArrayPool<byte>.Shared.Rent(buffer.Length);
                buffer.Span.CopyTo(sharedBuffer);
                return new ValueTask(FinishWriteAsync(stream.WriteAsync(sharedBuffer, 0, buffer.Length, cancellationToken), sharedBuffer));
            }
        }

        private static async Task FinishWriteAsync(Task writeTask, byte[] localBuffer)
        {
            try
            {
                await writeTask.ConfigureAwait(false);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(localBuffer);
            }
        }
    }
#endif
}
