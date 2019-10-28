using System.Threading;
using System.Threading.Tasks;

namespace TiffLibrary
{
    /// <summary>
    /// A decoder class to decode pixels from IFD.
    /// </summary>
    public abstract class TiffImageDecoder
    {
        /// <summary>
        /// The width of the image of the IFD.
        /// </summary>
        public abstract int Width { get; }

        /// <summary>
        /// The height of the image of the IFD.
        /// </summary>
        public abstract int Height { get; }

        /// <summary>
        /// Decode the image into the specified pixel buffer.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="offset">Number of columns and rows to skip in the source image.</param>
        /// <param name="readSize">Number of columns and rows to read from the source image.</param>
        /// <param name="destinationOffset">Number of columns and rows to skip in the destination writer.</param>
        /// <param name="writer">The pixel buffer writer to write pixels into.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort the decoding pipeline.</param>
        /// <returns>A <see cref="Task"/> that completes when the image has been decoded.</returns>
        public abstract Task DecodeAsync<TPixel>(TiffPoint offset, TiffSize readSize, TiffPoint destinationOffset, ITiffPixelBufferWriter<TPixel> writer, CancellationToken cancellationToken = default) where TPixel : unmanaged;
    }
}
