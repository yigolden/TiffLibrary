namespace TiffLibrary.Compression
{
    /// <summary>
    /// Information of the image file directory.
    /// </summary>
    public class TiffDecompressionContext
    {
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

        /// <summary>
        /// The number of scanlines that can be skipped when decompressing.
        /// </summary>
        public int SkippedScanlines { get; set; }

        /// <summary>
        /// The number of scanlines that are actually required to be decompressed.
        /// </summary>
        public int RequestedScanlines { get; set; }
    }
}
