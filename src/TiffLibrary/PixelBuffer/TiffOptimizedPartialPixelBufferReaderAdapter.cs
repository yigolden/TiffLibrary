using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using TiffLibrary.Utils;

namespace TiffLibrary.PixelBuffer
{
    /// <summary>
    /// Uses <see cref="ITiffPixelBuffer{TPixel}"/> as the underlying storage. Provides <see cref="ITiffPixelBufferReader{TPixel}"/> API to read pixels from <see cref="ITiffPixelBuffer{TPixel}"/>.
    /// </summary>
    /// <typeparam name="TPixel"></typeparam>
    public class TiffOptimizedPartialPixelBufferReaderAdapter<TPixel> : ITiffPixelBufferReader<TPixel> where TPixel : unmanaged
    {
        private readonly IPartialBufferProvider<TPixel> _partialBufferProvider;
        private Rectangle _croppedRegion;

        /// <summary>
        /// Initialize the object to wrap <paramref name="partialBufferProvider"/>.
        /// </summary>
        /// <param name="partialBufferProvider">The chunked pixel buffer provider for partial access.</param>
        public TiffOptimizedPartialPixelBufferReaderAdapter(IPartialBufferProvider<TPixel> partialBufferProvider)
        {
            _partialBufferProvider = partialBufferProvider;
        }

        /// <inheritdoc />
        public int Width => _partialBufferProvider.ImageSize.Width;

        /// <inheritdoc />
        public int Height => _partialBufferProvider.ImageSize.Height;

        /// <inheritdoc />
        public ValueTask ReadAsync(TiffPoint offset, TiffPixelBufferWriter<TPixel> destination, CancellationToken cancellationToken)
        {
            if (offset.X >= (uint)_partialBufferProvider.ImageSize.Width || offset.Y >= (uint)_partialBufferProvider.ImageSize.Height)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(offset));
            }

            int width = Math.Min(_croppedRegion.Width - offset.X, destination.Width);
            int height = Math.Min(_croppedRegion.Height - offset.Y, destination.Height);

            var memoryBuffer = _partialBufferProvider.GetMemoryBuffer(_croppedRegion);
            var result = memoryBuffer.GetReadOnlySpan();

            int bufferWidth = _croppedRegion.Width;

            for (int row = 0; row < height; row++)
            {
                ReadOnlySpan<TPixel> sourceSpan = result.Slice(bufferWidth * (offset.Y + row) + offset.X, width);
                using TiffPixelSpanHandle<TPixel> destinationHandle = destination.GetRowSpan(row);
                sourceSpan.CopyTo(destinationHandle.GetSpan());
            }

            return default;
        }

        /// <summary>
        /// Save the cropped region input to be processed by the reader later.
        /// </summary>
        /// <param name="imageOffset"></param>
        /// <param name="imageSize"></param>
        public void CacheCroppedRegion(TiffPoint imageOffset, TiffSize imageSize)
        {
            _croppedRegion = new Rectangle() {X = imageOffset.X, Y = imageOffset.Y, Width = imageSize.Width, Height = imageSize.Height};
        }
    }
}
