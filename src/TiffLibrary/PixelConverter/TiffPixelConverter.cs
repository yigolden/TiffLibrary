using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using TiffLibrary.PixelBuffer;

namespace TiffLibrary.PixelConverter
{
    /// <summary>
    /// A special kind of <see cref="ITiffPixelBufferWriter{TPixel}"/> implementation that can be useed to convert pixels from one format to another format.
    /// </summary>
    /// <typeparam name="TSource">The pixel format to convert from.</typeparam>
    /// <typeparam name="TDestination">The pixel format to convert to.</typeparam>
    [SuppressMessage("Design", "CA1063: Implement IDisposable correctly", Justification = "The semantics of Dispose method is different.")]
    public abstract class TiffPixelConverter<TSource, TDestination> : ITiffPixelBufferWriter<TSource>, ITiffPixelSpanConverter<TSource, TDestination>
        where TSource : unmanaged where TDestination : unmanaged
    {
        private ITiffPixelBufferWriter<TDestination> _writer;
        private readonly bool _canInPlaceConvert;

        private ConverterHandle _cachedHandle;

        /// <summary>
        /// Wraps <paramref name="writer"/> as the underlying storage.
        /// </summary>
        /// <param name="writer">The wrapped writer.</param>
        public TiffPixelConverter(ITiffPixelBufferWriter<TDestination> writer) : this(writer, allowInPlaceConvert: Unsafe.SizeOf<TSource>() == Unsafe.SizeOf<TDestination>()) { }

        /// <summary>
        /// Wraps <paramref name="writer"/> as the underlying storage.
        /// </summary>
        /// <param name="writer">The wrapped writer.</param>
        /// <param name="allowInPlaceConvert">If the size of two pixel formats are the same, set this flag to allow conversion to happen on the same buffer without allocating temporary buffer.</param>
        public TiffPixelConverter(ITiffPixelBufferWriter<TDestination> writer, bool allowInPlaceConvert)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _canInPlaceConvert = allowInPlaceConvert && Unsafe.SizeOf<TSource>() == Unsafe.SizeOf<TDestination>();
        }

        /// <summary>
        /// The number of columns in the region.
        /// </summary>
        public int Width => _writer.Width;

        /// <summary>
        /// The number of rows in the region.
        /// </summary>
        public int Height => _writer.Height;

        /// <summary>
        /// Gets a <see cref="TiffPixelSpanHandle{TPixel}"/> representing a write-only span of the <paramref name="rowIndex"/> row of the region, while skipping <paramref name="start"/> pixels and limiting the length of the span to <paramref name="length"/>. <see cref="TiffPixelSpanHandle{TPixel}"/> should be disposed after use to flush the pixels into the underlying storage.
        /// </summary>
        /// <param name="rowIndex">The row index.</param>
        /// <param name="start">Number of pixels to skip in this row.</param>
        /// <param name="length">Number of pixels to write.</param>
        /// <returns>A <see cref="TiffPixelSpanHandle{TPixel}"/> representing the temporary buffer consumer can write to. When disposed, it will flush the temporary pixels into the actual storage.</returns>
        public TiffPixelSpanHandle<TSource> GetRowSpan(int rowIndex, int start, int length)
        {
            int width = _writer.Width;
            int height = _writer.Height;
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

            ConverterHandle handle = Interlocked.Exchange(ref _cachedHandle, null);
            if (handle is null)
            {
                handle = new ConverterHandle();
            }
            handle.SetHandle(this, _writer.GetRowSpan(rowIndex, start, length), _canInPlaceConvert);
            return handle;
        }

