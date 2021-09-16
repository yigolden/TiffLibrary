using System.Threading;
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
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort the encoding pipeline.</param>
        /// <returns>A <see cref="Task{TiffStreamRegion}"/> that completes and return the position and length written into the stream when the image has been encoded.</returns>
        public static Task<TiffStreamRegion> EncodeAsync<TPixel>(this TiffImageEncoder<TPixel> encoder, TiffFileWriter writer, ITiffPixelBufferReader<TPixel> reader, CancellationToken cancellationToken = default) where TPixel : unmanaged
        {
            ThrowHelper.ThrowIfNull(encoder);
            ThrowHelper.ThrowIfNull(writer);
            ThrowHelper.ThrowIfNull(reader);

            return encoder.EncodeAsync(writer, default, new TiffSize(reader.Width, reader.Height), reader, cancellationToken);
        }

        /// <summary>
        /// Encode an image as well as associated IFD fields into TIFF stream.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="encoder">The image encoder.</param>
        /// <param name="writer">The <see cref="TiffImageFileDirectoryWriter"/> object to write encoded image data and fields to.</param>
        /// <param name="reader">The pixel buffer reader object.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort the encoding pipeline.</param>
        /// <returns>A <see cref="Task"/> that completes when the image and fields have been encoded.</returns>
        public static Task EncodeAsync<TPixel>(this TiffImageEncoder<TPixel> encoder, TiffImageFileDirectoryWriter writer, ITiffPixelBufferReader<TPixel> reader, CancellationToken cancellationToken = default) where TPixel : unmanaged
        {
            ThrowHelper.ThrowIfNull(encoder);
            ThrowHelper.ThrowIfNull(writer);
            ThrowHelper.ThrowIfNull(reader);

            return encoder.EncodeAsync(writer, default, new TiffSize(reader.Width, reader.Height), reader, cancellationToken);
        }

        /// <summary>
        /// Encode a single image without writing any IFD fields.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="encoder">The image encoder.</param>
        /// <param name="writer">The <see cref="TiffFileWriter"/> instance of the output TIFF file.</param>
        /// <param name="reader">The pixel buffer reader object.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort the encoding pipeline.</param>
        /// <returns>A <see cref="Task{TiffStreamRegion}"/> that completes and return the position and length written into the stream when the image has been encoded.</returns>
        public static Task<TiffStreamRegion> EncodeAsync<TPixel>(this TiffImageEncoder<TPixel> encoder, TiffFileWriter writer, TiffPixelBufferReader<TPixel> reader, CancellationToken cancellationToken = default) where TPixel : unmanaged
        {
            ThrowHelper.ThrowIfNull(encoder);
            ThrowHelper.ThrowIfNull(writer);

            if (reader.IsEmpty)
            {
                ThrowHelper.ThrowArgumentException("No image data is provided.", nameof(reader));
            }
            ITiffPixelBufferReader<TPixel> innerReader = TiffPixelBufferUnsafeMarshal.GetBuffer(reader, out TiffPoint offset, out TiffSize size);
            return encoder.EncodeAsync(writer, offset, size, innerReader, cancellationToken);
        }

        /// <summary>
        /// Encode an image as well as associated IFD fields into TIFF stream.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="encoder">The image encoder.</param>
        /// <param name="writer">The <see cref="TiffImageFileDirectoryWriter"/> object to write encoded image data and fields to.</param>
        /// <param name="reader">The pixel buffer reader object.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort the encoding pipeline.</param>
        /// <returns>A <see cref="Task"/> that completes when the image and fields have been encoded.</returns>
        public static Task EncodeAsync<TPixel>(this TiffImageEncoder<TPixel> encoder, TiffImageFileDirectoryWriter writer, TiffPixelBufferReader<TPixel> reader, CancellationToken cancellationToken = default) where TPixel : unmanaged
        {
            ThrowHelper.ThrowIfNull(encoder);
            ThrowHelper.ThrowIfNull(writer);

            if (reader.IsEmpty)
            {
                ThrowHelper.ThrowArgumentException("No image data is provided.", nameof(reader));
            }
            ITiffPixelBufferReader<TPixel> innerReader = TiffPixelBufferUnsafeMarshal.GetBuffer(reader, out TiffPoint offset, out TiffSize size);
            return encoder.EncodeAsync(writer, offset, size, innerReader, cancellationToken);
        }

        /// <summary>
        /// Encode a single image without writing any IFD fields.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="encoder">The image encoder.</param>
        /// <param name="writer">The <see cref="TiffFileWriter"/> instance of the output TIFF file.</param>
        /// <param name="buffer">The pixel buffer </param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort the encoding pipeline.</param>
        /// <returns>A <see cref="Task{TiffStreamRegion}"/> that completes and return the position and length written into the stream when the image has been encoded.</returns>
        public static Task<TiffStreamRegion> EncodeAsync<TPixel>(this TiffImageEncoder<TPixel> encoder, TiffFileWriter writer, TiffPixelBuffer<TPixel> buffer, CancellationToken cancellationToken = default) where TPixel : unmanaged
        {
            ThrowHelper.ThrowIfNull(encoder);
            ThrowHelper.ThrowIfNull(writer);

            if (buffer.IsEmpty)
            {
                ThrowHelper.ThrowArgumentException("No image data is provided.", nameof(buffer));
            }
            ITiffPixelBuffer<TPixel> innerBuffer = TiffPixelBufferUnsafeMarshal.GetBuffer(buffer, out TiffPoint offset, out TiffSize size);
            var reader = new TiffPixelBufferReaderAdapter<TPixel>(innerBuffer);
            return encoder.EncodeAsync(writer, offset, size, reader, cancellationToken);
        }

        /// <summary>
        /// Encode an image as well as associated IFD fields into TIFF stream.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="encoder">The image encoder.</param>
        /// <param name="writer">The <see cref="TiffImageFileDirectoryWriter"/> object to write encoded image data and fields to.</param>
        /// <param name="buffer">The pixel buffer </param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort the encoding pipeline.</param>
        /// <returns>A <see cref="Task"/> that completes when the image and fields have been encoded.</returns>
        public static Task EncodeAsync<TPixel>(this TiffImageEncoder<TPixel> encoder, TiffImageFileDirectoryWriter writer, TiffPixelBuffer<TPixel> buffer, CancellationToken cancellationToken = default) where TPixel : unmanaged
        {
            ThrowHelper.ThrowIfNull(encoder);
            ThrowHelper.ThrowIfNull(writer);

            if (buffer.IsEmpty)
            {
                ThrowHelper.ThrowArgumentException("No image data is provided.", nameof(buffer));
            }
            ITiffPixelBuffer<TPixel> innerBuffer = TiffPixelBufferUnsafeMarshal.GetBuffer(buffer, out TiffPoint offset, out TiffSize size);
            var reader = new TiffPixelBufferReaderAdapter<TPixel>(innerBuffer);
            return encoder.EncodeAsync(writer, offset, size, reader, cancellationToken);
        }

        /// <summary>
        /// Encode a single image without writing any IFD fields.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="encoder">The image encoder.</param>
        /// <param name="writer">The <see cref="TiffFileWriter"/> instance of the output TIFF file.</param>
        /// <param name="buffer">The pixel buffer </param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort the encoding pipeline.</param>
        /// <returns>A <see cref="Task{TiffStreamRegion}"/> that completes and return the position and length written into the stream when the image has been encoded.</returns>
        public static Task<TiffStreamRegion> EncodeAsync<TPixel>(this TiffImageEncoder<TPixel> encoder, TiffFileWriter writer, ITiffPixelBuffer<TPixel> buffer, CancellationToken cancellationToken = default) where TPixel : unmanaged
        {
            ThrowHelper.ThrowIfNull(encoder);
            ThrowHelper.ThrowIfNull(writer);
            ThrowHelper.ThrowIfNull(buffer);

            return encoder.EncodeAsync(writer, default, new TiffSize(buffer.Width, buffer.Height), new TiffPixelBufferReaderAdapter<TPixel>(buffer), cancellationToken);
        }

        /// <summary>
        /// Encode an image as well as associated IFD fields into TIFF stream.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="encoder">The image encoder.</param>
        /// <param name="writer">The <see cref="TiffImageFileDirectoryWriter"/> object to write encoded image data and fields to.</param>
        /// <param name="buffer">The pixel buffer </param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort the encoding pipeline.</param>
        /// <returns>A <see cref="Task"/> that completes when the image and fields have been encoded.</returns>
        public static Task EncodeAsync<TPixel>(this TiffImageEncoder<TPixel> encoder, TiffImageFileDirectoryWriter writer, ITiffPixelBuffer<TPixel> buffer, CancellationToken cancellationToken = default) where TPixel : unmanaged
        {
            ThrowHelper.ThrowIfNull(encoder);
            ThrowHelper.ThrowIfNull(writer);
            ThrowHelper.ThrowIfNull(buffer);

            return encoder.EncodeAsync(writer, default, new TiffSize(buffer.Width, buffer.Height), new TiffPixelBufferReaderAdapter<TPixel>(buffer), cancellationToken);
        }

        /// <summary>
        /// Encode a single image without writing any IFD fields.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="encoder">The image encoder.</param>
        /// <param name="writer">The <see cref="TiffFileWriter"/> instance of the output TIFF file.</param>
        /// <param name="offset">The number of columns and rows to skip in <paramref name="buffer"/>.</param>
        /// <param name="size">The number of columns and rows to encode in <paramref name="buffer"/>.</param>
        /// <param name="buffer">The pixel buffer </param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort the encoding pipeline.</param>
        /// <returns>A <see cref="Task{TiffStreamRegion}"/> that completes and return the position and length written into the stream when the image has been encoded.</returns>
        public static Task<TiffStreamRegion> EncodeAsync<TPixel>(this TiffImageEncoder<TPixel> encoder, TiffFileWriter writer, TiffPoint offset, TiffSize size, ITiffPixelBuffer<TPixel> buffer, CancellationToken cancellationToken = default) where TPixel : unmanaged
        {
            ThrowHelper.ThrowIfNull(encoder);
            ThrowHelper.ThrowIfNull(writer);
            ThrowHelper.ThrowIfNull(buffer);

            return encoder.EncodeAsync(writer, offset, size, new TiffPixelBufferReaderAdapter<TPixel>(buffer), cancellationToken);
        }

        /// <summary>
        /// Encode an image as well as associated IFD fields into TIFF stream.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="encoder">The image encoder.</param>
        /// <param name="writer">The <see cref="TiffImageFileDirectoryWriter"/> object to write encoded image data and fields to.</param>
        /// <param name="offset">The number of columns and rows to skip in <paramref name="buffer"/>.</param>
        /// <param name="size">The number of columns and rows to encode in <paramref name="buffer"/>.</param>
        /// <param name="buffer">The pixel buffer </param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort the encoding pipeline.</param>
        /// <returns>A <see cref="Task"/> that completes when the image and fields have been encoded.</returns>
        public static Task EncodeAsync<TPixel>(this TiffImageEncoder<TPixel> encoder, TiffImageFileDirectoryWriter writer, TiffPoint offset, TiffSize size, ITiffPixelBuffer<TPixel> buffer, CancellationToken cancellationToken = default) where TPixel : unmanaged
        {
            ThrowHelper.ThrowIfNull(encoder);
            ThrowHelper.ThrowIfNull(writer);
            ThrowHelper.ThrowIfNull(buffer);

            return encoder.EncodeAsync(writer, offset, size, new TiffPixelBufferReaderAdapter<TPixel>(buffer), cancellationToken);
        }
    }
}
