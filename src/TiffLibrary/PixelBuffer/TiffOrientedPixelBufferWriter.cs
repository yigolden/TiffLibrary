using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace TiffLibrary.PixelBuffer
{
    internal sealed class TiffOrientedPixelBufferWriter<TPixel> : ITiffPixelBufferWriter<TPixel> where TPixel : unmanaged
    {
        private TiffPixelBufferWriter<TPixel> _writer;
        private readonly bool _flipLeftRight;
        private readonly bool _flipTopBottom;

        private FlippedHandle? _cachedHandle;

        public TiffOrientedPixelBufferWriter(TiffPixelBufferWriter<TPixel> writer, bool flipLeftRight, bool flipTopBottom)
        {
            _writer = writer;
            _flipLeftRight = flipLeftRight;
            _flipTopBottom = flipTopBottom;
        }

        public int Width => _writer.Height;

        public int Height => _writer.Width;

        public void Dispose()
        {
            _cachedHandle = null;
            _writer.Dispose();
            _writer = default;
        }

        public TiffPixelSpanHandle<TPixel> GetRowSpan(int rowIndex, int start, int length)
        {
            int width = _writer.Width, height = _writer.Height;
            int colIndex = rowIndex;

            if ((uint)colIndex >= (uint)width)
            {
                throw new ArgumentOutOfRangeException(nameof(rowIndex));
            }
            if ((uint)start >= (uint)height)
            {
                throw new ArgumentOutOfRangeException(nameof(start));
            }
            if (length < 0 || (uint)(start + length) > (uint)height)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            if (_flipLeftRight)
            {
                colIndex = width - colIndex - 1;
            }

            if (!_flipTopBottom)
            {
                return _writer.GetColumnSpan(colIndex, start, length);
            }

            TiffPixelSpanHandle<TPixel> innerHandle = _writer.GetColumnSpan(colIndex, height - start - length, length);
            FlippedHandle? handle = Interlocked.Exchange(ref _cachedHandle, null);
            if (handle is null)
            {
                handle = new FlippedHandle();
            }
            handle.SetHandle(this, innerHandle);
            return handle;
        }


        public TiffPixelSpanHandle<TPixel> GetColumnSpan(int colIndex, int start, int length)
        {
            int width = _writer.Width, height = _writer.Height;
            int rowIndex = colIndex;

            if ((uint)rowIndex >= (uint)height)
            {
                throw new ArgumentOutOfRangeException(nameof(colIndex));
            }
            if ((uint)start >= (uint)width)
            {
                throw new ArgumentOutOfRangeException(nameof(start));
            }
            if (length < 0 || (uint)(start + length) > (uint)width)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            if (_flipTopBottom)
            {
                rowIndex = height - rowIndex - 1;
            }

            if (!_flipLeftRight)
            {
                return _writer.GetRowSpan(rowIndex, start, length);
            }

            TiffPixelSpanHandle<TPixel> innerHandle = _writer.GetRowSpan(rowIndex, width - start - length, length);
            FlippedHandle? handle = Interlocked.Exchange(ref _cachedHandle, null);
            if (handle is null)
            {
                handle = new FlippedHandle();
            }
            handle.SetHandle(this, innerHandle);
            return handle;
        }

        private class FlippedHandle : TiffPixelSpanHandle<TPixel>
        {
            private TiffOrientedPixelBufferWriter<TPixel>? _parent;
            private TiffPixelSpanHandle<TPixel>? _innerHandle;
            private int _length;

            internal void SetHandle(TiffOrientedPixelBufferWriter<TPixel> parent, TiffPixelSpanHandle<TPixel> handle)
            {
                _parent = parent;
                _innerHandle = handle;
                _length = handle.Length;
            }

            public override int Length => _length;

            public override Span<TPixel> GetSpan()
            {
                if (_innerHandle is null)
                {
                    throw new ObjectDisposedException(GetType().FullName);
                }

                return _innerHandle.GetSpan();
            }

            public override void Dispose()
            {
                if (_innerHandle is null)
                {
                    return;
                }

                Debug.Assert(_parent != null);
                _innerHandle.GetSpan().Reverse();
                _innerHandle.Dispose();
                TiffOrientedPixelBufferWriter<TPixel> parent = _parent!;
                _parent = null;
                _innerHandle = null;
                Interlocked.CompareExchange(ref parent._cachedHandle, this, null);
            }
        }

    }
}
