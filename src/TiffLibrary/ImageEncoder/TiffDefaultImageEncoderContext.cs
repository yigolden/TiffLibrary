using System;
using TiffLibrary.PixelBuffer;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.ImageEncoder
{
    /// <summary>
    /// Information of the current encoding process.
    /// </summary>
    /// <typeparam name="TPixel">The pixel type.</typeparam>
    public class TiffDefaultImageEncoderContext<TPixel> : TiffImageEncoderContext<TPixel> where TPixel : unmanaged
    {
        /// <summary>
        /// The <see cref="TiffFileWriter"/> to write image data as well as fields data to.
        /// </summary>
        public override TiffFileWriter FileWriter { get; set; }

        /// <summary>
        /// The <see cref="TiffImageFileDirectoryWriter"/> to write image file directory fields to.
        /// </summary>
        public override TiffImageFileDirectoryWriter IfdWriter { get; set; }

        /// <summary>
        /// The photometric interpretation of the current image.
        /// </summary>
        public override TiffPhotometricInterpretation PhotometricInterpretation { get; set; }

        /// <summary>
        /// Bits per sample of the current image.
        /// </summary>
        public override TiffValueCollection<ushort> BitsPerSample { get; set; }

        /// <summary>
        /// The compression method used for this image.
        /// </summary>
        public override TiffCompression Compression { get; set; }

        /// <summary>
        /// The size of the current image.
        /// </summary>
        public override TiffSize ImageSize { get; set; }

        /// <summary>
        /// Used to hold temporary pixel buffer.
        /// </summary>
        public override Memory<byte> UncompressedData { get; set; }

        /// <summary>
        /// The <see cref="TiffStreamRegion"/> written when encoding this image.
        /// </summary>
        public override TiffStreamRegion OutputRegion { get; set; }

        /// <summary>
        /// Gets or sets the reader to read pixels from.
        /// </summary>
        public TiffPixelBufferReader<TPixel> PixelBufferReader { get; set; }

        /// <summary>
        /// A <see cref="ITiffPixelConverterFactory"/> implementation to create converters for <see cref="ITiffPixelBufferWriter{TPixel}"/>.
        /// </summary>
        public ITiffPixelConverterFactory PixelConverterFactory { get; set; }

        /// <summary>
        /// Gets the reader to read pixels from.
        /// </summary>
        /// <returns>The reader to read pixels from.</returns>
        public override TiffPixelBufferReader<TPixel> GetReader()
        {
            return PixelBufferReader;
        }

        /// <summary>
        /// Converts pixel buffer writer of any pixel format <typeparamref name="TBuffer"/> into <see cref="TiffPixelBufferWriter{TPixel}"/>.
        /// </summary>
        /// <typeparam name="TBuffer">The specified pixel type.</typeparam>
        /// <param name="writer">The writer to be converted.</param>
        /// <returns>The converted writer.</returns>
        public override TiffPixelBufferWriter<TPixel> ConvertWriter<TBuffer>(TiffPixelBufferWriter<TBuffer> writer)
        {
            ITiffPixelBufferWriter<TBuffer> innerWriter = TiffPixelBufferUnsafeMarshal.GetBuffer(writer, out TiffPoint offset, out TiffSize size);
            ITiffPixelBufferWriter<TPixel> converted = PixelConverterFactory.CreateConverter<TPixel, TBuffer>(innerWriter);
            return converted.Crop(offset, size);
        }
    }
}
