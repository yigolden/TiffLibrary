using System;

namespace TiffLibrary.ImageEncoder
{
    /// <summary>
    /// Information of the current encoding process.
    /// </summary>
    /// <typeparam name="TPixel">The pixel type.</typeparam>
    public abstract class TiffImageEncoderContext<TPixel> where TPixel : unmanaged
    {
        /// <summary>
        /// The <see cref="TiffFileWriter"/> to write image data as well as fields data to.
        /// </summary>
        public abstract TiffFileWriter FileWriter { get; set; }

        /// <summary>
        /// The <see cref="TiffImageFileDirectoryWriter"/> to write image file directory fields to.
        /// </summary>
        public abstract TiffImageFileDirectoryWriter IfdWriter { get; set; }

        /// <summary>
        /// The photometric interpretation of the current image.
        /// </summary>
        public abstract TiffPhotometricInterpretation PhotometricInterpretation { get; set; }

        /// <summary>
        /// Bits per sample of the current image.
        /// </summary>
        public abstract TiffValueCollection<ushort> BitsPerSample { get; set; }

        /// <summary>
        /// The compression method used for this image.
        /// </summary>
        public abstract TiffCompression Compression { get; set; }

        /// <summary>
        /// The size of the current image.
        /// </summary>
        public abstract TiffSize ImageSize { get; set; }

        /// <summary>
        /// Used to hold temporary pixel buffer.
        /// </summary>
        public abstract Memory<byte> UncompressedData { get; set; }

        /// <summary>
        /// The <see cref="TiffStreamRegion"/> written when encoding this image.
        /// </summary>
        public abstract TiffStreamRegion OutputRegion { get; set; }

        /// <summary>
        /// Gets the reader to read pixels from.
        /// </summary>
        /// <returns>The reader to read pixels from.</returns>
        public abstract TiffPixelBufferReader<TPixel> GetReader();

        /// <summary>
        /// Converts pixel buffer writer of any pixel format <typeparamref name="TBuffer"/> into <see cref="TiffPixelBufferWriter{TPixel}"/>.
        /// </summary>
        /// <typeparam name="TBuffer">The specified pixel type.</typeparam>
        /// <param name="writer">The writer to be converted.</param>
        /// <returns>The converted writer.</returns>
        public abstract TiffPixelBufferWriter<TPixel> ConvertWriter<TBuffer>(TiffPixelBufferWriter<TBuffer> writer) where TBuffer : unmanaged;
    }
}