        /// <summary>
        /// Gets a <see cref="TiffPixelSpanHandle{TPixel}"/> representing a write-only span of the <paramref name="colIndex"/> column of the region, while skipping <paramref name="start"/> pixels and limiting the length of the span to <paramref name="length"/>. <see cref="TiffPixelSpanHandle{TPixel}"/> should be disposed after use to flush the pixels into the underlying storage.
        /// </summary>
        /// <param name="colIndex">The column index.</param>
        /// <param name="start">Number of pixels to skip in this column.</param>
        /// <param name="length">Number of pixels to write.</param>
        /// <returns>A <see cref="TiffPixelSpanHandle{TPixel}"/> representing the temporary buffer consumer can write to. When disposed, it will flush the temporary pixels into the actual storage.</returns>
        public TiffPixelSpanHandle<TSource> GetColumnSpan(int colIndex, int start, int length)
        {
            int width = _writer.Width;
            int height = _writer.Height;
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

            ConverterHandle handle = Interlocked.Exchange(ref _cachedHandle, null);
            if (handle is null)
            {
                handle = new ConverterHandle();
            }
            handle.SetHandle(this, _writer.GetColumnSpan(colIndex, start, length), _canInPlaceConvert);
            return handle;
        }

#pragma warning disable CA1816 // CA1816: Call GC.SuppressFinalize correctly
        /// <summary>
        /// Release the allocated resources.
        /// </summary>
        public void Dispose()
#pragma warning restore CA1816 // CA1816: Call GC.SuppressFinalize correctly
        {
            if (!(_cachedHandle is null))
            {
                _cachedHandle.ReleaseBuffer();
            }
            _cachedHandle = null;

            if (!(_writer is null))
            {
                _writer.Dispose();
            }
            _writer = null;
        }

        /// <summary>
        /// Method to convert from one pixel format to another.
        /// </summary>
        /// <param name="source">The source pixel span.</param>
        /// <param name="destination">The destination pixel span.</param>
        public abstract void Convert(ReadOnlySpan<TSource> source, Span<TDestination> destination);

        #region ConverterHandle

        private class ConverterHandle : TiffPixelSpanHandle<TSource>
        {
            private TiffPixelConverter<TSource, TDestination> _parent;
            private TiffPixelSpanHandle<TDestination> _innerHandle;
            private byte[] _buffer;
            private int _length;
            private bool _useInplaceConvert;

            internal void SetHandle(TiffPixelConverter<TSource, TDestination> parent, TiffPixelSpanHandle<TDestination> handle, bool useInplaceConvert)
            {
                _parent = parent;
                _innerHandle = handle;
                _length = handle.Length;
                _useInplaceConvert = useInplaceConvert;

                if (!useInplaceConvert)
                {
                    EnsureBufferSize(_length * Unsafe.SizeOf<TSource>());
                }
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

            public override int Length => _length;

            public override Span<TSource> GetSpan()
            {
                if (_innerHandle is null)
                {
                    throw new ObjectDisposedException(GetType().FullName);
                }
                if (_useInplaceConvert)
                {
                    return MemoryMarshal.Cast<TDestination, TSource>(_innerHandle.GetSpan());
                }
                return MemoryMarshal.Cast<byte, TSource>(_buffer.AsSpan(0, _length * Unsafe.SizeOf<TSource>()));
            }

            public override void Dispose()
            {
                if (_innerHandle is null)
                {
                    return;
                }

                Span<TDestination> destinationSpan = _innerHandle.GetSpan();
                if (_useInplaceConvert)
                {
                    Span<TSource> sourceSpan = MemoryMarshal.Cast<TDestination, TSource>(destinationSpan);
                    _parent.Convert(sourceSpan, destinationSpan);
                }
                else
                {
                    Span<TSource> sourceSpan = MemoryMarshal.Cast<byte, TSource>(_buffer.AsSpan(0, _length * Unsafe.SizeOf<TSource>()));
                    _parent.Convert(sourceSpan, destinationSpan);
                }

                _innerHandle.Dispose();
                TiffPixelConverter<TSource, TDestination> parent = _parent;
                _parent = null;
                _innerHandle = null;
                if (Interlocked.CompareExchange(ref parent._cachedHandle, this, null) != null)
                {
                    ReleaseBuffer();
                }
            }
        }

        #endregion

    }
}
