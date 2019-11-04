using System;
using System.Buffers;
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
        private Stream _stream;
        private bool _leaveOpen;
        private long _position;
        private readonly bool _useBigTiff;
        private bool _requireBigTiff;
        private bool _completed;
        private byte[] _smallBuffer;
        private TiffOperationContext _operationContext;
        private long _imageFileDirectoryOffset;

        private const int SmallBufferSize = 32;

        internal TiffFileWriter(Stream stream, bool leaveOpen, bool useBigTiff, byte[] smallBuffer)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            _leaveOpen = leaveOpen;
            _position = useBigTiff ? 16 : 8;
            _useBigTiff = useBigTiff;
            _requireBigTiff = false;
            _completed = false;
            _smallBuffer = smallBuffer;
            _operationContext = useBigTiff ? TiffOperationContext.BigTIFF : TiffOperationContext.StandardTIFF;
        }

        internal TiffOperationContext OperationContext => _operationContext;
        internal byte[] InternalBuffer => _smallBuffer;
        internal Stream InnerStream => _stream;

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
        /// <returns>The create <see cref="TiffFileWriter"/>.</returns>
        public static async Task<TiffFileWriter> OpenAsync(Stream stream, bool leaveOpen, bool useBigTiff = false)
        {
            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanSeek)
            {
                throw new ArgumentException("Stream must be seekable.", nameof(stream));
            }

            stream.Seek(0, SeekOrigin.Begin);
            byte[] smallBuffer = new byte[SmallBufferSize];
            await stream.WriteAsync(smallBuffer, 0, useBigTiff ? 16 : 8).ConfigureAwait(false);
            return new TiffFileWriter(stream, leaveOpen, useBigTiff, smallBuffer);
        }

        /// <summary>
        /// Opens the specified file for writing and creates <see cref="TiffFileWriter"/>.
        /// </summary>
        /// <param name="fileName">The file to write to.</param>
        /// <param name="useBigTiff">Whether to use BigTIFF format.</param>
        /// <returns>The create <see cref="TiffFileWriter"/>.</returns>
        public static async Task<TiffFileWriter> OpenAsync(string fileName, bool useBigTiff = false)
        {
            var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            try
            {
                byte[] smallBuffer = new byte[SmallBufferSize];
                await fs.WriteAsync(smallBuffer, 0, useBigTiff ? 16 : 8).ConfigureAwait(false);
                return new TiffFileWriter(Interlocked.Exchange(ref fs, null), false, useBigTiff, smallBuffer);
            }
            finally
            {
                fs?.Dispose();
            }

        }

        #region Allignment

        /// <summary>
        /// Align the current position to word boundary.
        /// </summary>
        /// <returns>A <see cref="ValueTask{TiffStreamOffset}"/> that completes when the align operation is completed. Returns the current position.</returns>
        public ValueTask<TiffStreamOffset> AlignToWordBoundaryAsync()
        {
            EnsureNotDisposed();

            long position = _position;
            if ((position & 0b1) != 0)
            {
                return new ValueTask<TiffStreamOffset>(InternalAlignToWordBoundaryAsync());
            }
            _stream.Seek(position, SeekOrigin.Begin);
            return new ValueTask<TiffStreamOffset>(new TiffStreamOffset(position));
        }

        private async Task<TiffStreamOffset> InternalAlignToWordBoundaryAsync()
        {
            int length = (int)_position & 0b1;
            _stream.Seek(_position, SeekOrigin.Begin);
            await _stream.WriteAsync(_smallBuffer, 0, length).ConfigureAwait(false);
            return new TiffStreamOffset(AdvancePosition(length));
        }

        #endregion

        internal void SeekToPosition()
        {
            _stream.Seek(_position, SeekOrigin.Begin);
        }

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
            long pos = position.ToInt64();
            _position = pos;
            _stream.Seek(pos, SeekOrigin.Begin);
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

        internal async Task UpdateImageFileDirectoryNextOffsetFieldAsync(TiffStreamOffset target, TiffStreamOffset ifdOffset)
        {
            _stream.Seek(target, SeekOrigin.Begin);

            // Attemps to read 8 bytes even though the size of IFD may be less then 8 bytes.
            int rwCount = await _stream.ReadAsync(_smallBuffer, 0, 8).ConfigureAwait(false);
            if (!(_useBigTiff && rwCount == 8) && !(!_useBigTiff && rwCount >= 4))
            {
                throw new InvalidDataException();
            }
            int count = ParseImageFileDirectoryCount(_smallBuffer.AsSpan(0, 8));

            // Skip over IFD entries.
            int entryFieldLength = _useBigTiff ? 20 : 12;
            _stream.Seek(target + _operationContext.ByteCountOfImageFileDirectoryCountField + count * entryFieldLength, SeekOrigin.Begin);

            // Update next ifd.
            if (_useBigTiff)
            {
                rwCount = 8;
                long offset = ifdOffset;
                MemoryMarshal.Write(_smallBuffer, ref offset);
            }
            else
            {
                rwCount = 4;
                int offset32 = (int)ifdOffset;
                MemoryMarshal.Write(_smallBuffer, ref offset32);
            }
            await _stream.WriteAsync(_smallBuffer, 0, rwCount).ConfigureAwait(false);
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
        /// <param name="index">The number of bytes to skip in the buffer.</param>
        /// <param name="count">The number of bytes to write.</param>
        /// <returns>A <see cref="Task"/> that completes when the bytes have been written.</returns>
        public async Task<TiffStreamOffset> WriteBytesAsync(byte[] buffer, int index, int count)
        {
            EnsureNotDisposed();
            EnsureNotCompleted();

            long position = _position;
            _stream.Seek(position, SeekOrigin.Begin);
            await _stream.WriteAsync(buffer, index, count).ConfigureAwait(false);
            AdvancePosition(count);

            return new TiffStreamOffset(position);
        }

        /// <summary>
        /// Writes a series of bytes into the TIFF stream.
        /// </summary>
        /// <param name="buffer">The bytes buffer.</param>
        /// <returns>A <see cref="Task"/> that completes when the bytes have been written.</returns>
        public async Task<TiffStreamOffset> WriteBytesAsync(ReadOnlyMemory<byte> buffer)
        {
            EnsureNotDisposed();
            EnsureNotCompleted();

            long position = _position;
            _stream.Seek(position, SeekOrigin.Begin);
            await _stream.WriteAsync(buffer).ConfigureAwait(false);
            AdvancePosition(buffer.Length);

            return new TiffStreamOffset(position);
        }

        /// <summary>
        /// Align to word boundary and writes a series of bytes into the TIFF stream.
        /// </summary>
        /// <param name="buffer">The bytes buffer.</param>
        /// <returns>A <see cref="Task"/> that completes when the bytes have been written.</returns>
        public Task<TiffStreamOffset> WriteAlignedBytesAsync(byte[] buffer)
        {
            if (buffer is null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            return WriteAlignedBytesAsync(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Align to word boundary and writes a series of bytes into the TIFF stream.
        /// </summary>
        /// <param name="buffer">The bytes buffer.</param>
        /// <param name="index">The number of bytes to skip in the buffer.</param>
        /// <param name="count">The number of bytes to write.</param>
        /// <returns>A <see cref="Task"/> that completes when the bytes have been written.</returns>
        public async Task<TiffStreamOffset> WriteAlignedBytesAsync(byte[] buffer, int index, int count)
        {
            if (buffer is null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            EnsureNotDisposed();
            EnsureNotCompleted();

            long position = await AlignToWordBoundaryAsync().ConfigureAwait(false);
            await _stream.WriteAsync(buffer, index, count).ConfigureAwait(false);
            AdvancePosition(count);

            return new TiffStreamOffset(position);
        }

        /// <summary>
        /// Align to word boundary and writes a series of bytes into the TIFF stream.
        /// </summary>
        /// <param name="buffer">The bytes buffer.</param>
        /// <returns>A <see cref="Task"/> that completes when the bytes have been written.</returns>
        public async Task<TiffStreamOffset> WriteAlignedBytesAsync(ReadOnlyMemory<byte> buffer)
        {
            EnsureNotDisposed();
            EnsureNotCompleted();

            long position = await AlignToWordBoundaryAsync().ConfigureAwait(false);
            long length = buffer.Length;
            await _stream.WriteAsync(buffer).ConfigureAwait(false);
            AdvancePosition(length);

            return new TiffStreamOffset(position);
        }

        internal async Task<TiffStreamOffset> WriteAlignedBytesAsync(ReadOnlySequence<byte> buffer)
        {
            EnsureNotDisposed();
            EnsureNotCompleted();

            long position = await AlignToWordBoundaryAsync().ConfigureAwait(false);
            long length = buffer.Length;
            foreach (ReadOnlyMemory<byte> segment in buffer)
            {
                await _stream.WriteAsync(segment).ConfigureAwait(false);
            }
            AdvancePosition(length);

            return new TiffStreamOffset(position);
        }


        internal async Task<TiffStreamRegion> WriteAlignedValues(TiffValueCollection<string> values)
        {
            EnsureNotDisposed();
            EnsureNotCompleted();

            long position = await AlignToWordBoundaryAsync().ConfigureAwait(false);

            int maxByteCount = 0;
            foreach (string item in values)
            {
                maxByteCount = Math.Max(maxByteCount, Encoding.ASCII.GetMaxByteCount(item.Length));
            }

            int bytesWritten = 0;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(maxByteCount + 1);
            try
            {
                foreach (string item in values)
                {
                    int length = Encoding.ASCII.GetBytes(item, 0, item.Length, buffer, 0);
                    buffer[length] = 0;
                    await _stream.WriteAsync(buffer, 0, length + 1).ConfigureAwait(false);
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

        internal async Task<TiffStreamRegion> WriteAlignedValues(TiffValueCollection<ushort> values)
        {
            EnsureNotDisposed();
            EnsureNotCompleted();

            long position = await AlignToWordBoundaryAsync().ConfigureAwait(false);

            const int ElementSize = sizeof(ushort);
            int byteCount = ElementSize * values.Count;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(byteCount);
            try
            {
                MemoryMarshal.AsBytes(values.GetOrCreateArray().AsSpan()).CopyTo(buffer);
                await _stream.WriteAsync(buffer, 0, byteCount).ConfigureAwait(false);
                AdvancePosition(byteCount);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            return new TiffStreamRegion(position, byteCount);
        }

        internal async Task<TiffStreamRegion> WriteAlignedValues(TiffValueCollection<short> values)
        {
            EnsureNotDisposed();
            EnsureNotCompleted();

            long position = await AlignToWordBoundaryAsync().ConfigureAwait(false);

            const int ElementSize = sizeof(short);
            int byteCount = ElementSize * values.Count;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(byteCount);
            try
            {
                MemoryMarshal.AsBytes(values.GetOrCreateArray().AsSpan()).CopyTo(buffer);
                await _stream.WriteAsync(buffer, 0, byteCount).ConfigureAwait(false);
                AdvancePosition(byteCount);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            return new TiffStreamRegion(position, byteCount);
        }

        internal async Task<TiffStreamRegion> WriteAlignedValues(TiffValueCollection<uint> values)
        {
            EnsureNotDisposed();
            EnsureNotCompleted();

            long position = await AlignToWordBoundaryAsync().ConfigureAwait(false);

            const int ElementSize = sizeof(uint);
            int byteCount = ElementSize * values.Count;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(byteCount);
            try
            {
                MemoryMarshal.AsBytes(values.GetOrCreateArray().AsSpan()).CopyTo(buffer);
                await _stream.WriteAsync(buffer, 0, byteCount).ConfigureAwait(false);
                AdvancePosition(byteCount);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            return new TiffStreamRegion(position, byteCount);
        }

        internal async Task<TiffStreamRegion> WriteAlignedValues(TiffValueCollection<int> values)
        {
            EnsureNotDisposed();
            EnsureNotCompleted();

            long position = await AlignToWordBoundaryAsync().ConfigureAwait(false);

            const int ElementSize = sizeof(int);
            int byteCount = ElementSize * values.Count;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(byteCount);
            try
            {
                MemoryMarshal.AsBytes(values.GetOrCreateArray().AsSpan()).CopyTo(buffer);
                await _stream.WriteAsync(buffer, 0, byteCount).ConfigureAwait(false);
                AdvancePosition(byteCount);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            return new TiffStreamRegion(position, byteCount);
        }

        internal async Task<TiffStreamRegion> WriteAlignedValues(TiffValueCollection<ulong> values)
        {
            EnsureNotDisposed();
            EnsureNotCompleted();

            long position = await AlignToWordBoundaryAsync().ConfigureAwait(false);

            const int ElementSize = sizeof(ulong);
            int byteCount = ElementSize * values.Count;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(byteCount);
            try
            {
                MemoryMarshal.AsBytes(values.GetOrCreateArray().AsSpan()).CopyTo(buffer);
                await _stream.WriteAsync(buffer, 0, byteCount).ConfigureAwait(false);
                AdvancePosition(byteCount);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            return new TiffStreamRegion(position, byteCount);
        }

        internal async Task<TiffStreamRegion> WriteAlignedValues(TiffValueCollection<long> values)
        {
            EnsureNotDisposed();
            EnsureNotCompleted();

            long position = await AlignToWordBoundaryAsync().ConfigureAwait(false);

            const int ElementSize = sizeof(long);
            int byteCount = ElementSize * values.Count;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(byteCount);
            try
            {
                MemoryMarshal.AsBytes(values.GetOrCreateArray().AsSpan()).CopyTo(buffer);
                await _stream.WriteAsync(buffer, 0, byteCount).ConfigureAwait(false);
                AdvancePosition(byteCount);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            return new TiffStreamRegion(position, byteCount);
        }

        internal async Task<TiffStreamRegion> WriteAlignedValues(TiffValueCollection<TiffRational> values)
        {
            EnsureNotDisposed();
            EnsureNotCompleted();

            long position = await AlignToWordBoundaryAsync().ConfigureAwait(false);

            const int ElementSize = 2 * sizeof(uint);
            int byteCount = ElementSize * values.Count;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(byteCount);
            try
            {
                MemoryMarshal.AsBytes(values.GetOrCreateArray().AsSpan()).CopyTo(buffer);
                await _stream.WriteAsync(buffer, 0, byteCount).ConfigureAwait(false);
                AdvancePosition(byteCount);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            return new TiffStreamRegion(position, byteCount);
        }

        internal async Task<TiffStreamRegion> WriteAlignedValues(TiffValueCollection<TiffSRational> values)
        {
            EnsureNotDisposed();
            EnsureNotCompleted();

            long position = await AlignToWordBoundaryAsync().ConfigureAwait(false);

            const int ElementSize = 2 * sizeof(int);
            int byteCount = ElementSize * values.Count;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(byteCount);
            try
            {
                MemoryMarshal.AsBytes(values.GetOrCreateArray().AsSpan()).CopyTo(buffer);
                await _stream.WriteAsync(buffer, 0, byteCount).ConfigureAwait(false);
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
        /// <returns></returns>
        public async Task FlushAsync()
        {
            EnsureNotDisposed();
            EnsureNotCompleted();

            if (_requireBigTiff && !_useBigTiff)
            {
                throw new InvalidOperationException("Must use BigTIFF format. But it is disabled.");
            }

            Array.Clear(_smallBuffer, 0, 16);
            TiffFileHeader.Write(_smallBuffer, _imageFileDirectoryOffset, BitConverter.IsLittleEndian, _useBigTiff);
            _stream.Seek(0, SeekOrigin.Begin);
            await _stream.WriteAsync(_smallBuffer, 0, _useBigTiff ? 16 : 8).ConfigureAwait(false);
            await _stream.FlushAsync().ConfigureAwait(false);
            _stream.Seek(_position, SeekOrigin.Begin);
        }

        private void EnsureNotCompleted()
        {
            if (_completed)
            {
                ThrowWriterCompleted();
            }
        }

        private static void ThrowWriterCompleted()
        {
            throw new InvalidOperationException("Writer is completed.");
        }

        #endregion

        #region Dispose support

        private void EnsureNotDisposed()
        {
            if (_stream is null)
            {
                ThrowObjectDisposedException();
            }
        }

        private static void ThrowObjectDisposedException()
        {
            throw new ObjectDisposedException(nameof(TiffFileWriter));
        }

        /// <summary>
        /// Dispose this instance.
        /// </summary>
        public void Dispose()
        {
            if (!_leaveOpen)
            {
                _stream?.Dispose();
            }
            _stream = null;
            _operationContext = null;
            _leaveOpen = true;
        }

        /// <summary>
        /// Dispose this instance.
        /// </summary>
        /// <returns>A <see cref="ValueTask"/> that completes when the instance is disposed.</returns>
        public async ValueTask DisposeAsync()
        {
            if (!_leaveOpen && !(_stream is null))
            {
                await _stream.DisposeAsync().ConfigureAwait(false);
            }
            _stream = null;
            _operationContext = null;
            _leaveOpen = true;
        }

        #endregion
    }
}
