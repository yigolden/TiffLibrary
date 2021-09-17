using System;
using System.IO.MemoryMappedFiles;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace TiffLibrary
{
    internal sealed class TiffMemoryMappedFileContentSource : TiffFileContentSource
    {
        private MemoryMappedFile? _file;
        private readonly bool _leaveOpen;
        private readonly long _capacity;

        private ContentReader? _readerCache;

        public TiffMemoryMappedFileContentSource(MemoryMappedFile file, bool leaveOpen)
        {
            _file = file;
            _leaveOpen = leaveOpen;

            using (MemoryMappedViewAccessor? accessor = file.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read))
            {
                _capacity = accessor.Capacity;
            }
        }

        public override TiffFileContentReader OpenReader()
        {
            MemoryMappedFile? file = _file;
            if (file is null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(TiffStreamContentSource));
            }
            ContentReader? reader = Interlocked.Exchange(ref _readerCache, null);
            if (reader is null)
            {
                reader = new ContentReader(file, _capacity);
            }
            return reader;
        }

        protected override void Dispose(bool disposing)
        {
            if (_file is not null && !_leaveOpen)
            {
                _file.Dispose();
            }
            _file = null;
            _readerCache = null;
        }

        public void ReturnReader(ContentReader reader)
        {
            if (_file is not null)
            {
                Interlocked.Exchange(ref _readerCache, reader);
            }
        }

        internal sealed class ContentReader : TiffFileContentReader
        {
            private MemoryMappedFile? _file;
            private readonly long _capacity;

            public ContentReader(MemoryMappedFile file, long capacity)
            {
                _file = file;
                _capacity = capacity;
            }

            public override unsafe int Read(TiffStreamOffset offset, Memory<byte> buffer)
            {
                MemoryMappedFile? file = Volatile.Read(ref _file);
                if (file is null)
                {
                    ThrowHelper.ThrowObjectDisposedException(nameof(ContentReader));
                }

                if (offset.Offset + buffer.Length > _capacity)
                {
                    buffer = buffer.Slice(0, Math.Max(0, (int)(_capacity - offset.Offset)));
                }

                if (buffer.IsEmpty)
                {
                    return 0;
                }

                using (MemoryMappedViewAccessor accessor = file.CreateViewAccessor(offset, 0, MemoryMappedFileAccess.Read))
                {
                    byte* pointer = null;
                    SafeMemoryMappedViewHandle handle = accessor.SafeMemoryMappedViewHandle;

#if !NET6_0_OR_GREATER
                    System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions();
#endif

                    try
                    {
                        handle.AcquirePointer(ref pointer);
                        if (pointer != null)
                        {
                            new Span<byte>(pointer + accessor.PointerOffset, buffer.Length).CopyTo(buffer.Span);
                        }

                        return buffer.Length;
                    }
                    finally
                    {
                        if (pointer != null)
                        {
                            handle.ReleasePointer();
                        }
                    }
                }
            }

            protected override void Dispose(bool disposing)
            {
                _file = null;
            }
        }
    }
}
