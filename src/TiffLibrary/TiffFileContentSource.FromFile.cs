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
        private string _filename;
        private FileStream _fileStream;

        public TiffFileStreamContentSource(string filename)
        {
            _filename = filename ?? throw new ArgumentNullException(nameof(filename));
            _fileStream = CreateStream();
        }

        private FileStream CreateStream()
        {
            string filename = _filename;
            if (filename is null)
            {
                throw new ObjectDisposedException(nameof(TiffFileStreamContentSource));
            }
            return new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
        }

        public override ValueTask<TiffFileContentReader> OpenReaderAsync()
        {
            FileStream fs = Interlocked.Exchange(ref _fileStream, null) ?? CreateStream();
            return new ValueTask<TiffFileContentReader>(new ContentReader(this, fs));
        }

        private void ReturnStream(FileStream fs)
        {
            if (!(Interlocked.CompareExchange(ref _fileStream, fs, null) is null))
            {
                fs.Dispose();
                return;
            }
            if (_filename is null)
            {
                fs = Interlocked.Exchange(ref _fileStream, null);
                if (!(fs is null))
                {
                    fs.Dispose();
                }
            }
        }

        private ValueTask ReturnStreamAsync(FileStream fs)
        {
            if (!(Interlocked.CompareExchange(ref _fileStream, fs, null) is null))
            {
                return fs.DisposeAsync();
            }
            if (_filename is null)
            {
                fs = Interlocked.Exchange(ref _fileStream, null);
                if (!(fs is null))
                {
                    return fs.DisposeAsync();
                }
            }
            return default;
        }

        protected override void Dispose(bool disposing)
        {
            _filename = null;

            if (disposing)
            {
                Stream fs = Interlocked.Exchange(ref _fileStream, null);
                fs?.Dispose();
            }
        }

        public override ValueTask DisposeAsync()
        {
            _filename = null;
            Stream fs = Interlocked.Exchange(ref _fileStream, null);
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
