using System;
using TiffLibrary.PixelBuffer;

namespace TiffLibrary
{
    public static partial class TiffImageDecoderExtensions
    {

        #region TiffPixelBuffer

        /// <summary>
        /// Decode the image into the specified pixel buffer.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="decoder">The image decoder.</param>
        /// <param name="buffer">The pixel buffer to write pixels into.</param>
        public static void Decode<TPixel>(this TiffImageDecoder decoder, TiffPixelBuffer<TPixel> buffer) where TPixel : unmanaged
        {
            ThrowHelper.ThrowIfNull(decoder);

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
            ThrowHelper.ThrowIfNull(decoder);

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
        /// <param name="offset">Number of columns and rows to skip in the source image.</param>
        /// <param name="readSize">Number of columns and rows to read from the source image.</param>
        /// <param name="buffer">The pixel buffer to write pixels into.</param>
        public static void Decode<TPixel>(this TiffImageDecoder decoder, TiffPoint offset, TiffSize readSize, TiffPixelBuffer<TPixel> buffer) where TPixel : unmanaged
        {
            ThrowHelper.ThrowIfNull(decoder);

            if (buffer.IsEmpty)
            {
                return;
            }

            ITiffPixelBuffer<TPixel> innerBuffer = TiffPixelBufferUnsafeMarshal.GetBuffer(buffer, out TiffPoint bufferDestinationOffset, out TiffSize bufferSize);
            readSize = new TiffSize(Math.Min(readSize.Width, bufferSize.Width), Math.Min(readSize.Height, bufferSize.Height));
            decoder.Decode(offset, readSize, bufferDestinationOffset, innerBuffer);
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
        public static void Decode<TPixel>(this TiffImageDecoder decoder, TiffPoint offset, TiffSize readSize, TiffPoint destinationOffset, TiffPixelBuffer<TPixel> buffer) where TPixel : unmanaged
        {
            ThrowHelper.ThrowIfNull(decoder);

            if (buffer.IsEmpty)
            {
                return;
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
            decoder.Decode(offset, readSize, new TiffPoint(destinationOffset.X + bufferDestinationOffset.X, destinationOffset.Y + bufferDestinationOffset.Y), innerBuffer);
        }

        #endregion

        #region ITiffPixelBuffer

        /// <summary>
        /// Decode the image into the specified pixel buffer.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="decoder">The image decoder.</param>
        /// <param name="buffer">The pixel buffer to write pixels into.</param>
        public static void Decode<TPixel>(this TiffImageDecoder decoder, ITiffPixelBuffer<TPixel> buffer) where TPixel : unmanaged
        {
            ThrowHelper.ThrowIfNull(decoder);
            ThrowHelper.ThrowIfNull(buffer);

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
            ThrowHelper.ThrowIfNull(decoder);
            ThrowHelper.ThrowIfNull(buffer);

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
            ThrowHelper.ThrowIfNull(decoder);
            ThrowHelper.ThrowIfNull(buffer);

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
            ThrowHelper.ThrowIfNull(decoder);
            ThrowHelper.ThrowIfNull(buffer);

            decoder.Decode(offset, readSize, destinationOffset, new TiffPixelBufferWriterAdapter<TPixel>(buffer));
        }

        #endregion

        #region TiffPixelBufferWriter

        /// <summary>
        /// Decode the image into the specified pixel buffer.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="decoder">The image decoder.</param>
        /// <param name="writer">The pixel buffer writer to write pixels into.</param>
        public static void Decode<TPixel>(this TiffImageDecoder decoder, TiffPixelBufferWriter<TPixel> writer) where TPixel : unmanaged
        {
            ThrowHelper.ThrowIfNull(decoder);

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
        /// <param name="writer">The pixel buffer writer to write pixels into.</param>
        public static void Decode<TPixel>(this TiffImageDecoder decoder, TiffPoint offset, TiffPixelBufferWriter<TPixel> writer) where TPixel : unmanaged
        {
            ThrowHelper.ThrowIfNull(decoder);

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
        /// <param name="offset">Number of columns and rows to skip in the source image.</param>
        /// <param name="readSize">Number of columns and rows to read from the source image.</param>
        /// <param name="writer">The pixel buffer writer to write pixels into.</param>
        public static void Decode<TPixel>(this TiffImageDecoder decoder, TiffPoint offset, TiffSize readSize, TiffPixelBufferWriter<TPixel> writer) where TPixel : unmanaged
        {
            ThrowHelper.ThrowIfNull(decoder);

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
        /// <param name="readSize">Number of columns and rows to read from the source image.</param>
        /// <param name="destinationOffset">Number of columns and rows to skip in the destination writer.</param>
        /// <param name="writer">The pixel buffer writer to write pixels into.</param>
        public static void Decode<TPixel>(this TiffImageDecoder decoder, TiffPoint offset, TiffSize readSize, TiffPoint destinationOffset, TiffPixelBufferWriter<TPixel> writer) where TPixel : unmanaged
        {
            ThrowHelper.ThrowIfNull(decoder);

            if (writer.IsEmpty)
            {
                return;
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
            decoder.Decode(offset, readSize, new TiffPoint(destinationOffset.X + writerDestinationOffset.X, destinationOffset.Y + writerDestinationOffset.Y), innerBuffer);
        }

        #endregion

        #region ITiffPixelBufferWriter

        /// <summary>
        /// Decode the image into the specified pixel buffer.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="decoder">The image decoder.</param>
        /// <param name="writer">The pixel buffer writer to write pixels into.</param>
        public static void Decode<TPixel>(this TiffImageDecoder decoder, ITiffPixelBufferWriter<TPixel> writer) where TPixel : unmanaged
        {
            ThrowHelper.ThrowIfNull(decoder);
            ThrowHelper.ThrowIfNull(writer);

            decoder.Decode(default, new TiffSize(writer.Width, writer.Height), default, writer);
        }

        /// <summary>
        /// Decode the image into the specified pixel buffer writer.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="decoder">The image decoder.</param>
        /// <param name="offset">Number of columns and rows to skip in the source image.</param>
        /// <param name="writer">The pixel buffer writer to write pixels into.</param>
        public static void Decode<TPixel>(this TiffImageDecoder decoder, TiffPoint offset, ITiffPixelBufferWriter<TPixel> writer) where TPixel : unmanaged
        {
            ThrowHelper.ThrowIfNull(decoder);
            ThrowHelper.ThrowIfNull(writer);

            decoder.Decode(offset, new TiffSize(writer.Width, writer.Height), default, writer);
        }

        /// <summary>
        /// Decode the image into the specified pixel buffer writer.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="decoder">The image decoder.</param>
        /// <param name="offset">Number of columns and rows to skip in the source image.</param>
        /// <param name="readSize">Number of columns and rows to read from the source image.</param>
        /// <param name="writer">The pixel buffer writer to write pixels into.</param>
        public static void Decode<TPixel>(this TiffImageDecoder decoder, TiffPoint offset, TiffSize readSize, ITiffPixelBufferWriter<TPixel> writer) where TPixel : unmanaged
        {
            ThrowHelper.ThrowIfNull(decoder);

            decoder.Decode(offset, readSize, default, writer);
        }

        #endregion
    }
}
