using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using TiffLibrary.PixelBuffer;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.ImageEncoder
{
    /// <summary>
    /// Information of the current encoding process.
    /// </summary>
    /// <typeparam name="TPixel">The pixel type.</typeparam>
    [CLSCompliant(false)]
    public class TiffDefaultImageEncoderContext<TPixel> : TiffImageEncoderContext<TPixel> where TPixel : unmanaged
    {
        private Dictionary<Type, object?>? _services;
        private SpinLock _servicesLock;

        /// <summary>
        /// The memory pool to use when allocating large chunk of memory.
        /// </summary>
        public override MemoryPool<byte>? MemoryPool { get; set; }

        /// <summary>
        /// The <see cref="CancellationToken"/> that fires if the user has requested to abort the encoding pipeline.
        /// </summary>
        public override CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// The <see cref="TiffFileWriter"/> to write image data as well as fields data to.
        /// </summary>
        public override TiffFileWriter? FileWriter { get; set; }

        /// <summary>
        /// The <see cref="TiffImageFileDirectoryWriter"/> to write image file directory fields to.
        /// </summary>
        public override TiffImageFileDirectoryWriter? IfdWriter { get; set; }

        /// <summary>
        /// The photometric interpretation of the current image.
        /// </summary>
        [CLSCompliant(false)]
        public override TiffPhotometricInterpretation PhotometricInterpretation { get; set; }

        /// <summary>
        /// Bits per sample of the current image.
        /// </summary>
        [CLSCompliant(false)]
        public override TiffValueCollection<ushort> BitsPerSample { get; set; }

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
        public ITiffPixelConverterFactory? PixelConverterFactory { get; set; }

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
            ITiffPixelConverterFactory? pixelConverterFactory = PixelConverterFactory;
            if (pixelConverterFactory is null)
            {
                throw new InvalidOperationException("Failed to acquire PixelConverterFactory");
            }

            ITiffPixelBufferWriter<TBuffer> innerWriter = TiffPixelBufferUnsafeMarshal.GetBuffer(writer, out TiffPoint offset, out TiffSize size);
            ITiffPixelBufferWriter<TPixel> converted = pixelConverterFactory.CreateConverter<TPixel, TBuffer>(innerWriter);
            return converted.Crop(offset, size);
        }

        /// <inheritdoc />
        public override void RegisterService(Type serviceType, object? service)
        {
            if (serviceType is null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            bool lockTaken = false;
            try
            {
                _servicesLock.Enter(ref lockTaken);

                if (_services is null)
                {
                    _services = new Dictionary<Type, object?>();
                }

                _services[serviceType] = service;
            }
            finally
            {
                if (lockTaken)
                {
                    _servicesLock.Exit();
                }
            }
        }

        /// <inheritdoc />
        public override object? GetService(Type serviceType)
        {
            if (serviceType is null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            bool lockTaken = false;
            try
            {
                _servicesLock.Enter(ref lockTaken);

                if (_services is null)
                {
                    return null;
                }

                _services.TryGetValue(serviceType, out object? service);
                return service;
            }
            finally
            {
                if (lockTaken)
                {
                    _servicesLock.Exit();
                }
            }
        }

    }
}
