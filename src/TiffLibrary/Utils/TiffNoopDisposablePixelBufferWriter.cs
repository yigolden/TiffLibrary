using System;

namespace TiffLibrary.PixelBuffer
{
    /// <summary>
    /// Provide methods to create <see cref="TiffNoopDisposablePixelBufferWriter{TPixel}"/>
    /// </summary>
    public static class TiffNoopDisposablePixelBufferWriter
    {
        /// <summary>
        /// Wraps a <see cref="ITiffPixelBufferWriter{TPixel}"/> and creates <see cref="TiffNoopDisposablePixelBufferWriter{TPixel}"/>.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="writer">The writer to wrap.</param>
        /// <returns></returns>
        public static ITiffPixelBufferWriter<TPixel> Wrap<TPixel>(ITiffPixelBufferWriter<TPixel> writer) where TPixel : unmanaged
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (writer is TiffNoopDisposablePixelBufferWriter<TPixel> noopWriter)
            {
                return noopWriter;
            }

            return new TiffNoopDisposablePixelBufferWriter<TPixel>(writer);
        }
    }

    /// <summary>
    /// Provides wrapper of <see cref="ITiffPixelBufferWriter{TPixel}"/> and block Dispose calls.
    /// </summary>
    /// <typeparam name="TPixel">The pixel type.</typeparam>
    public sealed class TiffNoopDisposablePixelBufferWriter<TPixel> : ITiffPixelBufferWriter<TPixel> where TPixel : unmanaged
    {
        private readonly ITiffPixelBufferWriter<TPixel> _writer;

        /// <summary>
        /// Create the instance from the specified writer.
        /// </summary>
        /// <param name="writer">The writer to wrap.</param>
        public TiffNoopDisposablePixelBufferWriter(ITiffPixelBufferWriter<TPixel> writer)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        }

        /// <summary>
        /// The number of columns in the region.
        /// </summary>
        public int Width => _writer.Width;

        /// <summary>
        /// The number of rows in the region.
        /// </summary>
        public int Height => _writer.Height;

        /// <summary>
        /// This is no op.
        /// </summary>
        public void Dispose()
        {
            // Noop
        }

        /// <summary>
        /// Gets a <see cref="TiffPixelSpanHandle{TPixel}"/> representing a write-only span of the <paramref name="rowIndex"/> row of the region, while skipping <paramref name="start"/> pixels and limiting the length of the span to <paramref name="length"/>. <see cref="TiffPixelSpanHandle{TPixel}"/> should be disposed after use to flush the pixels into the underlying storage.
        /// </summary>
        /// <param name="rowIndex">The row index.</param>
        /// <param name="start">Number of pixels to skip in this row.</param>
        /// <param name="length">Number of pixels to write.</param>
        /// <returns>A <see cref="TiffPixelSpanHandle{TPixel}"/> representing the temporary buffer consumer can write to. When disposed, it will flush the temporary pixels into the actual storage.</returns>
        public TiffPixelSpanHandle<TPixel> GetRowSpan(int rowIndex, int start, int length)
            => _writer.GetRowSpan(rowIndex, start, length);

        /// <summary>
        /// Gets a <see cref="TiffPixelSpanHandle{TPixel}"/> representing a write-only span of the <paramref name="colIndex"/> column of the region, while skipping <paramref name="start"/> pixels and limiting the length of the span to <paramref name="length"/>. <see cref="TiffPixelSpanHandle{TPixel}"/> should be disposed after use to flush the pixels into the underlying storage.
        /// </summary>
        /// <param name="colIndex">The column index.</param>
        /// <param name="start">Number of pixels to skip in this column.</param>
        /// <param name="length">Number of pixels to write.</param>
        /// <returns>A <see cref="TiffPixelSpanHandle{TPixel}"/> representing the temporary buffer consumer can write to. When disposed, it will flush the temporary pixels into the actual storage.</returns>
        public TiffPixelSpanHandle<TPixel> GetColumnSpan(int colIndex, int start, int length)
            => _writer.GetColumnSpan(colIndex, start, length);

    }
}
