using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace TiffLibrary
{
#pragma warning disable CA1815 // CA1815: Override equals and operator equals on value types
    /// <summary>
    /// The TIFF file header.
    /// </summary>
    public readonly struct TiffFileHeader
#pragma warning restore CA1815 // CA1815: Override equals and operator equals on value types
    {
        internal const short LittleEndianByteOrderFlag = 0x4949;
        internal const short BigEndianByteOrderFlag = 0x4D4D;
        internal const short StandardTiffVersion = 0x2A;
        internal const short BigTiffVersion = 0x2B;


        /// <summary>
        /// The byte order used within the file. Legal values are:
        /// “II” (4949.H)
        /// “MM” (4D4D.H)
        /// In the “II” format, byte order is always from the least significant byte to the most significant byte, for both 16-bit and 32-bit integers This is called little-endian byte order. In the “MM” format, byte order is always from most significant to least significant, for both 16-bit and 32-bit integers. This is called big-endian byte order.
        /// </summary>
        public short ByteOrderFlag { get; }

        /// <summary>
        /// 0x002A = standard TIFF, 0x002B = BigTiff
        /// </summary>
        public short Version { get; }

        /// <summary>
        /// BigTiff: 0x0008 bytesize of offsets
        /// </summary>
        public short ByteSizeOfOffsets { get; }

        /// <summary>
        /// The offset (in bytes) of the first IFD. The directory may be at any location in the file after the header but must begin on a word boundary. In particular, an Image File Directory may follow the image data it describes. Readers must follow the pointers wherever they may lead.
        /// The term byte offset is always used in this document to refer to a location with respect to the beginning of the TIFF file. The first byte of the file has an offset of 0.
        /// </summary>
        public long ImageFileDirectoryOffset { get; }

        /// <summary>
        /// Whether this file is BigTIFF
        /// </summary>
        public bool IsBigTiff => Version == 0x2B;

        /// <summary>
        /// Whether this file is little endian.
        /// </summary>
        public bool IsLittleEndian => ByteOrderFlag == LittleEndianByteOrderFlag;

        /// <summary>
        /// Construct a ImageFileHeader.
        /// </summary>
        /// <param name="byteOrderFlag">The ByteOrderFlag.</param>
        /// <param name="imageFileDirectoryOffset">The first IFD offset.</param>
        internal TiffFileHeader(short byteOrderFlag, int imageFileDirectoryOffset)
        {
            ByteOrderFlag = byteOrderFlag;
            Version = 0x2A;
            ByteSizeOfOffsets = 4;
            //BigTiffConstant = 0;
            ImageFileDirectoryOffset = imageFileDirectoryOffset;
        }

        /// <summary>
        /// Construct a ImageFileHeader.
        /// </summary>
        /// <param name="byteOrderFlag">The ByteOrderFlag.</param>
        /// <param name="version">The version field.</param>
        /// <param name="byteSizeOfOffsets">The ByteSizeOfOffsets field.</param>
        /// <param name="imageFileDirectoryOffset">The first IFD offset.</param>
        public TiffFileHeader(short byteOrderFlag, short version, short byteSizeOfOffsets, long imageFileDirectoryOffset)
        {
            ByteOrderFlag = byteOrderFlag;
            Version = version;
            ByteSizeOfOffsets = byteSizeOfOffsets;
            //BigTiffConstant = bigTiffConstant;
            ImageFileDirectoryOffset = imageFileDirectoryOffset;
        }

        /// <summary>
        /// Try to parse TIFF file header from the specified buffer.
        /// </summary>
        /// <param name="data">The buffer to use.</param>
        /// <param name="header">The parsed TIFF file header.</param>
        /// <returns>True if the TIFF file header is successfully parsed. Otherwise, false.</returns>
        public static bool TryParse(ReadOnlySpan<byte> data, out TiffFileHeader header)
        {
            if (data.Length < 8)
            {
                header = default;
                return false;
            }

            // Read byte order
            short byteOrderFlag = MemoryMarshal.Read<short>(data);
            bool reverseEndianRequired;
            if (byteOrderFlag == LittleEndianByteOrderFlag)
            {
                reverseEndianRequired = !BitConverter.IsLittleEndian;
            }
            else if (byteOrderFlag == BigEndianByteOrderFlag)
            {
                reverseEndianRequired = BitConverter.IsLittleEndian;
            }
            else
            {
                header = default;
                return false;
            }

            // Read TIFF version
            short version = MemoryMarshal.Read<short>(data.Slice(2));
            if (reverseEndianRequired)
            {
                version = BinaryPrimitives.ReverseEndianness(version);
            }

            if (version == StandardTiffVersion)
            {
                // Standard TIFF
                int imageFileDirectoryOffset32 = MemoryMarshal.Read<int>(data.Slice(4));
                if (reverseEndianRequired)
                {
                    imageFileDirectoryOffset32 = BinaryPrimitives.ReverseEndianness(imageFileDirectoryOffset32);
                }

                // An IFD can be at any location in the file after the header but must begin on a word boundary.
                if (imageFileDirectoryOffset32 < 8 || (imageFileDirectoryOffset32 & 0b1) == 1)
                {
                    header = default;
                    return false;
                }

                header = new TiffFileHeader(byteOrderFlag, imageFileDirectoryOffset32);
                return true;
            }
            else if (version == BigTiffVersion)
            {
                // BigTIFF
                data = data.Slice(4);
                if (data.Length < 12)
                {
                    header = default;
                    return false;
                }

                short byteSizeOfOffsets = MemoryMarshal.Read<short>(data);
                short bigTiffConstant = MemoryMarshal.Read<short>(data.Slice(2));
                long imageFileDirectoryOffset64 = MemoryMarshal.Read<long>(data.Slice(4));
                if (reverseEndianRequired)
                {
                    byteSizeOfOffsets = BinaryPrimitives.ReverseEndianness(byteSizeOfOffsets);
                    // Uncomment this when we support BigTIFF constant other than zero.
                    // bigTiffConstant = BinaryPrimitives.ReverseEndianness(bigTiffConstant);
                    imageFileDirectoryOffset64 = BinaryPrimitives.ReverseEndianness(imageFileDirectoryOffset64);
                }

                if (byteSizeOfOffsets != 8)
                {
                    // Unsupported byte size of offsets
                    header = default;
                    return false;
                }

                if (bigTiffConstant != 0)
                {
                    header = default;
                    return false;
                }

                // An IFD can be at any location in the file after the header but must begin on a word boundary.
                if (imageFileDirectoryOffset64 < 16 || (imageFileDirectoryOffset64 & 0b1) == 1)
                {
                    header = default;
                    return false;
                }

                header = new TiffFileHeader(byteOrderFlag, BigTiffVersion, byteSizeOfOffsets, imageFileDirectoryOffset64);
                return true;
            }
            else
            {
                header = default;
                return false;
            }
        }

        /// <summary>
        /// Writes the TIFF file header to the specified buffer.
        /// </summary>
        /// <param name="destination">The buffer to write to.</param>
        /// <param name="imageFileDirectoryOffset">The fist IFD offset.</param>
        /// <param name="isLittleEndian">Whether the TIFF file is little endian.</param>
        /// <param name="useBigTiff">Whether to use BigTIFF format.</param>
        public static void Write(Span<byte> destination, long imageFileDirectoryOffset, bool isLittleEndian, bool useBigTiff)
        {
            bool reverseEndiannessNeeded = isLittleEndian != BitConverter.IsLittleEndian;
            short byteOrderFlag = isLittleEndian ? LittleEndianByteOrderFlag : BigEndianByteOrderFlag;
            short version;
            short zeroConstant = 0;

            if (useBigTiff)
            {
                if (destination.Length < 16)
                {
                    throw new ArgumentException("Destination too short.", nameof(destination));
                }

                version = BigTiffVersion;
                short byteSizeOfOffsets = 8;

                if (reverseEndiannessNeeded)
                {
                    version = BinaryPrimitives.ReverseEndianness(version);
                    byteSizeOfOffsets = BinaryPrimitives.ReverseEndianness(byteSizeOfOffsets);
                    imageFileDirectoryOffset = BinaryPrimitives.ReverseEndianness(imageFileDirectoryOffset);
                }

                MemoryMarshal.Write(destination, ref byteOrderFlag);
                MemoryMarshal.Write(destination.Slice(2), ref version);
                MemoryMarshal.Write(destination.Slice(4), ref byteSizeOfOffsets);
                MemoryMarshal.Write(destination.Slice(6), ref zeroConstant);
                MemoryMarshal.Write(destination.Slice(8), ref imageFileDirectoryOffset);
            }
            else
            {
                if (imageFileDirectoryOffset > uint.MaxValue)
                {
                    throw new InvalidOperationException();
                }

                if (destination.Length < 8)
                {
                    throw new ArgumentException("Destination too short.", nameof(destination));
                }

                version = StandardTiffVersion;
                uint imageFileDirectoryOffset32 = (uint)imageFileDirectoryOffset;

                if (reverseEndiannessNeeded)
                {
                    version = BinaryPrimitives.ReverseEndianness(version);
                    imageFileDirectoryOffset32 = BinaryPrimitives.ReverseEndianness(imageFileDirectoryOffset32);
                }

                MemoryMarshal.Write(destination, ref byteOrderFlag);
                MemoryMarshal.Write(destination.Slice(2), ref version);
                MemoryMarshal.Write(destination.Slice(4), ref imageFileDirectoryOffset32);
            }

        }

        internal TiffOperationContext CreateOperationContext()
        {
            return new TiffOperationContext
            {
                IsLittleEndian = ByteOrderFlag == LittleEndianByteOrderFlag,
                ByteCountOfImageFileDirectoryCountField = (Version == BigTiffVersion ? (short)8 : (short)2),
                ByteCountOfValueOffsetField = ByteSizeOfOffsets,
                ImageFileDirectoryOffset = ImageFileDirectoryOffset
            };
        }
    }
}
