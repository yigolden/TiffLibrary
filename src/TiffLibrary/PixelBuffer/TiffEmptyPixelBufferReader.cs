using System.Threading;
using System.Threading.Tasks;

namespace TiffLibrary.PixelBuffer
{
    internal class TiffEmptyPixelBufferReader<TPixel> : ITiffPixelBufferReader<TPixel> where TPixel : unmanaged
    {
        public static ITiffPixelBufferReader<TPixel> Default { get; } = new TiffEmptyPixelBufferReader<TPixel>();

        public int Width => 0;

        public int Height => 0;

        public ValueTask ReadAsync(TiffPoint offset, TiffPixelBufferWriter<TPixel> destination, CancellationToken cancellationToken) => default;
    }
}
