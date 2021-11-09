#if !NO_RANDOM_ACCESS

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace TiffLibrary
{
    internal sealed class TiffRandomAccessFileContentReaderWriter : TiffFileContentReaderWriter
    {
        private SafeFileHandle? _fileHandle;
        private long _length;

        public TiffRandomAccessFileContentReaderWriter(string path, FileMode fileMode)
        {
            _fileHandle = File.OpenHandle(path, fileMode, FileAccess.ReadWrite, FileShare.Read, FileOptions.Asynchronous);
        }

        public override int Read(TiffStreamOffset offset, Memory<byte> buffer)
        {
            SafeFileHandle? fileHandle = _fileHandle;
            if (fileHandle is null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(TiffStreamContentReaderWriter));
            }
            if (offset.Offset > Interlocked.Read(ref _length))
            {
                return 0;
            }

            return RandomAccess.Read(fileHandle, buffer.Span, offset);
        }

        public override int Read(TiffStreamOffset offset, ArraySegment<byte> buffer)
        {
            SafeFileHandle? fileHandle = _fileHandle;
            if (fileHandle is null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(TiffStreamContentReaderWriter));
            }
            if (offset.Offset > Interlocked.Read(ref _length))
            {
                return default;
            }
            if (buffer.Array is null)
            {
                return 0;
            }

            return RandomAccess.Read(fileHandle, buffer.AsSpan(), offset);
        }

        public override ValueTask<int> ReadAsync(TiffStreamOffset offset, Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            SafeFileHandle? fileHandle = _fileHandle;
            if (fileHandle is null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(TiffStreamContentReaderWriter));
            }
            if (offset.Offset > Interlocked.Read(ref _length))
            {
                return default;
            }

            return RandomAccess.ReadAsync(fileHandle, buffer, offset, cancellationToken);
        }

        public override ValueTask<int> ReadAsync(TiffStreamOffset offset, ArraySegment<byte> buffer, CancellationToken cancellationToken = default)
        {
            SafeFileHandle? fileHandle = _fileHandle;
            if (fileHandle is null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(TiffStreamContentReaderWriter));
            }
            if (offset.Offset > Interlocked.Read(ref _length))
            {
                return default;
            }
            if (buffer.Array is null)
            {
                return default;
            }

            return RandomAccess.ReadAsync(fileHandle, buffer, offset, cancellationToken);
        }

        public override void Write(TiffStreamOffset offset, ArraySegment<byte> buffer)
        {
            SafeFileHandle? fileHandle = _fileHandle;
            if (fileHandle is null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(TiffStreamContentReaderWriter));
            }
            if (buffer.Array is null)
            {
                return;
            }

            RandomAccess.Write(fileHandle, buffer.AsSpan(), offset);
            Interlocked.Exchange(ref _length, Math.Max(Interlocked.Read(ref _length), offset + buffer.Count));
        }

        public override void Write(TiffStreamOffset offset, ReadOnlyMemory<byte> buffer)
        {
            SafeFileHandle? fileHandle = _fileHandle;
            if (fileHandle is null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(TiffStreamContentReaderWriter));
            }

            RandomAccess.Write(fileHandle, buffer.Span, offset);
            Interlocked.Exchange(ref _length, Math.Max(Interlocked.Read(ref _length), offset + buffer.Length));
        }

        public override ValueTask WriteAsync(TiffStreamOffset offset, ArraySegment<byte> buffer, CancellationToken cancellationToken)
        {
            SafeFileHandle? fileHandle = _fileHandle;
            if (fileHandle is null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(TiffStreamContentReaderWriter));
            }
            if (buffer.Array is null)
            {
                return default;
            }

            ValueTask valueTask = RandomAccess.WriteAsync(fileHandle, buffer, offset, cancellationToken);
            Interlocked.Exchange(ref _length, Math.Max(Interlocked.Read(ref _length), offset + buffer.Count));
            return valueTask;
        }


        public override ValueTask WriteAsync(TiffStreamOffset offset, ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
        {
            SafeFileHandle? fileHandle = _fileHandle;
            if (fileHandle is null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(TiffStreamContentReaderWriter));
            }

            ValueTask valueTask = RandomAccess.WriteAsync(fileHandle, buffer, offset, cancellationToken);
            Interlocked.Exchange(ref _length, Math.Max(Interlocked.Read(ref _length), offset + buffer.Length));
            return valueTask;
        }

        public override void Flush()
        {
            // Do nothing
        }

        public override ValueTask FlushAsync(CancellationToken cancellationToken)
        {
            // Do nothing
            return default;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Interlocked.Exchange(ref _fileHandle, null)?.Dispose();
            }
        }

        public override ValueTask DisposeAsync()
        {
            Interlocked.Exchange(ref _fileHandle, null)?.Dispose();
            return base.DisposeAsync();
        }
    }
}

#endif
