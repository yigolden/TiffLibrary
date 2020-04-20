namespace TiffLibrary.Compression
{
    /// <summary>
    /// The compression level used in Deflate algorithm. A value of 9 is best, and 1 is least compression. The default is 6.
    /// </summary>
    public enum TiffDeflateCompressionLevel
    {
        /// <summary>
        /// Optimal.
        /// </summary>
        Optimal = 9,

        /// <summary>
        /// Fatest.
        /// </summary>
        Fastest = 1,

        /// <summary>
        /// Default.
        /// </summary>
        Default = 6,

        /// <summary>
        /// NoCompression.
        /// </summary>
        NoCompression = 0,
    }
}
