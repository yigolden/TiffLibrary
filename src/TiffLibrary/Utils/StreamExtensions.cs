using System.Buffers;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace System.IO
{
#if NO_FAST_SPAN
    internal static class StreamExtensions
    {
        public static async ValueTask WriteAsync(this Stream stream, ReadOnlyMemory<byte> memory)
        {
            if (MemoryMarshal.TryGetArray(memory, out ArraySegment<byte> segment))
            {
                await stream.WriteAsync(segment.Array, segment.Offset, segment.Count).ConfigureAwait(false);
            }
            else
            {
                // Optimize this
                byte[] buffer = ArrayPool<byte>.Shared.Rent(memory.Length);
                try
                {
                    memory.CopyTo(buffer);
                    await stream.WriteAsync(buffer, 0, memory.Length).ConfigureAwait(false);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
        }
    }
#endif
}
