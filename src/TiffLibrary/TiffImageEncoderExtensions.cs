using System;
using System.Threading.Tasks;
using TiffLibrary.PixelBuffer;

namespace TiffLibrary
{
    /// <summary>
    /// Provides extension methods for <see cref="TiffImageEncoder{TPixel}"/>.
    /// </summary>
    public static class TiffImageEncoderExtensions
    {
        /// <summary>
        /// Encode a single image without writing any IFD fields.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="encoder">The image encoder.</param>
        /// <param name="writer">The <see cref="TiffFileWriter"/> instance of the output TIFF file.</param>
        /// <param name="reader">The pixel buffer reader object.</param>
        /// <returns>A <see cref="Task{TiffStreamRegion}"/> that completes and return the position and length written into the stream when the image has been encoded.</returns>
        public static Task<TiffStreamRegion> EncodeAsync<TPixel>(this TiffImageEncoder<TPixel> encoder, TiffFileWriter writer, TiffPixelBufferReader<TPixel> reader) where TPixel : unmanaged
        {
            if (encoder is null)
            {
                throw new ArgumentNullException(nameof(encoder));
            }
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }
            if (reader.IsEmpty)
            {
                throw new ArgumentException("No image data is provided.", nameof(reader));
            }
            ITiffPixelBufferReader<TPixel> innerReader = TiffPixelBufferUnsafeMarshal.GetBuffer(reader, out TiffPoint offset, out TiffSize size);
            return encoder.EncodeAsync(writer, offset, size, innerReader);
        }

        /// <summary>
        /// Encode an image as well as associated IFD fields into TIFF stream.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="encoder">The image encoder.</param>
        /// <param name="writer">The <see cref="TiffImageFileDirectoryWriter"/> object to write encoded image data and fields to.</param>
        /// <param name="reader">The pixel buffer reader object.</param>
        /// <returns>A <see cref="Task"/> that completes when the image and fields have been encoded.</returns>
        public static Task EncodeAsync<TPixel>(this TiffImageEncoder<TPixel> encoder, TiffImageFileDirectoryWriter writer, TiffPixelBufferReader<TPixel> reader) where TPixel : unmanaged
        {
            if (encoder is null)
            {
                throw new ArgumentNullException(nameof(encoder));
            }
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }
            if (reader.IsEmpty)
            {
                throw new ArgumentException("No image data is provided.", nameof(reader));
            }
            ITiffPixelBufferReader<TPixel> innerReader = TiffPixelBufferUnsafeMarshal.GetBuffer(reader, out TiffPoint offset, out TiffSize size);
            return encoder.EncodeAsync(writer, offset, size, innerReader);
        }

        /// <summary>
        /// Encode a single image without writing any IFD fields.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="encoder">The image encoder.</param>
        /// <param name="writer">The <see cref="TiffFileWriter"/> instance of the output TIFF file.</param>
        /// <param name="buffer">The pixel buffer </param>
        /// <returns>A <see cref="Task{TiffStreamRegion}"/> that completes and return the position and length written into the stream when the image has been encoded.</returns>
        public static Task<TiffStreamRegion> EncodeAsync<TPixel>(this TiffImageEncoder<TPixel> encoder, TiffFileWriter writer, TiffPixelBuffer<TPixel> buffer) where TPixel : unmanaged
        {
            if (encoder is null)
            {
                throw new ArgumentNullException(nameof(encoder));
            }
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }
            if (buffer.IsEmpty)
            {
                throw new ArgumentException("No image data is provided.", nameof(buffer));
            }
            ITiffPixelBuffer<TPixel> innerBuffer = TiffPixelBufferUnsafeMarshal.GetBuffer(buffer, out TiffPoint offset, out TiffSize size);
            var reader = new TiffPixelBufferReaderAdapter<TPixel>(innerBuffer);
            return encoder.EncodeAsync(writer, offset, size, reader);
        }

        /// <summary>
        /// Encode an image as well as associated IFD fields into TIFF stream.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="encoder">The image encoder.</param>
        /// <param name="writer">The <see cref="TiffImageFileDirectoryWriter"/> object to write encoded image data and fields to.</param>
        /// <param name="buffer">The pixel buffer </param>
        /// <returns>A <see cref="Task"/> that completes when the image and fields have been encoded.</returns>
        public static Task EncodeAsync<TPixel>(this TiffImageEncoder<TPixel> encoder, TiffImageFileDirectoryWriter writer, TiffPixelBuffer<TPixel> buffer) where TPixel : unmanaged
        {
            if (encoder is null)
            {
                throw new ArgumentNullException(nameof(encoder));
            }
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }
            if (buffer.IsEmpty)
            {
                throw new ArgumentException("No image data is provided.", nameof(buffer));
            }
            ITiffPixelBuffer<TPixel> innerBuffer = TiffPixelBufferUnsafeMarshal.GetBuffer(buffer, out TiffPoint offset, out TiffSize size);
            var reader = new TiffPixelBufferReaderAdapter<TPixel>(innerBuffer);
            return encoder.EncodeAsync(writer, offset, size, reader);
        }

        /// <summary>
        /// Encode a single image without writing any IFD fields.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="encoder">The image encoder.</param>
        /// <param name="writer">The <see cref="TiffFileWriter"/> instance of the output TIFF file.</param>
        /// <param name="buffer">The pixel buffer </param>
        /// <returns>A <see cref="Task{TiffStreamRegion}"/> that completes and return the position and length written into the stream when the image has been encoded.</returns>
        public static Task<TiffStreamRegion> EncodeAsync<TPixel>(this TiffImageEncoder<TPixel> encoder, TiffFileWriter writer, ITiffPixelBuffer<TPixel> buffer) where TPixel : unmanaged
            => EncodeAsync(encoder, writer, buffer.AsPixelBuffer());

        /// <summary>
        /// Encode an image as well as associated IFD fields into TIFF stream.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="encoder">The image encoder.</param>
        /// <param name="writer">The <see cref="TiffImageFileDirectoryWriter"/> object to write encoded image data and fields to.</param>
        /// <param name="buffer">The pixel buffer </param>
        /// <returns>A <see cref="Task"/> that completes when the image and fields have been encoded.</returns>
        public static Task EncodeAsync<TPixel>(this TiffImageEncoder<TPixel> encoder, TiffImageFileDirectoryWriter writer, ITiffPixelBuffer<TPixel> buffer) where TPixel : unmanaged
            => EncodeAsync(encoder, writer, buffer.AsPixelBuffer());

    }
}
