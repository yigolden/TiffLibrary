using System;
using System.Threading.Tasks;
using TiffLibrary.PixelBuffer;

namespace TiffLibrary
{
    /// <summary>
    /// Provides extension methods for <see cref="TiffImageDecoder"/>.
    /// </summary>
    public static class TiffImageDecoderExtensions
    {
        /// <summary>
        /// Decode the image into the specified pixel buffer.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="decoder">The image decoder.</param>
        /// <param name="buffer">The pixel buffer to write pixels into.</param>
        /// <returns>A <see cref="Task"/> that completes when the image has been decoded.</returns>
        public static Task DecodeAsync<TPixel>(this TiffImageDecoder decoder, TiffPixelBuffer<TPixel> buffer) where TPixel : unmanaged
        {
            if (decoder is null)
            {
                throw new ArgumentNullException(nameof(decoder));
            }

            if (buffer.IsEmpty)
            {
                return Task.CompletedTask;
            }

            ITiffPixelBuffer<TPixel> innerBuffer = TiffPixelBufferUnsafeMarshal.GetBuffer(buffer, out TiffPoint offset, out TiffSize size);
            return decoder.DecodeAsync(default, size, offset, innerBuffer);
        }

        /// <summary>
        /// Decode the image into the specified pixel buffer.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="decoder">The image decoder.</param>
        /// <param name="offset">Number of columns and rows to skip in the source image.</param>
        /// <param name="buffer">The pixel buffer to write pixels into.</param>
        /// <returns>A <see cref="Task"/> that completes when the image has been decoded.</returns>
        public static Task DecodeAsync<TPixel>(this TiffImageDecoder decoder, TiffPoint offset, TiffPixelBuffer<TPixel> buffer) where TPixel : unmanaged
        {
            if (decoder is null)
            {
                throw new ArgumentNullException(nameof(decoder));
            }

            if (buffer.IsEmpty)
            {
                return Task.CompletedTask;
            }

            ITiffPixelBuffer<TPixel> innerBuffer = TiffPixelBufferUnsafeMarshal.GetBuffer(buffer, out TiffPoint destinationOffset, out TiffSize size);
            return decoder.DecodeAsync(offset, size, destinationOffset, innerBuffer);
        }


