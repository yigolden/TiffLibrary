using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace TiffLibrary
{
    public partial class TiffFieldReader
    {

        #region Byte

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Byte"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<byte>> ReadByteFieldAsync(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadByteFieldAsync(GetAsyncReader(), entry, int.MaxValue, skipTypeValidation, cancellationToken);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Byte"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="sizeLimit">The maximum number of bytes to read from the IFD</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<byte>> ReadByteFieldAsync(TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadByteFieldAsync(GetAsyncReader(), entry, sizeLimit, skipTypeValidation, cancellationToken);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Byte"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="offset">The number of elements to skip reading.</param>
        /// <param name="destination">The destination buffer to store the values.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask"/> that completes when the values are read.</returns>
        public ValueTask ReadByteFieldAsync(TiffImageFileDirectoryEntry entry, int offset, Memory<byte> destination, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadByteFieldAsync(GetAsyncReader(), entry, offset, destination, skipTypeValidation, cancellationToken);

        private ValueTask<TiffValueCollection<byte>> ReadByteFieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
        {
            if (_context is null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(TiffFieldReader));
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (sizeLimit < 0)
            {
                sizeLimit = int.MaxValue;
            }

            if (skipTypeValidation || entry.IsKnownSingleByte())
            {
                // is inlined ?
                long valueCount = entry.ValueCount;
                if (valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    valueCount = Math.Min(sizeLimit, valueCount);
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

                return new ValueTask<TiffValueCollection<byte>>(SlowReadByteFieldAsync<byte>(reader, entry, sizeLimit, null, cancellationToken));
            }

            ThrowHelper.ThrowInvalidOperationException($"Can read tag of type {entry.Type}.");
            return default;
        }

        private ValueTask ReadByteFieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, int offset, Memory<byte> destination, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
        {
            if (_context is null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(TiffFieldReader));
            }

            long valueCount = entry.ValueCount;
            if ((ulong)offset > (ulong)valueCount)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(offset));
            }
            if ((ulong)destination.Length > (ulong)(valueCount - offset))
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(destination));
            }

            cancellationToken.ThrowIfCancellationRequested();

            Span<byte> rawOffset = stackalloc byte[8];

            if (skipTypeValidation || entry.IsKnownSingleByte())
            {
                if (destination.IsEmpty)
                {
                    return default;
                }

                // is inlined ?
                if (valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        Debug.Assert(destination.Length == 1);
                        destination.Span[0] = rawOffset[0];
                    }
                    else
                    {
                        rawOffset.Slice(offset, destination.Length).CopyTo(destination.Span);
                    }
                    return default;
                }

                return new ValueTask(SlowReadByteFieldAsync(reader, entry, offset, destination, null, cancellationToken));
            }

            ThrowHelper.ThrowInvalidOperationException($"Can read tag of type {entry.Type}.");
            return default;
        }

        private static async Task<TiffValueCollection<TDest>> SlowReadByteFieldAsync<TDest>(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, int sizeLimit, Func<byte, TDest>? convertFunc, CancellationToken cancellationToken) where TDest : struct
        {
            Debug.Assert(sizeLimit >= 0);
            TDest[] values = new TDest[Math.Min(sizeLimit, entry.ValueCount)];
            await SlowReadByteFieldAsync(reader, entry, 0, values, convertFunc, cancellationToken).ConfigureAwait(false);
            return TiffValueCollection.UnsafeWrap(values);
        }

        private static async Task SlowReadByteFieldAsync<TDest>(
            TiffFileContentReader reader, TiffImageFileDirectoryEntry entry,
            long offset, Memory<TDest> destination,
            Func<byte, TDest>? convertFunc, CancellationToken cancellationToken) where TDest : struct
        {
            const int sizeOfElement = sizeof(byte);

            int count = destination.Length;
            if (count == 0)
            {
                return;
            }

            Debug.Assert((ulong)offset <= (ulong)entry.ValueCount);
            Debug.Assert((ulong)count <= (ulong)(entry.ValueCount - offset));

            if (typeof(TDest) == typeof(byte) && convertFunc == null)
            {
                int readCount = await reader.ReadAsync(entry.ValueOffset + offset, Unsafe.As<Memory<TDest>, Memory<byte>>(ref destination), cancellationToken).ConfigureAwait(false);
                if (readCount != destination.Length)
                {
                    ThrowHelper.ThrowInvalidDataException("The number of bytes read from file is less than expected.");
                }

                return;
            }

            int fieldLength = checked(sizeOfElement * count);
            byte[] buffer = ArrayPool<byte>.Shared.Rent(fieldLength);
            try
            {
                int readCount = await reader.ReadAsync(entry.ValueOffset + sizeOfElement * offset, new ArraySegment<byte>(buffer, 0, fieldLength), cancellationToken).ConfigureAwait(false);
                if (readCount != fieldLength)
                {
                    ThrowHelper.ThrowInvalidDataException("The number of bytes read from file is less than expected.");
                }

                InternalCopyByteValues(buffer, destination.Span, convertFunc);
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
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        [CLSCompliant(false)]
        public ValueTask<TiffValueCollection<sbyte>> ReadSByteFieldAsync(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadSByteFieldAsync(GetAsyncReader(), entry, int.MaxValue, skipTypeValidation, cancellationToken);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SByte"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="sizeLimit">The maximum number of bytes to read from the IFD</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        [CLSCompliant(false)]
        public ValueTask<TiffValueCollection<sbyte>> ReadSByteFieldAsync(TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadSByteFieldAsync(GetAsyncReader(), entry, sizeLimit, skipTypeValidation, cancellationToken);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SByte"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="offset">The number of elements to skip reading.</param>
        /// <param name="destination">The destination buffer to store the values.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask"/> that completes when the values are read.</returns>
        [CLSCompliant(false)]
        public ValueTask ReadSByteFieldAsync(TiffImageFileDirectoryEntry entry, int offset, Memory<sbyte> destination, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadSByteFieldAsync(GetAsyncReader(), entry, offset, destination, skipTypeValidation, cancellationToken);

        private ValueTask<TiffValueCollection<sbyte>> ReadSByteFieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
        {
            if (_context is null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(TiffFieldReader));
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (sizeLimit < 0)
            {
                sizeLimit = int.MaxValue;
            }

            long valueCount = entry.ValueCount;
            Span<byte> rawOffset = stackalloc byte[8];
            if (skipTypeValidation || entry.IsKnownSingleByte())
            {
                // is inlined ?
                if (valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    valueCount = Math.Min(sizeLimit, valueCount);
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<sbyte>>(TiffValueCollection.Single((sbyte)rawOffset[0]));
                    }
                    sbyte[] values = new sbyte[valueCount];
                    MemoryMarshal.Cast<byte, sbyte>(rawOffset).Slice(0, (int)valueCount).CopyTo(values);
                    return new ValueTask<TiffValueCollection<sbyte>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<sbyte>>(SlowReadByteFieldAsync<sbyte>(reader, entry, sizeLimit, null, cancellationToken));
            }

            ThrowHelper.ThrowInvalidOperationException($"Can read tag of type {entry.Type}.");
            return default;
        }

        private ValueTask ReadSByteFieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, int offset, Memory<sbyte> destination, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
        {
            if (_context is null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(TiffFieldReader));
            }

            long valueCount = entry.ValueCount;
            if ((ulong)offset > (ulong)valueCount)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(offset));
            }
            if ((ulong)destination.Length > (ulong)(valueCount - offset))
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(destination));
            }

            cancellationToken.ThrowIfCancellationRequested();

            Span<byte> rawOffset = stackalloc byte[8];

            if (skipTypeValidation || entry.IsKnownSingleByte())
            {
                if (destination.IsEmpty)
                {
                    return default;
                }

                // is inlined ?
                if (valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        Debug.Assert(destination.Length == 1);
                        destination.Span[0] = (sbyte)rawOffset[0];
                    }
                    else
                    {
                        rawOffset.Slice(offset, destination.Length).CopyTo(MemoryMarshal.Cast<sbyte, byte>(destination.Span));
                    }
                    return default;
                }

                return new ValueTask(SlowReadByteFieldAsync(reader, entry, offset, destination, null, cancellationToken));
            }

            ThrowHelper.ThrowInvalidOperationException($"Can read tag of type {entry.Type}.");
            return default;
        }


        #endregion

        #region ASCII

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.ASCII"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<string>> ReadASCIIFieldAsync(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadASCIIFieldAsync(GetAsyncReader(), entry, skipTypeValidation, cancellationToken);

        /// <summary>
        /// Read the first string value of type <see cref="TiffFieldType.ASCII"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="sizeLimit">The maximum number of bytes to read from the IFD</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the string is read and return the read string.</returns>
        public ValueTask<string> ReadASCIIFieldFirstStringAsync(TiffImageFileDirectoryEntry entry, int sizeLimit = -1, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
                => ReadASCIIFieldFirstStringAsync(GetAsyncReader(), entry, sizeLimit, skipTypeValidation, cancellationToken);

        private ValueTask<TiffValueCollection<string>> ReadASCIIFieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
        {
            if (_context is null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(TiffFieldReader));
            }

            cancellationToken.ThrowIfCancellationRequested();

            long valueCount = entry.ValueCount;
            Span<byte> rawOffset = stackalloc byte[8];
            if (skipTypeValidation || entry.Type == TiffFieldType.ASCII)
            {
                // is inlined ?
                if (valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    return new ValueTask<TiffValueCollection<string>>(ParseASCIIArray(rawOffset.Slice(0, (int)valueCount)));
                }

                return new ValueTask<TiffValueCollection<string>>(SlowReadASCIIFieldAsync(reader, entry, cancellationToken));
            }

            ThrowHelper.ThrowInvalidOperationException($"Can read tag of type {entry.Type}.");
            return default;
        }

        private ValueTask<string> ReadASCIIFieldFirstStringAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
        {
            if (_context is null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(TiffFieldReader));
            }
            if (sizeLimit < 0)
            {
                sizeLimit = int.MaxValue;
            }

            cancellationToken.ThrowIfCancellationRequested();

            long valueCount = entry.ValueCount;
            Span<byte> rawOffset = stackalloc byte[8];
            if (skipTypeValidation || entry.Type == TiffFieldType.ASCII)
            {
                // is inlined ?
                if (valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    rawOffset = rawOffset.Slice(0, (int)valueCount);
                    if (rawOffset.Length > sizeLimit)
                    {
                        rawOffset = rawOffset.Slice(0, sizeLimit);
                    }
                    return new ValueTask<string>(ParseASCIIString(rawOffset));
                }

                return new ValueTask<string>(SlowReadASCIIFieldFirstStringAsync(reader, entry, sizeLimit, cancellationToken));
            }

            ThrowHelper.ThrowInvalidOperationException($"Can read tag of type {entry.Type}.");
            return default;
        }

        private static async Task<TiffValueCollection<string>> SlowReadASCIIFieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, CancellationToken cancellationToken)
        {
            int fieldLength = checked((int)entry.ValueCount);
            byte[] buffer = ArrayPool<byte>.Shared.Rent(fieldLength);
            try
            {
                int readCount = await reader.ReadAsync(entry.ValueOffset, new ArraySegment<byte>(buffer, 0, fieldLength), cancellationToken).ConfigureAwait(false);
                if (readCount != entry.ValueCount)
                {
                    ThrowHelper.ThrowInvalidDataException("The number of bytes read from file is less than expected.");
                }

                return ParseASCIIArray(new ReadOnlySpan<byte>(buffer, 0, readCount));
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        private static async Task<string> SlowReadASCIIFieldFirstStringAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, int sizeLimit, CancellationToken cancellationToken)
        {
            Debug.Assert(sizeLimit > 0);

            int fieldLength = Math.Min(sizeLimit, checked((int)entry.ValueCount));
            byte[] buffer = ArrayPool<byte>.Shared.Rent(fieldLength);
            try
            {
                int readCount = await reader.ReadAsync(entry.ValueOffset, new ArraySegment<byte>(buffer, 0, fieldLength), cancellationToken).ConfigureAwait(false);
                if (readCount != entry.ValueCount)
                {
                    ThrowHelper.ThrowInvalidDataException("The number of bytes read from file is less than expected.");
                }

                return ParseASCIIString(new ReadOnlySpan<byte>(buffer, 0, readCount));
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
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        [CLSCompliant(false)]
        public ValueTask<TiffValueCollection<ushort>> ReadShortFieldAsync(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadShortFieldAsync(GetAsyncReader(), entry, int.MaxValue, skipTypeValidation, cancellationToken);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Short"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        [CLSCompliant(false)]
        public ValueTask<TiffValueCollection<ushort>> ReadShortFieldAsync(TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadShortFieldAsync(GetAsyncReader(), entry, sizeLimit, skipTypeValidation, cancellationToken);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Short"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="offset">The number of elements to skip reading.</param>
        /// <param name="destination">The destination buffer to store the values.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask"/> that completes when the values are read.</returns>
        [CLSCompliant(false)]
        public ValueTask ReadShortFieldAsync(TiffImageFileDirectoryEntry entry, int offset, Memory<ushort> destination, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadShortFieldAsync(GetAsyncReader(), entry, offset, destination, skipTypeValidation, cancellationToken);

        private ValueTask<TiffValueCollection<ushort>> ReadShortFieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
        {
            if (_context is null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(TiffFieldReader));
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (sizeLimit < 0)
            {
                sizeLimit = int.MaxValue;
            }

            long valueCount = entry.ValueCount;
            Span<byte> rawOffset = stackalloc byte[8];

            if (skipTypeValidation || entry.Type == TiffFieldType.Short || entry.Type == TiffFieldType.SShort)
            {
                // is inlined ?
                if (sizeof(ushort) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    valueCount = Math.Min(sizeLimit, valueCount);
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

                return new ValueTask<TiffValueCollection<ushort>>(SlowReadShortFieldAsync<ushort>(reader, entry, sizeLimit, null, cancellationToken));
            }
            else if (entry.Type == TiffFieldType.Byte)
            {
                // is inlined ?
                if (valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    valueCount = Math.Min(sizeLimit, valueCount);
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<ushort>>(TiffValueCollection.Single<ushort>(rawOffset[0]));
                    }
                    ushort[] values = new ushort[valueCount];
                    InternalCopyByteValues<ushort>(rawOffset, values, v => v);
                    return new ValueTask<TiffValueCollection<ushort>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<ushort>>(SlowReadByteFieldAsync<ushort>(reader, entry, sizeLimit, v => v, cancellationToken));
            }

            ThrowHelper.ThrowInvalidOperationException($"Can read tag of type {entry.Type}.");
            return default;
        }

        private ValueTask ReadShortFieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, int offset, Memory<ushort> destination, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
        {
            if (_context is null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(TiffFieldReader));
            }

            long valueCount = entry.ValueCount;
            if ((ulong)offset > (ulong)valueCount)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(offset));
            }
            if ((ulong)destination.Length > (ulong)(valueCount - offset))
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(destination));
            }

            cancellationToken.ThrowIfCancellationRequested();

            Span<byte> rawOffset = stackalloc byte[8];

            if (skipTypeValidation || entry.Type == TiffFieldType.Short || entry.Type == TiffFieldType.SShort)
            {
                if (destination.IsEmpty)
                {
                    return default;
                }

                const int sizeOfElement = sizeof(ushort);
                // is inlined ?
                if (sizeOfElement * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        Debug.Assert(destination.Length == 1);
                        destination.Span[0] = _context.IsLittleEndian
                                ? BinaryPrimitives.ReadUInt16LittleEndian(rawOffset)
                                : BinaryPrimitives.ReadUInt16BigEndian(rawOffset);
                    }
                    else
                    {
                        InternalCopyInt16Values(rawOffset.Slice(sizeOfElement * offset, sizeOfElement * destination.Length), destination.Span, null);
                    }
                    return default;
                }

                return new ValueTask(SlowReadShortFieldAsync(reader, entry, offset, destination, null, cancellationToken));
            }
            else if (entry.Type == TiffFieldType.Byte)
            {
                if (destination.IsEmpty)
                {
                    return default;
                }

                const int sizeOfElement = sizeof(byte);
                // is inlined ?
                if (sizeOfElement * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        Debug.Assert(destination.Length == 1);
                        destination.Span[0] = rawOffset[0];
                    }
                    else
                    {
                        InternalCopyByteValues(rawOffset.Slice(sizeOfElement * offset, sizeOfElement * destination.Length), destination.Span, v => v);
                    }
                    return default;
                }

                return new ValueTask(SlowReadByteFieldAsync(reader, entry, offset, destination, v => v, cancellationToken));
            }

            ThrowHelper.ThrowInvalidOperationException($"Can read tag of type {entry.Type}.");
            return default;
        }

        private async Task<TiffValueCollection<TDest>> SlowReadShortFieldAsync<TDest>(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, int sizeLimit, Func<short, TDest>? convertFunc, CancellationToken cancellationToken) where TDest : struct
        {
            Debug.Assert(sizeLimit >= 0);
            TDest[] values = new TDest[Math.Min(sizeLimit, entry.ValueCount)];
            await SlowReadShortFieldAsync(reader, entry, 0, values, convertFunc, cancellationToken).ConfigureAwait(false);
            return TiffValueCollection.UnsafeWrap(values);
        }

        private async Task SlowReadShortFieldAsync<TDest>(
            TiffFileContentReader reader, TiffImageFileDirectoryEntry entry,
            long offset, Memory<TDest> destination,
            Func<short, TDest>? convertFunc, CancellationToken cancellationToken) where TDest : struct
        {
            const int sizeOfElement = sizeof(ushort);

            int count = destination.Length;
            if (count == 0)
            {
                return;
            }

            Debug.Assert((ulong)offset <= (ulong)entry.ValueCount);
            Debug.Assert((ulong)count <= (ulong)(entry.ValueCount - offset));

            int fieldLength = checked(sizeOfElement * count);
            byte[] buffer = ArrayPool<byte>.Shared.Rent(fieldLength);
            try
            {
                int readCount = await reader.ReadAsync(entry.ValueOffset + sizeOfElement * offset, new ArraySegment<byte>(buffer, 0, fieldLength), cancellationToken).ConfigureAwait(false);
                if (readCount != fieldLength)
                {
                    ThrowHelper.ThrowInvalidDataException("The number of bytes read from file is less than expected.");
                }

                InternalCopyInt16Values(buffer, destination.Span, convertFunc);
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
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<short>> ReadSShortFieldAsync(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadSShortFieldAsync(GetAsyncReader(), entry, int.MaxValue, skipTypeValidation, cancellationToken);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SShort"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<short>> ReadSShortFieldAsync(TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadSShortFieldAsync(GetAsyncReader(), entry, sizeLimit, skipTypeValidation, cancellationToken);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SShort"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="offset">The number of elements to skip reading.</param>
        /// <param name="destination">The destination buffer to store the values.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask"/> that completes when the values are read.</returns>
        public ValueTask ReadSShortFieldAsync(TiffImageFileDirectoryEntry entry, int offset, Memory<short> destination, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadSShortFieldAsync(GetAsyncReader(), entry, offset, destination, skipTypeValidation, cancellationToken);

        private ValueTask<TiffValueCollection<short>> ReadSShortFieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
        {
            if (_context is null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(TiffFieldReader));
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (sizeLimit < 0)
            {
                sizeLimit = int.MaxValue;
            }

            long valueCount = entry.ValueCount;
            Span<byte> rawOffset = stackalloc byte[8];

            if (skipTypeValidation || entry.Type == TiffFieldType.SShort || entry.Type == TiffFieldType.Short)
            {
                // is inlined ?
                if (sizeof(short) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    valueCount = Math.Min(sizeLimit, valueCount);
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

                return new ValueTask<TiffValueCollection<short>>(SlowReadShortFieldAsync<short>(reader, entry, sizeLimit, null, cancellationToken));
            }
            else if (entry.Type == TiffFieldType.SByte)
            {
                // is inlined ?
                if (valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    valueCount = Math.Min(sizeLimit, valueCount);
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<short>>(TiffValueCollection.Single<short>((sbyte)rawOffset[0]));
                    }
                    short[] values = new short[valueCount];
                    InternalCopyByteValues<short>(rawOffset, values, v => (sbyte)v);
                    return new ValueTask<TiffValueCollection<short>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<short>>(SlowReadByteFieldAsync<short>(reader, entry, sizeLimit, v => (sbyte)v, cancellationToken));
            }
            else if (entry.Type == TiffFieldType.Byte)
            {
                // is inlined ?
                if (valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    valueCount = Math.Min(sizeLimit, valueCount);
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<short>>(TiffValueCollection.Single<short>(rawOffset[0]));
                    }
                    short[] values = new short[valueCount];
                    InternalCopyByteValues<short>(rawOffset, values, v => v);
                    return new ValueTask<TiffValueCollection<short>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<short>>(SlowReadByteFieldAsync<short>(reader, entry, sizeLimit, v => v, cancellationToken));
            }

            ThrowHelper.ThrowInvalidOperationException($"Can read tag of type {entry.Type}.");
            return default;
        }

        private ValueTask ReadSShortFieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, int offset, Memory<short> destination, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
        {
            if (_context is null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(TiffFieldReader));
            }

            long valueCount = entry.ValueCount;
            if ((ulong)offset > (ulong)valueCount)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(offset));
            }
            if ((ulong)destination.Length > (ulong)(valueCount - offset))
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(destination));
            }

            cancellationToken.ThrowIfCancellationRequested();

            Span<byte> rawOffset = stackalloc byte[8];

            if (skipTypeValidation || entry.Type == TiffFieldType.Short || entry.Type == TiffFieldType.SShort)
            {
                if (destination.IsEmpty)
                {
                    return default;
                }

                const int sizeOfElement = sizeof(ushort);
                // is inlined ?
                if (sizeOfElement * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        Debug.Assert(destination.Length == 1);
                        destination.Span[0] = _context.IsLittleEndian
                                ? BinaryPrimitives.ReadInt16LittleEndian(rawOffset)
                                : BinaryPrimitives.ReadInt16BigEndian(rawOffset);
                    }
                    else
                    {
                        InternalCopyInt16Values(rawOffset.Slice(sizeOfElement * offset, sizeOfElement * destination.Length), destination.Span, null);
                    }
                    return default;
                }

                return new ValueTask(SlowReadShortFieldAsync(reader, entry, offset, destination, null, cancellationToken));
            }
            else if (entry.Type == TiffFieldType.SByte)
            {
                if (destination.IsEmpty)
                {
                    return default;
                }

                const int sizeOfElement = sizeof(byte);
                // is inlined ?
                if (sizeOfElement * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        Debug.Assert(destination.Length == 1);
                        destination.Span[0] = (sbyte)rawOffset[0];
                    }
                    else
                    {
                        InternalCopyByteValues(rawOffset.Slice(sizeOfElement * offset, sizeOfElement * destination.Length), destination.Span, v => (sbyte)v);
                    }
                    return default;
                }

                return new ValueTask(SlowReadByteFieldAsync(reader, entry, offset, destination, v => (sbyte)v, cancellationToken));
            }
            else if (entry.Type == TiffFieldType.Byte)
            {
                if (destination.IsEmpty)
                {
                    return default;
                }

                const int sizeOfElement = sizeof(byte);
                // is inlined ?
                if (sizeOfElement * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        Debug.Assert(destination.Length == 1);
                        destination.Span[0] = rawOffset[0];
                    }
                    else
                    {
                        InternalCopyByteValues(rawOffset.Slice(sizeOfElement * offset, sizeOfElement * destination.Length), destination.Span, v => v);
                    }
                    return default;
                }

                return new ValueTask(SlowReadByteFieldAsync(reader, entry, offset, destination, v => v, cancellationToken));
            }

            ThrowHelper.ThrowInvalidOperationException($"Can read tag of type {entry.Type}.");
            return default;
        }

        #endregion

        #region Long

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Long"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        [CLSCompliant(false)]
        public ValueTask<TiffValueCollection<uint>> ReadLongFieldAsync(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadLongFieldAsync(GetAsyncReader(), entry, int.MaxValue, skipTypeValidation, cancellationToken);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Long"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        [CLSCompliant(false)]
        public ValueTask<TiffValueCollection<uint>> ReadLongFieldAsync(TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadLongFieldAsync(GetAsyncReader(), entry, sizeLimit, skipTypeValidation, cancellationToken);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Long"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="offset">The number of elements to skip reading.</param>
        /// <param name="destination">The destination buffer to store the values.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask"/> that completes when the values are read.</returns>
        [CLSCompliant(false)]
        public ValueTask ReadLongFieldAsync(TiffImageFileDirectoryEntry entry, int offset, Memory<uint> destination, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadLongFieldAsync(GetAsyncReader(), entry, offset, destination, skipTypeValidation, cancellationToken);

        private ValueTask<TiffValueCollection<uint>> ReadLongFieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
        {
            if (_context is null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(TiffFieldReader));
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (sizeLimit < 0)
            {
                sizeLimit = int.MaxValue;
            }

            long valueCount = entry.ValueCount;
            Span<byte> rawOffset = stackalloc byte[8];
            if (skipTypeValidation || entry.Type == TiffFieldType.Long || entry.Type == TiffFieldType.SLong || entry.Type == TiffFieldType.IFD)
            {
                // is inlined ?
                if (sizeof(uint) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    valueCount = Math.Min(sizeLimit, valueCount);
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

                return new ValueTask<TiffValueCollection<uint>>(SlowReadLongFieldAsync<uint>(reader, entry, sizeLimit, null, cancellationToken));
            }
            else if (entry.Type == TiffFieldType.Short)
            {
                // is inlined ?
                if (sizeof(ushort) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    valueCount = Math.Min(sizeLimit, valueCount);
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

                return new ValueTask<TiffValueCollection<uint>>(SlowReadShortFieldAsync<uint>(reader, entry, sizeLimit, v => (ushort)v, cancellationToken));
            }
            else if (entry.Type == TiffFieldType.Byte)
            {
                // is inlined ?
                if (valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    valueCount = Math.Min(sizeLimit, valueCount);
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<uint>>(TiffValueCollection.Single<uint>(rawOffset[0]));
                    }
                    uint[] values = new uint[valueCount];
                    InternalCopyByteValues<uint>(rawOffset, values, v => v);
                    return new ValueTask<TiffValueCollection<uint>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<uint>>(SlowReadByteFieldAsync<uint>(reader, entry, sizeLimit, v => v, cancellationToken));
            }

            ThrowHelper.ThrowInvalidOperationException($"Can read tag of type {entry.Type}.");
            return default;
        }

        private ValueTask ReadLongFieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, int offset, Memory<uint> destination, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
        {
            if (_context is null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(TiffFieldReader));
            }

            long valueCount = entry.ValueCount;
            if ((ulong)offset > (ulong)valueCount)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(offset));
            }
            if ((ulong)destination.Length > (ulong)(valueCount - offset))
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(destination));
            }

            cancellationToken.ThrowIfCancellationRequested();

            Span<byte> rawOffset = stackalloc byte[8];
            if (skipTypeValidation || entry.Type == TiffFieldType.Long || entry.Type == TiffFieldType.SLong || entry.Type == TiffFieldType.IFD)
            {
                if (destination.IsEmpty)
                {
                    return default;
                }

                const int sizeOfElement = sizeof(uint);
                // is inlined ?
                if (sizeOfElement * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        Debug.Assert(destination.Length == 1);
                        destination.Span[0] = _context.IsLittleEndian
                                ? BinaryPrimitives.ReadUInt32LittleEndian(rawOffset)
                                : BinaryPrimitives.ReadUInt32BigEndian(rawOffset);
                    }
                    else
                    {
                        InternalCopyInt32Values(rawOffset.Slice(sizeOfElement * offset, sizeOfElement * destination.Length), destination.Span, null);
                    }
                    return default;
                }

                return new ValueTask(SlowReadLongFieldAsync(reader, entry, offset, destination, null, cancellationToken));
            }
            else if (entry.Type == TiffFieldType.Short)
            {
                if (destination.IsEmpty)
                {
                    return default;
                }

                const int sizeOfElement = sizeof(ushort);
                // is inlined ?
                if (sizeOfElement * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        Debug.Assert(destination.Length == 1);
                        destination.Span[0] = _context.IsLittleEndian
                                ? BinaryPrimitives.ReadUInt16LittleEndian(rawOffset)
                                : BinaryPrimitives.ReadUInt16BigEndian(rawOffset);
                    }
                    else
                    {
                        InternalCopyInt16Values(rawOffset.Slice(sizeOfElement * offset, sizeOfElement * destination.Length), destination.Span, v => (ushort)v);
                    }
                    return default;
                }

                return new ValueTask(SlowReadShortFieldAsync(reader, entry, offset, destination, v => (ushort)v, cancellationToken));
            }
            else if (entry.Type == TiffFieldType.Byte)
            {
                if (destination.IsEmpty)
                {
                    return default;
                }

                const int sizeOfElement = sizeof(byte);
                // is inlined ?
                if (sizeOfElement * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        Debug.Assert(destination.Length == 1);
                        destination.Span[0] = rawOffset[0];
                    }
                    else
                    {
                        InternalCopyByteValues(rawOffset.Slice(sizeOfElement * offset, sizeOfElement * destination.Length), destination.Span, v => v);
                    }
                    return default;
                }

                return new ValueTask(SlowReadByteFieldAsync(reader, entry, offset, destination, v => v, cancellationToken));
            }

            ThrowHelper.ThrowInvalidOperationException($"Can read tag of type {entry.Type}.");
            return default;
        }

        private async Task<TiffValueCollection<TDest>> SlowReadLongFieldAsync<TDest>(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, int sizeLimit, Func<int, TDest>? convertFunc, CancellationToken cancellationToken) where TDest : struct
        {
            Debug.Assert(sizeLimit >= 0);
            TDest[] values = new TDest[Math.Min(sizeLimit, entry.ValueCount)];
            await SlowReadLongFieldAsync(reader, entry, 0, values, convertFunc, cancellationToken).ConfigureAwait(false);
            return TiffValueCollection.UnsafeWrap(values);
        }

        private async Task SlowReadLongFieldAsync<TDest>(
            TiffFileContentReader reader, TiffImageFileDirectoryEntry entry,
            long offset, Memory<TDest> destination,
            Func<int, TDest>? convertFunc, CancellationToken cancellationToken) where TDest : struct
        {
            const int sizeOfElement = sizeof(int);

            int count = destination.Length;
            if (count == 0)
            {
                return;
            }

            Debug.Assert((ulong)offset <= (ulong)entry.ValueCount);
            Debug.Assert((ulong)count <= (ulong)(entry.ValueCount - offset));

            int fieldLength = checked(sizeOfElement * count);
            byte[] buffer = ArrayPool<byte>.Shared.Rent(fieldLength);
            try
            {
                int readCount = await reader.ReadAsync(entry.ValueOffset + sizeOfElement * offset, new ArraySegment<byte>(buffer, 0, fieldLength), cancellationToken).ConfigureAwait(false);
                if (readCount != fieldLength)
                {
                    ThrowHelper.ThrowInvalidDataException("The number of bytes read from file is less than expected.");
                }

                InternalCopyInt32Values(buffer, destination.Span, convertFunc);
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
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<int>> ReadSLongFieldAsync(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadSLongFieldAsync(GetAsyncReader(), entry, int.MaxValue, skipTypeValidation, cancellationToken);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SLong"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<int>> ReadSLongFieldAsync(TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadSLongFieldAsync(GetAsyncReader(), entry, sizeLimit, skipTypeValidation, cancellationToken);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SLong"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="offset">The number of elements to skip reading.</param>
        /// <param name="destination">The destination buffer to store the values.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask"/> that completes when the values are read.</returns>
        public ValueTask ReadSLongFieldAsync(TiffImageFileDirectoryEntry entry, int offset, Memory<int> destination, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadSLongFieldAsync(GetAsyncReader(), entry, offset, destination, skipTypeValidation, cancellationToken);

        private ValueTask<TiffValueCollection<int>> ReadSLongFieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
        {
            if (_context is null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(TiffFieldReader));
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (sizeLimit < 0)
            {
                sizeLimit = int.MaxValue;
            }

            long valueCount = entry.ValueCount;
            Span<byte> rawOffset = stackalloc byte[8];
            if (skipTypeValidation || entry.Type == TiffFieldType.Long || entry.Type == TiffFieldType.SLong || entry.Type == TiffFieldType.IFD)
            {
                // is inlined ?
                if (sizeof(int) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    valueCount = Math.Min(sizeLimit, valueCount);
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<int>>(TiffValueCollection.Single(
                            _context.IsLittleEndian
                                ? BinaryPrimitives.ReadInt32LittleEndian(rawOffset)
                                : BinaryPrimitives.ReadInt32BigEndian(rawOffset)));
                    }
                    int[] values = new int[valueCount];
                    InternalCopyInt32Values<int>(rawOffset, values, null);
                    return new ValueTask<TiffValueCollection<int>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<int>>(SlowReadLongFieldAsync<int>(reader, entry, sizeLimit, null, cancellationToken));
            }
            else if (entry.Type == TiffFieldType.SShort)
            {
                // is inlined ?
                if (sizeof(short) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    valueCount = Math.Min(sizeLimit, valueCount);
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

                return new ValueTask<TiffValueCollection<int>>(SlowReadShortFieldAsync<int>(reader, entry, sizeLimit, v => v, cancellationToken));
            }
            else if (entry.Type == TiffFieldType.Short)
            {
                // is inlined ?
                if (sizeof(ushort) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    valueCount = Math.Min(sizeLimit, valueCount);
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

                return new ValueTask<TiffValueCollection<int>>(SlowReadShortFieldAsync<int>(reader, entry, sizeLimit, v => (ushort)v, cancellationToken));
            }
            else if (entry.Type == TiffFieldType.SByte)
            {
                // is inlined ?
                if (valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    valueCount = Math.Min(sizeLimit, valueCount);
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<int>>(TiffValueCollection.Single<int>((sbyte)rawOffset[0]));
                    }
                    int[] values = new int[valueCount];
                    InternalCopyByteValues<int>(rawOffset, values, v => (sbyte)v);
                    return new ValueTask<TiffValueCollection<int>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<int>>(SlowReadByteFieldAsync<int>(reader, entry, sizeLimit, v => (sbyte)v, cancellationToken));
            }
            else if (entry.Type == TiffFieldType.Byte)
            {
                // is inlined ?
                if (valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    valueCount = Math.Min(sizeLimit, valueCount);
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<int>>(TiffValueCollection.Single<int>(rawOffset[0]));
                    }
                    int[] values = new int[valueCount];
                    InternalCopyByteValues<int>(rawOffset, values, v => v);
                    return new ValueTask<TiffValueCollection<int>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<int>>(SlowReadByteFieldAsync<int>(reader, entry, sizeLimit, v => v, cancellationToken));
            }

            ThrowHelper.ThrowInvalidOperationException($"Can read tag of type {entry.Type}.");
            return default;
        }

        private ValueTask ReadSLongFieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, int offset, Memory<int> destination, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
        {
            if (_context is null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(TiffFieldReader));
            }

            long valueCount = entry.ValueCount;
            if ((ulong)offset > (ulong)valueCount)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(offset));
            }
            if ((ulong)destination.Length > (ulong)(valueCount - offset))
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(destination));
            }

            cancellationToken.ThrowIfCancellationRequested();

            Span<byte> rawOffset = stackalloc byte[8];
            if (skipTypeValidation || entry.Type == TiffFieldType.Long || entry.Type == TiffFieldType.SLong || entry.Type == TiffFieldType.IFD)
            {
                if (destination.IsEmpty)
                {
                    return default;
                }

                const int sizeOfElement = sizeof(uint);
                // is inlined ?
                if (sizeOfElement * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        Debug.Assert(destination.Length == 1);
                        destination.Span[0] = _context.IsLittleEndian
                                ? BinaryPrimitives.ReadInt32LittleEndian(rawOffset)
                                : BinaryPrimitives.ReadInt32BigEndian(rawOffset);
                    }
                    else
                    {
                        InternalCopyInt32Values(rawOffset.Slice(sizeOfElement * offset, sizeOfElement * destination.Length), destination.Span, null);
                    }
                    return default;
                }

                return new ValueTask(SlowReadLongFieldAsync(reader, entry, offset, destination, null, cancellationToken));
            }
            else if (entry.Type == TiffFieldType.SShort)
            {
                if (destination.IsEmpty)
                {
                    return default;
                }

                const int sizeOfElement = sizeof(ushort);
                // is inlined ?
                if (sizeOfElement * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        Debug.Assert(destination.Length == 1);
                        destination.Span[0] = _context.IsLittleEndian
                                ? BinaryPrimitives.ReadInt16LittleEndian(rawOffset)
                                : BinaryPrimitives.ReadInt16BigEndian(rawOffset);
                    }
                    else
                    {
                        InternalCopyInt16Values(rawOffset.Slice(sizeOfElement * offset, sizeOfElement * destination.Length), destination.Span, v => v);
                    }
                    return default;
                }

                return new ValueTask(SlowReadShortFieldAsync(reader, entry, offset, destination, v => v, cancellationToken));
            }
            else if (entry.Type == TiffFieldType.Short)
            {
                if (destination.IsEmpty)
                {
                    return default;
                }

                const int sizeOfElement = sizeof(ushort);
                // is inlined ?
                if (sizeOfElement * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        Debug.Assert(destination.Length == 1);
                        destination.Span[0] = _context.IsLittleEndian
                                ? BinaryPrimitives.ReadInt16LittleEndian(rawOffset)
                                : BinaryPrimitives.ReadInt16BigEndian(rawOffset);
                    }
                    else
                    {
                        InternalCopyInt16Values(rawOffset.Slice(sizeOfElement * offset, sizeOfElement * destination.Length), destination.Span, v => (ushort)v);
                    }
                    return default;
                }

                return new ValueTask(SlowReadShortFieldAsync(reader, entry, offset, destination, v => (ushort)v, cancellationToken));
            }
            else if (entry.Type == TiffFieldType.SByte)
            {
                if (destination.IsEmpty)
                {
                    return default;
                }

                const int sizeOfElement = sizeof(byte);
                // is inlined ?
                if (sizeOfElement * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        Debug.Assert(destination.Length == 1);
                        destination.Span[0] = (sbyte)rawOffset[0];
                    }
                    else
                    {
                        InternalCopyByteValues(rawOffset.Slice(sizeOfElement * offset, sizeOfElement * destination.Length), destination.Span, v => (sbyte)v);
                    }
                    return default;
                }

                return new ValueTask(SlowReadByteFieldAsync(reader, entry, offset, destination, v => (sbyte)v, cancellationToken));
            }
            else if (entry.Type == TiffFieldType.Byte)
            {
                if (destination.IsEmpty)
                {
                    return default;
                }

                const int sizeOfElement = sizeof(byte);
                // is inlined ?
                if (sizeOfElement * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        Debug.Assert(destination.Length == 1);
                        destination.Span[0] = rawOffset[0];
                    }
                    else
                    {
                        InternalCopyByteValues(rawOffset.Slice(sizeOfElement * offset, sizeOfElement * destination.Length), destination.Span, v => v);
                    }
                    return default;
                }

                return new ValueTask(SlowReadByteFieldAsync(reader, entry, offset, destination, v => v, cancellationToken));
            }

            ThrowHelper.ThrowInvalidOperationException($"Can read tag of type {entry.Type}.");
            return default;
        }

        #endregion

        #region Long8

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Long8"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        [CLSCompliant(false)]
        public ValueTask<TiffValueCollection<ulong>> ReadLong8FieldAsync(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadLong8FieldAsync(GetAsyncReader(), entry, int.MaxValue, skipTypeValidation, cancellationToken);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Long8"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        [CLSCompliant(false)]
        public ValueTask<TiffValueCollection<ulong>> ReadLong8FieldAsync(TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadLong8FieldAsync(GetAsyncReader(), entry, sizeLimit, skipTypeValidation, cancellationToken);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Long8"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="offset">The number of elements to skip reading.</param>
        /// <param name="destination">The destination buffer to store the values.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask"/> that completes when the values are read.</returns>
        [CLSCompliant(false)]
        public ValueTask ReadLong8FieldAsync(TiffImageFileDirectoryEntry entry, int offset, Memory<ulong> destination, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadLong8FieldAsync(GetAsyncReader(), entry, offset, destination, skipTypeValidation, cancellationToken);

        private ValueTask<TiffValueCollection<ulong>> ReadLong8FieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
        {
            if (_context is null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(TiffFieldReader));
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (sizeLimit < 0)
            {
                sizeLimit = int.MaxValue;
            }

            long valueCount = entry.ValueCount;
            Span<byte> rawOffset = stackalloc byte[8];
            if (skipTypeValidation || entry.Type == TiffFieldType.Long8 || entry.Type == TiffFieldType.SLong8 || entry.Type == TiffFieldType.IFD8)
            {
                // is inlined ?
                if (sizeof(ulong) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    valueCount = Math.Min(sizeLimit, valueCount);
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

                return new ValueTask<TiffValueCollection<ulong>>(SlowReadLong8FieldAsync<ulong>(reader, entry, sizeLimit, null, cancellationToken));
            }
            else if (entry.Type == TiffFieldType.Long)
            {
                // is inlined ?
                if (sizeof(uint) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    valueCount = Math.Min(sizeLimit, valueCount);
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

                return new ValueTask<TiffValueCollection<ulong>>(SlowReadLongFieldAsync<ulong>(reader, entry, sizeLimit, v => (uint)v, cancellationToken));
            }
            else if (entry.Type == TiffFieldType.Short)
            {
                // is inlined ?
                if (sizeof(ushort) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    valueCount = Math.Min(sizeLimit, valueCount);
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

                return new ValueTask<TiffValueCollection<ulong>>(SlowReadShortFieldAsync<ulong>(reader, entry, sizeLimit, v => (ushort)v, cancellationToken));
            }
            else if (entry.Type == TiffFieldType.Byte)
            {
                // is inlined ?
                if (valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    valueCount = Math.Min(sizeLimit, valueCount);
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<ulong>>(TiffValueCollection.Single<ulong>(rawOffset[0]));
                    }
                    ulong[] values = new ulong[valueCount];
                    InternalCopyByteValues<ulong>(rawOffset, values, v => v);
                    return new ValueTask<TiffValueCollection<ulong>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<ulong>>(SlowReadByteFieldAsync<ulong>(reader, entry, sizeLimit, v => v, cancellationToken));
            }

            ThrowHelper.ThrowInvalidOperationException($"Can read tag of type {entry.Type}.");
            return default;
        }

        private ValueTask ReadLong8FieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, int offset, Memory<ulong> destination, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
        {
            if (_context is null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(TiffFieldReader));
            }

            long valueCount = entry.ValueCount;
            if ((ulong)offset > (ulong)valueCount)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(offset));
            }
            if ((ulong)destination.Length > (ulong)(valueCount - offset))
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(destination));
            }

            cancellationToken.ThrowIfCancellationRequested();

            Span<byte> rawOffset = stackalloc byte[8];
            if (skipTypeValidation || entry.Type == TiffFieldType.Long8 || entry.Type == TiffFieldType.SLong8 || entry.Type == TiffFieldType.IFD8)
            {
                if (destination.IsEmpty)
                {
                    return default;
                }

                const int sizeOfElement = sizeof(ulong);
                // is inlined ?
                if (sizeOfElement * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        Debug.Assert(destination.Length == 1);
                        destination.Span[0] = _context.IsLittleEndian
                                ? BinaryPrimitives.ReadUInt64LittleEndian(rawOffset)
                                : BinaryPrimitives.ReadUInt64BigEndian(rawOffset);
                    }
                    else
                    {
                        InternalCopyInt64Values(rawOffset.Slice(sizeOfElement * offset, sizeOfElement * destination.Length), destination.Span, null);
                    }
                    return default;
                }

                return new ValueTask(SlowReadLong8FieldAsync(reader, entry, offset, destination, null, cancellationToken));
            }
            else if (entry.Type == TiffFieldType.Long)
            {
                if (destination.IsEmpty)
                {
                    return default;
                }

                const int sizeOfElement = sizeof(uint);
                // is inlined ?
                if (sizeOfElement * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        Debug.Assert(destination.Length == 1);
                        destination.Span[0] = _context.IsLittleEndian
                                ? BinaryPrimitives.ReadUInt32LittleEndian(rawOffset)
                                : BinaryPrimitives.ReadUInt32BigEndian(rawOffset);
                    }
                    else
                    {
                        InternalCopyInt32Values(rawOffset.Slice(sizeOfElement * offset, sizeOfElement * destination.Length), destination.Span, v => (uint)v);
                    }
                    return default;
                }

                return new ValueTask(SlowReadLongFieldAsync(reader, entry, offset, destination, v => (uint)v, cancellationToken));
            }
            else if (entry.Type == TiffFieldType.Short)
            {
                if (destination.IsEmpty)
                {
                    return default;
                }

                const int sizeOfElement = sizeof(ushort);
                // is inlined ?
                if (sizeOfElement * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        Debug.Assert(destination.Length == 1);
                        destination.Span[0] = _context.IsLittleEndian
                                ? BinaryPrimitives.ReadUInt16LittleEndian(rawOffset)
                                : BinaryPrimitives.ReadUInt16BigEndian(rawOffset);
                    }
                    else
                    {
                        InternalCopyInt16Values(rawOffset.Slice(sizeOfElement * offset, sizeOfElement * destination.Length), destination.Span, v => (ushort)v);
                    }
                    return default;
                }

                return new ValueTask(SlowReadShortFieldAsync(reader, entry, offset, destination, v => (ushort)v, cancellationToken));
            }
            else if (entry.Type == TiffFieldType.Byte)
            {
                if (destination.IsEmpty)
                {
                    return default;
                }

                const int sizeOfElement = sizeof(byte);
                // is inlined ?
                if (sizeOfElement * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        Debug.Assert(destination.Length == 1);
                        destination.Span[0] = rawOffset[0];
                    }
                    else
                    {
                        InternalCopyByteValues(rawOffset.Slice(sizeOfElement * offset, sizeOfElement * destination.Length), destination.Span, v => v);
                    }
                    return default;
                }

                return new ValueTask(SlowReadByteFieldAsync(reader, entry, offset, destination, v => v, cancellationToken));
            }

            ThrowHelper.ThrowInvalidOperationException($"Can read tag of type {entry.Type}.");
            return default;
        }

        private async Task<TiffValueCollection<TDest>> SlowReadLong8FieldAsync<TDest>(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, int sizeLimit, Func<long, TDest>? convertFunc, CancellationToken cancellationToken) where TDest : struct
        {
            Debug.Assert(sizeLimit >= 0);
            TDest[] values = new TDest[Math.Min(sizeLimit, entry.ValueCount)];
            await SlowReadLong8FieldAsync(reader, entry, 0, values, convertFunc, cancellationToken).ConfigureAwait(false);
            return TiffValueCollection.UnsafeWrap(values);
        }

        private async Task SlowReadLong8FieldAsync<TDest>(
            TiffFileContentReader reader, TiffImageFileDirectoryEntry entry,
            long offset, Memory<TDest> destination,
            Func<long, TDest>? convertFunc, CancellationToken cancellationToken) where TDest : struct
        {
            const int sizeOfElement = sizeof(ulong);

            int count = destination.Length;
            if (count == 0)
            {
                return;
            }

            Debug.Assert((ulong)offset <= (ulong)entry.ValueCount);
            Debug.Assert((ulong)count <= (ulong)(entry.ValueCount - offset));

            int fieldLength = checked(sizeOfElement * count);
            byte[] buffer = ArrayPool<byte>.Shared.Rent(fieldLength);
            try
            {
                int readCount = await reader.ReadAsync(entry.ValueOffset + sizeOfElement * offset, new ArraySegment<byte>(buffer, 0, fieldLength), cancellationToken).ConfigureAwait(false);
                if (readCount != fieldLength)
                {
                    ThrowHelper.ThrowInvalidDataException("The number of bytes read from file is less than expected.");
                }

                InternalCopyInt64Values(buffer, destination.Span, convertFunc);
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
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<long>> ReadSLong8FieldAsync(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadSLong8FieldAsync(GetAsyncReader(), entry, int.MaxValue, skipTypeValidation, cancellationToken);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SLong8"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<long>> ReadSLong8FieldAsync(TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadSLong8FieldAsync(GetAsyncReader(), entry, sizeLimit, skipTypeValidation, cancellationToken);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SLong8"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="offset">The number of elements to skip reading.</param>
        /// <param name="destination">The destination buffer to store the values.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask"/> that completes when the values are read.</returns>
        public ValueTask ReadSLong8FieldAsync(TiffImageFileDirectoryEntry entry, int offset, Memory<long> destination, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadSLong8FieldAsync(GetAsyncReader(), entry, offset, destination, skipTypeValidation, cancellationToken);

        private ValueTask<TiffValueCollection<long>> ReadSLong8FieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
        {
            if (_context is null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(TiffFieldReader));
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (sizeLimit < 0)
            {
                sizeLimit = int.MaxValue;
            }

            long valueCount = entry.ValueCount;
            Span<byte> rawOffset = stackalloc byte[8];
            if (skipTypeValidation || entry.Type == TiffFieldType.SLong8 || entry.Type == TiffFieldType.Long8 || entry.Type == TiffFieldType.IFD8)
            {
                // is inlined ?
                if (sizeof(long) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    valueCount = Math.Min(sizeLimit, valueCount);
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

                return new ValueTask<TiffValueCollection<long>>(SlowReadLong8FieldAsync<long>(reader, entry, sizeLimit, null, cancellationToken));
            }
            else if (entry.Type == TiffFieldType.SLong)
            {
                // is inlined ?
                if (sizeof(int) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    valueCount = Math.Min(sizeLimit, valueCount);
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

                return new ValueTask<TiffValueCollection<long>>(SlowReadLongFieldAsync<long>(reader, entry, sizeLimit, v => v, cancellationToken));
            }
            else if (entry.Type == TiffFieldType.Long)
            {
                // is inlined ?
                if (sizeof(uint) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    valueCount = Math.Min(sizeLimit, valueCount);
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<long>>(TiffValueCollection.Single<long>(
                            _context.IsLittleEndian
                                ? BinaryPrimitives.ReadUInt32LittleEndian(rawOffset)
                                : BinaryPrimitives.ReadUInt32BigEndian(rawOffset)));
                    }
                    long[] values = new long[valueCount];
                    InternalCopyInt32Values<long>(rawOffset, values, v => (uint)v);
                    return new ValueTask<TiffValueCollection<long>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<long>>(SlowReadLongFieldAsync<long>(reader, entry, sizeLimit, v => (uint)v, cancellationToken));
            }
            else if (entry.Type == TiffFieldType.SShort)
            {
                // is inlined ?
                if (sizeof(short) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    valueCount = Math.Min(sizeLimit, valueCount);
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

                return new ValueTask<TiffValueCollection<long>>(SlowReadShortFieldAsync<long>(reader, entry, sizeLimit, v => v, cancellationToken));
            }
            else if (entry.Type == TiffFieldType.Short)
            {
                // is inlined ?
                if (sizeof(ushort) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    valueCount = Math.Min(sizeLimit, valueCount);
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<long>>(TiffValueCollection.Single<long>(
                            _context.IsLittleEndian
                                ? BinaryPrimitives.ReadUInt16LittleEndian(rawOffset)
                                : BinaryPrimitives.ReadUInt16BigEndian(rawOffset)));
                    }
                    long[] values = new long[valueCount];
                    InternalCopyInt16Values<long>(rawOffset, values, v => (ushort)v);
                    return new ValueTask<TiffValueCollection<long>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<long>>(SlowReadShortFieldAsync<long>(reader, entry, sizeLimit, v => (ushort)v, cancellationToken));
            }
            else if (entry.Type == TiffFieldType.SByte)
            {
                // is inlined ?
                if (valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    valueCount = Math.Min(sizeLimit, valueCount);
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<long>>(TiffValueCollection.Single<long>((sbyte)rawOffset[0]));
                    }
                    long[] values = new long[valueCount];
                    InternalCopyByteValues<long>(rawOffset, values, v => (sbyte)v);
                    return new ValueTask<TiffValueCollection<long>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<long>>(SlowReadByteFieldAsync<long>(reader, entry, sizeLimit, v => (sbyte)v, cancellationToken));
            }
            else if (entry.Type == TiffFieldType.Byte)
            {
                // is inlined ?
                if (valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    valueCount = Math.Min(sizeLimit, valueCount);
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<long>>(TiffValueCollection.Single<long>(rawOffset[0]));
                    }
                    long[] values = new long[valueCount];
                    InternalCopyByteValues<long>(rawOffset, values, v => v);
                    return new ValueTask<TiffValueCollection<long>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<long>>(SlowReadByteFieldAsync<long>(reader, entry, sizeLimit, v => v, cancellationToken));
            }

            ThrowHelper.ThrowInvalidOperationException($"Can read tag of type {entry.Type}.");
            return default;
        }

        private ValueTask ReadSLong8FieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, int offset, Memory<long> destination, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
        {
            if (_context is null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(TiffFieldReader));
            }

            long valueCount = entry.ValueCount;
            if ((ulong)offset > (ulong)valueCount)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(offset));
            }
            if ((ulong)destination.Length > (ulong)(valueCount - offset))
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(destination));
            }

            cancellationToken.ThrowIfCancellationRequested();

            Span<byte> rawOffset = stackalloc byte[8];
            if (skipTypeValidation || entry.Type == TiffFieldType.SLong8 || entry.Type == TiffFieldType.Long8 || entry.Type == TiffFieldType.IFD8)
            {
                if (destination.IsEmpty)
                {
                    return default;
                }

                const int sizeOfElement = sizeof(long);
                // is inlined ?
                if (sizeOfElement * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        Debug.Assert(destination.Length == 1);
                        destination.Span[0] = _context.IsLittleEndian
                                ? BinaryPrimitives.ReadInt64LittleEndian(rawOffset)
                                : BinaryPrimitives.ReadInt64BigEndian(rawOffset);
                    }
                    else
                    {
                        InternalCopyInt64Values(rawOffset.Slice(sizeOfElement * offset, sizeOfElement * destination.Length), destination.Span, null);
                    }
                    return default;
                }

                return new ValueTask(SlowReadLong8FieldAsync(reader, entry, offset, destination, null, cancellationToken));
            }
            else if (entry.Type == TiffFieldType.SLong)
            {
                if (destination.IsEmpty)
                {
                    return default;
                }

                const int sizeOfElement = sizeof(int);
                // is inlined ?
                if (sizeOfElement * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        Debug.Assert(destination.Length == 1);
                        destination.Span[0] = _context.IsLittleEndian
                                ? BinaryPrimitives.ReadInt32LittleEndian(rawOffset)
                                : BinaryPrimitives.ReadInt32BigEndian(rawOffset);
                    }
                    else
                    {
                        InternalCopyInt32Values(rawOffset.Slice(sizeOfElement * offset, sizeOfElement * destination.Length), destination.Span, v => v);
                    }
                    return default;
                }

                return new ValueTask(SlowReadLongFieldAsync(reader, entry, offset, destination, v => v, cancellationToken));
            }
            else if (entry.Type == TiffFieldType.Long)
            {
                if (destination.IsEmpty)
                {
                    return default;
                }

                const int sizeOfElement = sizeof(uint);
                // is inlined ?
                if (sizeOfElement * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        Debug.Assert(destination.Length == 1);
                        destination.Span[0] = _context.IsLittleEndian
                                ? BinaryPrimitives.ReadUInt32LittleEndian(rawOffset)
                                : BinaryPrimitives.ReadUInt32BigEndian(rawOffset);
                    }
                    else
                    {
                        InternalCopyInt32Values(rawOffset.Slice(sizeOfElement * offset, sizeOfElement * destination.Length), destination.Span, v => (uint)v);
                    }
                    return default;
                }

                return new ValueTask(SlowReadLongFieldAsync(reader, entry, offset, destination, v => (uint)v, cancellationToken));
            }
            else if (entry.Type == TiffFieldType.SShort)
            {
                if (destination.IsEmpty)
                {
                    return default;
                }

                const int sizeOfElement = sizeof(short);
                // is inlined ?
                if (sizeOfElement * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        Debug.Assert(destination.Length == 1);
                        destination.Span[0] = _context.IsLittleEndian
                                ? BinaryPrimitives.ReadInt16LittleEndian(rawOffset)
                                : BinaryPrimitives.ReadInt16BigEndian(rawOffset);
                    }
                    else
                    {
                        InternalCopyInt16Values(rawOffset.Slice(sizeOfElement * offset, sizeOfElement * destination.Length), destination.Span, v => v);
                    }
                    return default;
                }

                return new ValueTask(SlowReadShortFieldAsync(reader, entry, offset, destination, v => v, cancellationToken));
            }
            else if (entry.Type == TiffFieldType.Short)
            {
                if (destination.IsEmpty)
                {
                    return default;
                }

                const int sizeOfElement = sizeof(ushort);
                // is inlined ?
                if (sizeOfElement * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        Debug.Assert(destination.Length == 1);
                        destination.Span[0] = _context.IsLittleEndian
                                ? BinaryPrimitives.ReadUInt16LittleEndian(rawOffset)
                                : BinaryPrimitives.ReadUInt16BigEndian(rawOffset);
                    }
                    else
                    {
                        InternalCopyInt16Values(rawOffset.Slice(sizeOfElement * offset, sizeOfElement * destination.Length), destination.Span, v => (ushort)v);
                    }
                    return default;
                }

                return new ValueTask(SlowReadShortFieldAsync(reader, entry, offset, destination, v => (ushort)v, cancellationToken));
            }
            else if (entry.Type == TiffFieldType.SByte)
            {
                if (destination.IsEmpty)
                {
                    return default;
                }

                const int sizeOfElement = sizeof(sbyte);
                // is inlined ?
                if (sizeOfElement * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        Debug.Assert(destination.Length == 1);
                        destination.Span[0] = (sbyte)rawOffset[0];
                    }
                    else
                    {
                        InternalCopyByteValues(rawOffset.Slice(sizeOfElement * offset, sizeOfElement * destination.Length), destination.Span, v => (sbyte)v);
                    }
                    return default;
                }

                return new ValueTask(SlowReadByteFieldAsync(reader, entry, offset, destination, v => (sbyte)v, cancellationToken));
            }
            else if (entry.Type == TiffFieldType.Byte)
            {
                if (destination.IsEmpty)
                {
                    return default;
                }

                const int sizeOfElement = sizeof(byte);
                // is inlined ?
                if (sizeOfElement * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        Debug.Assert(destination.Length == 1);
                        destination.Span[0] = rawOffset[0];
                    }
                    else
                    {
                        InternalCopyByteValues(rawOffset.Slice(sizeOfElement * offset, sizeOfElement * destination.Length), destination.Span, v => v);
                    }
                    return default;
                }

                return new ValueTask(SlowReadByteFieldAsync(reader, entry, offset, destination, v => v, cancellationToken));
            }

            ThrowHelper.ThrowInvalidOperationException($"Can read tag of type {entry.Type}.");
            return default;
        }

        #endregion

        #region Float

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Float"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<float>> ReadFloatFieldAsync(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadFloatFieldAsync(GetAsyncReader(), entry, int.MaxValue, skipTypeValidation, cancellationToken);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Float"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<float>> ReadFloatFieldAsync(TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadFloatFieldAsync(GetAsyncReader(), entry, sizeLimit, skipTypeValidation, cancellationToken);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Float"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="offset">The number of elements to skip reading.</param>
        /// <param name="destination">The destination buffer to store the values.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask"/> that completes when the values are read.</returns>
        public ValueTask ReadFloatFieldAsync(TiffImageFileDirectoryEntry entry, int offset, Memory<float> destination, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadFloatFieldAsync(GetAsyncReader(), entry, offset, destination, skipTypeValidation, cancellationToken);

        private ValueTask<TiffValueCollection<float>> ReadFloatFieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
        {
            if (_context is null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(TiffFieldReader));
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (sizeLimit < 0)
            {
                sizeLimit = int.MaxValue;
            }

            long valueCount = entry.ValueCount;
            Span<byte> rawOffset = stackalloc byte[8];
            if (skipTypeValidation || entry.Type == TiffFieldType.Float)
            {
                // is inlined ?
                if (sizeof(float) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    valueCount = Math.Min(sizeLimit, valueCount);
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

                return new ValueTask<TiffValueCollection<float>>(SlowReadFloatFieldAsync<float>(reader, entry, sizeLimit, null, cancellationToken));
            }

            ThrowHelper.ThrowInvalidOperationException($"Can read tag of type {entry.Type}.");
            return default;
        }

        private ValueTask ReadFloatFieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, int offset, Memory<float> destination, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
        {
            if (_context is null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(TiffFieldReader));
            }

            long valueCount = entry.ValueCount;
            if ((ulong)offset > (ulong)valueCount)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(offset));
            }
            if ((ulong)destination.Length > (ulong)(valueCount - offset))
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(destination));
            }

            cancellationToken.ThrowIfCancellationRequested();

            Span<byte> rawOffset = stackalloc byte[8];
            if (skipTypeValidation || entry.Type == TiffFieldType.Float)
            {
                if (destination.IsEmpty)
                {
                    return default;
                }

                // is inlined ?
                if (sizeof(float) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    const int sizeOfElement = sizeof(float);

                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        Debug.Assert(destination.Length == 1);
                        int value = MemoryMarshal.Read<int>(rawOffset);
                        if (_context.IsLittleEndian != BitConverter.IsLittleEndian)
                        {
                            value = BinaryPrimitives.ReverseEndianness(value);
                        }
                        destination.Span[0] = Int32BitsToSingle(value);
                    }
                    else
                    {
                        InternalCopyFloatValues(rawOffset.Slice(sizeOfElement * offset, sizeOfElement * destination.Length), destination.Span, null);
                    }
                    return default;
                }

                return new ValueTask(SlowReadFloatFieldAsync(reader, entry, offset, destination, null, cancellationToken));
            }

            ThrowHelper.ThrowInvalidOperationException($"Can read tag of type {entry.Type}.");
            return default;
        }

        private async Task<TiffValueCollection<TDest>> SlowReadFloatFieldAsync<TDest>(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, int sizeLimit, Func<float, TDest>? convertFunc, CancellationToken cancellationToken) where TDest : struct
        {
            Debug.Assert(sizeLimit >= 0);
            TDest[] values = new TDest[Math.Min(sizeLimit, entry.ValueCount)];
            await SlowReadFloatFieldAsync(reader, entry, 0, values, convertFunc, cancellationToken).ConfigureAwait(false);
            return TiffValueCollection.UnsafeWrap(values);
        }

        private async Task SlowReadFloatFieldAsync<TDest>(
            TiffFileContentReader reader, TiffImageFileDirectoryEntry entry,
            long offset, Memory<TDest> destination,
            Func<float, TDest>? convertFunc, CancellationToken cancellationToken) where TDest : struct
        {
            const int sizeOfElement = sizeof(float);

            int count = destination.Length;
            if (count == 0)
            {
                return;
            }

            Debug.Assert((ulong)offset <= (ulong)entry.ValueCount);
            Debug.Assert((ulong)count <= (ulong)(entry.ValueCount - offset));

            int fieldLength = checked(sizeOfElement * count);
            byte[] buffer = ArrayPool<byte>.Shared.Rent(fieldLength);
            try
            {
                int readCount = await reader.ReadAsync(entry.ValueOffset + sizeOfElement * offset, new ArraySegment<byte>(buffer, 0, fieldLength), cancellationToken).ConfigureAwait(false);
                if (readCount != fieldLength)
                {
                    ThrowHelper.ThrowInvalidDataException("The number of bytes read from file is less than expected.");
                }

                InternalCopyFloatValues(buffer, destination.Span, convertFunc);
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
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<double>> ReadDoubleFieldAsync(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadDoubleFieldAsync(GetAsyncReader(), entry, int.MaxValue, skipTypeValidation, cancellationToken);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Double"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<double>> ReadDoubleFieldAsync(TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadDoubleFieldAsync(GetAsyncReader(), entry, sizeLimit, skipTypeValidation, cancellationToken);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Double"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="offset">The number of elements to skip reading.</param>
        /// <param name="destination">The destination buffer to store the values.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask"/> that completes when the values are read.</returns>
        public ValueTask ReadDoubleFieldAsync(TiffImageFileDirectoryEntry entry, int offset, Memory<double> destination, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadDoubleFieldAsync(GetAsyncReader(), entry, offset, destination, skipTypeValidation, cancellationToken);

        private ValueTask<TiffValueCollection<double>> ReadDoubleFieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
        {
            if (_context is null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(TiffFieldReader));
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (sizeLimit < 0)
            {
                sizeLimit = int.MaxValue;
            }

            long valueCount = entry.ValueCount;
            Span<byte> rawOffset = stackalloc byte[8];
            if (skipTypeValidation || entry.Type == TiffFieldType.Double)
            {
                // is inlined ?
                if (sizeof(double) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    valueCount = Math.Min(sizeLimit, valueCount);
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

                return new ValueTask<TiffValueCollection<double>>(SlowReadDoubleFieldAsync<double>(reader, entry, sizeLimit, null, cancellationToken));
            }
            else if (entry.Type == TiffFieldType.Float)
            {
                // is inlined ?
                if (sizeof(float) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    valueCount = Math.Min(sizeLimit, valueCount);
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        int value = MemoryMarshal.Read<int>(rawOffset);
                        if (_context.IsLittleEndian != BitConverter.IsLittleEndian)
                        {
                            value = BinaryPrimitives.ReverseEndianness(value);
                        }
                        return new ValueTask<TiffValueCollection<double>>(TiffValueCollection.Single((double)Int32BitsToSingle(value)));
                    }
                    double[] values = new double[valueCount];
                    InternalCopyFloatValues<double>(rawOffset, values, v => v);
                    return new ValueTask<TiffValueCollection<double>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<double>>(SlowReadFloatFieldAsync<double>(reader, entry, sizeLimit, v => v, cancellationToken));
            }

            ThrowHelper.ThrowInvalidOperationException($"Can read tag of type {entry.Type}.");
            return default;
        }

        private ValueTask ReadDoubleFieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, int offset, Memory<double> destination, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
        {
            if (_context is null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(TiffFieldReader));
            }

            long valueCount = entry.ValueCount;
            if ((ulong)offset > (ulong)valueCount)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(offset));
            }
            if ((ulong)destination.Length > (ulong)(valueCount - offset))
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(destination));
            }

            cancellationToken.ThrowIfCancellationRequested();

            Span<byte> rawOffset = stackalloc byte[8];

            if (skipTypeValidation || entry.Type == TiffFieldType.Double)
            {
                if (destination.IsEmpty)
                {
                    return default;
                }

                const int sizeOfElement = sizeof(double);
                // is inlined ?
                if (sizeOfElement * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        Debug.Assert(destination.Length == 1);

                        long value = MemoryMarshal.Read<long>(rawOffset);
                        if (_context.IsLittleEndian != BitConverter.IsLittleEndian)
                        {
                            value = BinaryPrimitives.ReverseEndianness(value);
                        }
                        destination.Span[0] = Int64BitsToDouble(value);
                    }
                    else
                    {
                        InternalCopyInt64Values(rawOffset.Slice(sizeOfElement * offset, sizeOfElement * destination.Length), destination.Span, null);
                    }
                    return default;
                }
                return new ValueTask(SlowReadDoubleFieldAsync(reader, entry, offset, destination, null, cancellationToken));
            }
            else if (entry.Type == TiffFieldType.Float)
            {
                if (destination.IsEmpty)
                {
                    return default;
                }

                const int sizeOfElement = sizeof(float);
                // is inlined ?
                if (sizeOfElement * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        Debug.Assert(destination.Length == 1);
                        int value = MemoryMarshal.Read<int>(rawOffset);
                        if (_context.IsLittleEndian != BitConverter.IsLittleEndian)
                        {
                            value = BinaryPrimitives.ReverseEndianness(value);
                        }
                        destination.Span[0] = Int32BitsToSingle(value);
                    }
                    else
                    {
                        InternalCopyFloatValues(rawOffset.Slice(sizeOfElement * offset, sizeOfElement * destination.Length), destination.Span, v => v);
                    }
                    return default;
                }

                return new ValueTask(SlowReadFloatFieldAsync(reader, entry, offset, destination, v => v, cancellationToken));
            }

            ThrowHelper.ThrowInvalidOperationException($"Can read tag of type {entry.Type}.");
            return default;
        }

        private async Task<TiffValueCollection<TDest>> SlowReadDoubleFieldAsync<TDest>(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, int sizeLimit, Func<double, TDest>? convertFunc, CancellationToken cancellationToken) where TDest : struct
        {
            Debug.Assert(sizeLimit >= 0);
            TDest[] values = new TDest[Math.Min(sizeLimit, entry.ValueCount)];
            await SlowReadDoubleFieldAsync(reader, entry, 0, values, convertFunc, cancellationToken).ConfigureAwait(false);
            return TiffValueCollection.UnsafeWrap(values);
        }

        private async Task SlowReadDoubleFieldAsync<TDest>(
            TiffFileContentReader reader, TiffImageFileDirectoryEntry entry,
            long offset, Memory<TDest> destination,
            Func<double, TDest>? convertFunc, CancellationToken cancellationToken) where TDest : struct
        {
            const int sizeOfElement = sizeof(double);

            int count = destination.Length;
            if (count == 0)
            {
                return;
            }

            Debug.Assert((ulong)offset <= (ulong)entry.ValueCount);
            Debug.Assert((ulong)count <= (ulong)(entry.ValueCount - offset));

            int fieldLength = checked(sizeOfElement * count);
            byte[] buffer = ArrayPool<byte>.Shared.Rent(fieldLength);
            try
            {
                int readCount = await reader.ReadAsync(entry.ValueOffset + sizeOfElement * offset, new ArraySegment<byte>(buffer, 0, fieldLength), cancellationToken).ConfigureAwait(false);
                if (readCount != fieldLength)
                {
                    ThrowHelper.ThrowInvalidDataException("The number of bytes read from file is less than expected.");
                }

                InternalCopyDoubleValues(buffer, destination.Span, convertFunc);
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
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        [CLSCompliant(false)]
        public ValueTask<TiffValueCollection<TiffRational>> ReadRationalFieldAsync(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadRationalFieldAsync(GetAsyncReader(), entry, int.MaxValue, skipTypeValidation, cancellationToken);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Rational"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        [CLSCompliant(false)]
        public ValueTask<TiffValueCollection<TiffRational>> ReadRationalFieldAsync(TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadRationalFieldAsync(GetAsyncReader(), entry, sizeLimit, skipTypeValidation, cancellationToken);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Rational"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="offset">The number of elements to skip reading.</param>
        /// <param name="destination">The destination buffer to store the values.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask"/> that completes when the values are read.</returns>
        [CLSCompliant(false)]
        public ValueTask ReadRationalFieldAsync(TiffImageFileDirectoryEntry entry, int offset, Memory<TiffRational> destination, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadRationalFieldAsync(GetAsyncReader(), entry, offset, destination, skipTypeValidation, cancellationToken);

        private ValueTask<TiffValueCollection<TiffRational>> ReadRationalFieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
        {
            if (_context is null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(TiffFieldReader));
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (sizeLimit < 0)
            {
                sizeLimit = int.MaxValue;
            }

            // is inlined ?
            long valueCount = entry.ValueCount;
            Span<byte> rawOffset = stackalloc byte[8];
            if (skipTypeValidation || entry.Type == TiffFieldType.Rational)
            {
                // is inlined ?
                if (8 * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    valueCount = Math.Min(sizeLimit, valueCount);
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

                return new ValueTask<TiffValueCollection<TiffRational>>(SlowReadRationalFieldAsync<TiffRational>(reader, entry, sizeLimit, null, cancellationToken));
            }
            else if (entry.Type == TiffFieldType.Long)
            {
                // is inlined ?
                if (sizeof(uint) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    valueCount = Math.Min(sizeLimit, valueCount);
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<TiffRational>>(TiffValueCollection.Single(
                            _context.IsLittleEndian
                                ? new TiffRational(BinaryPrimitives.ReadUInt32LittleEndian(rawOffset), 1)
                                : new TiffRational(BinaryPrimitives.ReadUInt32BigEndian(rawOffset), 1)));
                    }
                    TiffRational[] values = new TiffRational[valueCount];
                    InternalCopyInt32Values(rawOffset, values, v => new TiffRational((uint)v, 1));
                    return new ValueTask<TiffValueCollection<TiffRational>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<TiffRational>>(SlowReadLongFieldAsync(reader, entry, sizeLimit, v => new TiffRational((uint)v, 1), cancellationToken));
            }
            else if (entry.Type == TiffFieldType.Short)
            {
                // is inlined ?
                if (sizeof(ushort) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    valueCount = Math.Min(sizeLimit, valueCount);
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<TiffRational>>(TiffValueCollection.Single(
                            _context.IsLittleEndian
                                ? new TiffRational(BinaryPrimitives.ReadUInt16LittleEndian(rawOffset), 1)
                                : new TiffRational(BinaryPrimitives.ReadUInt16BigEndian(rawOffset), 1)));
                    }
                    TiffRational[] values = new TiffRational[valueCount];
                    InternalCopyInt16Values(rawOffset, values, v => new TiffRational((ushort)v, 1));
                    return new ValueTask<TiffValueCollection<TiffRational>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<TiffRational>>(SlowReadShortFieldAsync(reader, entry, sizeLimit, v => new TiffRational((ushort)v, 1), cancellationToken));
            }
            else if (entry.Type == TiffFieldType.Byte)
            {
                // is inlined ?
                if (valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    valueCount = Math.Min(sizeLimit, valueCount);
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<TiffRational>>(TiffValueCollection.Single(new TiffRational(rawOffset[0], 1)));
                    }
                    TiffRational[] values = new TiffRational[valueCount];
                    InternalCopyByteValues(rawOffset, values, v => new TiffRational(v, 1));
                    return new ValueTask<TiffValueCollection<TiffRational>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<TiffRational>>(SlowReadByteFieldAsync(reader, entry, sizeLimit, v => new TiffRational(v, 1), cancellationToken));
            }

            ThrowHelper.ThrowInvalidOperationException($"Can read tag of type {entry.Type}.");
            return default;
        }

        private ValueTask ReadRationalFieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, int offset, Memory<TiffRational> destination, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
        {
            if (_context is null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(TiffFieldReader));
            }

            long valueCount = entry.ValueCount;
            if ((ulong)offset > (ulong)valueCount)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(offset));
            }
            if ((ulong)destination.Length > (ulong)(valueCount - offset))
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(destination));
            }

            cancellationToken.ThrowIfCancellationRequested();

            Span<byte> rawOffset = stackalloc byte[8];

            if (skipTypeValidation || entry.Type == TiffFieldType.Rational)
            {
                if (destination.IsEmpty)
                {
                    return default;
                }

                const int sizeOfElement = 8;

                // is inlined ?
                if (sizeOfElement * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    InternalCopyRationalValues(rawOffset.Slice(sizeOfElement * offset, sizeOfElement * destination.Length), destination.Span, null);
                    return default;
                }

                return new ValueTask(SlowReadRationalFieldAsync(reader, entry, offset, destination, null, cancellationToken));
            }
            else if (entry.Type == TiffFieldType.Long)
            {
                if (destination.IsEmpty)
                {
                    return default;
                }

                const int sizeOfElement = sizeof(uint);

                // is inlined ?
                if (sizeof(uint) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        Debug.Assert(destination.Length == 1);
                        destination.Span[0] = _context.IsLittleEndian
                                ? new TiffRational(BinaryPrimitives.ReadUInt32LittleEndian(rawOffset), 1)
                                : new TiffRational(BinaryPrimitives.ReadUInt32BigEndian(rawOffset), 1);
                    }
                    else
                    {
                        InternalCopyInt32Values(rawOffset.Slice(sizeOfElement * offset, sizeOfElement * destination.Length), destination.Span, v => new TiffRational((uint)v, 1));
                    }
                    return default;
                }

                return new ValueTask(SlowReadLongFieldAsync(reader, entry, offset, destination, v => new TiffRational((uint)v, 1), cancellationToken));
            }
            else if (entry.Type == TiffFieldType.Short)
            {
                if (destination.IsEmpty)
                {
                    return default;
                }

                const int sizeOfElement = sizeof(ushort);

                // is inlined ?
                if (sizeOfElement * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        Debug.Assert(destination.Length == 1);
                        destination.Span[0] = _context.IsLittleEndian
                                ? new TiffRational(BinaryPrimitives.ReadUInt16LittleEndian(rawOffset), 1)
                                : new TiffRational(BinaryPrimitives.ReadUInt16BigEndian(rawOffset), 1);
                    }
                    else
                    {
                        InternalCopyInt16Values(rawOffset.Slice(sizeOfElement * offset, sizeOfElement * destination.Length), destination.Span, v => new TiffRational((ushort)v, 1));
                    }
                    return default;
                }

                return new ValueTask(SlowReadShortFieldAsync(reader, entry, offset, destination, v => new TiffRational((ushort)v, 1), cancellationToken));
            }
            else if (entry.Type == TiffFieldType.Byte)
            {
                if (destination.IsEmpty)
                {
                    return default;
                }

                const int sizeOfElement = sizeof(byte);

                // is inlined ?
                if (sizeOfElement * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        Debug.Assert(destination.Length == 1);
                        destination.Span[0] = new TiffRational(rawOffset[0], 1);
                    }
                    else
                    {
                        InternalCopyByteValues(rawOffset.Slice(sizeOfElement * offset, sizeOfElement * destination.Length), destination.Span, v => new TiffRational(v, 1));
                    }
                    return default;
                }

                return new ValueTask(SlowReadByteFieldAsync(reader, entry, offset, destination, v => new TiffRational(v, 1), cancellationToken));
            }

            ThrowHelper.ThrowInvalidOperationException($"Can read tag of type {entry.Type}.");
            return default;
        }

        private async Task<TiffValueCollection<TDest>> SlowReadRationalFieldAsync<TDest>(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, int sizeLimit, Func<TiffRational, TDest>? convertFunc, CancellationToken cancellationToken) where TDest : struct
        {
            Debug.Assert(sizeLimit >= 0);
            TDest[] values = new TDest[Math.Min(sizeLimit, entry.ValueCount)];
            await SlowReadRationalFieldAsync(reader, entry, 0, values, convertFunc, cancellationToken).ConfigureAwait(false);
            return TiffValueCollection.UnsafeWrap(values);
        }

        private async Task SlowReadRationalFieldAsync<TDest>(
            TiffFileContentReader reader, TiffImageFileDirectoryEntry entry,
            long offset, Memory<TDest> destination,
            Func<TiffRational, TDest>? convertFunc, CancellationToken cancellationToken) where TDest : struct
        {
            const int sizeOfElement = 8;

            int count = destination.Length;
            if (count == 0)
            {
                return;
            }

            Debug.Assert((ulong)offset <= (ulong)entry.ValueCount);
            Debug.Assert((ulong)count <= (ulong)(entry.ValueCount - offset));

            int fieldLength = checked(sizeOfElement * count);
            byte[] buffer = ArrayPool<byte>.Shared.Rent(fieldLength);
            try
            {
                int readCount = await reader.ReadAsync(entry.ValueOffset + sizeOfElement * offset, new ArraySegment<byte>(buffer, 0, fieldLength), cancellationToken).ConfigureAwait(false);
                if (readCount != fieldLength)
                {
                    ThrowHelper.ThrowInvalidDataException("The number of bytes read from file is less than expected.");
                }

                InternalCopyRationalValues(buffer, destination.Span, convertFunc);
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
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<TiffSRational>> ReadSRationalFieldAsync(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadSRationalFieldAsync(GetAsyncReader(), entry, int.MaxValue, skipTypeValidation, cancellationToken);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SRational"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<TiffSRational>> ReadSRationalFieldAsync(TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadSRationalFieldAsync(GetAsyncReader(), entry, sizeLimit, skipTypeValidation, cancellationToken);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SRational"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="offset">The number of elements to skip reading.</param>
        /// <param name="destination">The destination buffer to store the values.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask"/> that completes when the values are read.</returns>
        public ValueTask ReadSRationalFieldAsync(TiffImageFileDirectoryEntry entry, int offset, Memory<TiffSRational> destination, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadSRationalFieldAsync(GetAsyncReader(), entry, offset, destination, skipTypeValidation, cancellationToken);

        private ValueTask<TiffValueCollection<TiffSRational>> ReadSRationalFieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
        {
            if (_context is null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(TiffFieldReader));
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (sizeLimit < 0)
            {
                sizeLimit = int.MaxValue;
            }

            long valueCount = entry.ValueCount;
            Span<byte> rawOffset = stackalloc byte[8];
            if (skipTypeValidation || entry.Type == TiffFieldType.SRational)
            {
                // is inlined ?
                if (8 * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    valueCount = Math.Min(sizeLimit, valueCount);
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

                return new ValueTask<TiffValueCollection<TiffSRational>>(SlowReadRationalFieldAsync<TiffSRational>(reader, entry, sizeLimit, null, cancellationToken));
            }
            else if (entry.Type == TiffFieldType.SLong || entry.Type == TiffFieldType.Long)
            {
                // is inlined ?
                if (sizeof(int) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    valueCount = Math.Min(sizeLimit, valueCount);
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<TiffSRational>>(TiffValueCollection.Single(
                            _context.IsLittleEndian
                                ? new TiffSRational(BinaryPrimitives.ReadInt32LittleEndian(rawOffset), 1)
                                : new TiffSRational(BinaryPrimitives.ReadInt32BigEndian(rawOffset), 1)));
                    }
                    TiffSRational[] values = new TiffSRational[valueCount];
                    InternalCopyInt32Values(rawOffset, values, v => new TiffSRational(v, 1));
                    return new ValueTask<TiffValueCollection<TiffSRational>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<TiffSRational>>(SlowReadLongFieldAsync(reader, entry, sizeLimit, v => new TiffSRational(v, 1), cancellationToken));
            }
            else if (entry.Type == TiffFieldType.SShort)
            {
                // is inlined ?
                if (sizeof(short) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    valueCount = Math.Min(sizeLimit, valueCount);
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<TiffSRational>>(TiffValueCollection.Single(
                            _context.IsLittleEndian
                                ? new TiffSRational(BinaryPrimitives.ReadInt16LittleEndian(rawOffset), 1)
                                : new TiffSRational(BinaryPrimitives.ReadInt16BigEndian(rawOffset), 1)));
                    }
                    TiffSRational[] values = new TiffSRational[valueCount];
                    InternalCopyInt16Values(rawOffset, values, v => new TiffSRational(v, 1));
                    return new ValueTask<TiffValueCollection<TiffSRational>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<TiffSRational>>(SlowReadShortFieldAsync(reader, entry, sizeLimit, v => new TiffSRational(v, 1), cancellationToken));
            }
            else if (entry.Type == TiffFieldType.Short)
            {
                // is inlined ?
                if (sizeof(ushort) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    valueCount = Math.Min(sizeLimit, valueCount);
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<TiffSRational>>(TiffValueCollection.Single(
                            _context.IsLittleEndian
                                ? new TiffSRational(BinaryPrimitives.ReadUInt16LittleEndian(rawOffset), 1)
                                : new TiffSRational(BinaryPrimitives.ReadUInt16BigEndian(rawOffset), 1)));
                    }
                    TiffSRational[] values = new TiffSRational[valueCount];
                    InternalCopyInt16Values(rawOffset, values, v => new TiffSRational((ushort)v, 1));
                    return new ValueTask<TiffValueCollection<TiffSRational>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<TiffSRational>>(SlowReadShortFieldAsync(reader, entry, sizeLimit, v => new TiffSRational((ushort)v, 1), cancellationToken));
            }
            else if (entry.Type == TiffFieldType.SByte)
            {
                // is inlined ?
                if (valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    valueCount = Math.Min(sizeLimit, valueCount);
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<TiffSRational>>(TiffValueCollection.Single(new TiffSRational((sbyte)rawOffset[0], 1)));
                    }
                    TiffSRational[] values = new TiffSRational[valueCount];
                    InternalCopyByteValues(rawOffset, values, v => new TiffSRational((sbyte)v, 1));
                    return new ValueTask<TiffValueCollection<TiffSRational>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<TiffSRational>>(SlowReadByteFieldAsync(reader, entry, sizeLimit, v => new TiffSRational((sbyte)v, 1), cancellationToken));
            }
            else if (entry.Type == TiffFieldType.Byte)
            {
                // is inlined ?
                if (valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    valueCount = Math.Min(sizeLimit, valueCount);
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        return new ValueTask<TiffValueCollection<TiffSRational>>(TiffValueCollection.Single(new TiffSRational(rawOffset[0], 1)));
                    }
                    TiffSRational[] values = new TiffSRational[valueCount];
                    InternalCopyByteValues(rawOffset, values, v => new TiffSRational(v, 1));
                    return new ValueTask<TiffValueCollection<TiffSRational>>(TiffValueCollection.UnsafeWrap(values));
                }

                return new ValueTask<TiffValueCollection<TiffSRational>>(SlowReadByteFieldAsync(reader, entry, sizeLimit, v => new TiffSRational(v, 1), cancellationToken));
            }

            ThrowHelper.ThrowInvalidOperationException($"Can read tag of type {entry.Type}.");
            return default;
        }

        private ValueTask ReadSRationalFieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, int offset, Memory<TiffSRational> destination, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
        {
            if (_context is null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(TiffFieldReader));
            }

            long valueCount = entry.ValueCount;
            if ((ulong)offset > (ulong)valueCount)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(offset));
            }
            if ((ulong)destination.Length > (ulong)(valueCount - offset))
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(destination));
            }

            cancellationToken.ThrowIfCancellationRequested();

            Span<byte> rawOffset = stackalloc byte[8];
            if (skipTypeValidation || entry.Type == TiffFieldType.SRational)
            {
                if (destination.IsEmpty)
                {
                    return default;
                }

                const int sizeOfElement = 8;

                // is inlined ?
                if (sizeOfElement * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    InternalCopyRationalValues(rawOffset.Slice(sizeOfElement * offset, sizeOfElement * destination.Length), destination.Span, null);
                    return default;
                }

                return new ValueTask(SlowReadRationalFieldAsync(reader, entry, offset, destination, null, cancellationToken));
            }
            else if (entry.Type == TiffFieldType.SLong || entry.Type == TiffFieldType.Long)
            {
                if (destination.IsEmpty)
                {
                    return default;
                }

                const int sizeOfElement = sizeof(int);

                // is inlined ?
                if (sizeOfElement * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        Debug.Assert(destination.Length == 1);
                        destination.Span[0] = _context.IsLittleEndian
                                ? new TiffSRational(BinaryPrimitives.ReadInt32LittleEndian(rawOffset), 1)
                                : new TiffSRational(BinaryPrimitives.ReadInt32BigEndian(rawOffset), 1);
                    }
                    else
                    {
                        InternalCopyInt32Values(rawOffset.Slice(sizeOfElement * offset, sizeOfElement * destination.Length), destination.Span, v => new TiffSRational(v, 1));
                    }
                    return default;
                }

                return new ValueTask(SlowReadLongFieldAsync(reader, entry, offset, destination, v => new TiffSRational(v, 1), cancellationToken));
            }
            else if (entry.Type == TiffFieldType.SShort)
            {
                if (destination.IsEmpty)
                {
                    return default;
                }

                const int sizeOfElement = sizeof(short);

                // is inlined ?
                if (sizeOfElement * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        Debug.Assert(destination.Length == 1);
                        destination.Span[0] = _context.IsLittleEndian
                                ? new TiffSRational(BinaryPrimitives.ReadInt16LittleEndian(rawOffset), 1)
                                : new TiffSRational(BinaryPrimitives.ReadInt16BigEndian(rawOffset), 1);
                    }
                    else
                    {
                        InternalCopyInt16Values(rawOffset.Slice(sizeOfElement * offset, sizeOfElement * destination.Length), destination.Span, v => new TiffSRational(v, 1));
                    }
                    return default;
                }

                return new ValueTask(SlowReadShortFieldAsync(reader, entry, offset, destination, v => new TiffSRational(v, 1), cancellationToken));
            }
            else if (entry.Type == TiffFieldType.Short)
            {
                if (destination.IsEmpty)
                {
                    return default;
                }

                const int sizeOfElement = sizeof(ushort);

                // is inlined ?
                if (sizeOfElement * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        Debug.Assert(destination.Length == 1);
                        destination.Span[0] = _context.IsLittleEndian
                                ? new TiffSRational(BinaryPrimitives.ReadUInt16LittleEndian(rawOffset), 1)
                                : new TiffSRational(BinaryPrimitives.ReadUInt16BigEndian(rawOffset), 1);
                    }
                    else
                    {
                        InternalCopyInt16Values(rawOffset.Slice(sizeOfElement * offset, sizeOfElement * destination.Length), destination.Span, v => new TiffSRational((ushort)v, 1));
                    }
                    return default;
                }

                return new ValueTask(SlowReadShortFieldAsync(reader, entry, offset, destination, v => new TiffSRational((ushort)v, 1), cancellationToken));
            }
            else if (entry.Type == TiffFieldType.SByte)
            {
                if (destination.IsEmpty)
                {
                    return default;
                }

                const int sizeOfElement = sizeof(sbyte);

                // is inlined ?
                if (sizeOfElement * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        Debug.Assert(destination.Length == 1);
                        destination.Span[0] = new TiffSRational((sbyte)rawOffset[0], 1);
                    }
                    else
                    {
                        InternalCopyByteValues(rawOffset.Slice(sizeOfElement * offset, sizeOfElement * destination.Length), destination.Span, v => new TiffSRational((sbyte)v, 1));
                    }
                    return default;
                }

                return new ValueTask(SlowReadByteFieldAsync(reader, entry, offset, destination, v => new TiffSRational((sbyte)v, 1), cancellationToken));
            }
            else if (entry.Type == TiffFieldType.Byte)
            {
                if (destination.IsEmpty)
                {
                    return default;
                }

                const int sizeOfElement = sizeof(byte);

                // is inlined ?
                if (sizeOfElement * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        Debug.Assert(destination.Length == 1);
                        destination.Span[0] = new TiffSRational(rawOffset[0], 1);
                    }
                    else
                    {
                        InternalCopyByteValues(rawOffset.Slice(sizeOfElement * offset, sizeOfElement * destination.Length), destination.Span, v => new TiffSRational(v, 1));
                    }
                    return default;
                }

                return new ValueTask(SlowReadByteFieldAsync(reader, entry, offset, destination, v => new TiffSRational(v, 1), cancellationToken));
            }

            ThrowHelper.ThrowInvalidOperationException($"Can read tag of type {entry.Type}.");
            return default;
        }

        #endregion

        #region IFD

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.IFD"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<TiffStreamOffset>> ReadIFDFieldAsync(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadIFDFieldAsync(GetAsyncReader(), entry, int.MaxValue, skipTypeValidation, cancellationToken);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.IFD"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<TiffStreamOffset>> ReadIFDFieldAsync(TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadIFDFieldAsync(GetAsyncReader(), entry, sizeLimit, skipTypeValidation, cancellationToken);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.IFD"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="offset">The number of elements to skip reading.</param>
        /// <param name="destination">The destination buffer to store the values.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask"/> that completes when the values are read.</returns>
        public ValueTask ReadIFDFieldAsync(TiffImageFileDirectoryEntry entry, int offset, Memory<TiffStreamOffset> destination, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadIFDFieldAsync(GetAsyncReader(), entry, offset, destination, skipTypeValidation, cancellationToken);

        private ValueTask<TiffValueCollection<TiffStreamOffset>> ReadIFDFieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
        {
            if (_context is null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(TiffFieldReader));
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (sizeLimit < 0)
            {
                sizeLimit = int.MaxValue;
            }

            long valueCount = entry.ValueCount;
            Span<byte> rawOffset = stackalloc byte[8];
            if (skipTypeValidation || entry.Type == TiffFieldType.Long || entry.Type == TiffFieldType.SLong || entry.Type == TiffFieldType.IFD)
            {
                // is inlined ?
                if (sizeof(int) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    valueCount = Math.Min(sizeLimit, valueCount);
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

                return new ValueTask<TiffValueCollection<TiffStreamOffset>>(SlowReadLongFieldAsync(reader, entry, sizeLimit, offset => new TiffStreamOffset(offset), cancellationToken));
            }

            ThrowHelper.ThrowInvalidOperationException($"Can read tag of type {entry.Type}.");
            return default;
        }

        private ValueTask ReadIFDFieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, int offset, Memory<TiffStreamOffset> destination, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
        {
            if (_context is null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(TiffFieldReader));
            }

            long valueCount = entry.ValueCount;
            if ((ulong)offset > (ulong)valueCount)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(offset));
            }
            if ((ulong)destination.Length > (ulong)(valueCount - offset))
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(destination));
            }

            cancellationToken.ThrowIfCancellationRequested();

            Span<byte> rawOffset = stackalloc byte[8];
            if (skipTypeValidation || entry.Type == TiffFieldType.Long || entry.Type == TiffFieldType.SLong || entry.Type == TiffFieldType.IFD)
            {
                if (destination.IsEmpty)
                {
                    return default;
                }

                const int sizeOfElement = sizeof(int);

                // is inlined ?
                if (sizeof(int) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        Debug.Assert(destination.Length == 1);
                        destination.Span[0] = new TiffStreamOffset(
                                        _context.IsLittleEndian
                                            ? BinaryPrimitives.ReadInt32LittleEndian(rawOffset)
                                            : BinaryPrimitives.ReadInt32BigEndian(rawOffset));
                    }
                    else
                    {
                        InternalCopyInt32Values(rawOffset.Slice(sizeOfElement * offset, sizeOfElement * destination.Length), destination.Span, offset => new TiffStreamOffset(offset));
                    }
                    return default;
                }

                return new ValueTask(SlowReadLongFieldAsync(reader, entry, offset, destination, offset => new TiffStreamOffset(offset), cancellationToken));
            }

            ThrowHelper.ThrowInvalidOperationException($"Can read tag of type {entry.Type}.");
            return default;
        }

        #endregion

        #region IFD8

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.IFD8"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<TiffStreamOffset>> ReadIFD8FieldAsync(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadIFD8FieldAsync(GetAsyncReader(), entry, int.MaxValue, skipTypeValidation, cancellationToken);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.IFD8"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<TiffStreamOffset>> ReadIFD8FieldAsync(TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadIFD8FieldAsync(GetAsyncReader(), entry, sizeLimit, skipTypeValidation, cancellationToken);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.IFD8"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="offset">The number of elements to skip reading.</param>
        /// <param name="destination">The destination buffer to store the values.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask"/> that completes when the values are read.</returns>
        public ValueTask ReadIFD8FieldAsync(TiffImageFileDirectoryEntry entry, int offset, Memory<TiffStreamOffset> destination, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
            => ReadIFD8FieldAsync(GetAsyncReader(), entry, offset, destination, skipTypeValidation, cancellationToken);

        private ValueTask<TiffValueCollection<TiffStreamOffset>> ReadIFD8FieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
        {
            if (_context is null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(TiffFieldReader));
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (sizeLimit < 0)
            {
                sizeLimit = int.MaxValue;
            }

            long valueCount = entry.ValueCount;
            Span<byte> rawOffset = stackalloc byte[8];
            if (skipTypeValidation || entry.Type == TiffFieldType.Long8 || entry.Type == TiffFieldType.SLong8 || entry.Type == TiffFieldType.IFD8)
            {
                // is inlined ?
                if (sizeof(long) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    valueCount = Math.Min(sizeLimit, valueCount);
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

                return new ValueTask<TiffValueCollection<TiffStreamOffset>>(SlowReadLong8FieldAsync(reader, entry, sizeLimit, offset => new TiffStreamOffset(offset), cancellationToken));
            }
            else if (entry.Type == TiffFieldType.Long || entry.Type == TiffFieldType.IFD)
            {
                // is inlined ?
                if (sizeof(uint) * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    valueCount = Math.Min(sizeLimit, valueCount);
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

                return new ValueTask<TiffValueCollection<TiffStreamOffset>>(SlowReadLongFieldAsync(reader, entry, sizeLimit, v => new TiffStreamOffset((uint)v), cancellationToken));
            }

            ThrowHelper.ThrowInvalidOperationException($"Can read tag of type {entry.Type}.");
            return default;
        }


        private ValueTask ReadIFD8FieldAsync(TiffFileContentReader reader, TiffImageFileDirectoryEntry entry, int offset, Memory<TiffStreamOffset> destination, bool skipTypeValidation = false, CancellationToken cancellationToken = default)
        {
            if (_context is null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(TiffFieldReader));
            }

            long valueCount = entry.ValueCount;
            if ((ulong)offset > (ulong)valueCount)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(offset));
            }
            if ((ulong)destination.Length > (ulong)(valueCount - offset))
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(destination));
            }

            cancellationToken.ThrowIfCancellationRequested();

            Span<byte> rawOffset = stackalloc byte[8];
            if (skipTypeValidation || entry.Type == TiffFieldType.Long8 || entry.Type == TiffFieldType.SLong8 || entry.Type == TiffFieldType.IFD8)
            {
                if (destination.IsEmpty)
                {
                    return default;
                }

                const int sizeOfElement = sizeof(long);

                // is inlined ?
                if (sizeOfElement * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        Debug.Assert(destination.Length == 1);
                        destination.Span[0] = new TiffStreamOffset(
                                        _context.IsLittleEndian
                                            ? BinaryPrimitives.ReadInt64LittleEndian(rawOffset)
                                            : BinaryPrimitives.ReadInt64BigEndian(rawOffset));
                    }
                    else
                    {
                        InternalCopyInt64Values(rawOffset.Slice(sizeOfElement * offset, sizeOfElement * destination.Length), destination.Span, offset => new TiffStreamOffset(offset));
                    }
                    return default;
                }

                return new ValueTask(SlowReadLong8FieldAsync(reader, entry, offset, destination, offset => new TiffStreamOffset(offset), cancellationToken));
            }
            else if (entry.Type == TiffFieldType.Long || entry.Type == TiffFieldType.IFD)
            {
                if (destination.IsEmpty)
                {
                    return default;
                }

                const int sizeOfElement = sizeof(uint);

                // is inlined ?
                if (sizeOfElement * valueCount <= _context.ByteCountOfValueOffsetField)
                {
                    entry.RestoreRawOffsetBytes(_context, rawOffset);
                    if (valueCount == 1)
                    {
                        Debug.Assert(destination.Length == 1);
                        destination.Span[0] = _context.IsLittleEndian
                                ? BinaryPrimitives.ReadUInt32LittleEndian(rawOffset)
                                : BinaryPrimitives.ReadUInt32BigEndian(rawOffset);
                    }
                    else
                    {
                        InternalCopyInt32Values(rawOffset.Slice(sizeOfElement * offset, sizeOfElement * destination.Length), destination.Span, v => new TiffStreamOffset((uint)v));
                    }
                    return default;
                }

                return new ValueTask(SlowReadLongFieldAsync(reader, entry, offset, destination, v => new TiffStreamOffset((uint)v), cancellationToken));
            }

            ThrowHelper.ThrowInvalidOperationException($"Can read tag of type {entry.Type}.");
            return default;
        }


        #endregion

    }
}
