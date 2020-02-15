using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TiffLibrary.PixelBuffer;

namespace TiffLibrary.ImageSharpAdapter
{
    internal sealed class TiffPassthroughPixelBufferReader<TSource, TDestination> : ITiffPixelBufferReader<TDestination> where TSource : unmanaged where TDestination : unmanaged
    {
        private readonly ITiffPixelBufferReader<TSource> _source;

        public TiffPassthroughPixelBufferReader(ITiffPixelBufferReader<TSource> source)
        {
            Debug.Assert(Unsafe.SizeOf<TSource>() == Unsafe.SizeOf<TDestination>());
            _source = source;
        }

        public int Width => _source.Width;

        public int Height => _source.Height;

        public ValueTask ReadAsync(TiffPoint offset, TiffPixelBufferWriter<TDestination> destination, CancellationToken cancellationToken)
        {
            ITiffPixelBufferWriter<TDestination> destinationWriter = TiffPixelBufferUnsafeMarshal.GetBuffer(destination, out TiffPoint destinationOffset, out TiffSize size);
            TiffPixelBufferWriter<TSource> newDestination = new TiffPassthroughPixelBufferWriter<TSource, TDestination>(destinationWriter).AsPixelBufferWriter().Crop(destinationOffset, size);
            return _source.ReadAsync(offset, newDestination, cancellationToken);
        }
    }
}
