using System;
using System.Threading;
using System.Threading.Tasks;
using TiffLibrary.PixelBuffer;

namespace TiffLibrary
{
    /// <summary>
    /// Provides extension methods for <see cref="TiffImageDecoder"/>.
    /// </summary>
    public static partial class TiffImageDecoderExtensions
    {

        #region TiffPixelBuffer

        /// <summary>
        /// Decode the image into the specified pixel buffer.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="decoder">The image decoder.</param>
        /// <param name="buffer">The pixel buffer to write pixels into.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort the decoding pipeline.</param>
        /// <returns>A <see cref="Task"/> that completes when the image has been decoded.</returns>
        public static Task DecodeAsync<TPixel>(this TiffImageDecoder decoder, TiffPixelBuffer<TPixel> buffer, CancellationToken cancellationToken = default) where TPixel : unmanaged
        {
            ThrowHelper.ThrowIfNull(decoder);
            if (buffer.IsEmpty)
            {
                return Task.CompletedTask;
            }

            ITiffPixelBuffer<TPixel> innerBuffer = TiffPixelBufferUnsafeMarshal.GetBuffer(buffer, out TiffPoint offset, out TiffSize size);
            return decoder.DecodeAsync(default, size, offset, innerBuffer, cancellationToken);
        }

        /// <summary>
        /// Decode the image into the specified pixel buffer.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="decoder">The image decoder.</param>
        /// <param name="offset">Number of columns and rows to skip in the source image.</param>
        /// <param name="buffer">The pixel buffer to write pixels into.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort the decoding pipeline.</param>
        /// <returns>A <see cref="Task"/> that completes when the image has been decoded.</returns>
        public static Task DecodeAsync<TPixel>(this TiffImageDecoder decoder, TiffPoint offset, TiffPixelBuffer<TPixel> buffer, CancellationToken cancellationToken = default) where TPixel : unmanaged
        {
            ThrowHelper.ThrowIfNull(decoder);
            if (buffer.IsEmpty)
            {
                return Task.CompletedTask;
            }

            ITiffPixelBuffer<TPixel> innerBuffer = TiffPixelBufferUnsafeMarshal.GetBuffer(buffer, out TiffPoint destinationOffset, out TiffSize size);
            return decoder.DecodeAsync(offset, size, destinationOffset, innerBuffer, cancellationToken);
        }

        /// <summary>
        /// Decode the image into the specified pixel buffer.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="decoder">The image decoder.</param>
        /// <param name="offset">Number of columns and rows to skip in the source image.</param>
        /// <param name="readSize">Number of columns and rows to read from the source image.</param>
        /// <param name="buffer">The pixel buffer to write pixels into.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort the decoding pipeline.</param>
        /// <returns>A <see cref="Task"/> that completes when the image has been decoded.</returns>
        public static Task DecodeAsync<TPixel>(this TiffImageDecoder decoder, TiffPoint offset, TiffSize readSize, TiffPixelBuffer<TPixel> buffer, CancellationToken cancellationToken = default) where TPixel : unmanaged
        {
            ThrowHelper.ThrowIfNull(decoder);
            if (buffer.IsEmpty)
            {
                return Task.CompletedTask;
            }

            ITiffPixelBuffer<TPixel> innerBuffer = TiffPixelBufferUnsafeMarshal.GetBuffer(buffer, out TiffPoint bufferDestinationOffset, out TiffSize bufferSize);
            readSize = new TiffSize(Math.Min(readSize.Width, bufferSize.Width), Math.Min(readSize.Height, bufferSize.Height));
            return decoder.DecodeAsync(offset, readSize, bufferDestinationOffset, innerBuffer, cancellationToken);
        }

