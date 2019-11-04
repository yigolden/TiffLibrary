using System;

namespace TiffLibrary
{
    /// <summary>
    /// Parameters of how the TIFF file should be parsed.
    /// </summary>
    public sealed class TiffOperationContext
    {
        /// <summary>
        /// Gets or sets whether the TIFF file is little-endian.
        /// </summary>
        public bool IsLittleEndian { get; set; }

        /// <summary>
        /// Gets or sets the byte count of the IFD count field.
        /// </summary>
        public short ByteCountOfImageFileDirectoryCountField { get; set; }

        /// <summary>
        /// Gets or sets the byte count of the "Value Offset" field in the IFD entry.
        /// </summary>
        public short ByteCountOfValueOffsetField { get; set; }

        internal static TiffOperationContext StandardTIFF { get; } = new TiffOperationContext
        {
            IsLittleEndian = BitConverter.IsLittleEndian,
            ByteCountOfImageFileDirectoryCountField = 2,
            ByteCountOfValueOffsetField = 4
        };

        internal static TiffOperationContext BigTIFF { get; } = new TiffOperationContext
        {
            IsLittleEndian = BitConverter.IsLittleEndian,
            ByteCountOfImageFileDirectoryCountField = 8,
            ByteCountOfValueOffsetField = 8
        };

    }
}
