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
        private string _fileName;
        private readonly bool _preferAsync;

        private object _lock;
        private FileStream _fileStream;

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
                string fileName = _fileName;
                if (fileName is null)
                {
                    throw new ObjectDisposedException(nameof(TiffFileStreamContentSource));
                }
                FileStream fs = _fileStream;
                if (!(fs is null) && fs.IsAsync == useAsync)
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

        public override ValueTask<TiffFileContentReader> OpenReaderAsync()
        {
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
                if (!(_fileStream is null))
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
                else if (!(_fileStream is null))
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
            Stream fs = null;
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

        public override ValueTask DisposeAsync()
        {
            Stream fs = null;
            lock (_lock)
            {
                fs = _fileStream;
                _fileName = null;
                _fileStream = null;
            }

            if (!(fs is null))
            {
                return fs.DisposeAsync();
            }
            return default;
        }


        internal sealed class ContentReader : TiffFileContentReader
        {
            private readonly TiffFileStreamContentSource _parent;
            private FileStream _stream;
            private int _streamInUse;

            public ContentReader(TiffFileStreamContentSource parent, FileStream stream)
            {
                _parent = parent;
                _stream = stream;
                _streamInUse = 0;
            }

            public override ValueTask DisposeAsync()
            {
                FileStream fs = Interlocked.Exchange(ref _stream, null);
                if (!(fs is null))
                {
                    return _parent.ReturnStreamAsync(fs);
                }
                return default;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    FileStream fs = Interlocked.Exchange(ref _stream, null);
                    if (!(fs is null))
                    {

                        _parent.ReturnStream(fs);
                    }
                }
            }

            public override int Read(long offset, ArraySegment<byte> buffer)
            {
                Stream stream = _stream;
                if (stream is null)
                {
                    throw new ObjectDisposedException(nameof(ContentReader));
                }
                if (offset > stream.Length)
                {
                    return default;
                }
                if (Interlocked.Exchange(ref _streamInUse, 1) == 1)
                {
                    throw new InvalidOperationException("Concurrent reads on stream source is not supported. Please check that you are not reading the TIFF file from multiple threads at the same time.");
                }
                try
                {
                    stream.Seek(offset, SeekOrigin.Begin);
                    return stream.Read(buffer.Array, buffer.Offset, buffer.Count);
                }
                finally
                {
                    Interlocked.Exchange(ref _streamInUse, 0);
                }
            }

            public override int Read(long offset, Memory<byte> buffer)
            {
                Stream stream = _stream;
                if (stream is null)
                {
                    throw new ObjectDisposedException(nameof(ContentReader));
                }
                if (offset > stream.Length)
                {
                    return 0;
                }

                if (Interlocked.Exchange(ref _streamInUse, 1) == 1)
                {
                    throw new InvalidOperationException("Concurrent reads on stream source is not supported. Please check that you are not reading the TIFF file from multiple threads at the same time.");
                }
                try
                {
                    stream.Seek(offset, SeekOrigin.Begin);

                    if (MemoryMarshal.TryGetArray(buffer, out ArraySegment<byte> arraySegment))
                    {
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

            public override async ValueTask<int> ReadAsync(long offset, ArraySegment<byte> buffer, CancellationToken cancellationToken)
            {
                Stream stream = _stream;
                if (stream is null)
                {
                    throw new ObjectDisposedException(nameof(ContentReader));
                }
                if (offset > stream.Length)
                {
                    return default;
                }
                if (Interlocked.Exchange(ref _streamInUse, 1) == 1)
                {
                    throw new InvalidOperationException("Concurrent reads on stream source is not supported. Please check that you are not reading the TIFF file from multiple threads at the same time.");
                }
                try
                {
                    stream.Seek(offset, SeekOrigin.Begin);
                    return await stream.ReadAsync(buffer.Array, buffer.Offset, buffer.Count, cancellationToken).ConfigureAwait(false);
                }
                finally
                {
                    Interlocked.Exchange(ref _streamInUse, 0);
                }
            }

            public override async ValueTask<int> ReadAsync(long offset, Memory<byte> buffer, CancellationToken cancellationToken)
            {
                Stream stream = _stream;
                if (stream is null)
                {
                    throw new ObjectDisposedException(nameof(ContentReader));
                }
                if (offset > stream.Length)
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
                    stream.Seek(offset, SeekOrigin.Begin);
                    return await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
#else
                    stream.Seek(offset, SeekOrigin.Begin);
                    if (MemoryMarshal.TryGetArray(buffer, out ArraySegment<byte> arraySegment))
                    {
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
