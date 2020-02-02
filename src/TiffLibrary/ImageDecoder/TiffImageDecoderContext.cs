using System;
using System.Buffers;
using System.Threading;

namespace TiffLibrary.ImageDecoder
{
    /// <summary>
    /// Information of the current decoding process.
    /// </summary>
    public abstract class TiffImageDecoderContext
    {
        /// <summary>
        /// The memory pool to use when allocating large chunk of memory.
        /// </summary>
        public abstract MemoryPool<byte>? MemoryPool { get; set; }

        /// <summary>
        /// The <see cref="CancellationToken"/> that fires if the user has requested to abort the decoding pipeline.
        /// </summary>
        public abstract CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// Gets whether the uncompressed data is little endian.
        /// </summary>
        public virtual bool IsLittleEndian => OperationContext?.IsLittleEndian ?? throw new InvalidOperationException("Operation context is not specified.");

        /// <summary>
        /// Parameters of how the TIFF file should be parsed.
        /// </summary>
        public abstract TiffOperationContext? OperationContext { get; set; }

        /// <summary>
        /// The content reader to read data from.
        /// </summary>
        public abstract TiffFileContentReader? ContentReader { get; set; }

        /// <summary>
        /// The regions in the stream to read each plane data from.
        /// </summary>
        public abstract TiffValueCollection<TiffStreamRegion> PlanarRegions { get; set; }

        /// <summary>
        /// Data decompressed from raw data. (It should contains enough data for SourceImageSize)
        /// </summary>
        public abstract Memory<byte> UncompressedData { get; set; }

        /// <summary>
        /// Image size of the decompressed data.
        /// </summary>
        public abstract TiffSize SourceImageSize { get; set; }

        /// <summary>
        /// Read offset from source image (decompressed image).
        /// </summary>
        public abstract TiffPoint SourceReadOffset { get; set; }

        /// <summary>
        /// The size of the image to read from the uncompressed data and write to destination buffer.
        /// </summary>
        public abstract TiffSize ReadSize { get; set; }

        /// <summary>
        /// A function to get destination buffer in the specified pixel format.
        /// </summary>
        public abstract TiffPixelBufferWriter<TPixel> GetWriter<TPixel>() where TPixel : unmanaged;
    }
}