        /// <summary>
        /// Decode the image into the specified pixel buffer.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="decoder">The image decoder.</param>
        /// <param name="offset">Number of columns and rows to skip in the source image.</param>
        /// <param name="readSize">Number of columns and rows to read from the source image.</param>
        /// <param name="destinationOffset">Number of columns and rows to skip in the destination writer.</param>
        /// <param name="buffer">The pixel buffer to write pixels into.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort the decoding pipeline.</param>
        /// <returns>A <see cref="Task"/> that completes when the image has been decoded.</returns>
        public static Task DecodeAsync<TPixel>(this TiffImageDecoder decoder, TiffPoint offset, TiffSize readSize, TiffPoint destinationOffset, TiffPixelBuffer<TPixel> buffer, CancellationToken cancellationToken = default) where TPixel : unmanaged
        {
            ThrowHelper.ThrowIfNull(decoder);
            if (buffer.IsEmpty)
            {
                return Task.CompletedTask;
            }

            // We don't allow negative destinationOffset here. Because it may cause pixels to be written outside the bounds defined by the writer struct.
            if (destinationOffset.X < 0)
            {
                offset = new TiffPoint(offset.X - destinationOffset.X, offset.Y);
                readSize = new TiffSize(readSize.Width + destinationOffset.X, readSize.Height);
                destinationOffset = new TiffPoint(0, destinationOffset.Y);
            }
            if (destinationOffset.Y < 0)
            {
                offset = new TiffPoint(offset.X, offset.Y - destinationOffset.Y);
                readSize = new TiffSize(readSize.Width, readSize.Height + destinationOffset.Y);
                destinationOffset = new TiffPoint(destinationOffset.X, 0);
            }

            ITiffPixelBuffer<TPixel> innerBuffer = TiffPixelBufferUnsafeMarshal.GetBuffer(buffer, out TiffPoint bufferDestinationOffset, out TiffSize bufferSize);
            readSize = new TiffSize(Math.Min(readSize.Width, bufferSize.Width), Math.Min(readSize.Height, bufferSize.Height));
            return decoder.DecodeAsync(offset, readSize, new TiffPoint(destinationOffset.X + bufferDestinationOffset.X, destinationOffset.Y + bufferDestinationOffset.Y), innerBuffer, cancellationToken);
        }

        #endregion

        #region ITiffPixelBuffer

