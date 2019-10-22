using System.Threading.Tasks;

namespace TiffLibrary
{
    /// <summary>
    /// An encoder class to encode pixel data into TIFF stream.
    /// </summary>
    /// <typeparam name="TPixel">The pixel type.</typeparam>
    public abstract class TiffImageEncoder<TPixel> where TPixel : unmanaged
    {
        /// <summary>
        /// Encode a single image without writing any IFD fields.
        /// </summary>
        /// <param name="writer">The <see cref="TiffFileWriter"/> object to write encoded image data to.</param>
        /// <param name="offset">The number of columns and rows to skip in <paramref name="reader"/>.</param>
        /// <param name="size">The number of columns and rows to encode in <paramref name="reader"/>.</param>
        /// <param name="reader">The pixel buffer reader object.</param>
        /// <returns>A <see cref="Task{TiffStreamRegion}"/> that completes and return the position and length written into the stream when the image has been encoded.</returns>
        public abstract Task<TiffStreamRegion> EncodeAsync(TiffFileWriter writer, TiffPoint offset, TiffSize size, ITiffPixelBufferReader<TPixel> reader);

        /// <summary>
        /// Encode an image as well as associated IFD fields into TIFF stream.
        /// </summary>
        /// <param name="writer">The <see cref="TiffImageFileDirectoryWriter"/> object to write encoded image data and fields to.</param>
        /// <param name="offset">The number of columns and rows to skip in <paramref name="reader"/>.</param>
        /// <param name="size">The number of columns and rows to encode in <paramref name="reader"/>.</param>
        /// <param name="reader">The pixel buffer reader object.</param>
        /// <returns>A <see cref="Task"/> that completes when the image and fields have been encoded.</returns>
        public abstract Task EncodeAsync(TiffImageFileDirectoryWriter writer, TiffPoint offset, TiffSize size, ITiffPixelBufferReader<TPixel> reader);
    }
}
