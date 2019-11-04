using System;
using TiffLibrary.PixelBuffer;

namespace TiffLibrary
{
    public static partial class TiffImageDecoderExtensions
    {
        /// <summary>
        /// Decode the image into the specified pixel buffer.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="decoder">The image decoder.</param>
        /// <param name="buffer">The pixel buffer to write pixels into.</param>
        public static void Decode<TPixel>(this TiffImageDecoder decoder, TiffPixelBuffer<TPixel> buffer) where TPixel : unmanaged
        {
            if (decoder is null)
            {
                throw new ArgumentNullException(nameof(decoder));
            }

            if (buffer.IsEmpty)
            {
                return;
            }

            ITiffPixelBuffer<TPixel> innerBuffer = TiffPixelBufferUnsafeMarshal.GetBuffer(buffer, out TiffPoint offset, out TiffSize size);
            decoder.Decode(default, size, offset, innerBuffer);
        }

        /// <summary>
        /// Decode the image into the specified pixel buffer.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="decoder">The image decoder.</param>
        /// <param name="offset">Number of columns and rows to skip in the source image.</param>
        /// <param name="buffer">The pixel buffer to write pixels into.</param>
        public static void Decode<TPixel>(this TiffImageDecoder decoder, TiffPoint offset, TiffPixelBuffer<TPixel> buffer) where TPixel : unmanaged
        {
            if (decoder is null)
            {
                throw new ArgumentNullException(nameof(decoder));
            }

            if (buffer.IsEmpty)
            {
                return;
            }

            ITiffPixelBuffer<TPixel> innerBuffer = TiffPixelBufferUnsafeMarshal.GetBuffer(buffer, out TiffPoint destinationOffset, out TiffSize size);
            decoder.Decode(offset, size, destinationOffset, innerBuffer);
        }


        /// <summary>
        /// Decode the image into the specified pixel buffer.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="decoder">The image decoder.</param>
        /// <param name="buffer">The pixel buffer to write pixels into.</param>
        public static void Decode<TPixel>(this TiffImageDecoder decoder, ITiffPixelBuffer<TPixel> buffer) where TPixel : unmanaged
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
            decoder.Decode(default, new TiffSize(buffer.Width, buffer.Height), default, adapter);
        }

        /// <summary>
        /// Decode the image into the specified pixel buffer.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="decoder">The image decoder.</param>
        /// <param name="offset">Number of columns and rows to skip in the source image.</param>
        /// <param name="buffer">The pixel buffer to write pixels into.</param>
        public static void Decode<TPixel>(this TiffImageDecoder decoder, TiffPoint offset, ITiffPixelBuffer<TPixel> buffer) where TPixel : unmanaged
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
            decoder.Decode(offset, new TiffSize(buffer.Width, buffer.Height), default, adapter);
        }

        /// <summary>
        /// Decode the image into the specified pixel buffer.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="decoder">The image decoder.</param>
        /// <param name="offset">Number of columns and rows to skip in the source image.</param>
        /// <param name="readSize">Number of columns and rows to read from the source image.</param>
        /// <param name="buffer">The pixel buffer to write pixels into.</param>
        public static void Decode<TPixel>(this TiffImageDecoder decoder, TiffPoint offset, TiffSize readSize, ITiffPixelBuffer<TPixel> buffer) where TPixel : unmanaged
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
            decoder.Decode(offset, readSize, default, adapter);
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
        public static void Decode<TPixel>(this TiffImageDecoder decoder, TiffPoint offset, TiffSize readSize, TiffPoint destinationOffset, ITiffPixelBuffer<TPixel> buffer) where TPixel : unmanaged
        {
            if (decoder is null)
            {
                throw new ArgumentNullException(nameof(decoder));
            }

            if (buffer is null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            decoder.Decode(offset, readSize, destinationOffset, new TiffPixelBufferWriterAdapter<TPixel>(buffer));
        }

        /// <summary>
        /// Decode the image into the specified pixel buffer.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="decoder">The image decoder.</param>
        /// <param name="writer">The pixel buffer writer to write pixels into.</param>
        public static void Decode<TPixel>(this TiffImageDecoder decoder, TiffPixelBufferWriter<TPixel> writer) where TPixel : unmanaged
        {
            if (decoder is null)
            {
                throw new ArgumentNullException(nameof(decoder));
            }

            if (writer.IsEmpty)
            {
                return;
            }

            ITiffPixelBufferWriter<TPixel> innerBuffer = TiffPixelBufferUnsafeMarshal.GetBuffer(writer, out TiffPoint offset, out TiffSize size);
            decoder.Decode(default, size, offset, innerBuffer);
        }

        /// <summary>
        /// Decode the image into the specified pixel buffer.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="decoder">The image decoder.</param>
        /// <param name="offset">Number of columns and rows to skip in the source image.</param>
        /// <param name="readSize">Number of columns and rows to read from the source image.</param>
        /// <param name="writer">The pixel buffer writer to write pixels into.</param>
        public static void Decode<TPixel>(this TiffImageDecoder decoder, TiffPoint offset, TiffSize readSize, TiffPixelBufferWriter<TPixel> writer) where TPixel : unmanaged
        {
            if (decoder is null)
            {
                throw new ArgumentNullException(nameof(decoder));
            }

            if (writer.IsEmpty)
            {
                return;
            }

            ITiffPixelBufferWriter<TPixel> innerBuffer = TiffPixelBufferUnsafeMarshal.GetBuffer(writer, out TiffPoint destinationOffset, out _);
            decoder.Decode(offset, readSize, destinationOffset, innerBuffer);
        }

        /// <summary>
        /// Decode the image into the specified pixel buffer.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="decoder">The image decoder.</param>
        /// <param name="offset">Number of columns and rows to skip in the source image.</param>
        /// <param name="writer">The pixel buffer writer to write pixels into.</param>
        public static void Decode<TPixel>(this TiffImageDecoder decoder, TiffPoint offset, TiffPixelBufferWriter<TPixel> writer) where TPixel : unmanaged
        {
            if (decoder is null)
            {
                throw new ArgumentNullException(nameof(decoder));
            }

            if (writer.IsEmpty)
            {
                return;
            }

            ITiffPixelBufferWriter<TPixel> innerBuffer = TiffPixelBufferUnsafeMarshal.GetBuffer(writer, out TiffPoint destinationOffset, out TiffSize size);
            decoder.Decode(offset, size, destinationOffset, innerBuffer);
        }

        /// <summary>
        /// Decode the image into the specified pixel buffer.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="decoder">The image decoder.</param>
        /// <param name="writer">The pixel buffer writer to write pixels into.</param>
        public static void Decode<TPixel>(this TiffImageDecoder decoder, ITiffPixelBufferWriter<TPixel> writer) where TPixel : unmanaged
        {
            if (decoder is null)
            {
                throw new ArgumentNullException(nameof(decoder));
            }

            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            decoder.Decode(default, new TiffSize(writer.Width, writer.Height), default, writer);
        }
    }
}
