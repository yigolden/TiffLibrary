using System.Buffers;
using TiffLibrary.PixelConverter;

namespace TiffLibrary.ImageDecoder
{
    /// <summary>
    /// Parameters for <see cref="TiffImageDecoderPipelineAdapter"/> to decode an IFD.
    /// </summary>
    public sealed class TiffImageDecoderParameters
    {
        /// <summary>
        /// The memory pool to use when allocating large chunk of memory.
        /// </summary>
        public MemoryPool<byte> MemoryPool { get; set; }

        /// <summary>
        /// Parameters of how the TIFF file should be parsed.
        /// </summary>
        public TiffOperationContext OperationContext { get; set; }

        /// <summary>
        /// An instance for opening <see cref="TiffFileContentReader"/> of specified TIFF file.
        /// </summary>
        public ITiffFileContentSource ContentSource { get; set; }

        /// <summary>
        /// The IFD to be decoded.
        /// </summary>
        public TiffImageFileDirectory ImageFileDirectory { get; set; }

        /// <summary>
        /// A factory instance for creating <see cref="TiffPixelConverter{TSource, TDestination}"/> object to convert from one pixel format to another.
        /// </summary>
        public ITiffPixelConverterFactory PixelConverterFactory { get; set; }

        /// <summary>
        /// The with and height defined in  ImageWidth and ImageLength tags.
        /// </summary>
        public TiffSize ImageSize { get; set; }

        /// <summary>
        /// The orientation defined in the Orientation tag.
        /// </summary>
        public TiffOrientation Orientation { get; set; }

        /// <summary>
        /// Byte count per scan line of each plane.
        /// </summary>
        public TiffValueCollection<int> BytesPerScanline { get; set; }
    }
}
