using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace TiffLibrary
{
    public partial class TiffFieldReader
    {

        #region Byte

        /// <summary>
        /// Read bytes from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="destination">The destination buffer.</param>
        /// <param name="sizePerElement">Byte count per element.</param>
        /// <returns>A <see cref="ValueTask"/> that completes when the value of the IFD entry is read.</returns>
        public ValueTask ReadByteFieldAsync(TiffImageFileDirectoryEntry entry, Memory<byte> destination, int sizePerElement = 0)
            => ReadByteFieldAsync(GetAsyncReader(), entry, destination, sizePerElement);

        private ValueTask ReadByteFieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, Memory<byte> destination, int sizePerElement = 0)
        {
            long length;
            if (_context is null)
            {
                throw new ObjectDisposedException(nameof(TiffFieldReader));
            }

            if (sizePerElement <= 0)
            {
                if (!entry.TryDetermineValueLength(out length))
                {
                    throw new InvalidOperationException();
                }
            }
            else
            {
                length = sizePerElement * entry.ValueCount;
            }

            if (destination.Length < length)
            {
                throw new ArgumentOutOfRangeException(nameof(destination));
            }

            // is inlined ?
            if (length <= _context.ByteCountOfValueOffsetField)
            {
                Span<byte> rawOffset = stackalloc byte[8];
                entry.RestoreRawOffsetBytes(_context, rawOffset);
                rawOffset.Slice(0, (int)length).CopyTo(destination.Span);
                return default;
            }

            return new ValueTask(SlowReadByteFieldAsync(reader, entry, destination.Slice(0, checked((int)length))));
        }

        private async Task SlowReadByteFieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, Memory<byte> destination)
        {
            int readCount;
            int length = destination.Length;

            _cancellationToken.ThrowIfCancellationRequested();

            if (MemoryMarshal.TryGetArray(destination, out ArraySegment<byte> segment))
            {
                readCount = await reader.ReadAsync(entry.ValueOffset, segment, _cancellationToken).ConfigureAwait(false);
            }
            else
            {
                byte[] buffer = ArrayPool<byte>.Shared.Rent(length);
                try
                {
                    readCount = await reader.ReadAsync(entry.ValueOffset, new ArraySegment<byte>(buffer, 0, length), _cancellationToken).ConfigureAwait(false);
                    new Span<byte>(buffer, 0, readCount).CopyTo(destination.Span);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }

            if (readCount != length)
            {
                throw new InvalidDataException();
            }
        }

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Byte"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<byte>> ReadByteFieldAsync(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadByteFieldAsync(GetAsyncReader(), entry, skipTypeValidation);

        private ValueTask<TiffValueCollection<byte>> ReadByteFieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
        {
            if (_context is null)
            {
                throw new ObjectDisposedException(nameof(TiffFieldReader));
            }
            if (!skipTypeValidation && !entry.IsKnownSingleByte())
            {
                throw new InvalidOperationException();
            }

            // is inlined ?
            long valueCount = entry.ValueCount;
            if (valueCount <= _context.ByteCountOfValueOffsetField)
            {
                Span<byte> rawOffset = stackalloc byte[8];
                entry.RestoreRawOffsetBytes(_context, rawOffset);
                if (valueCount == 1)
                {
                    return new ValueTask<TiffValueCollection<byte>>(TiffValueCollection.Single(rawOffset[0]));
                }
                byte[] values = new byte[valueCount];
                rawOffset.Slice(0, (int)valueCount).CopyTo(values);
                return new ValueTask<TiffValueCollection<byte>>(TiffValueCollection.UnsafeWrap(values));
            }

            return new ValueTask<TiffValueCollection<byte>>(SlowReadByteFieldAsync(reader, entry));
        }

        private async Task<TiffValueCollection<byte>> SlowReadByteFieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            byte[] buffer = new byte[entry.ValueCount];

            int readCount = await reader.ReadAsync(entry.ValueOffset, new ArraySegment<byte>(buffer, 0, checked((int)entry.ValueCount)), _cancellationToken).ConfigureAwait(false);
            if (readCount != entry.ValueCount)
            {
                throw new InvalidDataException();
            }

            return TiffValueCollection.UnsafeWrap(buffer);
        }

        private async Task<TiffValueCollection<TDest>> SlowReadByteFieldAsync<TDest>(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, Func<byte, TDest> convertFunc) where TDest : struct
        {
            _cancellationToken.ThrowIfCancellationRequested();

            int fieldLength = checked((int)entry.ValueCount);
            byte[] buffer = ArrayPool<byte>.Shared.Rent(fieldLength);
            try
            {
                int readCount = await reader.ReadAsync(entry.ValueOffset, new ArraySegment<byte>(buffer, 0, fieldLength), _cancellationToken).ConfigureAwait(false);
                if (readCount != entry.ValueCount)
                {
                    throw new InvalidDataException();
                }

                TDest[] values = new TDest[readCount];
                InternalCopyByteValues(buffer, values, convertFunc);
                return TiffValueCollection.UnsafeWrap(values);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        #endregion

        #region SByte

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SByte"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<sbyte>> ReadSByteFieldAsync(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadSByteFieldAsync(GetAsyncReader(), entry, skipTypeValidation);

        private ValueTask<TiffValueCollection<sbyte>> ReadSByteFieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
        {
            if (_context is null)
            {
                throw new ObjectDisposedException(nameof(TiffFieldReader));
            }
            if (!skipTypeValidation && !entry.IsKnownSingleByte())
            {
                throw new InvalidOperationException();
            }

            // is inlined ?
            long valueCount = entry.ValueCount;
            if (valueCount <= _context.ByteCountOfValueOffsetField)
            {
                Span<byte> rawOffset = stackalloc byte[8];
                entry.RestoreRawOffsetBytes(_context, rawOffset);
                if (valueCount == 1)
                {
                    return new ValueTask<TiffValueCollection<sbyte>>(TiffValueCollection.Single((sbyte)rawOffset[0]));
                }
                sbyte[] values = new sbyte[valueCount];
                MemoryMarshal.Cast<byte, sbyte>(rawOffset).Slice(0, (int)valueCount).CopyTo(values);
                return new ValueTask<TiffValueCollection<sbyte>>(TiffValueCollection.UnsafeWrap(values));
            }

            return new ValueTask<TiffValueCollection<sbyte>>(SlowReadSByteFieldAsync(reader, entry));
        }

        private async Task<TiffValueCollection<sbyte>> SlowReadSByteFieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            int fieldLength = checked((int)entry.ValueCount);
            byte[] buffer = ArrayPool<byte>.Shared.Rent(fieldLength);
            try
            {
                int readCount = await reader.ReadAsync(entry.ValueOffset, new ArraySegment<byte>(buffer, 0, fieldLength), _cancellationToken).ConfigureAwait(false);
                if (readCount != entry.ValueCount)
                {
                    throw new InvalidDataException();
                }

                sbyte[] values = new sbyte[readCount];
                MemoryMarshal.Cast<byte, sbyte>(new Span<byte>(buffer, 0, readCount)).CopyTo(values);
                return TiffValueCollection.UnsafeWrap(values);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        #endregion

        #region ASCII

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.ASCII"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<string>> ReadASCIIFieldAsync(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadASCIIFieldAsync(GetAsyncReader(), entry, skipTypeValidation);

        private ValueTask<TiffValueCollection<string>> ReadASCIIFieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
        {
            if (_context is null)
            {
                throw new ObjectDisposedException(nameof(TiffFieldReader));
            }
            if (!skipTypeValidation && entry.Type != TiffFieldType.ASCII)
            {
                throw new InvalidOperationException();
            }

            // is inlined ?
            long valueCount = entry.ValueCount;
            if (valueCount <= _context.ByteCountOfValueOffsetField)
            {
                Span<byte> rawOffset = stackalloc byte[8];
                entry.RestoreRawOffsetBytes(_context, rawOffset);
                return new ValueTask<TiffValueCollection<string>>(ParseASCIIArray(rawOffset.Slice(0, (int)valueCount)));
            }

            return new ValueTask<TiffValueCollection<string>>(SlowReadASCIIFieldAsync(reader, entry));
        }

        private async Task<TiffValueCollection<string>> SlowReadASCIIFieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            int fieldLength = checked((int)entry.ValueCount);
            byte[] buffer = ArrayPool<byte>.Shared.Rent(fieldLength);
            try
            {
                int readCount = await reader.ReadAsync(entry.ValueOffset, new ArraySegment<byte>(buffer, 0, fieldLength), _cancellationToken).ConfigureAwait(false);
                if (readCount != entry.ValueCount)
                {
                    throw new InvalidDataException();
                }

                return ParseASCIIArray(new ReadOnlySpan<byte>(buffer, 0, readCount));
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        #endregion

        #region Short

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Short"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<ushort>> ReadShortFieldAsync(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadShortFieldAsync(GetAsyncReader(), entry, skipTypeValidation);

        private ValueTask<TiffValueCollection<ushort>> ReadShortFieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
        {
            if (_context is null)
            {
                throw new ObjectDisposedException(nameof(TiffFieldReader));
            }

            long valueCount = entry.ValueCount;
            Span<byte> rawOffset = stackalloc byte[8];

            if (skipTypeValidation || entry.Type == TiffFieldType.Short || entry.Type == TiffFieldType.SShort)
            {
                // is inlined ?
                if (sizeof(ushort) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<ushort>>(TiffValueCollection.Single(
                            _context.IsLittleEndian
                                ? BinaryPrimitives.ReadUInt16LittleEndian(rawOffset)
                                : BinaryPrimitives.ReadUInt16BigEndian(rawOffset)));
                    }
                    ushort[] values = new ushort[valueCount];
                    InternalCopyInt16Values<ushort>(rawOffset, values, null);
                    return new ValueTask<TiffValueCollection<ushort>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<ushort>>(SlowReadShortFieldAsync<ushort>(reader, entry));
            }
            else if (entry.Type == TiffFieldType.Byte)
            {
                // is inlined ?
                if (valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<ushort>>(TiffValueCollection.Single<ushort>(rawOffset[0]));
                    }
                    ushort[] values = new ushort[valueCount];
                    InternalCopyByteValues<ushort>(rawOffset, values, v => v);
                    return new ValueTask<TiffValueCollection<ushort>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<ushort>>(SlowReadByteFieldAsync<ushort>(reader, entry, v => v));
            }

            throw new InvalidOperationException();
        }

        private async Task<TiffValueCollection<TDest>> SlowReadShortFieldAsync<TDest>(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, Func<short, TDest>? convertFunc = null) where TDest : struct
        {
            _cancellationToken.ThrowIfCancellationRequested();

            int fieldLength = checked(sizeof(ushort) * (int)entry.ValueCount);
            byte[] buffer = ArrayPool<byte>.Shared.Rent(fieldLength);
            try
            {
                int readCount = await reader.ReadAsync(entry.ValueOffset, new ArraySegment<byte>(buffer, 0, fieldLength), _cancellationToken).ConfigureAwait(false);
                if (readCount != fieldLength)
                {
                    throw new InvalidDataException();
                }

                TDest[] values = new TDest[entry.ValueCount];
                InternalCopyInt16Values(buffer, values, convertFunc);
                return TiffValueCollection.UnsafeWrap(values);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        #endregion

        #region SShort

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SShort"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<short>> ReadSShortFieldAsync(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadSShortFieldAsync(GetAsyncReader(), entry, skipTypeValidation);

        private ValueTask<TiffValueCollection<short>> ReadSShortFieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
        {
            if (_context is null)
            {
                throw new ObjectDisposedException(nameof(TiffFieldReader));
            }

            long valueCount = entry.ValueCount;
            Span<byte> rawOffset = stackalloc byte[8];

            if (skipTypeValidation || entry.Type == TiffFieldType.SShort || entry.Type == TiffFieldType.Short)
            {
                // is inlined ?
                if (sizeof(short) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<short>>(TiffValueCollection.Single(
                            _context.IsLittleEndian
                                ? BinaryPrimitives.ReadInt16LittleEndian(rawOffset)
                                : BinaryPrimitives.ReadInt16BigEndian(rawOffset)));
                    }
                    short[] values = new short[valueCount];
                    InternalCopyInt16Values<short>(rawOffset, values, null);
                    return new ValueTask<TiffValueCollection<short>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<short>>(SlowReadShortFieldAsync<short>(reader, entry));
            }
            else if (entry.Type == TiffFieldType.SByte)
            {
                // is inlined ?
                if (valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<short>>(TiffValueCollection.Single<short>(rawOffset[0]));
                    }
                    short[] values = new short[valueCount];
                    InternalCopyByteValues<short>(rawOffset, values, v => (sbyte)v);
                    return new ValueTask<TiffValueCollection<short>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<short>>(SlowReadByteFieldAsync<short>(reader, entry, v => (sbyte)v));
            }

            throw new InvalidOperationException();
        }

        #endregion

        #region Long

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Long"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<uint>> ReadLongFieldAsync(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadLongFieldAsync(GetAsyncReader(), entry, skipTypeValidation);

        private ValueTask<TiffValueCollection<uint>> ReadLongFieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
        {
            if (_context is null)
            {
                throw new ObjectDisposedException(nameof(TiffFieldReader));
            }

            long valueCount = entry.ValueCount;
            Span<byte> rawOffset = stackalloc byte[8];
            if (skipTypeValidation || entry.Type == TiffFieldType.Long || entry.Type == TiffFieldType.SLong || entry.Type == TiffFieldType.IFD)
            {
                // is inlined ?
                if (sizeof(uint) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<uint>>(TiffValueCollection.Single(
                            _context.IsLittleEndian
                                ? BinaryPrimitives.ReadUInt32LittleEndian(rawOffset)
                                : BinaryPrimitives.ReadUInt32BigEndian(rawOffset)));
                    }
                    uint[] values = new uint[valueCount];
                    InternalCopyInt32Values<uint>(rawOffset, values, null);
                    return new ValueTask<TiffValueCollection<uint>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<uint>>(SlowReadLongFieldAsync<uint>(reader, entry));
            }
            else if (entry.Type == TiffFieldType.Short)
            {
                // is inlined ?
                if (sizeof(ushort) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<uint>>(TiffValueCollection.Single<uint>(
                            _context.IsLittleEndian
                                ? BinaryPrimitives.ReadUInt16LittleEndian(rawOffset)
                                : BinaryPrimitives.ReadUInt16BigEndian(rawOffset)));
                    }
                    uint[] values = new uint[valueCount];
                    InternalCopyInt16Values<uint>(rawOffset, values, v => (ushort)v);
                    return new ValueTask<TiffValueCollection<uint>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<uint>>(SlowReadShortFieldAsync<uint>(reader, entry, v => (ushort)v));
            }
            else if (entry.Type == TiffFieldType.Byte)
            {
                // is inlined ?
                if (valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<uint>>(TiffValueCollection.Single<uint>(rawOffset[0]));
                    }
                    uint[] values = new uint[valueCount];
                    InternalCopyByteValues<uint>(rawOffset, values, v => v);
                    return new ValueTask<TiffValueCollection<uint>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<uint>>(SlowReadByteFieldAsync<uint>(reader, entry, v => v));
            }

            throw new InvalidOperationException();
        }

        private async Task<TiffValueCollection<TDest>> SlowReadLongFieldAsync<TDest>(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, Func<int, TDest>? convertFunc = null) where TDest : struct
        {
            _cancellationToken.ThrowIfCancellationRequested();

            int fieldLength = checked(sizeof(uint) * (int)entry.ValueCount);
            byte[] buffer = ArrayPool<byte>.Shared.Rent(fieldLength);
            try
            {
                int readCount = await reader.ReadAsync(entry.ValueOffset, new ArraySegment<byte>(buffer, 0, fieldLength), _cancellationToken).ConfigureAwait(false);
                if (readCount != fieldLength)
                {
                    throw new InvalidDataException();
                }

                TDest[] values = new TDest[entry.ValueCount];
                InternalCopyInt32Values(buffer, values, convertFunc);
                return TiffValueCollection.UnsafeWrap(values);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        #endregion

        #region SLong

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SLong"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<int>> ReadSLongFieldAsync(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadSLongFieldAsync(GetAsyncReader(), entry, skipTypeValidation);

        private ValueTask<TiffValueCollection<int>> ReadSLongFieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
        {
            if (_context is null)
            {
                throw new ObjectDisposedException(nameof(TiffFieldReader));
            }

            long valueCount = entry.ValueCount;
            Span<byte> rawOffset = stackalloc byte[8];
            if (skipTypeValidation || entry.Type == TiffFieldType.Long || entry.Type == TiffFieldType.SLong || entry.Type == TiffFieldType.IFD)
            {
                // is inlined ?
                if (sizeof(int) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<int>>(TiffValueCollection.Single<int>(
                            _context.IsLittleEndian
                                ? BinaryPrimitives.ReadInt32LittleEndian(rawOffset)
                                : BinaryPrimitives.ReadInt32BigEndian(rawOffset)));
                    }
                    int[] values = new int[valueCount];
                    InternalCopyInt32Values<int>(rawOffset, values, null);
                    return new ValueTask<TiffValueCollection<int>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<int>>(SlowReadLongFieldAsync<int>(reader, entry));
            }
            else if (entry.Type == TiffFieldType.Short)
            {
                // is inlined ?
                if (sizeof(ushort) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<int>>(TiffValueCollection.Single<int>(
                            _context.IsLittleEndian
                                ? BinaryPrimitives.ReadUInt16LittleEndian(rawOffset)
                                : BinaryPrimitives.ReadUInt16BigEndian(rawOffset)));
                    }
                    int[] values = new int[valueCount];
                    InternalCopyInt16Values<int>(rawOffset, values, v => (ushort)v);
                    return new ValueTask<TiffValueCollection<int>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<int>>(SlowReadShortFieldAsync<int>(reader, entry, v => (ushort)v));
            }
            else if (entry.Type == TiffFieldType.SShort)
            {
                // is inlined ?
                if (sizeof(short) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<int>>(TiffValueCollection.Single<int>(
                            _context.IsLittleEndian
                                ? BinaryPrimitives.ReadInt16LittleEndian(rawOffset)
                                : BinaryPrimitives.ReadInt16BigEndian(rawOffset)));
                    }
                    int[] values = new int[valueCount];
                    InternalCopyInt16Values<int>(rawOffset, values, v => v);
                    return new ValueTask<TiffValueCollection<int>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<int>>(SlowReadShortFieldAsync<int>(reader, entry, v => v));
            }
            else if (entry.Type == TiffFieldType.Byte)
            {
                // is inlined ?
                if (valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<int>>(TiffValueCollection.Single<int>(rawOffset[0]));
                    }
                    int[] values = new int[valueCount];
                    InternalCopyByteValues<int>(rawOffset, values, v => v);
                    return new ValueTask<TiffValueCollection<int>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<int>>(SlowReadByteFieldAsync<int>(reader, entry, v => v));
            }
            else if (entry.Type == TiffFieldType.SByte)
            {
                // is inlined ?
                if (valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<int>>(TiffValueCollection.Single<int>((sbyte)rawOffset[0]));
                    }
                    int[] values = new int[valueCount];
                    InternalCopyByteValues<int>(rawOffset, values, v => (sbyte)v);
                    return new ValueTask<TiffValueCollection<int>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<int>>(SlowReadByteFieldAsync<int>(reader, entry, v => (sbyte)v));
            }

            throw new InvalidOperationException();
        }

        #endregion

        #region Long8

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Long8"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<ulong>> ReadLong8FieldAsync(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadLong8FieldAsync(GetAsyncReader(), entry, skipTypeValidation);

        private ValueTask<TiffValueCollection<ulong>> ReadLong8FieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
        {
            if (_context is null)
            {
                throw new ObjectDisposedException(nameof(TiffFieldReader));
            }

            long valueCount = entry.ValueCount;
            Span<byte> rawOffset = stackalloc byte[8];
            if (skipTypeValidation || entry.Type == TiffFieldType.Long8 || entry.Type == TiffFieldType.SLong8 || entry.Type == TiffFieldType.IFD8)
            {
                // is inlined ?
                if (sizeof(ulong) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<ulong>>(TiffValueCollection.Single(
                            _context.IsLittleEndian
                                ? BinaryPrimitives.ReadUInt64LittleEndian(rawOffset)
                                : BinaryPrimitives.ReadUInt64BigEndian(rawOffset)));
                    }
                    ulong[] values = new ulong[valueCount];
                    InternalCopyInt64Values<ulong>(rawOffset, values, null);
                    return new ValueTask<TiffValueCollection<ulong>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<ulong>>(SlowReadLong8FieldAsync<ulong>(reader, entry));
            }
            else if (entry.Type == TiffFieldType.Long)
            {
                // is inlined ?
                if (sizeof(uint) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<ulong>>(TiffValueCollection.Single<ulong>(
                            _context.IsLittleEndian
                                ? BinaryPrimitives.ReadUInt32LittleEndian(rawOffset)
                                : BinaryPrimitives.ReadUInt32BigEndian(rawOffset)));
                    }
                    ulong[] values = new ulong[valueCount];
                    InternalCopyInt32Values<ulong>(rawOffset, values, v => (uint)v);
                    return new ValueTask<TiffValueCollection<ulong>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<ulong>>(SlowReadLongFieldAsync<ulong>(reader, entry, v => (uint)v));
            }
            else if (entry.Type == TiffFieldType.Short)
            {
                // is inlined ?
                if (sizeof(ushort) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<ulong>>(TiffValueCollection.Single<ulong>(
                            _context.IsLittleEndian
                                ? BinaryPrimitives.ReadUInt16LittleEndian(rawOffset)
                                : BinaryPrimitives.ReadUInt16BigEndian(rawOffset)));
                    }
                    ulong[] values = new ulong[valueCount];
                    InternalCopyInt16Values<ulong>(rawOffset, values, v => (ushort)v);
                    return new ValueTask<TiffValueCollection<ulong>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<ulong>>(SlowReadShortFieldAsync<ulong>(reader, entry, v => (ushort)v));
            }
            else if (entry.Type == TiffFieldType.Byte)
            {
                // is inlined ?
                if (valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<ulong>>(TiffValueCollection.Single<ulong>(rawOffset[0]));
                    }
                    ulong[] values = new ulong[valueCount];
                    InternalCopyByteValues<ulong>(rawOffset, values, v => v);
                    return new ValueTask<TiffValueCollection<ulong>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<ulong>>(SlowReadByteFieldAsync<ulong>(reader, entry, v => v));
            }

            throw new InvalidOperationException();
        }

        private async Task<TiffValueCollection<TDest>> SlowReadLong8FieldAsync<TDest>(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, Func<long, TDest>? convertFunc = null) where TDest : struct
        {
            _cancellationToken.ThrowIfCancellationRequested();

            int fieldLength = checked(sizeof(ulong) * (int)entry.ValueCount);
            byte[] buffer = ArrayPool<byte>.Shared.Rent(fieldLength);
            try
            {
                int readCount = await reader.ReadAsync(entry.ValueOffset, new ArraySegment<byte>(buffer, 0, fieldLength), _cancellationToken).ConfigureAwait(false);
                if (readCount != fieldLength)
                {
                    throw new InvalidDataException();
                }

                TDest[] values = new TDest[entry.ValueCount];
                InternalCopyInt64Values(buffer, values, convertFunc);
                return TiffValueCollection.UnsafeWrap(values);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        #endregion

        #region SLong8

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SLong8"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<long>> ReadSLong8FieldAsync(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadSLong8FieldAsync(GetAsyncReader(), entry, skipTypeValidation);

        private ValueTask<TiffValueCollection<long>> ReadSLong8FieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
        {
            if (_context is null)
            {
                throw new ObjectDisposedException(nameof(TiffFieldReader));
            }

            long valueCount = entry.ValueCount;
            Span<byte> rawOffset = stackalloc byte[8];
            if (skipTypeValidation || entry.Type == TiffFieldType.SLong8 || entry.Type == TiffFieldType.Long8 || entry.Type == TiffFieldType.IFD8)
            {
                // is inlined ?
                if (sizeof(long) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<long>>(TiffValueCollection.Single(
                            _context.IsLittleEndian
                                ? BinaryPrimitives.ReadInt64LittleEndian(rawOffset)
                                : BinaryPrimitives.ReadInt64BigEndian(rawOffset)));
                    }
                    long[] values = new long[valueCount];
                    InternalCopyInt64Values<long>(rawOffset, values, null);
                    return new ValueTask<TiffValueCollection<long>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<long>>(SlowReadLong8FieldAsync<long>(reader, entry));
            }
            else if (entry.Type == TiffFieldType.SLong)
            {
                // is inlined ?
                if (sizeof(int) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<long>>(TiffValueCollection.Single<long>(
                            _context.IsLittleEndian
                                ? BinaryPrimitives.ReadInt32LittleEndian(rawOffset)
                                : BinaryPrimitives.ReadInt32BigEndian(rawOffset)));
                    }
                    long[] values = new long[valueCount];
                    InternalCopyInt32Values<long>(rawOffset, values, v => v);
                    return new ValueTask<TiffValueCollection<long>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<long>>(SlowReadLongFieldAsync<long>(reader, entry, v => v));
            }
            else if (entry.Type == TiffFieldType.SShort)
            {
                // is inlined ?
                if (sizeof(short) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<long>>(TiffValueCollection.Single<long>(
                            _context.IsLittleEndian
                                ? BinaryPrimitives.ReadInt16LittleEndian(rawOffset)
                                : BinaryPrimitives.ReadInt16BigEndian(rawOffset)));
                    }
                    long[] values = new long[valueCount];
                    InternalCopyInt16Values<long>(rawOffset, values, v => v);
                    return new ValueTask<TiffValueCollection<long>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<long>>(SlowReadShortFieldAsync<long>(reader, entry, v => v));
            }
            else if (entry.Type == TiffFieldType.SByte)
            {
                // is inlined ?
                if (valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<long>>(TiffValueCollection.Single<long>(rawOffset[0]));
                    }
                    long[] values = new long[valueCount];
                    InternalCopyByteValues<long>(rawOffset, values, v => v);
                    return new ValueTask<TiffValueCollection<long>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<long>>(SlowReadByteFieldAsync<long>(reader, entry, v => v));
            }

            throw new InvalidOperationException();
        }

        #endregion

        #region Float

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Float"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<float>> ReadFloatFieldAsync(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadFloatFieldAsync(GetAsyncReader(), entry, skipTypeValidation);

        private ValueTask<TiffValueCollection<float>> ReadFloatFieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
        {
            if (_context is null)
            {
                throw new ObjectDisposedException(nameof(TiffFieldReader));
            }

            if (!skipTypeValidation && entry.Type != TiffFieldType.Float)
            {
                throw new InvalidOperationException();
            }

            // is inlined ?
            long valueCount = entry.ValueCount;
            if (sizeof(float) * valueCount <= _context.ByteCountOfValueOffsetField)
            {
                Span<byte> rawOffset = stackalloc byte[8];
                entry.RestoreRawOffsetBytes(_context, rawOffset);
                if (valueCount == 1)
                {
                    int value = MemoryMarshal.Read<int>(rawOffset);
                    if (_context.IsLittleEndian != BitConverter.IsLittleEndian)
                    {
                        value = BinaryPrimitives.ReverseEndianness(value);
                    }
                    return new ValueTask<TiffValueCollection<float>>(TiffValueCollection.Single(Int32BitsToSingle(value)));
                }
                float[] values = new float[valueCount];
                InternalCopyFloatValues<float>(rawOffset, values, null);
                return new ValueTask<TiffValueCollection<float>>(TiffValueCollection.UnsafeWrap(values));
            }

            return new ValueTask<TiffValueCollection<float>>(SlowReadFloatFieldAsync<float>(reader, entry));
        }

        private async Task<TiffValueCollection<TDest>> SlowReadFloatFieldAsync<TDest>(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, Func<float, TDest>? convertFunc = null) where TDest : struct
        {
            _cancellationToken.ThrowIfCancellationRequested();

            int fieldLength = checked(sizeof(float) * (int)entry.ValueCount);
            byte[] buffer = ArrayPool<byte>.Shared.Rent(fieldLength);
            try
            {
                int readCount = await reader.ReadAsync(entry.ValueOffset, new ArraySegment<byte>(buffer, 0, fieldLength), _cancellationToken).ConfigureAwait(false);
                if (readCount != fieldLength)
                {
                    throw new InvalidDataException();
                }

                TDest[] values = new TDest[entry.ValueCount];
                InternalCopyFloatValues(buffer, values, convertFunc);
                return TiffValueCollection.UnsafeWrap(values);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        #endregion

        #region Double

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Double"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<double>> ReadDoubleFieldAsync(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadDoubleFieldAsync(GetAsyncReader(), entry, skipTypeValidation);

        private ValueTask<TiffValueCollection<double>> ReadDoubleFieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
        {
            if (_context is null)
            {
                throw new ObjectDisposedException(nameof(TiffFieldReader));
            }

            if (!skipTypeValidation && entry.Type != TiffFieldType.Double)
            {
                throw new InvalidOperationException();
            }

            // is inlined ?
            long valueCount = entry.ValueCount;
            if (sizeof(double) * valueCount <= _context.ByteCountOfValueOffsetField)
            {
                Span<byte> rawOffset = stackalloc byte[8];
                entry.RestoreRawOffsetBytes(_context, rawOffset);
                if (valueCount == 1)
                {
                    long value = MemoryMarshal.Read<long>(rawOffset);
                    if (_context.IsLittleEndian != BitConverter.IsLittleEndian)
                    {
                        value = BinaryPrimitives.ReverseEndianness(value);
                    }
                    return new ValueTask<TiffValueCollection<double>>(TiffValueCollection.Single(Int64BitsToDouble(value)));
                }
                double[] values = new double[valueCount];
                InternalCopyDoubleValues<double>(rawOffset, values, null);
                return new ValueTask<TiffValueCollection<double>>(TiffValueCollection.UnsafeWrap(values));
            }

            return new ValueTask<TiffValueCollection<double>>(SlowReadDoubleFieldAsync<double>(reader, entry));
        }

        private async Task<TiffValueCollection<TDest>> SlowReadDoubleFieldAsync<TDest>(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, Func<double, TDest>? convertFunc = null) where TDest : struct
        {
            _cancellationToken.ThrowIfCancellationRequested();

            int fieldLength = checked(sizeof(float) * (int)entry.ValueCount);
            byte[] buffer = ArrayPool<byte>.Shared.Rent(fieldLength);
            try
            {
                int readCount = await reader.ReadAsync(entry.ValueOffset, new ArraySegment<byte>(buffer, 0, fieldLength), _cancellationToken).ConfigureAwait(false);
                if (readCount != fieldLength)
                {
                    throw new InvalidDataException();
                }

                TDest[] values = new TDest[entry.ValueCount];
                InternalCopyDoubleValues(buffer, values, convertFunc);
                return TiffValueCollection.UnsafeWrap(values);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        #endregion

        #region Rational

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Rational"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<TiffRational>> ReadRationalFieldAsync(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadRationalFieldAsync(GetAsyncReader(), entry, skipTypeValidation);

        private ValueTask<TiffValueCollection<TiffRational>> ReadRationalFieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
        {
            if (_context is null)
            {
                throw new ObjectDisposedException(nameof(TiffFieldReader));
            }

            if (!skipTypeValidation && entry.Type != TiffFieldType.Rational)
            {
                throw new InvalidOperationException();
            }

            // is inlined ?
            long valueCount = entry.ValueCount;
            if (8 * valueCount <= _context.ByteCountOfValueOffsetField)
            {
                Span<byte> rawOffset = stackalloc byte[8];
                entry.RestoreRawOffsetBytes(_context, rawOffset);
                if (valueCount == 1)
                {
                    Span<TiffRational> singleValueSpan = stackalloc TiffRational[1];
                    InternalCopyRationalValues(rawOffset, singleValueSpan, null);
                    return new ValueTask<TiffValueCollection<TiffRational>>(TiffValueCollection.Single(singleValueSpan[0]));
                }
                TiffRational[] values = new TiffRational[valueCount];
                InternalCopyRationalValues<TiffRational>(rawOffset, values, null);
                return new ValueTask<TiffValueCollection<TiffRational>>(TiffValueCollection.UnsafeWrap(values));
            }

            return new ValueTask<TiffValueCollection<TiffRational>>(SlowReadRationalFieldAsync<TiffRational>(reader, entry));
        }

        private async Task<TiffValueCollection<TDest>> SlowReadRationalFieldAsync<TDest>(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, Func<TiffRational, TDest>? convertFunc = null) where TDest : struct
        {
            _cancellationToken.ThrowIfCancellationRequested();

            int fieldLength = checked(8 * (int)entry.ValueCount);
            byte[] buffer = ArrayPool<byte>.Shared.Rent(fieldLength);
            try
            {
                int readCount = await reader.ReadAsync(entry.ValueOffset, new ArraySegment<byte>(buffer, 0, fieldLength), _cancellationToken).ConfigureAwait(false);
                if (readCount != fieldLength)
                {
                    throw new InvalidDataException();
                }

                TDest[] values = new TDest[entry.ValueCount];
                InternalCopyRationalValues<TDest>(buffer, values, convertFunc);
                return TiffValueCollection.UnsafeWrap(values);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        #endregion

        #region SRational

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SRational"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<TiffSRational>> ReadSRationalFieldAsync(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadSRationalFieldAsync(GetAsyncReader(), entry, skipTypeValidation);

        private ValueTask<TiffValueCollection<TiffSRational>> ReadSRationalFieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
        {
            if (_context is null)
            {
                throw new ObjectDisposedException(nameof(TiffFieldReader));
            }

            if (!skipTypeValidation && entry.Type != TiffFieldType.SRational)
            {
                throw new InvalidOperationException();
            }

            // is inlined ?
            long valueCount = entry.ValueCount;
            if (8 * valueCount <= _context.ByteCountOfValueOffsetField)
            {
                Span<byte> rawOffset = stackalloc byte[8];
                entry.RestoreRawOffsetBytes(_context, rawOffset);
                if (valueCount == 1)
                {
                    Span<TiffSRational> singleValueSpan = stackalloc TiffSRational[1];
                    InternalCopyRationalValues(rawOffset, singleValueSpan, null);
                    return new ValueTask<TiffValueCollection<TiffSRational>>(TiffValueCollection.Single(singleValueSpan[0]));
                }
                TiffSRational[] values = new TiffSRational[valueCount];
                InternalCopyRationalValues<TiffSRational>(rawOffset, values, null);
                return new ValueTask<TiffValueCollection<TiffSRational>>(TiffValueCollection.UnsafeWrap(values));
            }

            return new ValueTask<TiffValueCollection<TiffSRational>>(SlowReadRationalFieldAsync<TiffSRational>(reader, entry));
        }

        #endregion

        #region IFD

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.IFD"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<TiffStreamOffset>> ReadIFDFieldAsync(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadIFDFieldAsync(GetAsyncReader(), entry, skipTypeValidation);

        private ValueTask<TiffValueCollection<TiffStreamOffset>> ReadIFDFieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
        {
            if (_context is null)
            {
                throw new ObjectDisposedException(nameof(TiffFieldReader));
            }

            long valueCount = entry.ValueCount;
            Span<byte> rawOffset = stackalloc byte[8];
            if (skipTypeValidation || entry.Type == TiffFieldType.Long || entry.Type == TiffFieldType.SLong || entry.Type == TiffFieldType.IFD)
            {
                // is inlined ?
                if (sizeof(int) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<TiffStreamOffset>>(
                                TiffValueCollection.Single(
                                    new TiffStreamOffset(
                                        _context.IsLittleEndian
                                            ? BinaryPrimitives.ReadInt32LittleEndian(rawOffset)
                                            : BinaryPrimitives.ReadInt32BigEndian(rawOffset))));
                    }
                    TiffStreamOffset[] values = new TiffStreamOffset[valueCount];
                    InternalCopyInt32Values(rawOffset, values, offset => new TiffStreamOffset(offset));
                    return new ValueTask<TiffValueCollection<TiffStreamOffset>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<TiffStreamOffset>>(SlowReadLongFieldAsync(reader, entry, offset => new TiffStreamOffset(offset)));
            }

            throw new InvalidOperationException();
        }

        #endregion

        #region IFD8

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.IFD8"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<TiffStreamOffset>> ReadIFD8FieldAsync(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadIFD8FieldAsync(GetAsyncReader(), entry, skipTypeValidation);

        private ValueTask<TiffValueCollection<TiffStreamOffset>> ReadIFD8FieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
        {
            if (_context is null)
            {
                throw new ObjectDisposedException(nameof(TiffFieldReader));
            }

            long valueCount = entry.ValueCount;
            Span<byte> rawOffset = stackalloc byte[8];
            if (skipTypeValidation || entry.Type == TiffFieldType.Long8 || entry.Type == TiffFieldType.SLong8 || entry.Type == TiffFieldType.IFD8)
            {
                // is inlined ?
                if (sizeof(long) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<TiffStreamOffset>>(
                                TiffValueCollection.Single(
                                    new TiffStreamOffset(
                                        _context.IsLittleEndian
                                            ? BinaryPrimitives.ReadInt64LittleEndian(rawOffset)
                                            : BinaryPrimitives.ReadInt64BigEndian(rawOffset))));
                    }
                    TiffStreamOffset[] values = new TiffStreamOffset[valueCount];
                    InternalCopyInt64Values(rawOffset, values, offset => new TiffStreamOffset(offset));
                    return new ValueTask<TiffValueCollection<TiffStreamOffset>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<TiffStreamOffset>>(SlowReadLong8FieldAsync(reader, entry, offset => new TiffStreamOffset(offset)));
            }
            else if (entry.Type == TiffFieldType.Long || entry.Type == TiffFieldType.IFD)
            {
                // is inlined ?
                if (sizeof(uint) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<TiffStreamOffset>>(TiffValueCollection.Single<TiffStreamOffset>(
                            _context.IsLittleEndian
                                ? BinaryPrimitives.ReadUInt32LittleEndian(rawOffset)
                                : BinaryPrimitives.ReadUInt32BigEndian(rawOffset)));
                    }
                    TiffStreamOffset[] values = new TiffStreamOffset[valueCount];
                    InternalCopyInt32Values(rawOffset, values, v => new TiffStreamOffset((uint)v));
                    return new ValueTask<TiffValueCollection<TiffStreamOffset>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<TiffStreamOffset>>(SlowReadLongFieldAsync(reader, entry, v => new TiffStreamOffset((uint)v)));
            }

            throw new InvalidOperationException();
        }

        #endregion

    }
}
