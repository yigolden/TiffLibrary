namespace TiffLibrary.PixelBuffer
{
    /// <summary>
    /// Contains methods to extract inner buffer from pixel buffer related structure.
    /// </summary>
    public static class TiffPixelBufferUnsafeMarshal
    {
        /// <summary>
        /// Extract inner buffer from <paramref name="buffer"/>.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="buffer">The structure to extract from.</param>
        /// <param name="offset">The extracted offset.</param>
        /// <param name="size">The extracted size.</param>
        /// <returns>The extracted inner buffer.</returns>
        public static ITiffPixelBuffer<TPixel> GetBuffer<TPixel>(in TiffPixelBuffer<TPixel> buffer, out TiffPoint offset, out TiffSize size) where TPixel : unmanaged
        {
            offset = buffer._offset;
            size = buffer._size;
            return buffer._buffer;
        }

        /// <summary>
        /// Extract inner buffer from <paramref name="writer"/>.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="writer">The structure to extract from.</param>
        /// <param name="offset">The extracted offset.</param>
        /// <param name="size">The extracted size.</param>
        /// <returns>The extracted inner buffer.</returns>
        public static ITiffPixelBufferWriter<TPixel> GetBuffer<TPixel>(in TiffPixelBufferWriter<TPixel> writer, out TiffPoint offset, out TiffSize size) where TPixel : unmanaged
        {
            offset = writer._offset;
            size = writer._size;
            return writer._writer;
        }

        /// <summary>
        /// Extract inner buffer from <paramref name="reader"/>.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="reader">The structure to extract from.</param>
        /// <param name="offset">The extracted offset.</param>
        /// <param name="size">The extracted size.</param>
        /// <returns>The extracted inner buffer.</returns>
        public static ITiffPixelBufferReader<TPixel> GetBuffer<TPixel>(in TiffPixelBufferReader<TPixel> reader, out TiffPoint offset, out TiffSize size) where TPixel : unmanaged
        {
            offset = reader._offset;
            size = reader._size;
            return reader._reader;
        }
    }
}