        /// <summary>
        /// Decode the image into the specified pixel buffer.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="decoder">The image decoder.</param>
        /// <param name="buffer">The pixel buffer to write pixels into.</param>
        /// <returns>A <see cref="Task"/> that completes when the image has been decoded.</returns>
        public static async Task DecodeAsync<TPixel>(this TiffImageDecoder decoder, ITiffPixelBuffer<TPixel> buffer) where TPixel : unmanaged
        {
            if (decoder is null)
            {
                throw new ArgumentNullException(nameof(decoder));
            }

            if (buffer is null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            using var adapter = new TiffPixelBufferWriterAdapter<TPixel>(buffer);
            await decoder.DecodeAsync(default, new TiffSize(buffer.Width, buffer.Height), default, adapter).ConfigureAwait(false);
        }

        /// <summary>
        /// Decode the image into the specified pixel buffer.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="decoder">The image decoder.</param>
        /// <param name="offset">Number of columns and rows to skip in the source image.</param>
        /// <param name="buffer">The pixel buffer to write pixels into.</param>
        /// <returns>A <see cref="Task"/> that completes when the image has been decoded.</returns>
        public static async Task DecodeAsync<TPixel>(this TiffImageDecoder decoder, TiffPoint offset, ITiffPixelBuffer<TPixel> buffer) where TPixel : unmanaged
        {
            if (decoder is null)
            {
                throw new ArgumentNullException(nameof(decoder));
            }

            if (buffer is null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            using var adapter = new TiffPixelBufferWriterAdapter<TPixel>(buffer);
            await decoder.DecodeAsync(offset, new TiffSize(buffer.Width, buffer.Height), default, adapter).ConfigureAwait(false);
        }

        /// <summary>
        /// Decode the image into the specified pixel buffer.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="decoder">The image decoder.</param>
        /// <param name="offset">Number of columns and rows to skip in the source image.</param>
        /// <param name="readSize">Number of columns and rows to read from the source image.</param>
        /// <param name="buffer">The pixel buffer to write pixels into.</param>
        /// <returns>A <see cref="Task"/> that completes when the image has been decoded.</returns>
        public static async Task DecodeAsync<TPixel>(this TiffImageDecoder decoder, TiffPoint offset, TiffSize readSize, ITiffPixelBuffer<TPixel> buffer) where TPixel : unmanaged
        {
            if (decoder is null)
            {
                throw new ArgumentNullException(nameof(decoder));
            }

            if (buffer is null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            using var adapter = new TiffPixelBufferWriterAdapter<TPixel>(buffer);
            await decoder.DecodeAsync(offset, readSize, default, adapter).ConfigureAwait(false);
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
        /// <returns>A <see cref="Task"/> that completes when the image has been decoded.</returns>
        public static Task DecodeAsync<TPixel>(this TiffImageDecoder decoder, TiffPoint offset, TiffSize readSize, TiffPoint destinationOffset, ITiffPixelBuffer<TPixel> buffer) where TPixel : unmanaged
        {
            if (decoder is null)
            {
                throw new ArgumentNullException(nameof(decoder));
            }

            if (buffer is null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            return decoder.DecodeAsync(offset, readSize, destinationOffset, new TiffPixelBufferWriterAdapter<TPixel>(buffer));
        }

        /// <summary>
        /// Decode the image into the specified pixel buffer.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="decoder">The image decoder.</param>
        /// <param name="writer">The pixel buffer writer to write pixels into.</param>
        /// <returns>A <see cref="Task"/> that completes when the image has been decoded.</returns>
        public static Task DecodeAsync<TPixel>(this TiffImageDecoder decoder, TiffPixelBufferWriter<TPixel> writer) where TPixel : unmanaged
        {
            if (decoder is null)
            {
                throw new ArgumentNullException(nameof(decoder));
            }

            if (writer.IsEmpty)
            {
                return Task.CompletedTask;
            }

            ITiffPixelBufferWriter<TPixel> innerBuffer = TiffPixelBufferUnsafeMarshal.GetBuffer(writer, out TiffPoint offset, out TiffSize size);
            return decoder.DecodeAsync(default, size, offset, innerBuffer);
        }

        /// <summary>
        /// Decode the image into the specified pixel buffer.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="decoder">The image decoder.</param>
        /// <param name="offset">Number of columns and rows to skip in the source image.</param>
        /// <param name="readSize">Number of columns and rows to read from the source image.</param>
        /// <param name="writer">The pixel buffer writer to write pixels into.</param>
        /// <returns>A <see cref="Task"/> that completes when the image has been decoded.</returns>
        public static Task DecodeAsync<TPixel>(this TiffImageDecoder decoder, TiffPoint offset, TiffSize readSize, TiffPixelBufferWriter<TPixel> writer) where TPixel : unmanaged
        {
            if (decoder is null)
            {
                throw new ArgumentNullException(nameof(decoder));
            }

            if (writer.IsEmpty)
            {
                return Task.CompletedTask;
            }

            ITiffPixelBufferWriter<TPixel> innerBuffer = TiffPixelBufferUnsafeMarshal.GetBuffer(writer, out TiffPoint destinationOffset, out _);
            return decoder.DecodeAsync(offset, readSize, destinationOffset, innerBuffer);
        }

        /// <summary>
        /// Decode the image into the specified pixel buffer.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="decoder">The image decoder.</param>
        /// <param name="offset">Number of columns and rows to skip in the source image.</param>
        /// <param name="writer">The pixel buffer writer to write pixels into.</param>
        /// <returns>A <see cref="Task"/> that completes when the image has been decoded.</returns>
        public static Task DecodeAsync<TPixel>(this TiffImageDecoder decoder, TiffPoint offset, TiffPixelBufferWriter<TPixel> writer) where TPixel : unmanaged
        {
            if (decoder is null)
            {
                throw new ArgumentNullException(nameof(decoder));
            }

            if (writer.IsEmpty)
            {
                return Task.CompletedTask;
            }

            ITiffPixelBufferWriter<TPixel> innerBuffer = TiffPixelBufferUnsafeMarshal.GetBuffer(writer, out TiffPoint destinationOffset, out TiffSize size);
            return decoder.DecodeAsync(offset, size, destinationOffset, innerBuffer);
        }

        /// <summary>
        /// Decode the image into the specified pixel buffer.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="decoder">The image decoder.</param>
        /// <param name="writer">The pixel buffer writer to write pixels into.</param>
        /// <returns>A <see cref="Task"/> that completes when the image has been decoded.</returns>
        public static Task DecodeAsync<TPixel>(this TiffImageDecoder decoder, ITiffPixelBufferWriter<TPixel> writer) where TPixel : unmanaged
        {
            if (decoder is null)
            {
                throw new ArgumentNullException(nameof(decoder));
            }

            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            return decoder.DecodeAsync(default, new TiffSize(writer.Width, writer.Height), default, writer);
        }
    }
}
