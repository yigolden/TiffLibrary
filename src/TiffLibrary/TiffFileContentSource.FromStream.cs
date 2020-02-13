using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace TiffLibrary
{
    internal sealed class TiffStreamContentSource : TiffFileContentSource
    {
        private Stream? _stream;
        private ContentReader? _reader;
        private readonly bool _leaveOpen;

        public TiffStreamContentSource(Stream stream, bool leaveOpen)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            if (!stream.CanSeek)
            {
                throw new ArgumentException("Stream must be seekable.", nameof(stream));
            }
            _reader = new ContentReader(stream, true);
            _leaveOpen = leaveOpen;
        }

        public override TiffFileContentReader OpenReader()
        {
            ContentReader? reader = _reader;
            if (reader is null)
            {
                throw new ObjectDisposedException(nameof(TiffStreamContentSource));
            }
            return reader;
        }

        public override ValueTask<TiffFileContentReader> OpenReaderAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ContentReader? reader = _reader;
            if (reader is null)
            {
                throw new ObjectDisposedException(nameof(TiffStreamContentSource));
            }
            return new ValueTask<TiffFileContentReader>(reader);
        }

        protected override void Dispose(bool disposing)
        {
            if (!(_stream is null) && !_leaveOpen)
            {
                _stream.Dispose();
            }
            _stream = null;
            _reader = null;
        }

#if !NO_ASYNC_DISPOSABLE_ON_STREAM

        public override async ValueTask DisposeAsync()
        {
            if (!(_stream is null) && !_leaveOpen)
            {
                await _stream.DisposeAsync().ConfigureAwait(false);
            }
            _stream = null;
            _reader = null;
        }
#endif

        internal sealed class ContentReader : TiffFileContentReader
        {
            private Stream? _stream;
            private readonly bool _leaveOpen;
            private int _streamInUse;

            public ContentReader(Stream stream, bool leaveOpen)
            {
                _stream = stream;
                _leaveOpen = leaveOpen;
                _streamInUse = 0;
            }

#if NO_ASYNC_DISPOSABLE_ON_STREAM
            public override ValueTask DisposeAsync()
            {
                if (!(_stream is null) && !_leaveOpen)
                {
                    _stream.Dispose();
                    _stream = null;
                }
                return default;
            }
#else
            public override async ValueTask DisposeAsync()
            {
                if (!(_stream is null) && !_leaveOpen)
                {
                    await _stream.DisposeAsync().ConfigureAwait(false);
                    _stream = null;
                }
            }
#endif

            protected override void Dispose(bool disposing)
            {
                if (disposing && !(_stream is null) && !_leaveOpen)
                {
                    _stream.Dispose();
                    _stream = null;
                }
            }

            public override int Read(TiffStreamOffset offset, ArraySegment<byte> buffer)
            {
                Stream? stream = _stream;
                if (stream is null)
                {
                    throw new ObjectDisposedException(nameof(ContentReader));
                }
                if (offset.Offset > stream.Length)
                {
                    return default;
                }
                if (buffer.Array is null)
                {
                    return 0;
                }

                if (Interlocked.Exchange(ref _streamInUse, 1) == 1)
                {
                    throw new InvalidOperationException("Concurrent reads on stream source is not supported. Please check that you are not reading the TIFF file from multiple threads at the same time.");
                }
                try
                {
                    stream.Seek(offset.Offset, SeekOrigin.Begin);
                    return stream.Read(buffer.Array, buffer.Offset, buffer.Count);
                }
                finally
                {
                    Interlocked.Exchange(ref _streamInUse, 0);
                }
            }

            public override int Read(TiffStreamOffset offset, Memory<byte> buffer)
            {
                Stream? stream = _stream;
                if (stream is null)
                {
                    throw new ObjectDisposedException(nameof(ContentReader));
                }
                if (offset.Offset > stream.Length)
                {
                    return 0;
                }

                if (Interlocked.Exchange(ref _streamInUse, 1) == 1)
                {
                    throw new InvalidOperationException("Concurrent reads on stream source is not supported. Please check that you are not reading the TIFF file from multiple threads at the same time.");
                }
                try
                {
                    stream.Seek(offset.Offset, SeekOrigin.Begin);

                    if (MemoryMarshal.TryGetArray(buffer, out ArraySegment<byte> arraySegment))
                    {
                        if (arraySegment.Array is null)
                        {
                            return 0;
                        }

                        return stream.Read(arraySegment.Array, arraySegment.Offset, arraySegment.Count);
                    }

#if !NO_FAST_SPAN
                    return stream.Read(buffer.Span);
#else
                    // Slow path
                    byte[] temp = ArrayPool<byte>.Shared.Rent(buffer.Length);
                    try
                    {
                        int count = stream.Read(temp, 0, buffer.Length);
                        temp.AsMemory(0, count).CopyTo(buffer);
                        return count;
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(temp);
                    }
#endif
                }
                finally
                {
                    Interlocked.Exchange(ref _streamInUse, 0);
                }
            }

            public override async ValueTask<int> ReadAsync(TiffStreamOffset offset, ArraySegment<byte> buffer, CancellationToken cancellationToken)
            {
                Stream? stream = _stream;
                if (stream is null)
                {
                    throw new ObjectDisposedException(nameof(ContentReader));
                }
                if (offset.Offset > stream.Length)
                {
                    return default;
                }
                if (buffer.Array is null)
                {
                    return 0;
                }
                if (Interlocked.Exchange(ref _streamInUse, 1) == 1)
                {
                    throw new InvalidOperationException("Concurrent reads on stream source is not supported. Please check that you are not reading the TIFF file from multiple threads at the same time.");
                }
                try
                {
                    stream.Seek(offset.Offset, SeekOrigin.Begin);
                    return await stream.ReadAsync(buffer.Array, buffer.Offset, buffer.Count, cancellationToken).ConfigureAwait(false);
                }
                finally
                {
                    Interlocked.Exchange(ref _streamInUse, 0);
                }
            }

            public override async ValueTask<int> ReadAsync(TiffStreamOffset offset, Memory<byte> buffer, CancellationToken cancellationToken)
            {
                Stream? stream = _stream;
                if (stream is null)
                {
                    throw new ObjectDisposedException(nameof(ContentReader));
                }
                if (offset.Offset > stream.Length)
                {
                    return 0;
                }

                if (Interlocked.Exchange(ref _streamInUse, 1) == 1)
                {
                    throw new InvalidOperationException("Concurrent reads on stream source is not supported. Please check that you are not reading the TIFF file from multiple threads at the same time.");
                }
                try
                {

#if !NO_FAST_SPAN
                    stream.Seek(offset.Offset, SeekOrigin.Begin);
                    return await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
#else
                    stream.Seek(offset.Offset, SeekOrigin.Begin);
                    if (MemoryMarshal.TryGetArray(buffer, out ArraySegment<byte> arraySegment))
                    {
                        if (arraySegment.Array is null)
                        {
                            return 0;
                        }

                        return await stream.ReadAsync(arraySegment.Array, arraySegment.Offset, arraySegment.Count, cancellationToken).ConfigureAwait(false);
                    }

                    // Slow path
                    byte[] temp = ArrayPool<byte>.Shared.Rent(buffer.Length);
                    try
                    {
                        int count = await stream.ReadAsync(temp, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
                        temp.AsMemory(0, count).CopyTo(buffer);
                        return count;
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(temp);
                    }
#endif
                }
                finally
                {
                    Interlocked.Exchange(ref _streamInUse, 0);
                }
            }

        }
    }
}
