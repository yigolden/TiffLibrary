using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.PixelFormats;
using TiffLibrary.PixelBuffer;

namespace TiffLibrary.ImageSharpAdapter
{
    internal sealed class ImageSharpPixelBufferWriter<TImageSharpPixel, TTiffPixel> : ITiffPixelBufferWriter<TTiffPixel> where TImageSharpPixel : unmanaged, IPixel<TImageSharpPixel> where TTiffPixel : unmanaged
    {
        private readonly ImageFrame<TImageSharpPixel> _image;

        private RowSpanHandle? _cachedRowHandle;
        private ColumnSpanHandle? _cachedColHandle;

        public ImageSharpPixelBufferWriter(ImageFrame<TImageSharpPixel> image)
        {
            Debug.Assert(Unsafe.SizeOf<TImageSharpPixel>() == Unsafe.SizeOf<TTiffPixel>());
            _image = image;
        }

        public int Width => _image.Width;

        public int Height => _image.Height;

        public TiffPixelSpanHandle<TTiffPixel> GetRowSpan(int rowIndex, int start, int length)
        {
            int width = _image.Width;
            int height = _image.Height;
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
            handle.SetHandle(this, _image, rowIndex, start, length);
            return handle;
        }

        public TiffPixelSpanHandle<TTiffPixel> GetColumnSpan(int colIndex, int start, int length)
        {
            int width = _image.Width;
            int height = _image.Height;
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
            handle.SetHandle(this, _image, colIndex, start, length);
            return handle;
        }

        public void Dispose()
        {
            if (!(_cachedColHandle is null))
            {
                _cachedColHandle.ReleaseBuffer();
            }
        }


        #region RowSpanHandle

        private class RowSpanHandle : TiffPixelSpanHandle<TTiffPixel>
        {
            private ImageSharpPixelBufferWriter<TImageSharpPixel, TTiffPixel>? _parent;
            private ImageFrame<TImageSharpPixel>? _image;
            private int _rowIndex;
            private int _start;
            private int _length;

            internal void SetHandle(ImageSharpPixelBufferWriter<TImageSharpPixel, TTiffPixel> parent, ImageFrame<TImageSharpPixel> image, int rowIndex, int start, int length)
            {
                _parent = parent;
                _image = image;
                _rowIndex = rowIndex;
                _start = start;
                _length = length;
            }

            public override Span<TTiffPixel> GetSpan()
            {
                if (_image is null)
                {
                    throw new ObjectDisposedException(GetType().FullName);
                }
                return MemoryMarshal.Cast<TImageSharpPixel, TTiffPixel>(_image.GetPixelRowSpan(_rowIndex).Slice(_start, _length));
            }

            public override void Dispose()
            {
                _image = null;
                if (_parent != null)
                {
                    ImageSharpPixelBufferWriter<TImageSharpPixel, TTiffPixel> parent = _parent;
                    _parent = null;
                    Interlocked.CompareExchange(ref parent._cachedRowHandle, this, null);
                }
            }
        }

        #endregion

        #region ColumnSpanHandle

        private class ColumnSpanHandle : TiffPixelSpanHandle<TTiffPixel>
        {
            private ImageSharpPixelBufferWriter<TImageSharpPixel, TTiffPixel>? _parent;
            private ImageFrame<TImageSharpPixel>? _image;
            private byte[]? _buffer;
            private int _colIndex;
            private int _start;
            private int _length;

            internal void SetHandle(ImageSharpPixelBufferWriter<TImageSharpPixel, TTiffPixel> parent, ImageFrame<TImageSharpPixel> image, int colIndex, int start, int length)
            {
                _parent = parent;
                _image = image;
                _colIndex = colIndex;
                _start = start;
                _length = length;

                EnsureBufferSize(_length * Unsafe.SizeOf<TTiffPixel>());
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

            public override Span<TTiffPixel> GetSpan()
            {
                if (_image is null)
                {
                    throw new ObjectDisposedException(GetType().FullName);
                }
                return MemoryMarshal.Cast<byte, TTiffPixel>(_buffer.AsSpan(0, _length * Unsafe.SizeOf<TTiffPixel>()));
            }

            public override void Dispose()
            {
                ImageFrame<TImageSharpPixel>? image = _image;
                if (image is null)
                {
                    return;
                }
                IMemoryGroup<TImageSharpPixel> memoryGroup = image.GetPixelMemoryGroup();
                long totalLength = memoryGroup.TotalLength;

                // Copy pixels into this column
                int width = image.Width;
                int start = _start;
                int end = start + _length;
                int colIndex = _colIndex;
                int index = 0;
                Span<TImageSharpPixel> sourceSpan = MemoryMarshal.Cast<byte, TImageSharpPixel>(_buffer.AsSpan(0, _length * Unsafe.SizeOf<TTiffPixel>()));
                for (int rowIndex = start; rowIndex < end; rowIndex++)
                {
                    GetBoundedSlice(memoryGroup, totalLength, rowIndex * (long)width, width)[colIndex] = sourceSpan[index++];
                }

                _image = null;
                if (_parent != null)
                {
                    ImageSharpPixelBufferWriter<TImageSharpPixel, TTiffPixel> parent = _parent;
                    _parent = null;
                    if (Interlocked.CompareExchange(ref parent._cachedColHandle, this, null) != null)
                    {
                        ReleaseBuffer();
                    }
                }
            }

            internal static Span<T> GetBoundedSlice<T>(IMemoryGroup<T> group, long totalLength, long start, int length) where T : struct
            {
                Debug.Assert(!(group is null));
                Debug.Assert(group!.IsValid);
                Debug.Assert(length >= 0);
                Debug.Assert(start <= group.TotalLength);
                Debug.Assert(totalLength == group.TotalLength);

                int bufferIdx = (int)(start / totalLength);
                if (bufferIdx >= group.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(start));
                }

                int bufferStart = (int)(start % totalLength);
                int bufferEnd = bufferStart + length;
                Memory<T> memory = group[bufferIdx];

                if (bufferEnd > memory.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(length));
                }

                return memory.Span.Slice(bufferStart, length);
            }
        }

        #endregion
    }
}