        /// <summary>
        /// Decode the image into the specified pixel buffer.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="decoder">The image decoder.</param>
        /// <param name="buffer">The pixel buffer to write pixels into.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort the decoding pipeline.</param>
        /// <returns>A <see cref="Task"/> that completes when the image has been decoded.</returns>
        public static async Task DecodeAsync<TPixel>(this TiffImageDecoder decoder, ITiffPixelBuffer<TPixel> buffer, CancellationToken cancellationToken = default) where TPixel : unmanaged
        {
            ThrowHelper.ThrowIfNull(decoder);
            ThrowHelper.ThrowIfNull(buffer);

            using var adapter = new TiffPixelBufferWriterAdapter<TPixel>(buffer);
            await decoder.DecodeAsync(default, new TiffSize(buffer.Width, buffer.Height), default, adapter, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Decode the image into the specified pixel buffer.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="decoder">The image decoder.</param>
        /// <param name="offset">Number of columns and rows to skip in the source image.</param>
        /// <param name="buffer">The pixel buffer to write pixels into.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort the decoding pipeline.</param>
        /// <returns>A <see cref="Task"/> that completes when the image has been decoded.</returns>
        public static async Task DecodeAsync<TPixel>(this TiffImageDecoder decoder, TiffPoint offset, ITiffPixelBuffer<TPixel> buffer, CancellationToken cancellationToken = default) where TPixel : unmanaged
        {
            ThrowHelper.ThrowIfNull(decoder);
            ThrowHelper.ThrowIfNull(buffer);

            using var adapter = new TiffPixelBufferWriterAdapter<TPixel>(buffer);
            await decoder.DecodeAsync(offset, new TiffSize(buffer.Width, buffer.Height), default, adapter, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Decode the image into the specified pixel buffer.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="decoder">The image decoder.</param>
        /// <param name="offset">Number of columns and rows to skip in the source image.</param>
        /// <param name="readSize">Number of columns and rows to read from the source image.</param>
        /// <param name="buffer">The pixel buffer to write pixels into.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort the decoding pipeline.</param>
        /// <returns>A <see cref="Task"/> that completes when the image has been decoded.</returns>
        public static async Task DecodeAsync<TPixel>(this TiffImageDecoder decoder, TiffPoint offset, TiffSize readSize, ITiffPixelBuffer<TPixel> buffer, CancellationToken cancellationToken = default) where TPixel : unmanaged
        {
            ThrowHelper.ThrowIfNull(decoder);
            ThrowHelper.ThrowIfNull(buffer);

            using var adapter = new TiffPixelBufferWriterAdapter<TPixel>(buffer);
            await decoder.DecodeAsync(offset, readSize, default, adapter, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Decode the image into the specified pixel buffer.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="decoder">The image decoder.</param>
        /// <param name="offset">Number of columns and rows to skip in the source image.</param>
        /// <param name="readSize">Number of columns and rows to read from the source image.</param>
        /// <param name="destinationOffset">Number of columns and rows to skip in the destination writer.</param>
        /// <param name="buffer">The pixel buffer to write pixels into.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort the decoding pipeline.</param>
        /// <returns>A <see cref="Task"/> that completes when the image has been decoded.</returns>
        public static Task DecodeAsync<TPixel>(this TiffImageDecoder decoder, TiffPoint offset, TiffSize readSize, TiffPoint destinationOffset, ITiffPixelBuffer<TPixel> buffer, CancellationToken cancellationToken = default) where TPixel : unmanaged
        {
            ThrowHelper.ThrowIfNull(decoder);
            ThrowHelper.ThrowIfNull(buffer);

            return decoder.DecodeAsync(offset, readSize, destinationOffset, new TiffPixelBufferWriterAdapter<TPixel>(buffer), cancellationToken);
        }

        #endregion

        #region TiffPixelBufferWriter

        /// <summary>
        /// Decode the image into the specified pixel buffer.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="decoder">The image decoder.</param>
        /// <param name="writer">The pixel buffer writer to write pixels into.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort the decoding pipeline.</param>
        /// <returns>A <see cref="Task"/> that completes when the image has been decoded.</returns>
        public static Task DecodeAsync<TPixel>(this TiffImageDecoder decoder, TiffPixelBufferWriter<TPixel> writer, CancellationToken cancellationToken = default) where TPixel : unmanaged
        {
            ThrowHelper.ThrowIfNull(decoder);

            if (writer.IsEmpty)
            {
                return Task.CompletedTask;
            }

            ITiffPixelBufferWriter<TPixel> innerBuffer = TiffPixelBufferUnsafeMarshal.GetBuffer(writer, out TiffPoint offset, out TiffSize size);
            return decoder.DecodeAsync(default, size, offset, innerBuffer, cancellationToken);
        }

        /// <summary>
        /// Decode the image into the specified pixel buffer.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="decoder">The image decoder.</param>
        /// <param name="offset">Number of columns and rows to skip in the source image.</param>
        /// <param name="writer">The pixel buffer writer to write pixels into.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort the decoding pipeline.</param>
        /// <returns>A <see cref="Task"/> that completes when the image has been decoded.</returns>
        public static Task DecodeAsync<TPixel>(this TiffImageDecoder decoder, TiffPoint offset, TiffPixelBufferWriter<TPixel> writer, CancellationToken cancellationToken = default) where TPixel : unmanaged
        {
            ThrowHelper.ThrowIfNull(decoder);

            if (writer.IsEmpty)
            {
                return Task.CompletedTask;
            }

            ITiffPixelBufferWriter<TPixel> innerBuffer = TiffPixelBufferUnsafeMarshal.GetBuffer(writer, out TiffPoint destinationOffset, out TiffSize size);
            return decoder.DecodeAsync(offset, size, destinationOffset, innerBuffer, cancellationToken);
        }

        /// <summary>
        /// Decode the image into the specified pixel buffer.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="decoder">The image decoder.</param>
        /// <param name="offset">Number of columns and rows to skip in the source image.</param>
        /// <param name="readSize">Number of columns and rows to read from the source image.</param>
        /// <param name="writer">The pixel buffer writer to write pixels into.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort the decoding pipeline.</param>
        /// <returns>A <see cref="Task"/> that completes when the image has been decoded.</returns>
        public static Task DecodeAsync<TPixel>(this TiffImageDecoder decoder, TiffPoint offset, TiffSize readSize, TiffPixelBufferWriter<TPixel> writer, CancellationToken cancellationToken = default) where TPixel : unmanaged
        {
            ThrowHelper.ThrowIfNull(decoder);

            if (writer.IsEmpty)
            {
                return Task.CompletedTask;
            }

            ITiffPixelBufferWriter<TPixel> innerBuffer = TiffPixelBufferUnsafeMarshal.GetBuffer(writer, out TiffPoint destinationOffset, out _);
            return decoder.DecodeAsync(offset, readSize, destinationOffset, innerBuffer, cancellationToken);
        }

        /// <summary>
        /// Decode the image into the specified pixel buffer.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="decoder">The image decoder.</param>
        /// <param name="offset">Number of columns and rows to skip in the source image.</param>
        /// <param name="readSize">Number of columns and rows to read from the source image.</param>
        /// <param name="destinationOffset">Number of columns and rows to skip in the destination writer.</param>
        /// <param name="writer">The pixel buffer writer to write pixels into.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort the decoding pipeline.</param>
        /// <returns>A <see cref="Task"/> that completes when the image has been decoded.</returns>
        public static Task DecodeAsync<TPixel>(this TiffImageDecoder decoder, TiffPoint offset, TiffSize readSize, TiffPoint destinationOffset, TiffPixelBufferWriter<TPixel> writer, CancellationToken cancellationToken = default) where TPixel : unmanaged
        {
            ThrowHelper.ThrowIfNull(decoder);

            if (writer.IsEmpty)
            {
                return Task.CompletedTask;
            }

            // We don't allow negative destinationOffset here. Because it may cause pixels to be written outside the bounds defined by the writer struct.
            if (destinationOffset.X < 0)
            {
                offset = new TiffPoint(offset.X - destinationOffset.X, offset.Y);
                readSize = new TiffSize(readSize.Width + destinationOffset.X, readSize.Height);
                destinationOffset = new TiffPoint(0, destinationOffset.Y);
            }
            if (destinationOffset.Y < 0)
            {
                offset = new TiffPoint(offset.X, offset.Y - destinationOffset.Y);
                readSize = new TiffSize(readSize.Width, readSize.Height + destinationOffset.Y);
                destinationOffset = new TiffPoint(destinationOffset.X, 0);
            }

            ITiffPixelBufferWriter<TPixel> innerBuffer = TiffPixelBufferUnsafeMarshal.GetBuffer(writer, out TiffPoint writerDestinationOffset, out TiffSize writerSize);
            readSize = new TiffSize(Math.Min(readSize.Width, writerSize.Width), Math.Min(readSize.Height, writerSize.Height));
            return decoder.DecodeAsync(offset, readSize, new TiffPoint(destinationOffset.X + writerDestinationOffset.X, destinationOffset.Y + writerDestinationOffset.Y), innerBuffer, cancellationToken);
        }

        #endregion

        #region ITiffPixelBufferWriter

        /// <summary>
        /// Decode the image into the specified pixel buffer.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="decoder">The image decoder.</param>
        /// <param name="writer">The pixel buffer writer to write pixels into.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort the decoding pipeline.</param>
        /// <returns>A <see cref="Task"/> that completes when the image has been decoded.</returns>
        public static Task DecodeAsync<TPixel>(this TiffImageDecoder decoder, ITiffPixelBufferWriter<TPixel> writer, CancellationToken cancellationToken = default) where TPixel : unmanaged
        {
            ThrowHelper.ThrowIfNull(decoder);
            ThrowHelper.ThrowIfNull(writer);

            return decoder.DecodeAsync(default, new TiffSize(writer.Width, writer.Height), default, writer, cancellationToken);
        }

        /// <summary>
        /// Decode the image into the specified pixel buffer writer.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="decoder">The image decoder.</param>
        /// <param name="offset">Number of columns and rows to skip in the source image.</param>
        /// <param name="writer">The pixel buffer writer to write pixels into.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort the decoding pipeline.</param>
        /// <returns>A <see cref="Task"/> that completes when the image has been decoded.</returns>
        public static Task DecodeAsync<TPixel>(this TiffImageDecoder decoder, TiffPoint offset, ITiffPixelBufferWriter<TPixel> writer, CancellationToken cancellationToken = default) where TPixel : unmanaged
        {
            ThrowHelper.ThrowIfNull(decoder);
            ThrowHelper.ThrowIfNull(writer);

            return decoder.DecodeAsync(offset, new TiffSize(writer.Width, writer.Height), default, writer, cancellationToken);
        }

        /// <summary>
        /// Decode the image into the specified pixel buffer writer.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="decoder">The image decoder.</param>
        /// <param name="offset">Number of columns and rows to skip in the source image.</param>
        /// <param name="readSize">Number of columns and rows to read from the source image.</param>
        /// <param name="writer">The pixel buffer writer to write pixels into.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort the decoding pipeline.</param>
        /// <returns>A <see cref="Task"/> that completes when the image has been decoded.</returns>
        public static Task DecodeAsync<TPixel>(this TiffImageDecoder decoder, TiffPoint offset, TiffSize readSize, ITiffPixelBufferWriter<TPixel> writer, CancellationToken cancellationToken = default) where TPixel : unmanaged
        {
            ThrowHelper.ThrowIfNull(decoder);

            return decoder.DecodeAsync(offset, readSize, default, writer, cancellationToken);
        }

        #endregion
    }
}
