using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TiffLibrary
{
    public partial class TiffImageFileDirectoryWriter
    {
        private bool TryFindEntry(TiffTag tag, out int index, out TiffImageFileDirectoryEntry entry)
        {
            List<TiffImageFileDirectoryEntry> entries = _entries;
            for (int i = 0; i < entries.Count; i++)
            {
                TiffImageFileDirectoryEntry item = entries[i];
                if (item.Tag == tag)
                {
                    index = i;
                    entry = item;
                    return true;
                }
            }
            index = default;
            entry = default;
            return false;
        }

        private void AddOrUpdateEntry(TiffTag tag, TiffFieldType type, int count, TiffStreamOffset offset)
        {
            if (TryFindEntry(tag, out int i, out _))
            {
                _entries[i] = new TiffImageFileDirectoryEntry(tag, type, count, offset);
            }
            else
            {
                _entries.Add(new TiffImageFileDirectoryEntry(tag, type, count, offset));
            }
        }

        private void AddOrUpdateEntry(TiffTag tag, TiffFieldType type, int count, Span<byte> buffer)
        {
            Debug.Assert(_writer != null);

            if (TryFindEntry(tag, out int i, out _))
            {
                _entries[i] = new TiffImageFileDirectoryEntry(_writer!.OperationContext, tag, type, count, buffer);
            }
            else
            {
                _entries.Add(new TiffImageFileDirectoryEntry(_writer!.OperationContext, tag, type, count, buffer));
            }
        }

        #region Raw

        internal void AddInlineTag(TiffTag tag, TiffFieldType type, int count, ReadOnlySpan<byte> rawData)
        {
            Debug.Assert(_writer != null);

            if (rawData.Length > _writer!.OperationContext.ByteCountOfValueOffsetField)
            {
                ThrowHelper.ThrowArgumentException("rawData too big.", nameof(rawData));
            }

            Span<byte> buffer = stackalloc byte[8];
            rawData.CopyTo(buffer);

            AddOrUpdateEntry(tag, type, count, buffer);
        }

        internal void AddPointerTag(TiffTag tag, TiffFieldType type, int count, TiffStreamOffset offset)
        {
            if (TryFindEntry(tag, out int i, out _))
            {
                _entries[i] = new TiffImageFileDirectoryEntry(tag, type, count, offset);
            }
            else
            {
                _entries.Add(new TiffImageFileDirectoryEntry(tag, type, count, offset));
            }
        }

        /// <summary>
        /// Write values of the specified type to the specified tag in this IFD.
        /// </summary>
        /// <param name="tag">The specified tag.</param>
        /// <param name="type">The specified type.</param>
        /// <param name="valueCount">The value count.</param>
        /// <param name="values">The values to write.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort this operation.</param>
        /// <returns>A <see cref="ValueTask"/> that completes when the values have been written.</returns>
        [CLSCompliant(false)]
        public ValueTask WriteTagAsync(TiffTag tag, TiffFieldType type, int valueCount, TiffValueCollection<byte> values, CancellationToken cancellationToken = default)
        {
            EnsureNotDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            Debug.Assert(_writer != null);

            if (values.Count <= _writer!.OperationContext.ByteCountOfValueOffsetField)
            {
                Span<byte> stackBuffer = stackalloc byte[8];
                values.CopyTo(stackBuffer);

                AddOrUpdateEntry(tag, type, valueCount, stackBuffer);

                return default;
            }
            else
            {
                return new ValueTask(WriteTagSlowAsync(tag, type, valueCount, values, cancellationToken));
            }
        }

        internal async Task WriteTagSlowAsync(TiffTag tag, TiffFieldType type, int valueCount, TiffValueCollection<byte> values, CancellationToken cancellationToken)
        {
            Debug.Assert(_writer != null);
            TiffStreamOffset offset = await _writer!.WriteAlignedBytesAsync(values.GetOrCreateArray(), cancellationToken).ConfigureAwait(false);
            AddOrUpdateEntry(tag, type, valueCount, offset);
        }

        #endregion

        #region Bytes

        /// <summary>
        /// Write values of <see cref="TiffFieldType.Byte"/> to the specified tag in this IFD.
        /// </summary>
        /// <param name="tag">The specified tag.</param>
        /// <param name="values">The values to write.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort this operation.</param>
        /// <returns>A <see cref="ValueTask"/> that completes when the values have been written.</returns>
        [CLSCompliant(false)]
        public ValueTask WriteTagAsync(TiffTag tag, TiffValueCollection<byte> values, CancellationToken cancellationToken = default)
            => WriteTagAsync(tag, TiffFieldType.Byte, values, cancellationToken);

        /// <summary>
        /// Write values of the specified type to the specified tag in this IFD.
        /// </summary>
        /// <param name="tag">The specified tag.</param>
        /// <param name="type">The specified type.</param>
        /// <param name="values">The values to write.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort this operation.</param>
        /// <returns>A <see cref="ValueTask"/> that completes when the values have been written.</returns>
        [CLSCompliant(false)]
        public ValueTask WriteTagAsync(TiffTag tag, TiffFieldType type, TiffValueCollection<byte> values, CancellationToken cancellationToken = default)
        {
            EnsureNotDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            Debug.Assert(_writer != null);

            const int ElementSize = sizeof(byte);
            int byteCount = ElementSize * values.Count;

            if (byteCount <= _writer!.OperationContext.ByteCountOfValueOffsetField)
            {
                Span<byte> stackBuffer = stackalloc byte[8];
                int i;
                for (i = 0; i < values.Count; i++)
                {
                    byte item = values[i];
                    MemoryMarshal.Write(stackBuffer.Slice(ElementSize * i), ref item);
                }

                AddOrUpdateEntry(tag, type, values.Count, stackBuffer);

                return default;
            }
            else
            {
                return new ValueTask(WriteTagSlowAsync(tag, type, values, cancellationToken));
            }
        }

        internal async Task WriteTagSlowAsync(TiffTag tag, TiffFieldType type, TiffValueCollection<byte> values, CancellationToken cancellationToken)
        {
            Debug.Assert(_writer != null);
            TiffStreamOffset offset = await _writer!.WriteAlignedBytesAsync(values.GetOrCreateArray(), cancellationToken).ConfigureAwait(false);
            AddOrUpdateEntry(tag, type, values.Count, offset);
        }

        #endregion

        #region ASCII

        /// <summary>
        /// Write values of <see cref="TiffFieldType.ASCII"/> to the specified tag in this IFD.
        /// </summary>
        /// <param name="tag">The specified tag.</param>
        /// <param name="values">The values to write.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort this operation.</param>
        /// <returns>A <see cref="ValueTask"/> that completes when the values have been written.</returns>
        [CLSCompliant(false)]
        public ValueTask WriteTagAsync(TiffTag tag, TiffValueCollection<string> values, CancellationToken cancellationToken = default)
        {
            EnsureNotDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            Debug.Assert(_writer != null);

            int estimatedLength = 0;
            foreach (string value in values)
            {
                estimatedLength += Encoding.ASCII.GetByteCount(value) + 1;
            }

            if (estimatedLength <= _writer!.OperationContext.ByteCountOfValueOffsetField)
            {
                Span<byte> stackBuffer = stackalloc byte[8];

                int bytesWritten = 0;
                foreach (string value in values)
                {
                    int writeCount = EncodingASCIIWriteString(value.AsSpan(), stackBuffer.Slice(bytesWritten));
                    stackBuffer[bytesWritten] = 0;
                    bytesWritten += writeCount + 1;
                }

                if (TryFindEntry(tag, out int i, out TiffImageFileDirectoryEntry entry))
                {
                    _entries[i] = new TiffImageFileDirectoryEntry(_writer.OperationContext, tag, TiffFieldType.ASCII, bytesWritten, stackBuffer);
                }
                else
                {
                    _entries.Add(new TiffImageFileDirectoryEntry(_writer.OperationContext, tag, TiffFieldType.ASCII, bytesWritten, stackBuffer));
                }

                return default;
            }
            else
            {
                return new ValueTask(WriteTagSlowAsync(tag, values, cancellationToken));
            }
        }

        internal async Task WriteTagSlowAsync(TiffTag tag, TiffValueCollection<string> values, CancellationToken cancellationToken)
        {
            Debug.Assert(_writer != null);
            TiffStreamRegion region = await _writer!.WriteAlignedValues(values, cancellationToken).ConfigureAwait(false);
            AddOrUpdateEntry(tag, TiffFieldType.ASCII, region.Length, region.Offset);
        }

        private static unsafe int EncodingASCIIWriteString(ReadOnlySpan<char> data, Span<byte> destination)
        {
#if NO_FAST_SPAN
            fixed (char* pData = data)
            fixed (byte* pDest = destination)
            {
                return Encoding.ASCII.GetBytes(pData, data.Length, pDest, destination.Length);
            }
#else
            return Encoding.ASCII.GetBytes(data, destination);
#endif
        }

        #endregion

        #region Short

        internal void AddTag(TiffTag tag, ushort value)
        {
            Span<byte> stackBuffer = stackalloc byte[8];
            MemoryMarshal.Write(stackBuffer, ref value);

            AddOrUpdateEntry(tag, TiffFieldType.Short, 1, stackBuffer);
        }

        /// <summary>
        /// Write values of <see cref="TiffFieldType.Short"/> to the specified tag in this IFD.
        /// </summary>
        /// <param name="tag">The specified tag.</param>
        /// <param name="values">The values to write.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort this operation.</param>
        /// <returns>A <see cref="ValueTask"/> that completes when the values have been written.</returns>
        [CLSCompliant(false)]
        public ValueTask WriteTagAsync(TiffTag tag, TiffValueCollection<ushort> values, CancellationToken cancellationToken = default)
        {
            EnsureNotDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            Debug.Assert(_writer != null);

            const int ElementSize = sizeof(ushort);
            int byteCount = ElementSize * values.Count;

            if (byteCount <= _writer!.OperationContext.ByteCountOfValueOffsetField)
            {
                Span<byte> stackBuffer = stackalloc byte[8];
                int i;
                for (i = 0; i < values.Count; i++)
                {
                    ushort item = values[i];
                    MemoryMarshal.Write(stackBuffer.Slice(ElementSize * i), ref item);
                }

                AddOrUpdateEntry(tag, TiffFieldType.Short, values.Count, stackBuffer);

                return default;
            }
            else
            {
                return new ValueTask(WriteTagSlowAsync(tag, values, cancellationToken));
            }
        }

        internal async Task WriteTagSlowAsync(TiffTag tag, TiffValueCollection<ushort> values, CancellationToken cancellationToken = default)
        {
            Debug.Assert(_writer != null);
            TiffStreamRegion region = await _writer!.WriteAlignedValues(values, cancellationToken).ConfigureAwait(false);
            AddOrUpdateEntry(tag, TiffFieldType.Short, values.Count, region.Offset);
        }

        #endregion

        #region SShort

        internal void AddTag(TiffTag tag, short value)
        {
            Span<byte> stackBuffer = stackalloc byte[8];
            MemoryMarshal.Write(stackBuffer, ref value);

            AddOrUpdateEntry(tag, TiffFieldType.SShort, 1, stackBuffer);
        }

        /// <summary>
        /// Write values of <see cref="TiffFieldType.SShort"/> to the specified tag in this IFD.
        /// </summary>
        /// <param name="tag">The specified tag.</param>
        /// <param name="values">The values to write.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort this operation.</param>
        /// <returns>A <see cref="ValueTask"/> that completes when the values have been written.</returns>
        [CLSCompliant(false)]
        public ValueTask WriteTagAsync(TiffTag tag, TiffValueCollection<short> values, CancellationToken cancellationToken = default)
        {
            EnsureNotDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            Debug.Assert(_writer != null);

            const int ElementSize = sizeof(short);
            int byteCount = ElementSize * values.Count;

            if (byteCount <= _writer!.OperationContext.ByteCountOfValueOffsetField)
            {
                Span<byte> stackBuffer = stackalloc byte[8];
                int i;
                for (i = 0; i < values.Count; i++)
                {
                    short item = values[i];
                    MemoryMarshal.Write(stackBuffer.Slice(ElementSize * i), ref item);
                }

                AddOrUpdateEntry(tag, TiffFieldType.SShort, values.Count, stackBuffer);

                return default;
            }
            else
            {
                return new ValueTask(WriteTagSlowAsync(tag, values, cancellationToken));
            }
        }

        internal async Task WriteTagSlowAsync(TiffTag tag, TiffValueCollection<short> values, CancellationToken cancellationToken)
        {
            Debug.Assert(_writer != null);
            TiffStreamRegion region = await _writer!.WriteAlignedValues(values, cancellationToken).ConfigureAwait(false);
            AddOrUpdateEntry(tag, TiffFieldType.SShort, values.Count, region.Offset);
        }

        #endregion

        #region Long

        internal void AddTag(TiffTag tag, uint value)
        {
            Span<byte> stackBuffer = stackalloc byte[8];
            MemoryMarshal.Write(stackBuffer, ref value);

            AddOrUpdateEntry(tag, TiffFieldType.Long, 1, stackBuffer);
        }

        /// <summary>
        /// Write values of <see cref="TiffFieldType.Long"/> to the specified tag in this IFD.
        /// </summary>
        /// <param name="tag">The specified tag.</param>
        /// <param name="values">The values to write.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort this operation.</param>
        /// <returns>A <see cref="ValueTask"/> that completes when the values have been written.</returns>
        [CLSCompliant(false)]
        public ValueTask WriteTagAsync(TiffTag tag, TiffValueCollection<uint> values, CancellationToken cancellationToken = default)
        {
            EnsureNotDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            Debug.Assert(_writer != null);

            const int ElementSize = sizeof(uint);
            int byteCount = ElementSize * values.Count;

            if (byteCount <= _writer!.OperationContext.ByteCountOfValueOffsetField)
            {
                Span<byte> stackBuffer = stackalloc byte[8];
                int i;
                for (i = 0; i < values.Count; i++)
                {
                    uint item = values[i];
                    MemoryMarshal.Write(stackBuffer.Slice(ElementSize * i), ref item);
                }

                AddOrUpdateEntry(tag, TiffFieldType.Long, values.Count, stackBuffer);

                return default;
            }
            else
            {
                return new ValueTask(WriteTagSlowAsync(tag, values, cancellationToken));
            }
        }

        internal async Task WriteTagSlowAsync(TiffTag tag, TiffValueCollection<uint> values, CancellationToken cancellationToken = default)
        {
            Debug.Assert(_writer != null);
            TiffStreamRegion region = await _writer!.WriteAlignedValues(values, cancellationToken).ConfigureAwait(false);
            AddOrUpdateEntry(tag, TiffFieldType.Long, values.Count, region.Offset);
        }

        #endregion

        #region SLong

        internal void AddTag(TiffTag tag, int value)
        {
            Span<byte> stackBuffer = stackalloc byte[8];
            MemoryMarshal.Write(stackBuffer, ref value);

            AddOrUpdateEntry(tag, TiffFieldType.SLong, 1, stackBuffer);
        }

        /// <summary>
        /// Write values of <see cref="TiffFieldType.SLong"/> to the specified tag in this IFD.
        /// </summary>
        /// <param name="tag">The specified tag.</param>
        /// <param name="values">The values to write.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort this operation.</param>
        /// <returns>A <see cref="ValueTask"/> that completes when the values have been written.</returns>
        [CLSCompliant(false)]
        public ValueTask WriteTagAsync(TiffTag tag, TiffValueCollection<int> values, CancellationToken cancellationToken = default)
        {
            EnsureNotDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            Debug.Assert(_writer != null);


            const int ElementSize = sizeof(int);
            int byteCount = ElementSize * values.Count;

            if (byteCount <= _writer!.OperationContext.ByteCountOfValueOffsetField)
            {
                Span<byte> stackBuffer = stackalloc byte[8];
                int i;
                for (i = 0; i < values.Count; i++)
                {
                    int item = values[i];
                    MemoryMarshal.Write(stackBuffer.Slice(ElementSize * i), ref item);
                }

                AddOrUpdateEntry(tag, TiffFieldType.SLong, values.Count, stackBuffer);

                return default;
            }
            else
            {
                return new ValueTask(WriteTagSlowAsync(tag, values, cancellationToken));
            }
        }

        internal async Task WriteTagSlowAsync(TiffTag tag, TiffValueCollection<int> values, CancellationToken cancellationToken)
        {
            Debug.Assert(_writer != null);
            TiffStreamRegion region = await _writer!.WriteAlignedValues(values, cancellationToken).ConfigureAwait(false);
            AddOrUpdateEntry(tag, TiffFieldType.SLong, values.Count, region.Offset);
        }

        #endregion

        #region Long8

        /// <summary>
        /// Write values of <see cref="TiffFieldType.Long8"/> to the specified tag in this IFD.
        /// </summary>
        /// <param name="tag">The specified tag.</param>
        /// <param name="values">The values to write.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort this operation.</param>
        /// <returns>A <see cref="ValueTask"/> that completes when the values have been written.</returns>
        [CLSCompliant(false)]
        public ValueTask WriteTagAsync(TiffTag tag, TiffValueCollection<ulong> values, CancellationToken cancellationToken = default)
        {
            EnsureNotDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            Debug.Assert(_writer != null);

            const int ElementSize = sizeof(ulong);
            int byteCount = ElementSize * values.Count;

            if (byteCount <= _writer!.OperationContext.ByteCountOfValueOffsetField)
            {
                Span<byte> stackBuffer = stackalloc byte[8];
                int i;
                for (i = 0; i < values.Count; i++)
                {
                    ulong item = values[i];
                    MemoryMarshal.Write(stackBuffer.Slice(ElementSize * i), ref item);
                }

                AddOrUpdateEntry(tag, TiffFieldType.Long8, values.Count, stackBuffer);

                return default;
            }
            else
            {
                return new ValueTask(WriteTagSlowAsync(tag, values, cancellationToken));
            }
        }

        internal async Task WriteTagSlowAsync(TiffTag tag, TiffValueCollection<ulong> values, CancellationToken cancellationToken)
        {
            Debug.Assert(_writer != null);
            TiffStreamRegion region = await _writer!.WriteAlignedValues(values, cancellationToken).ConfigureAwait(false);
            AddOrUpdateEntry(tag, TiffFieldType.Long8, values.Count, region.Offset);
        }

        #endregion

        #region SLong8

        /// <summary>
        /// Write values of <see cref="TiffFieldType.SLong8"/> to the specified tag in this IFD.
        /// </summary>
        /// <param name="tag">The specified tag.</param>
        /// <param name="values">The values to write.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort this operation.</param>
        /// <returns>A <see cref="ValueTask"/> that completes when the values have been written.</returns>
        [CLSCompliant(false)]
        public ValueTask WriteTagAsync(TiffTag tag, TiffValueCollection<long> values, CancellationToken cancellationToken = default)
        {
            EnsureNotDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            Debug.Assert(_writer != null);

            const int ElementSize = sizeof(long);
            int byteCount = ElementSize * values.Count;

            if (byteCount <= _writer!.OperationContext.ByteCountOfValueOffsetField)
            {
                Span<byte> stackBuffer = stackalloc byte[8];
                int i;
                for (i = 0; i < values.Count; i++)
                {
                    long item = values[i];
                    MemoryMarshal.Write(stackBuffer.Slice(ElementSize * i), ref item);
                }

                AddOrUpdateEntry(tag, TiffFieldType.SLong8, values.Count, stackBuffer);

                return default;
            }
            else
            {
                return new ValueTask(WriteTagSlowAsync(tag, values, cancellationToken));
            }
        }

        internal async Task WriteTagSlowAsync(TiffTag tag, TiffValueCollection<long> values, CancellationToken cancellationToken)
        {
            Debug.Assert(_writer != null);
            TiffStreamRegion region = await _writer!.WriteAlignedValues(values, cancellationToken).ConfigureAwait(false);
            AddOrUpdateEntry(tag, TiffFieldType.SLong8, values.Count, region.Offset);
        }

        #endregion

        #region Float

        /// <summary>
        /// Write values of <see cref="TiffFieldType.Float"/> to the specified tag in this IFD.
        /// </summary>
        /// <param name="tag">The specified tag.</param>
        /// <param name="values">The values to write.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort this operation.</param>
        /// <returns>A <see cref="ValueTask"/> that completes when the values have been written.</returns>
        [CLSCompliant(false)]
        public ValueTask WriteTagAsync(TiffTag tag, TiffValueCollection<float> values, CancellationToken cancellationToken = default)
        {
            EnsureNotDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            Debug.Assert(_writer != null);

            const int ElementSize = sizeof(float);
            int byteCount = ElementSize * values.Count;

            if (byteCount <= _writer!.OperationContext.ByteCountOfValueOffsetField)
            {
                Span<byte> stackBuffer = stackalloc byte[8];
                int i;
                for (i = 0; i < values.Count; i++)
                {
                    float item = values[i];
                    MemoryMarshal.Write(stackBuffer.Slice(ElementSize * i), ref item);
                }

                AddOrUpdateEntry(tag, TiffFieldType.Float, values.Count, stackBuffer);

                return default;
            }
            else
            {
                return new ValueTask(WriteTagSlowAsync(tag, values, cancellationToken));
            }
        }

        internal async Task WriteTagSlowAsync(TiffTag tag, TiffValueCollection<float> values, CancellationToken cancellationToken)
        {
            Debug.Assert(_writer != null);
            TiffStreamRegion region = await _writer!.WriteAlignedValues(values, cancellationToken).ConfigureAwait(false);
            AddOrUpdateEntry(tag, TiffFieldType.Float, values.Count, region.Offset);
        }

        #endregion

        #region Double

        /// <summary>
        /// Write values of <see cref="TiffFieldType.Double"/> to the specified tag in this IFD.
        /// </summary>
        /// <param name="tag">The specified tag.</param>
        /// <param name="values">The values to write.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort this operation.</param>
        /// <returns>A <see cref="ValueTask"/> that completes when the values have been written.</returns>
        [CLSCompliant(false)]
        public ValueTask WriteTagAsync(TiffTag tag, TiffValueCollection<double> values, CancellationToken cancellationToken = default)
        {
            EnsureNotDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            Debug.Assert(_writer != null);

            const int ElementSize = sizeof(double);
            int byteCount = ElementSize * values.Count;

            if (byteCount <= _writer!.OperationContext.ByteCountOfValueOffsetField)
            {
                Span<byte> stackBuffer = stackalloc byte[8];
                int i;
                for (i = 0; i < values.Count; i++)
                {
                    double item = values[i];
                    MemoryMarshal.Write(stackBuffer.Slice(ElementSize * i), ref item);
                }

                AddOrUpdateEntry(tag, TiffFieldType.Double, values.Count, stackBuffer);

                return default;
            }
            else
            {
                return new ValueTask(WriteTagSlowAsync(tag, values, cancellationToken));
            }
        }

        internal async Task WriteTagSlowAsync(TiffTag tag, TiffValueCollection<double> values, CancellationToken cancellationToken)
        {
            Debug.Assert(_writer != null);
            TiffStreamRegion region = await _writer!.WriteAlignedValues(values, cancellationToken).ConfigureAwait(false);
            AddOrUpdateEntry(tag, TiffFieldType.Double, values.Count, region.Offset);
        }

        #endregion

        #region Rational

        /// <summary>
        /// Write values of <see cref="TiffFieldType.Rational"/> to the specified tag in this IFD.
        /// </summary>
        /// <param name="tag">The specified tag.</param>
        /// <param name="values">The values to write.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort this operation.</param>
        /// <returns>A <see cref="ValueTask"/> that completes when the values have been written.</returns>
        [CLSCompliant(false)]
        public ValueTask WriteTagAsync(TiffTag tag, TiffValueCollection<TiffRational> values, CancellationToken cancellationToken = default)
        {
            EnsureNotDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            Debug.Assert(_writer != null);

            const int ElementSize = 2 * sizeof(uint);
            int byteCount = ElementSize * values.Count;

            if (byteCount <= _writer!.OperationContext.ByteCountOfValueOffsetField)
            {
                Span<byte> stackBuffer = stackalloc byte[8];
                int i;
                for (i = 0; i < values.Count; i++)
                {
                    TiffRational item = values[i];
                    MemoryMarshal.Write(stackBuffer.Slice(ElementSize * i), ref item);
                }

                AddOrUpdateEntry(tag, TiffFieldType.Rational, values.Count, stackBuffer);

                return default;
            }
            else
            {
                return new ValueTask(WriteTagSlowAsync(tag, values, cancellationToken));
            }
        }

        internal async Task WriteTagSlowAsync(TiffTag tag, TiffValueCollection<TiffRational> values, CancellationToken cancellationToken)
        {
            Debug.Assert(_writer != null);
            TiffStreamRegion region = await _writer!.WriteAlignedValues(values, cancellationToken).ConfigureAwait(false);
            AddOrUpdateEntry(tag, TiffFieldType.Rational, values.Count, region.Offset);
        }

        #endregion

        #region SRational

        /// <summary>
        /// Write values of <see cref="TiffFieldType.SRational"/> to the specified tag in this IFD.
        /// </summary>
        /// <param name="tag">The specified tag.</param>
        /// <param name="values">The values to write.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort this operation.</param>
        /// <returns>A <see cref="ValueTask"/> that completes when the values have been written.</returns>
        [CLSCompliant(false)]
        public ValueTask WriteTagAsync(TiffTag tag, TiffValueCollection<TiffSRational> values, CancellationToken cancellationToken = default)
        {
            EnsureNotDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            Debug.Assert(_writer != null);

            const int ElementSize = 2 * sizeof(int);
            int byteCount = ElementSize * values.Count;

            if (byteCount <= _writer!.OperationContext.ByteCountOfValueOffsetField)
            {
                Span<byte> stackBuffer = stackalloc byte[8];
                int i;
                for (i = 0; i < values.Count; i++)
                {
                    TiffSRational item = values[i];
                    MemoryMarshal.Write(stackBuffer.Slice(ElementSize * i), ref item);
                }

                AddOrUpdateEntry(tag, TiffFieldType.SRational, values.Count, stackBuffer);

                return default;
            }
            else
            {
                return new ValueTask(WriteTagSlowAsync(tag, values, cancellationToken));
            }
        }

        internal async Task WriteTagSlowAsync(TiffTag tag, TiffValueCollection<TiffSRational> values, CancellationToken cancellationToken)
        {
            Debug.Assert(_writer != null);
            TiffStreamRegion region = await _writer!.WriteAlignedValues(values, cancellationToken).ConfigureAwait(false);
            AddOrUpdateEntry(tag, TiffFieldType.SRational, values.Count, region.Offset);
        }

        #endregion
    }
}
