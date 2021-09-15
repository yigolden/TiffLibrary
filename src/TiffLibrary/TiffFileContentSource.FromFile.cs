using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace TiffLibrary
{
    internal sealed class TiffFileStreamContentSource : TiffFileContentSource
    {
        private string? _fileName;
        private readonly bool _preferAsync;

        private object _lock;
        private FileStream? _fileStream;

        public TiffFileStreamContentSource(string fileName, bool preferAsync)
        {
            _fileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            _preferAsync = preferAsync;
            _lock = new object();
            _fileStream = GetOrCreateStream(preferAsync);
        }

        private FileStream GetOrCreateStream(bool useAsync)
        {
            lock (_lock)
            {
                string? fileName = _fileName;
                if (fileName is null)
                {
                    throw new ObjectDisposedException(nameof(TiffFileStreamContentSource));
                }
                FileStream? fs = _fileStream;
                if (fs is not null && fs.IsAsync == useAsync)
                {
                    _fileStream = null;
                    return fs;
                }
                return new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync);
            }
        }

        public override TiffFileContentReader OpenReader()
        {
            return new ContentReader(this, GetOrCreateStream(useAsync: false));
        }

        public override ValueTask<TiffFileContentReader> OpenReaderAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return new ValueTask<TiffFileContentReader>(new ContentReader(this, GetOrCreateStream(useAsync: false)));
        }

        private void ReturnStream(FileStream fs)
        {
            lock (_lock)
            {
                if (_fileName is null)
                {
                    fs.Dispose();
                    return;
                }
                if (_fileStream is not null)
                {
                    fs.Dispose();
                    return;
                }
                if (fs.IsAsync == _preferAsync)
                {
                    _fileStream = fs;
                }
                else
                {
                    fs.Dispose();
                }
            }
        }

        private ValueTask ReturnStreamAsync(FileStream fs)
        {
            ValueTask disposeTask = default;
            lock (_lock)
            {
                if (_fileName is null)
                {
                    disposeTask = fs.DisposeAsync();
                }
                else if (_fileStream is not null)
                {
                    disposeTask = fs.DisposeAsync();
                }
                else if (fs.IsAsync == _preferAsync)
                {
                    _fileStream = fs;
                }
                else
                {
                    disposeTask = fs.DisposeAsync();
                }
            }
            return disposeTask;
        }

        protected override void Dispose(bool disposing)
        {
            Stream? fs = null;
            lock (_lock)
            {
                if (disposing)
                {
                    fs = _fileStream;
                }
                _fileName = null;
                _fileStream = null;
            }

            fs?.Dispose();
        }

        public override async ValueTask DisposeAsync()
        {
            Stream? fs = null;
            lock (_lock)
            {
                fs = _fileStream;
                _fileName = null;
                _fileStream = null;
            }

            if (fs is not null)
            {
                await fs.DisposeAsync().ConfigureAwait(false);
            }
            await base.DisposeAsync().ConfigureAwait(false);
        }


        internal sealed class ContentReader : TiffFileContentReader
        {
            private readonly TiffFileStreamContentSource _parent;
            private FileStream? _stream;
            private SemaphoreSlim? _semaphore;

            public ContentReader(TiffFileStreamContentSource parent, FileStream stream)
            {
                _parent = parent;
                _stream = stream;
                _semaphore = new SemaphoreSlim(1);
            }

            public override async ValueTask DisposeAsync()
            {
                FileStream? fs = Interlocked.Exchange(ref _stream, null);
                if (fs is not null)
                {
                    await _parent.ReturnStreamAsync(fs).ConfigureAwait(false);
                }
                if (_semaphore is not null)
                {
                    _semaphore.Dispose();
                    _semaphore = null;
                }
                await base.DisposeAsync().ConfigureAwait(false);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    FileStream? fs = Interlocked.Exchange(ref _stream, null);
                    if (fs is not null)
                    {
                        _parent.ReturnStream(fs);
                    }
                    if (_semaphore is not null)
                    {
                        _semaphore.Dispose();
                        _semaphore = null;
                    }
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
