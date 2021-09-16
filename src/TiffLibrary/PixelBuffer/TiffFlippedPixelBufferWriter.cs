using System;
using System.Diagnostics;
using System.Threading;

namespace TiffLibrary.PixelBuffer
{
    internal sealed class TiffFlippedPixelBufferWriter<TPixel> : ITiffPixelBufferWriter<TPixel> where TPixel : unmanaged
    {
        private TiffPixelBufferWriter<TPixel> _writer;
        private readonly bool _flipLeftRight;
        private readonly bool _flipTopBottom;

        private FlippedHandle? _cachedHandle;

        public TiffFlippedPixelBufferWriter(TiffPixelBufferWriter<TPixel> writer, bool flipLeftRight, bool flipTopBottom)
        {
            _writer = writer;
            _flipLeftRight = flipLeftRight;
            _flipTopBottom = flipTopBottom;
        }

        public int Width => _writer.Width;

        public int Height => _writer.Height;

        public void Dispose()
        {
            _cachedHandle = null;
            _writer.Dispose();
            _writer = default;
        }

        public TiffPixelSpanHandle<TPixel> GetRowSpan(int rowIndex, int start, int length)
        {
            int width = _writer.Width, height = _writer.Height;
            if ((uint)rowIndex >= (uint)height)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(rowIndex));
            }
            if ((uint)start >= (uint)width)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(start));
            }
            if (length < 0 || (uint)(start + length) > (uint)width)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(length));
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


        public TiffPixelSpanHandle<TPixel> GetColumnSpan(int colIndex, int start, int length)
        {
            int width = _writer.Width, height = _writer.Height;
            if ((uint)colIndex >= (uint)width)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(colIndex));
            }
            if ((uint)start >= (uint)height)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(start));
            }
            if (length < 0 || (uint)(start + length) > (uint)height)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(length));
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

        private class FlippedHandle : TiffPixelSpanHandle<TPixel>
        {
            private TiffFlippedPixelBufferWriter<TPixel>? _parent;
            private TiffPixelSpanHandle<TPixel>? _innerHandle;
            private int _length;

            internal void SetHandle(TiffFlippedPixelBufferWriter<TPixel> parent, TiffPixelSpanHandle<TPixel> handle)
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
                    ThrowHelper.ThrowObjectDisposedException(GetType().FullName);
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
                TiffFlippedPixelBufferWriter<TPixel> parent = _parent!;
                _parent = null;
                _innerHandle = null;
                Interlocked.CompareExchange(ref parent._cachedHandle, this, null);
            }

        }
    }
}
