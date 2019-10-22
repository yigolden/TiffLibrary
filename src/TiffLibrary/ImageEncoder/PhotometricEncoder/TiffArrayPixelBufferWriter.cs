using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using TiffLibrary.PixelBuffer;

namespace TiffLibrary.ImageEncoder.PhotometricEncoder
{
    internal class TiffArrayPixelBufferWriter<TPixel> : MemoryManager<TPixel>, ITiffPixelBufferWriter<TPixel> where TPixel : unmanaged
    {
        private readonly byte[] _array;
        private readonly int _width;
        private readonly int _height;

        private RowSpanHandle _cachedRowHandle;
        private ColumnSpanHandle _cachedColHandle;

        public TiffArrayPixelBufferWriter(byte[] array, int width, int height)
        {
            _array = array;
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
            if ((uint)(start + length) > (uint)width)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            RowSpanHandle handle = Interlocked.Exchange(ref _cachedRowHandle, null);
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
            if ((uint)(start + length) > (uint)height)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            ColumnSpanHandle handle = Interlocked.Exchange(ref _cachedColHandle, null);
            if (handle is null)
            {
                handle = new ColumnSpanHandle();
            }
            handle.SetHandle(this, colIndex, start, length);
            return handle;
        }

        public override Span<TPixel> GetSpan()
        {
            return MemoryMarshal.Cast<byte, TPixel>(_array.AsSpan(0, Unsafe.SizeOf<TPixel>() * _width * _height));
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
            GCHandle handle = GCHandle.Alloc(_array, GCHandleType.Pinned);
            return new MemoryHandle((void*)(handle.AddrOfPinnedObject() + offset), handle);
        }

        public override void Unpin() { }


        #region RowSpanHandle

        private class RowSpanHandle : TiffPixelSpanHandle<TPixel>
        {
            private TiffArrayPixelBufferWriter<TPixel> _parent;
            private Memory<TPixel> _memory;

            internal void SetHandle(TiffArrayPixelBufferWriter<TPixel> parent, Memory<TPixel> memory)
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
                    TiffArrayPixelBufferWriter<TPixel> parent = _parent;
                    _parent = null;
                    Interlocked.CompareExchange(ref parent._cachedRowHandle, this, null);
                }
            }
        }

        #endregion


        #region ColumnSpanHandle

        private class ColumnSpanHandle : TiffPixelSpanHandle<TPixel>
        {
            private TiffArrayPixelBufferWriter<TPixel> _parent;
            private byte[] _buffer;
            private int _colIndex;
            private int _start;
            private int _length;

            internal void SetHandle(TiffArrayPixelBufferWriter<TPixel> parent, int colIndex, int start, int length)
            {
                _parent = parent;
                _colIndex = colIndex;
                _start = start;
                _length = length;

                EnsureBufferSize(_length * Unsafe.SizeOf<TPixel>());
            }

            internal void EnsureBufferSize(int size)
            {
                if (_buffer is null)
                {
                    _buffer = ArrayPool<byte>.Shared.Rent(size);
                    return;
                }
                if (_buffer.Length < size)
                {
                    ArrayPool<byte>.Shared.Return(_buffer);
                    _buffer = ArrayPool<byte>.Shared.Rent(size);
                }
            }

            internal void ReleaseBuffer()
            {
                if (!(_buffer is null))
                {
                    ArrayPool<byte>.Shared.Return(_buffer);
                    _buffer = null;
                }
            }

            public override Span<TPixel> GetSpan()
            {
                if (_parent is null)
                {
                    throw new ObjectDisposedException(GetType().FullName);
                }
                return MemoryMarshal.Cast<byte, TPixel>(_buffer.AsSpan(0, _length * Unsafe.SizeOf<TPixel>()));
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
                Span<TPixel> sourceSpan = MemoryMarshal.Cast<byte, TPixel>(_buffer.AsSpan(0, _length * Unsafe.SizeOf<TPixel>()));
                Span<TPixel> destinationSpan = _parent.GetSpan().Slice(_start * width);
                for (int i = 0; i < sourceSpan.Length; i++)
                {
                    destinationSpan[colIndex + i * width] = sourceSpan[i];
                }

                if (_parent != null)
                {
                    TiffArrayPixelBufferWriter<TPixel> parent = _parent;
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
