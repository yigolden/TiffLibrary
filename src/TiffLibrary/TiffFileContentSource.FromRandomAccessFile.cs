#if !NO_RANDOM_ACCESS

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace TiffLibrary
{
    internal sealed class TiffRandomAccessFileContentSource : TiffFileContentSource
    {
        private string? _fileName;
        private readonly bool _preferAsync;

        private object _lock;
        private SafeFileHandle? _fileHandle;

        public TiffRandomAccessFileContentSource(string fileName, bool preferAsync)
        {
            ThrowHelper.ThrowIfNull(fileName);
            _fileName = fileName;
            _preferAsync = preferAsync;
            _lock = new object();
            _fileHandle = GetOrCreateFileHandle(preferAsync);
        }

        private SafeFileHandle GetOrCreateFileHandle(bool useAsync)
        {
            lock (_lock)
            {
                string? fileName = _fileName;
                if (fileName is null)
                {
                    ThrowHelper.ThrowObjectDisposedException(nameof(TiffFileStreamContentSource));
                }
                SafeFileHandle? fileHandle = _fileHandle;
                if (fileHandle is not null && fileHandle.IsAsync == useAsync)
                {
                    _fileHandle = null;
                    return fileHandle;
                }
                return File.OpenHandle(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, useAsync ? FileOptions.Asynchronous : FileOptions.None);
            }
        }

        public override TiffFileContentReader OpenReader()
        {
            return new ContentReader(this, GetOrCreateFileHandle(useAsync: false));
        }

        public override ValueTask<TiffFileContentReader> OpenReaderAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return new ValueTask<TiffFileContentReader>(new ContentReader(this, GetOrCreateFileHandle(useAsync: false)));
        }

        private void ReturnHandle(SafeFileHandle fileHandle)
        {
            lock (_lock)
            {
                if (_fileName is null)
                {
                    fileHandle.Dispose();
                    return;
                }
                if (_fileHandle is not null)
                {
                    fileHandle.Dispose();
                    return;
                }
                if (fileHandle.IsAsync == _preferAsync)
                {
                    _fileHandle = fileHandle;
                }
                else
                {
                    fileHandle.Dispose();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            SafeFileHandle? fileHandle = null;
            lock (_lock)
            {
                if (disposing)
                {
                    fileHandle = _fileHandle;
                }
                _fileName = null;
                _fileHandle = null;
            }

            fileHandle?.Dispose();
        }

        internal sealed class ContentReader : TiffFileContentReader
        {
            private readonly TiffRandomAccessFileContentSource _parent;
            private SafeFileHandle? _fileHandle;
            private long _length;

            public ContentReader(TiffRandomAccessFileContentSource parent, SafeFileHandle fileHandle)
            {
                _parent = parent;
                _fileHandle = fileHandle;
                _length = RandomAccess.GetLength(fileHandle);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    SafeFileHandle? fileHandle = Interlocked.Exchange(ref _fileHandle, null);
                    if (fileHandle is not null)
                    {
                        _parent.ReturnHandle(fileHandle);
                    }
                }
            }

            public override int Read(TiffStreamOffset offset, ArraySegment<byte> buffer)
            {
                SafeFileHandle? fileHandle = _fileHandle;
                if (fileHandle is null)
                {
                    ThrowHelper.ThrowObjectDisposedException(nameof(ContentReader));
                }
                if (offset.Offset > _length)
                {
                    return default;
                }
                if (buffer.Array is null)
                {
                    return 0;
                }

                return RandomAccess.Read(fileHandle, buffer.AsSpan(), offset);
            }

            public override int Read(TiffStreamOffset offset, Memory<byte> buffer)
            {
                SafeFileHandle? fileHandle = _fileHandle;
                if (fileHandle is null)
                {
                    ThrowHelper.ThrowObjectDisposedException(nameof(ContentReader));
                }
                if (offset.Offset > _length)
                {
                    return 0;
                }

                return RandomAccess.Read(fileHandle, buffer.Span, offset);
            }

            public override ValueTask<int> ReadAsync(TiffStreamOffset offset, ArraySegment<byte> buffer, CancellationToken cancellationToken)
            {
                SafeFileHandle? fileHandle = _fileHandle;
                if (fileHandle is null)
                {
                    ThrowHelper.ThrowObjectDisposedException(nameof(ContentReader));
                }
                if (offset.Offset > _length)
                {
                    return default;
                }
                if (buffer.Array is null)
                {
                    return default;
                }

                return RandomAccess.ReadAsync(fileHandle, buffer.AsMemory(), offset, cancellationToken);
            }

            public override ValueTask<int> ReadAsync(TiffStreamOffset offset, Memory<byte> buffer, CancellationToken cancellationToken)
            {
                SafeFileHandle? fileHandle = _fileHandle;
                if (fileHandle is null)
                {
                    ThrowHelper.ThrowObjectDisposedException(nameof(ContentReader));
                }
                if (offset.Offset > _length)
                {
                    return default;
                }

                return RandomAccess.ReadAsync(fileHandle, buffer, offset, cancellationToken);
            }

        }

    }
}

#endif
