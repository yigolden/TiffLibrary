using System;
using System.Buffers;
using System.Threading;

namespace TiffLibrary.ImageEncoder
{
    /// <summary>
    /// Information of the current encoding process.
    /// </summary>
    /// <typeparam name="TPixel">The pixel type.</typeparam>
    [CLSCompliant(false)]
    public abstract class TiffImageEncoderContext<TPixel> where TPixel : unmanaged
    {
        /// <summary>
        /// The memory pool to use when allocating large chunk of memory.
        /// </summary>
        public abstract MemoryPool<byte>? MemoryPool { get; set; }

        /// <summary>
        /// The <see cref="CancellationToken"/> that fires if the user has requested to abort the encoding pipeline.
        /// </summary>
        public abstract CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// The <see cref="TiffFileWriter"/> to write image data as well as fields data to.
        /// </summary>
        public abstract TiffFileWriter? FileWriter { get; set; }

        /// <summary>
        /// The <see cref="TiffImageFileDirectoryWriter"/> to write image file directory fields to.
        /// </summary>
        public abstract TiffImageFileDirectoryWriter? IfdWriter { get; set; }

        /// <summary>
        /// The photometric interpretation of the current image.
        /// </summary>
        public abstract TiffPhotometricInterpretation PhotometricInterpretation { get; set; }

        /// <summary>
        /// Bits per sample of the current image.
        /// </summary>
        public abstract TiffValueCollection<ushort> BitsPerSample { get; set; }

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
        public abstract ITiffPixelBufferReader<TPixel> GetReader();

        /// <summary>
        /// Converts pixel buffer writer of any pixel format <typeparamref name="TBuffer"/> into <see cref="TiffPixelBufferWriter{TPixel}"/>.
        /// </summary>
        /// <typeparam name="TBuffer">The specified pixel type.</typeparam>
        /// <param name="writer">The writer to be converted.</param>
        /// <returns>The converted writer.</returns>
        public abstract TiffPixelBufferWriter<TPixel> ConvertWriter<TBuffer>(TiffPixelBufferWriter<TBuffer> writer) where TBuffer : unmanaged;

        /// <summary>
        /// Register a service with the current context.
        /// </summary>
        /// <param name="serviceType">The type of the service.</param>
        /// <param name="service">The service instance.</param>
        public abstract void RegisterService(Type serviceType, object? service);

        /// <summary>
        /// Get the service of the specified type.
        /// </summary>
        /// <param name="serviceType">The type of the service.</param>
        /// <returns>The service instance.</returns>
        public abstract object? GetService(Type serviceType);
    }
}
