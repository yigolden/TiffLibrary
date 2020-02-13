using System.Buffers;

namespace TiffLibrary.Compression
{
    /// <summary>
    /// Information of the image to be encoded.
    /// </summary>
    public class TiffCompressionContext
    {
        /// <summary>
        /// The memory pool to use when allocating large chunk of memory.
        /// </summary>
        public MemoryPool<byte>? MemoryPool { get; set; }

        /// <summary>
        /// The photometric interpretation.
        /// </summary>
        public TiffPhotometricInterpretation PhotometricInterpretation { get; set; }

        /// <summary>
        /// Bits per sample.
        /// </summary>
        public TiffValueCollection<ushort> BitsPerSample { get; set; }

        /// <summary>
        /// The size of the image.
        /// </summary>
        public TiffSize ImageSize { get; set; }

        /// <summary>
        /// The calculated byte count per scanline.
        /// </summary>
        public int BytesPerScanline { get; set; }
    }
}
