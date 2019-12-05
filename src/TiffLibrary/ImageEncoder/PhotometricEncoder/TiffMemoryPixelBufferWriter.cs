using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using TiffLibrary.PixelBuffer;

namespace TiffLibrary.ImageEncoder.PhotometricEncoder
{
    internal sealed class TiffMemoryPixelBufferWriter<TPixel> : MemoryManager<TPixel>, ITiffPixelBufferWriter<TPixel> where TPixel : unmanaged
    {
        private readonly MemoryPool<byte> _memoryPool;
        private readonly Memory<byte> _memory;
        private readonly int _width;
        private readonly int _height;

        private RowSpanHandle? _cachedRowHandle;
        private ColumnSpanHandle? _cachedColHandle;

        public TiffMemoryPixelBufferWriter(MemoryPool<byte> memoryPool, Memory<byte> memory, int width, int height)
        {
            _memoryPool = memoryPool;
            _memory = memory;
            _width = width;
            _height = height;
        }

        public int Width => _width;

        public int Height => _height;

        public TiffPixelSpanHandle<TPixel> GetRowSpan(int rowIndex, int start, int length)
        {
            int width = _width;
            int height = _height;
            if ((uint)rowIndex >= (uint)height)
            {
                throw new ArgumentOutOfRangeException(nameof(rowIndex));
            }
            if ((uint)start > (uint)width)
            {
                throw new ArgumentOutOfRangeException(nameof(start));
            }
            if (length < 0 || (uint)(start + length) > (uint)width)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            RowSpanHandle? handle = Interlocked.Exchange(ref _cachedRowHandle, null);
            if (handle is null)
            {
                handle = new RowSpanHandle();
            }
            handle.SetHandle(this, Memory.Slice(rowIndex * width + start, length));
            return handle;
        }

        public TiffPixelSpanHandle<TPixel> GetColumnSpan(int colIndex, int start, int length)
        {
            int width = _width;
            int height = _height;
            if ((uint)colIndex >= (uint)width)
            {
                throw new ArgumentOutOfRangeException(nameof(colIndex));
            }
            if ((uint)start > (uint)height)
            {
                throw new ArgumentOutOfRangeException(nameof(start));
            }
            if (length < 0 || (uint)(start + length) > (uint)height)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            ColumnSpanHandle? handle = Interlocked.Exchange(ref _cachedColHandle, null);
            if (handle is null)
            {
                handle = new ColumnSpanHandle();
            }
            handle.SetHandle(this, colIndex, start, length);
            return handle;
        }

        public override Span<TPixel> GetSpan()
        {
            return MemoryMarshal.Cast<byte, TPixel>(_memory.Span.Slice(0, Unsafe.SizeOf<TPixel>() * _width * _height));
        }

        protected override void Dispose(bool disposing)
        {
            if (!(_cachedColHandle is null))
            {
                _cachedColHandle.ReleaseBuffer();
            }
        }

        public override unsafe MemoryHandle Pin(int elementIndex = 0)
        {
            if ((uint)elementIndex >= (uint)(_width * _height))
            {
                throw new ArgumentOutOfRangeException(nameof(elementIndex));
            }

            int offset = Unsafe.SizeOf<TPixel>() * elementIndex;
            GCHandle handle = GCHandle.Alloc(_memory, GCHandleType.Pinned);
            return new MemoryHandle((void*)(handle.AddrOfPinnedObject() + offset), handle);
        }

        public override void Unpin() { }


        #region RowSpanHandle

        private class RowSpanHandle : TiffPixelSpanHandle<TPixel>
        {
            private TiffMemoryPixelBufferWriter<TPixel>? _parent;
            private Memory<TPixel> _memory;

            internal void SetHandle(TiffMemoryPixelBufferWriter<TPixel> parent, Memory<TPixel> memory)
            {
                _parent = parent;
                _memory = memory;
            }

            public override Span<TPixel> GetSpan()
            {
                if (_parent is null)
                {
                    throw new ObjectDisposedException(GetType().FullName);
                }
                return _memory.Span;
            }

            public override void Dispose()
            {
                _memory = default;
                if (_parent != null)
                {
                    TiffMemoryPixelBufferWriter<TPixel> parent = _parent;
                    _parent = null;
                    Interlocked.CompareExchange(ref parent._cachedRowHandle, this, null);
                }
            }
        }

        #endregion


        #region ColumnSpanHandle

        private class ColumnSpanHandle : TiffPixelSpanHandle<TPixel>
        {
            private TiffMemoryPixelBufferWriter<TPixel>? _parent;

            private MemoryPool<byte>? _memoryPool;
            private IMemoryOwner<byte>? _bufferHandle;
            private Memory<byte> _buffer;
            private int _colIndex;
            private int _start;
            private int _length;

            internal void SetHandle(TiffMemoryPixelBufferWriter<TPixel> parent, int colIndex, int start, int length)
            {
                _parent = parent;
                _memoryPool = parent._memoryPool;
                _colIndex = colIndex;
                _start = start;
                _length = length;

                EnsureBufferSize(_length * Unsafe.SizeOf<TPixel>());
            }

            internal void EnsureBufferSize(int size)
            {
                MemoryPool<byte> memoryPool = _memoryPool ?? MemoryPool<byte>.Shared;
                if (_bufferHandle is null)
                {
                    _bufferHandle = memoryPool.Rent(size);
                    _buffer = _bufferHandle.Memory;
                    return;
                }
                if (_buffer.Length < size)
                {
                    _bufferHandle.Dispose();
                    _bufferHandle = memoryPool.Rent(size);
                    _buffer = _bufferHandle.Memory;
                }
            }

            internal void ReleaseBuffer()
            {
                if (!(_bufferHandle is null))
                {
                    _bufferHandle.Dispose();
                    _bufferHandle = null;
                }
                _buffer = default;
            }

            public override Span<TPixel> GetSpan()
            {
                if (_parent is null)
                {
                    throw new ObjectDisposedException(GetType().FullName);
                }
                return MemoryMarshal.Cast<byte, TPixel>(_buffer.Span.Slice(0, _length * Unsafe.SizeOf<TPixel>()));
            }

            public override void Dispose()
            {
                if (_parent is null)
                {
                    return;
                }

                // Copy pixels into this column
                int colIndex = _colIndex;
                int width = _parent.Width;
                Span<TPixel> sourceSpan = MemoryMarshal.Cast<byte, TPixel>(_buffer.Span.Slice(0, _length * Unsafe.SizeOf<TPixel>()));
                Span<TPixel> destinationSpan = _parent.GetSpan().Slice(_start * width);
                for (int i = 0; i < sourceSpan.Length; i++)
                {
                    destinationSpan[colIndex + i * width] = sourceSpan[i];
                }

                if (_parent != null)
                {
                    TiffMemoryPixelBufferWriter<TPixel> parent = _parent;
                    _parent = null;
                    if (Interlocked.CompareExchange(ref parent._cachedColHandle, this, null) != null)
                    {
                        ReleaseBuffer();
                    }
                }
            }


        }

        #endregion

    }
}
