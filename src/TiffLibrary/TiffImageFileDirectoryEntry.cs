using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using TiffLibrary.Utils;

namespace TiffLibrary
{
    /// <summary>
    /// IFD Entry
    /// </summary>
    [DebuggerDisplay("{Tag}({Type}[{ValueCount}])")]
    public readonly struct TiffImageFileDirectoryEntry : IEquatable<TiffImageFileDirectoryEntry>
    {
        /// <summary>
        /// The Tag that identifies the field.
        /// </summary>
        [CLSCompliant(false)]
        public TiffTag Tag { get; }

        /// <summary>
        /// The field Type.
        /// </summary>
        public TiffFieldType Type { get; }

        /// <summary>
        /// The number of values, Count of the indicated Type.
        /// Count—called Length in previous versions of the specification—is the number of values. Note that Count is not the total number of bytes. For example, a single 16bit word (SHORT) has a Count of 1; not 2.
        /// </summary>
        public long ValueCount { get; }

        /// <summary>
        /// The Value Offset, the file offset (in bytes) of the Value for the field. The Value is expected to begin on a word boundary; the corresponding Value Offset will thus be an even number. This file offset may point anywhere in the file, even after the image data.
        /// To save time and space the Value Offset contains the Value instead of pointing to the Value if and only if the Value fits into 4 bytes. If the Value is shorter than 4 bytes, it is left-justified within the 4-byte Value Offset, i.e., stored in the lowernumbered bytes. Whether the Value fits within 4 bytes is determined by the Type and Count of the field.
        /// </summary>
        public long ValueOffset { get; }

        /// <summary>
        /// Construct a TiffImageFileDirectoryEntry.
        /// </summary>
        /// <param name="tag">The tag of the IFD entry.</param>
        /// <param name="type">The field type of the values.</param>
        /// <param name="valueCount">The number of elements.</param>
        /// <param name="valueOffset">The offset in the TIFF stream.</param>
        [CLSCompliant(false)]
        public TiffImageFileDirectoryEntry(TiffTag tag, TiffFieldType type, long valueCount, long valueOffset)
        {
            Tag = tag;
            Type = type;
            ValueCount = valueCount;
            ValueOffset = valueOffset;
        }

        internal TiffImageFileDirectoryEntry(TiffOperationContext context, TiffTag tag, TiffFieldType type, long valueCount, ReadOnlySpan<byte> valueData)
        {
            Tag = tag;
            Type = type;
            ValueCount = valueCount;
            if (context.ByteCountOfValueOffsetField == 4)
            {
                uint offset32 = MemoryMarshal.Read<uint>(valueData);
                if (context.IsLittleEndian != BitConverter.IsLittleEndian)
                {
                    offset32 = BinaryPrimitives.ReverseEndianness(offset32);
                }
                ValueOffset = (long)(ulong)offset32;
            }
            else if (context.ByteCountOfValueOffsetField == 8)
            {
                ulong offset64 = MemoryMarshal.Read<ulong>(valueData);
                if (context.IsLittleEndian != BitConverter.IsLittleEndian)
                {
                    offset64 = BinaryPrimitives.ReverseEndianness(offset64);
                }
                ValueOffset = (long)offset64;
            }
            else
            {
                ValueOffset = 0;
            }
        }

        internal static bool TryParse(TiffOperationContext context, ReadOnlySpan<byte> data, out TiffImageFileDirectoryEntry entry)
        {
            if (data.Length < 12)
            {
                entry = default;
                return false;
            }

            ushort tag;
            short type;
            tag = MemoryMarshal.Read<ushort>(data);
            type = MemoryMarshal.Read<short>(data.Slice(2));
            if (context.IsLittleEndian != BitConverter.IsLittleEndian)
            {
                tag = BinaryPrimitives.ReverseEndianness(tag);
                type = BinaryPrimitives.ReverseEndianness(type);
            }

            data = data.Slice(4);
            if (context.ByteCountOfValueOffsetField == 4)
            {
                uint count32 = MemoryMarshal.Read<uint>(data);
                uint offset32 = MemoryMarshal.Read<uint>(data.Slice(4));
                if (context.IsLittleEndian != BitConverter.IsLittleEndian)
                {
                    count32 = BinaryPrimitives.ReverseEndianness(count32);
                    offset32 = BinaryPrimitives.ReverseEndianness(offset32);
                }

                entry = new TiffImageFileDirectoryEntry((TiffTag)tag, (TiffFieldType)type, (long)(ulong)count32, (long)(ulong)offset32);
                return true;
            }
            else if (context.ByteCountOfValueOffsetField == 8)
            {
                if (data.Length < 16)
                {
                    entry = default;
                    return false;
                }
                ulong count64 = MemoryMarshal.Read<ulong>(data);
                ulong offset64 = MemoryMarshal.Read<ulong>(data.Slice(8));
                if (context.IsLittleEndian != BitConverter.IsLittleEndian)
                {
                    count64 = BinaryPrimitives.ReverseEndianness(count64);
                    offset64 = BinaryPrimitives.ReverseEndianness(offset64);
                }

                entry = new TiffImageFileDirectoryEntry((TiffTag)tag, (TiffFieldType)type, (long)count64, (long)offset64);
                return true;
            }
            else
            {
                entry = default;
                return false;
            }
        }

        internal int Write(TiffOperationContext context, Span<byte> destination)
        {
            if (destination.Length < 12)
            {
                throw new ArgumentException("Destination too short.", nameof(destination));
            }

            ushort tag = (ushort)Tag, type = (ushort)Type;
            if (context.IsLittleEndian != BitConverter.IsLittleEndian)
            {
                tag = BinaryPrimitives.ReverseEndianness(tag);
                type = BinaryPrimitives.ReverseEndianness(type);
            }
            MemoryMarshal.Write(destination, ref tag);
            MemoryMarshal.Write(destination.Slice(2), ref type);

            destination = destination.Slice(4);
            if (context.ByteCountOfValueOffsetField == 4)
            {
                uint count32 = (uint)ValueCount, offset32 = (uint)ValueOffset;
                if (context.IsLittleEndian != BitConverter.IsLittleEndian)
                {
                    count32 = BinaryPrimitives.ReverseEndianness(count32);
                    offset32 = BinaryPrimitives.ReverseEndianness(offset32);
                }
                MemoryMarshal.Write(destination, ref count32);
                MemoryMarshal.Write(destination.Slice(4), ref offset32);

                return 12;
            }
            else if (context.ByteCountOfValueOffsetField == 8)
            {
                if (destination.Length < 16)
                {
                    throw new ArgumentException("Destination too short.", nameof(destination));
                }

                ulong count64 = (ulong)ValueCount, offset64 = (ulong)ValueOffset;
                if (context.IsLittleEndian != BitConverter.IsLittleEndian)
                {
                    count64 = BinaryPrimitives.ReverseEndianness(count64);
                    offset64 = BinaryPrimitives.ReverseEndianness(offset64);
                }
                MemoryMarshal.Write(destination, ref count64);
                MemoryMarshal.Write(destination.Slice(8), ref offset64);

                return 20;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        internal bool IsKnownSingleByte()
        {
            TiffFieldType type = Type;
            return type == TiffFieldType.Byte || type == TiffFieldType.ASCII || type == TiffFieldType.SByte ||
                   type == TiffFieldType.Undefined;
        }

        /// <summary>
        /// Try to determine the total byte count of the values.
        /// </summary>
        /// <param name="bytesLength"></param>
        /// <returns></returns>
        public bool TryDetermineValueLength(out long bytesLength)
        {
            switch (Type)
            {
                case TiffFieldType.Byte:
                    bytesLength = ValueCount;
                    return true;
                case TiffFieldType.ASCII:
                    bytesLength = ValueCount;
                    return true;
                case TiffFieldType.Short:
                    bytesLength = 2 * ValueCount;
                    return true;
                case TiffFieldType.Long:
                    bytesLength = 4 * ValueCount;
                    return true;
                case TiffFieldType.Rational:
                    bytesLength = 8 * ValueCount;
                    return true;
                case TiffFieldType.SByte:
                    bytesLength = ValueCount;
                    return true;
                case TiffFieldType.Undefined:
                    bytesLength = ValueCount;
                    return true;
                case TiffFieldType.SShort:
                    bytesLength = 2 * ValueCount;
                    return true;
                case TiffFieldType.SLong:
                    bytesLength = 4 * ValueCount;
                    return true;
                case TiffFieldType.SRational:
                    bytesLength = 8 * ValueCount;
                    return true;
                case TiffFieldType.Float:
                    bytesLength = 4 * ValueCount;
                    return true;
                case TiffFieldType.Double:
                    bytesLength = 8 * ValueCount;
                    return true;
                case TiffFieldType.IFD:
                    bytesLength = 4 * ValueCount;
                    return true;
                case TiffFieldType.Long8:
                    bytesLength = 8 * ValueCount;
                    return true;
                case TiffFieldType.SLong8:
                    bytesLength = 8 * ValueCount;
                    return true;
                case TiffFieldType.IFD8:
                    bytesLength = 8 * ValueCount;
                    return true;
            }

            bytesLength = 0;
            return false;
        }

        internal bool TryDetermineInlined(TiffOperationContext context, out bool isInlined)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!TryDetermineValueLength(out long bytesLength))
            {
                isInlined = false;
                return false;
            }

            isInlined = bytesLength <= context.ByteCountOfValueOffsetField;
            return true;
        }

        /// <summary>
        /// Restore the ValueOffset field into original bytes in the TIFF stream.
        /// </summary>
        /// <param name="context">Parameters of how the TIFF file should be parsed.</param>
        /// <param name="destination">The destination buffer.</param>
        public void RestoreRawOffsetBytes(TiffOperationContext context, Span<byte> destination)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (destination.Length < context.ByteCountOfValueOffsetField)
            {
                throw new ArgumentException($"Destination requires at least {context.ByteCountOfValueOffsetField} bytes.", nameof(destination));
            }

            if (context.ByteCountOfValueOffsetField == 4)
            {
                uint valueOffset32 = (uint)ValueOffset;
                MemoryMarshal.Write(destination, ref valueOffset32);
            }
            else
            {
                ulong valueOffset64 = (ulong)ValueOffset;
                MemoryMarshal.Write(destination, ref valueOffset64);
            }

            if (context.IsLittleEndian != BitConverter.IsLittleEndian)
            {
                destination.Slice(0, context.ByteCountOfValueOffsetField).Reverse();
            }
        }

