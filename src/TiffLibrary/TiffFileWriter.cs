using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TiffLibrary
{
    /// <summary>
    /// A writer class that write content into the TIFF steam.
    /// </summary>
    public sealed class TiffFileWriter : IDisposable, IAsyncDisposable
    {
        private TiffFileContentReaderWriter? _writer;
        private bool _leaveOpen;
        private long _position;
        private readonly bool _useBigTiff;
        private bool _requireBigTiff;
        private TiffOperationContext? _operationContext;
        private long _imageFileDirectoryOffset;

        private const int SmallBufferSize = 32;

        internal TiffFileWriter(TiffFileContentReaderWriter writer, bool leaveOpen, bool useBigTiff)
        {
            ThrowHelper.ThrowIfNull(writer);
            _writer = writer;
            _leaveOpen = leaveOpen;

            _position = useBigTiff ? 16 : 8;
            _useBigTiff = useBigTiff;
            _requireBigTiff = false;
            _operationContext = useBigTiff ? TiffOperationContext.BigTIFF : TiffOperationContext.StandardTIFF;
        }

        internal TiffOperationContext OperationContext => _operationContext ?? ThrowHelper.ThrowObjectDisposedException<TiffOperationContext>(GetType().FullName);
        internal TiffFileContentReaderWriter InnerWriter => _writer ?? ThrowHelper.ThrowObjectDisposedException<TiffFileContentReaderWriter>(GetType().FullName);

        /// <summary>
        /// Gets whether to use BigTIFF format.
        /// </summary>
        public bool UseBigTiff => _useBigTiff;

        /// <summary>
        /// The current position of the stream.
        /// </summary>
        public TiffStreamOffset Position => new TiffStreamOffset(_position);

        /// <summary>
        /// Uses the specified stream to create <see cref="TiffFileWriter"/>.
        /// </summary>
        /// <param name="stream">A seekable and writable stream to use.</param>
        /// <param name="leaveOpen">Whether to leave the stream open when <see cref="TiffFileWriter"/> is dispsoed.</param>
        /// <param name="useBigTiff">Whether to use BigTIFF format.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort this operation.</param>
        /// <returns>The create <see cref="TiffFileWriter"/>.</returns>
        public static Task<TiffFileWriter> OpenAsync(Stream stream, bool leaveOpen, bool useBigTiff = false, CancellationToken cancellationToken = default)
        {
            ThrowHelper.ThrowIfNull(stream);

            if (!stream.CanSeek)
            {
                ThrowHelper.ThrowArgumentException("Stream must be seekable.", nameof(stream));
            }
            if (!stream.CanWrite)
            {
                ThrowHelper.ThrowArgumentException("Stream must be writable.", nameof(stream));
            }

            return OpenAsync(new TiffStreamContentReaderWriter(stream, leaveOpen), false, useBigTiff, cancellationToken);
        }

        /// <summary>
        /// Opens the specified file for writing and creates <see cref="TiffFileWriter"/>.
        /// </summary>
        /// <param name="fileName">The file to write to.</param>
        /// <param name="useBigTiff">Whether to use BigTIFF format.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort this operation.</param>
        /// <returns>The create <see cref="TiffFileWriter"/>.</returns>
        public static Task<TiffFileWriter> OpenAsync(string fileName, bool useBigTiff = false, CancellationToken cancellationToken = default)
        {
            ThrowHelper.ThrowIfNull(fileName);

            var fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite);
            return OpenAsync(new TiffStreamContentReaderWriter(fs, false), false, useBigTiff, cancellationToken);
        }

        /// <summary>
        /// Uses the specified content writer to create <see cref="TiffFileWriter"/>.
        /// </summary>
        /// <param name="writer">The content writer to use.</param>
        /// <param name="leaveOpen">Whether to leave the content writer open when <see cref="TiffFileWriter"/> is dispsoed.</param>
        /// <param name="useBigTiff">Whether to use BigTIFF format.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort this operation.</param>
        /// <returns>The create <see cref="TiffFileWriter"/>.</returns>
        public static async Task<TiffFileWriter> OpenAsync(TiffFileContentReaderWriter writer, bool leaveOpen, bool useBigTiff = false, CancellationToken cancellationToken = default)
        {
            ThrowHelper.ThrowIfNull(writer);

            TiffFileContentReaderWriter? disposeInstance = writer;
            byte[] smallBuffer = ArrayPool<byte>.Shared.Rent(SmallBufferSize);
            try
            {
                smallBuffer.AsSpan().Clear();
                await writer.WriteAsync(0, new ArraySegment<byte>(smallBuffer, 0, useBigTiff ? 16 : 8), cancellationToken).ConfigureAwait(false);
                disposeInstance = null;
                return new TiffFileWriter(writer, leaveOpen, useBigTiff);
            }
            finally
            {
#pragma warning disable CA1508 // Avoid dead conditional code
                if (!leaveOpen && disposeInstance is not null)
#pragma warning restore CA1508
                {
                    await disposeInstance.DisposeAsync().ConfigureAwait(false);
                }
            }

        }

        #region Allignment

        /// <summary>
        /// Align the current position to word boundary.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort this operation.</param>
        /// <returns>A <see cref="ValueTask{TiffStreamOffset}"/> that completes when the align operation is completed. Returns the current position.</returns>
        public ValueTask<TiffStreamOffset> AlignToWordBoundaryAsync(CancellationToken cancellationToken = default)
        {
            EnsureNotDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            long position = _position;
            if ((position & 0b1) != 0)
            {
                return new ValueTask<TiffStreamOffset>(InternalAlignToWordBoundaryAsync(cancellationToken));
            }
            return new ValueTask<TiffStreamOffset>(new TiffStreamOffset(position));
        }

        private async Task<TiffStreamOffset> InternalAlignToWordBoundaryAsync(CancellationToken cancellationToken)
        {
            Debug.Assert(_writer != null);

            int length = (int)_position & 0b1;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(SmallBufferSize);
            try
            {
                buffer[0] = 0;
                await _writer!.WriteAsync(_position, new ArraySegment<byte>(buffer, 0, length), cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
            return new TiffStreamOffset(AdvancePosition(length));
        }

        #endregion

        internal long AdvancePosition(int length)
        {
            return AdvancePosition(checked((uint)length));
        }

        internal long AdvancePosition(uint length)
        {
            long position = _position;
            position += length;
            if (position > uint.MaxValue)
            {
                _requireBigTiff = true;
            }
            return _position = position;
        }

        internal long AdvancePosition(long length)
        {
            long position = _position;
            position += length;
            if (position > uint.MaxValue)
            {
                _requireBigTiff = true;
            }
            return _position = position;
        }

        /// <summary>
        /// Seek to the specified position.
        /// </summary>
        /// <param name="position">The specified position in the stream.</param>
        public void Seek(TiffStreamOffset position)
        {
            EnsureNotDisposed();

            long pos = position.ToInt64();
            _position = pos;
        }

        #region IFDs

        /// <summary>
        /// Sets the first IFD offset to the specified offset.
        /// </summary>
        /// <param name="ifdOffset">The offset of the first IFD.</param>
        public void SetFirstImageFileDirectoryOffset(TiffStreamOffset ifdOffset)
        {
            _imageFileDirectoryOffset = ifdOffset;
        }

        /// <summary>
        /// Creates a <see cref="TiffImageFileDirectoryEntry"/> for writing a new IFD.
        /// </summary>
        /// <returns></returns>
        public TiffImageFileDirectoryWriter CreateImageFileDirectory()
        {
            return new TiffImageFileDirectoryWriter(this);
        }

        internal async Task UpdateImageFileDirectoryNextOffsetFieldAsync(TiffStreamOffset target, TiffStreamOffset ifdOffset, CancellationToken cancellationToken)
        {
            EnsureNotDisposed();

            Debug.Assert(_writer != null);
            Debug.Assert(_operationContext != null);

            // Attemps to read 8 bytes even though the size of IFD may be less then 8 bytes.
            byte[] buffer = ArrayPool<byte>.Shared.Rent(SmallBufferSize);
            try
            {
                int rwCount = await _writer!.ReadAsync(target, new ArraySegment<byte>(buffer, 0, 8), cancellationToken).ConfigureAwait(false);
                if (!(_useBigTiff && rwCount == 8) && !(!_useBigTiff && rwCount >= 4))
                {
                    ThrowHelper.ThrowInvalidDataException();
                }
                int count = ParseImageFileDirectoryCount(buffer.AsSpan(0, 8));

                // Prepare next ifd.
                if (_useBigTiff)
                {
                    rwCount = 8;
                    long offset = ifdOffset;
                    MemoryMarshal.Write(buffer, ref offset);
                }
                else
                {
                    rwCount = 4;
                    int offset32 = (int)ifdOffset;
                    MemoryMarshal.Write(buffer, ref offset32);
                }

                // Skip over IFD entries.
                int entryFieldLength = _useBigTiff ? 20 : 12;
                await _writer.WriteAsync(target + _operationContext!.ByteCountOfImageFileDirectoryCountField + count * entryFieldLength, new ArraySegment<byte>(buffer, 0, rwCount), cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        internal int ParseImageFileDirectoryCount(ReadOnlySpan<byte> buffer)
        {
            if (_useBigTiff)
            {
                return checked((int)MemoryMarshal.Read<ulong>(buffer));
            }
            return MemoryMarshal.Read<ushort>(buffer);
        }

        #endregion

        #region Primitives


        /// <summary>
        /// Writes a series of bytes into the TIFF stream.
        /// </summary>
        /// <param name="buffer">The bytes buffer.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort this operation.</param>
        /// <returns>A <see cref="Task"/> that completes when the bytes have been written.</returns>
        public async Task<TiffStreamOffset> WriteBytesAsync(byte[] buffer, CancellationToken cancellationToken = default)
        {
            ThrowHelper.ThrowIfNull(buffer);

            EnsureNotDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            Debug.Assert(_writer != null);
            long position = _position;
            await _writer!.WriteAsync(position, buffer, cancellationToken).ConfigureAwait(false);
            AdvancePosition(buffer.Length);

            return new TiffStreamOffset(position);
        }


        /// <summary>
        /// Writes a series of bytes into the TIFF stream.
        /// </summary>
        /// <param name="buffer">The bytes buffer.</param>
        /// <param name="index">The number of bytes to skip in the buffer.</param>
        /// <param name="count">The number of bytes to write.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort this operation.</param>
        /// <returns>A <see cref="Task"/> that completes when the bytes have been written.</returns>
        public async Task<TiffStreamOffset> WriteBytesAsync(byte[] buffer, int index, int count, CancellationToken cancellationToken = default)
        {
            ThrowHelper.ThrowIfNull(buffer);
            if ((uint)index >= (uint)buffer.Length)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(index));
            }
            if ((uint)(index + count) > (uint)buffer.Length)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(count));
            }

            EnsureNotDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            Debug.Assert(_writer != null);
            long position = _position;
            await _writer!.WriteAsync(position, new ArraySegment<byte>(buffer, index, count), cancellationToken).ConfigureAwait(false);
            AdvancePosition(count);

            return new TiffStreamOffset(position);
        }

        /// <summary>
        /// Writes a series of bytes into the TIFF stream.
        /// </summary>
        /// <param name="buffer">The bytes buffer.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort this operation.</param>
        /// <returns>A <see cref="Task"/> that completes when the bytes have been written.</returns>
        public async Task<TiffStreamOffset> WriteBytesAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureNotDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            Debug.Assert(_writer != null);
            long position = _position;
            await _writer!.WriteAsync(position, buffer, cancellationToken).ConfigureAwait(false);
            AdvancePosition(buffer.Length);

            return new TiffStreamOffset(position);
        }

        /// <summary>
        /// Writes a series of bytes into the TIFF stream.
        /// </summary>
        /// <param name="buffer">The bytes buffer.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort this operation.</param>
        /// <returns>A <see cref="Task"/> that completes when the bytes have been written.</returns>
        public async Task<TiffStreamOffset> WriteBytesAsync(ReadOnlySequence<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureNotDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            Debug.Assert(_writer == null);
            long offset = _position;
            long position = offset;
            foreach (ReadOnlyMemory<byte> segment in buffer)
            {
                await _writer!.WriteAsync(offset, segment, cancellationToken).ConfigureAwait(false);
                offset = AdvancePosition(segment.Length);
            }
            return new TiffStreamOffset(position);
        }

        /// <summary>
        /// Align to word boundary and writes a series of bytes into the TIFF stream.
        /// </summary>
        /// <param name="buffer">The bytes buffer.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort this operation.</param>
        /// <returns>A <see cref="Task"/> that completes when the bytes have been written.</returns>
        public Task<TiffStreamOffset> WriteAlignedBytesAsync(byte[] buffer, CancellationToken cancellationToken = default)
        {
            ThrowHelper.ThrowIfNull(buffer);

            return WriteAlignedBytesAsync(buffer, 0, buffer.Length, cancellationToken);
        }

        /// <summary>
        /// Align to word boundary and writes a series of bytes into the TIFF stream.
        /// </summary>
        /// <param name="buffer">The bytes buffer.</param>
        /// <param name="index">The number of bytes to skip in the buffer.</param>
        /// <param name="count">The number of bytes to write.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort this operation.</param>
        /// <returns>A <see cref="Task"/> that completes when the bytes have been written.</returns>
        public async Task<TiffStreamOffset> WriteAlignedBytesAsync(byte[] buffer, int index, int count, CancellationToken cancellationToken = default)
        {
            ThrowHelper.ThrowIfNull(buffer);
            if ((uint)index >= (uint)buffer.Length)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(index));
            }
            if ((uint)(index + count) > (uint)buffer.Length)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(count));
            }

            EnsureNotDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            Debug.Assert(_writer != null);
            long position = await AlignToWordBoundaryAsync(cancellationToken).ConfigureAwait(false);
            await _writer!.WriteAsync(position, new ArraySegment<byte>(buffer, index, count), cancellationToken).ConfigureAwait(false);
            AdvancePosition(count);

            return new TiffStreamOffset(position);
        }

        /// <summary>
        /// Align to word boundary and writes a series of bytes into the TIFF stream.
        /// </summary>
        /// <param name="buffer">The bytes buffer.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort this operation.</param>
        /// <returns>A <see cref="Task"/> that completes when the bytes have been written.</returns>
        public async Task<TiffStreamOffset> WriteAlignedBytesAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureNotDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            Debug.Assert(_writer != null);
            long position = await AlignToWordBoundaryAsync(cancellationToken).ConfigureAwait(false);
            long length = buffer.Length;
            await _writer!.WriteAsync(position, buffer, cancellationToken).ConfigureAwait(false);
            AdvancePosition(length);

            return new TiffStreamOffset(position);
        }

        /// <summary>
        /// Align to word boundary and writes a series of bytes into the TIFF stream.
        /// </summary>
        /// <param name="buffer">The bytes buffer.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort this operation.</param>
        /// <returns>A <see cref="Task"/> that completes when the bytes have been written.</returns>
        public async Task<TiffStreamOffset> WriteAlignedBytesAsync(ReadOnlySequence<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureNotDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            Debug.Assert(_writer != null);
            long position = await AlignToWordBoundaryAsync(cancellationToken).ConfigureAwait(false);
            long offset = position;
            foreach (ReadOnlyMemory<byte> segment in buffer)
            {
                await _writer!.WriteAsync(offset, segment, cancellationToken).ConfigureAwait(false);
                offset = AdvancePosition(segment.Length);
            }

            return new TiffStreamOffset(position);
        }


        internal async Task<TiffStreamRegion> WriteAlignedValues(TiffValueCollection<string> values, CancellationToken cancellationToken)
        {
            EnsureNotDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            Debug.Assert(_writer != null);
            long position = await AlignToWordBoundaryAsync(cancellationToken).ConfigureAwait(false);

            int maxByteCount = 0;
            foreach (string item in values)
            {
                maxByteCount = Math.Max(maxByteCount, Encoding.ASCII.GetMaxByteCount(item.Length));
            }

            long offset = position;
            int bytesWritten = 0;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(maxByteCount + 1);
            try
            {
                foreach (string item in values)
                {
                    int length = Encoding.ASCII.GetBytes(item, 0, item.Length, buffer, 0);
                    buffer[length] = 0;
                    await _writer!.WriteAsync(offset, new ArraySegment<byte>(buffer, 0, length + 1), cancellationToken).ConfigureAwait(false);
                    offset += length + 1;
                    AdvancePosition(length + 1);
                    bytesWritten += length + 1;
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            return new TiffStreamRegion(position, bytesWritten);
        }

        internal async Task<TiffStreamRegion> WriteAlignedValues(TiffValueCollection<ushort> values, CancellationToken cancellationToken)
        {
            EnsureNotDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            Debug.Assert(_writer != null);
            long position = await AlignToWordBoundaryAsync(cancellationToken).ConfigureAwait(false);

            const int ElementSize = sizeof(ushort);
            int byteCount = ElementSize * values.Count;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(byteCount);
            try
            {
                MemoryMarshal.AsBytes(values.GetOrCreateArray().AsSpan()).CopyTo(buffer);
                await _writer!.WriteAsync(position, new ArraySegment<byte>(buffer, 0, byteCount), cancellationToken).ConfigureAwait(false);
                AdvancePosition(byteCount);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            return new TiffStreamRegion(position, byteCount);
        }

        internal async Task<TiffStreamRegion> WriteAlignedValues(TiffValueCollection<short> values, CancellationToken cancellationToken)
        {
            EnsureNotDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            Debug.Assert(_writer != null);
            long position = await AlignToWordBoundaryAsync(cancellationToken).ConfigureAwait(false);

            const int ElementSize = sizeof(short);
            int byteCount = ElementSize * values.Count;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(byteCount);
            try
            {
                MemoryMarshal.AsBytes(values.GetOrCreateArray().AsSpan()).CopyTo(buffer);
                await _writer!.WriteAsync(position, new ArraySegment<byte>(buffer, 0, byteCount), cancellationToken).ConfigureAwait(false);
                AdvancePosition(byteCount);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            return new TiffStreamRegion(position, byteCount);
        }

        internal async Task<TiffStreamRegion> WriteAlignedValues(TiffValueCollection<uint> values, CancellationToken cancellationToken)
        {
            EnsureNotDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            Debug.Assert(_writer != null);
            long position = await AlignToWordBoundaryAsync(cancellationToken).ConfigureAwait(false);

            const int ElementSize = sizeof(uint);
            int byteCount = ElementSize * values.Count;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(byteCount);
            try
            {
                MemoryMarshal.AsBytes(values.GetOrCreateArray().AsSpan()).CopyTo(buffer);
                await _writer!.WriteAsync(position, new ArraySegment<byte>(buffer, 0, byteCount), cancellationToken).ConfigureAwait(false);
                AdvancePosition(byteCount);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            return new TiffStreamRegion(position, byteCount);
        }

        internal async Task<TiffStreamRegion> WriteAlignedValues(TiffValueCollection<int> values, CancellationToken cancellationToken)
        {
            EnsureNotDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            Debug.Assert(_writer != null);
            long position = await AlignToWordBoundaryAsync(cancellationToken).ConfigureAwait(false);

            const int ElementSize = sizeof(int);
            int byteCount = ElementSize * values.Count;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(byteCount);
            try
            {
                MemoryMarshal.AsBytes(values.GetOrCreateArray().AsSpan()).CopyTo(buffer);
                await _writer!.WriteAsync(position, new ArraySegment<byte>(buffer, 0, byteCount), cancellationToken).ConfigureAwait(false);
                AdvancePosition(byteCount);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            return new TiffStreamRegion(position, byteCount);
        }

        internal async Task<TiffStreamRegion> WriteAlignedValues(TiffValueCollection<ulong> values, CancellationToken cancellationToken)
        {
            EnsureNotDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            Debug.Assert(_writer != null);
            long position = await AlignToWordBoundaryAsync(cancellationToken).ConfigureAwait(false);

            const int ElementSize = sizeof(ulong);
            int byteCount = ElementSize * values.Count;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(byteCount);
            try
            {
                MemoryMarshal.AsBytes(values.GetOrCreateArray().AsSpan()).CopyTo(buffer);
                await _writer!.WriteAsync(position, new ArraySegment<byte>(buffer, 0, byteCount), cancellationToken).ConfigureAwait(false);
                AdvancePosition(byteCount);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            return new TiffStreamRegion(position, byteCount);
        }

        internal async Task<TiffStreamRegion> WriteAlignedValues(TiffValueCollection<long> values, CancellationToken cancellationToken)
        {
            EnsureNotDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            Debug.Assert(_writer != null);
            long position = await AlignToWordBoundaryAsync(cancellationToken).ConfigureAwait(false);

            const int ElementSize = sizeof(long);
            int byteCount = ElementSize * values.Count;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(byteCount);
            try
            {
                MemoryMarshal.AsBytes(values.GetOrCreateArray().AsSpan()).CopyTo(buffer);
                await _writer!.WriteAsync(position, new ArraySegment<byte>(buffer, 0, byteCount), cancellationToken).ConfigureAwait(false);
                AdvancePosition(byteCount);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            return new TiffStreamRegion(position, byteCount);
        }

        internal async Task<TiffStreamRegion> WriteAlignedValues(TiffValueCollection<float> values, CancellationToken cancellationToken)
        {
            EnsureNotDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            Debug.Assert(_writer != null);
            long position = await AlignToWordBoundaryAsync(cancellationToken).ConfigureAwait(false);

            const int ElementSize = sizeof(float);
            int byteCount = ElementSize * values.Count;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(byteCount);
            try
            {
                MemoryMarshal.AsBytes(values.GetOrCreateArray().AsSpan()).CopyTo(buffer);
                await _writer!.WriteAsync(position, new ArraySegment<byte>(buffer, 0, byteCount), cancellationToken).ConfigureAwait(false);
                AdvancePosition(byteCount);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            return new TiffStreamRegion(position, byteCount);
        }

        internal async Task<TiffStreamRegion> WriteAlignedValues(TiffValueCollection<double> values, CancellationToken cancellationToken)
        {
            EnsureNotDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            Debug.Assert(_writer != null);
            long position = await AlignToWordBoundaryAsync(cancellationToken).ConfigureAwait(false);

            const int ElementSize = sizeof(double);
            int byteCount = ElementSize * values.Count;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(byteCount);
            try
            {
                MemoryMarshal.AsBytes(values.GetOrCreateArray().AsSpan()).CopyTo(buffer);
                await _writer!.WriteAsync(position, new ArraySegment<byte>(buffer, 0, byteCount), cancellationToken).ConfigureAwait(false);
                AdvancePosition(byteCount);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            return new TiffStreamRegion(position, byteCount);
        }

        internal async Task<TiffStreamRegion> WriteAlignedValues(TiffValueCollection<TiffRational> values, CancellationToken cancellationToken)
        {
            EnsureNotDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            Debug.Assert(_writer != null);
            long position = await AlignToWordBoundaryAsync(cancellationToken).ConfigureAwait(false);

            const int ElementSize = 2 * sizeof(uint);
            int byteCount = ElementSize * values.Count;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(byteCount);
            try
            {
                MemoryMarshal.AsBytes(values.GetOrCreateArray().AsSpan()).CopyTo(buffer);
                await _writer!.WriteAsync(position, new ArraySegment<byte>(buffer, 0, byteCount), cancellationToken).ConfigureAwait(false);
                AdvancePosition(byteCount);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            return new TiffStreamRegion(position, byteCount);
        }

        internal async Task<TiffStreamRegion> WriteAlignedValues(TiffValueCollection<TiffSRational> values, CancellationToken cancellationToken)
        {
            EnsureNotDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            Debug.Assert(_writer != null);
            long position = await AlignToWordBoundaryAsync(cancellationToken).ConfigureAwait(false);

            const int ElementSize = 2 * sizeof(int);
            int byteCount = ElementSize * values.Count;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(byteCount);
            try
            {
                MemoryMarshal.AsBytes(values.GetOrCreateArray().AsSpan()).CopyTo(buffer);
                await _writer!.WriteAsync(position, new ArraySegment<byte>(buffer, 0, byteCount), cancellationToken).ConfigureAwait(false);
                AdvancePosition(byteCount);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            return new TiffStreamRegion(position, byteCount);
        }

        #endregion

        #region Tiff file header

        /// <summary>
        /// Flush the TIFF file header into the stream.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort this operation.</param>
        /// <returns></returns>
        public async Task FlushAsync(CancellationToken cancellationToken = default)
        {
            EnsureNotDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            Debug.Assert(_writer != null);
            if (_requireBigTiff && !_useBigTiff)
            {
                ThrowHelper.ThrowInvalidOperationException("Must use BigTIFF format. But it is disabled.");
            }

            byte[] buffer = ArrayPool<byte>.Shared.Rent(SmallBufferSize);
            try
            {
                Array.Clear(buffer, 0, 16);
                TiffFileHeader.Write(buffer, _imageFileDirectoryOffset, BitConverter.IsLittleEndian, _useBigTiff);
                await _writer!.WriteAsync(0, new ArraySegment<byte>(buffer, 0, _useBigTiff ? 16 : 8), cancellationToken).ConfigureAwait(false);
                await _writer.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

        }

        #endregion

        #region Dispose support

        private void EnsureNotDisposed()
        {
            if (_writer is null)
            {
                ThrowHelper.ThrowObjectDisposedException(GetType().FullName);
            }
        }


        /// <inheritdoc />
        public void Dispose()
        {
            if (!_leaveOpen)
            {
                _writer?.Dispose();
            }
            _writer = null;
            _operationContext = null;
            _leaveOpen = true;
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (!_leaveOpen && _writer is not null)
            {
                await _writer.DisposeAsync().ConfigureAwait(false);
            }
            _writer = null;
            _operationContext = null;
            _leaveOpen = true;
        }

        #endregion
    }
}
