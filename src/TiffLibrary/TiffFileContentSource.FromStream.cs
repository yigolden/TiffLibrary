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
        private SemaphoreSlim? _semaphore;
        private ContentReader? _reader;
        private readonly bool _leaveOpen;

        public TiffStreamContentSource(Stream stream, bool leaveOpen)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            if (!stream.CanSeek)
            {
                throw new ArgumentException("Stream must be seekable.", nameof(stream));
            }
            _semaphore = new SemaphoreSlim(1);
            _reader = new ContentReader(stream, _semaphore);
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
            if (!(_semaphore is null))
            {
                _semaphore.Dispose();
                _semaphore = null;
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
                _stream = null;
            }
            if (!(_semaphore is null))
            {
                _semaphore.Dispose();
                _semaphore = null;
            }
            await base.DisposeAsync().ConfigureAwait(false);
            _reader = null;
        }
#endif

        internal sealed class ContentReader : TiffFileContentReader
        {
            private readonly Stream _stream;
            private readonly SemaphoreSlim _semaphore;

            public ContentReader(Stream stream, SemaphoreSlim semaphore)
            {
                _stream = stream;
                _semaphore = semaphore;
            }

            protected override void Dispose(bool disposing) { }

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

                _semaphore!.Wait();
                try
                {
                    stream.Seek(offset.Offset, SeekOrigin.Begin);
                    return stream.Read(buffer.Array, buffer.Offset, buffer.Count);
                }
                finally
                {
                    _semaphore.Release();
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

                _semaphore!.Wait();
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
                    _semaphore.Release();
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

                await _semaphore!.WaitAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    stream.Seek(offset.Offset, SeekOrigin.Begin);
#if !NO_FAST_SPAN
                    return await stream.ReadAsync(buffer.AsMemory(), cancellationToken).ConfigureAwait(false);
#else
                    return await stream.ReadAsync(buffer.Array, buffer.Offset, buffer.Count, cancellationToken).ConfigureAwait(false);
#endif
                }
                finally
                {
                    _semaphore.Release();
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

                await _semaphore!.WaitAsync(cancellationToken).ConfigureAwait(false);
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
                    _semaphore.Release();
                }
            }

        }
    }
}
