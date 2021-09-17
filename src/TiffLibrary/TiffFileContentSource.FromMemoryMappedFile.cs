using System;
using System.IO.MemoryMappedFiles;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace TiffLibrary
{
    internal sealed class TiffMemoryMappedFileContentSource : TiffFileContentSource
    {
        private MemoryMappedFile? _file;
        private MemoryMappedViewAccessor? _accessor;
        private readonly bool _leaveOpen;
        private readonly long _capacity;

        public TiffMemoryMappedFileContentSource(MemoryMappedFile file, bool leaveOpen)
        {
            _file = file;
            _accessor = file.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
            _leaveOpen = leaveOpen;
            _capacity = _accessor.Capacity;
        }

        public override TiffFileContentReader OpenReader()
        {
            MemoryMappedViewAccessor? accessor = _accessor;
            if (accessor is null)
            {
                ThrowHelper.ThrowObjectDisposedException(nameof(TiffStreamContentSource));
            }
            return new ContentReader(accessor, _capacity);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_accessor is not null)
                {
                    _accessor.Dispose();
                }
                if (_file is not null && !_leaveOpen)
                {
                    _file.Dispose();
                }
            }

            _accessor = null;
            _file = null;
        }

        internal sealed class ContentReader : TiffFileContentReader
        {
            private MemoryMappedViewAccessor? _accessor;
            private readonly long _capacity;

            public ContentReader(MemoryMappedViewAccessor accessor, long capacity)
            {
                _accessor = accessor;
                _capacity = capacity;
            }

            public override unsafe int Read(TiffStreamOffset offset, Memory<byte> buffer)
            {
                MemoryMappedViewAccessor? accessor = Volatile.Read(ref _accessor);
                if (accessor is null)
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
                        new Span<byte>(pointer + accessor.PointerOffset + offset, buffer.Length).CopyTo(buffer.Span);
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

            protected override void Dispose(bool disposing)
            {
                _accessor = null;
            }
        }
    }
}
