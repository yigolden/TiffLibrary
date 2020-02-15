using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using TiffLibrary.PixelBuffer;

namespace TiffLibrary.ImageSharpAdapter
{
    internal sealed class TiffPassthroughPixelBufferWriter<TSource, TDestination> : ITiffPixelBufferWriter<TSource> where TSource : unmanaged where TDestination : unmanaged
    {
        private readonly ITiffPixelBufferWriter<TDestination> _writer;

        private PassthoughRowSpanHandle? _cachedHandle;

        public TiffPassthroughPixelBufferWriter(ITiffPixelBufferWriter<TDestination> writer)
        {
            Debug.Assert(Unsafe.SizeOf<TSource>() == Unsafe.SizeOf<TDestination>());
            _writer = writer;
        }

        public int Width => _writer.Width;

        public int Height => _writer.Height;

        public TiffPixelSpanHandle<TSource> GetRowSpan(int rowIndex, int start, int length)
        {
            PassthoughRowSpanHandle? handle = Interlocked.Exchange(ref _cachedHandle, null);
            if (handle is null)
            {
                handle = new PassthoughRowSpanHandle();
            }
            handle.SetHandle(this, _writer.GetRowSpan(rowIndex, start, length), length);
            return handle;
        }

        public TiffPixelSpanHandle<TSource> GetColumnSpan(int colIndex, int start, int length)
        {
            PassthoughRowSpanHandle? handle = Interlocked.Exchange(ref _cachedHandle, null);
            if (handle is null)
            {
                handle = new PassthoughRowSpanHandle();
            }
            handle.SetHandle(this, _writer.GetColumnSpan(colIndex, start, length), length);
            return handle;
        }


        public void Dispose()
        {
            _cachedHandle = null;
        }

        private class PassthoughRowSpanHandle : TiffPixelSpanHandle<TSource>
        {
            private TiffPassthroughPixelBufferWriter<TSource, TDestination>? _parent;
            private TiffPixelSpanHandle<TDestination>? _innerHandle;
            private int _length;

            internal void SetHandle(TiffPassthroughPixelBufferWriter<TSource, TDestination> parent, TiffPixelSpanHandle<TDestination> handle, int length)
            {
                _parent = parent;
                _innerHandle = handle;
                _length = length;
            }

            public override int Length => _length;

            public override Span<TSource> GetSpan()
            {
                if (_innerHandle is null)
                {
                    throw new ObjectDisposedException(GetType().FullName);
                }
                return MemoryMarshal.Cast<TDestination, TSource>(_innerHandle.GetSpan());
            }

            public override void Dispose()
            {
                _innerHandle?.Dispose();
                _innerHandle = null;
                if (_parent != null)
                {
                    TiffPassthroughPixelBufferWriter<TSource, TDestination> parent = _parent;
                    _parent = null;
                    Interlocked.CompareExchange(ref parent._cachedHandle, this, null);
                }
            }
        }
    }
}