        /// <inheritdoc />
        public bool Equals(TiffImageFileDirectoryEntry other)
        {
            return Tag == other.Tag && Type == other.Type && ValueCount == other.ValueCount && ValueOffset == other.ValueOffset;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is TiffImageFileDirectoryEntry other && Equals(other);
        }

        /// <summary>
        /// Compares two objects for equality.
        /// </summary>
        /// <param name="left">The object on the left side of the operand.</param>
        /// <param name="right">The object on the right side of the operand.</param>
        /// <returns>True if the <paramref name="left"/> parameter is equal to the <paramref name="right"/> parameter; otherwise, false.</returns>
        public static bool operator ==(in TiffImageFileDirectoryEntry left, in TiffImageFileDirectoryEntry right) => left.Equals(right);

        /// <summary>
        /// Compares two objects for equality.
        /// </summary>
        /// <param name="left">The object on the left side of the operand.</param>
        /// <param name="right">The object on the right side of the operand.</param>
        /// <returns>True if the <paramref name="left"/> parameter is not equal to the <paramref name="right"/> parameter; otherwise, false.</returns>
        public static bool operator !=(in TiffImageFileDirectoryEntry left, in TiffImageFileDirectoryEntry right) => !left.Equals(right);

        /// <inheritdoc />
        public override int GetHashCode()
        {
#if NO_HASHCODE
            int hashCode = (((ushort)Tag) << 16 | (ushort)Type).GetHashCode();
            return HashHelpers.Combine(HashHelpers.Combine(hashCode, ValueCount.GetHashCode()), ValueOffset.GetHashCode());
#else
            return HashCode.Combine(Tag, Type, ValueCount, ValueOffset);
#endif
        }
    }

    internal class TiffImageFileDirectoryEntryComparer : Comparer<TiffImageFileDirectoryEntry>
    {
        public static readonly TiffImageFileDirectoryEntryComparer Instance = new TiffImageFileDirectoryEntryComparer();

        public override int Compare(TiffImageFileDirectoryEntry x, TiffImageFileDirectoryEntry y)
        {
            if (x.Tag > y.Tag)
            {
                return 1;
            }
            else if (x.Tag < y.Tag)
            {
                return -1;
            }
            return 0;
        }
    }
}
