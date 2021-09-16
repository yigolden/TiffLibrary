using System;
using System.Buffers;
using System.Diagnostics;
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
        private ITiffPixelBufferWriter<TDestination>? _writer;
        private readonly bool _canInPlaceConvert;

        private ConverterHandle? _cachedHandle;

        /// <summary>
        /// Wraps <paramref name="writer"/> as the underlying storage.
        /// </summary>
        /// <param name="writer">The wrapped writer.</param>
        protected TiffPixelConverter(ITiffPixelBufferWriter<TDestination> writer) : this(writer, allowInPlaceConvert: Unsafe.SizeOf<TSource>() == Unsafe.SizeOf<TDestination>()) { }

        /// <summary>
        /// Wraps <paramref name="writer"/> as the underlying storage.
        /// </summary>
        /// <param name="writer">The wrapped writer.</param>
        /// <param name="allowInPlaceConvert">If the size of two pixel formats are the same, set this flag to allow conversion to happen on the same buffer without allocating temporary buffer.</param>
        protected TiffPixelConverter(ITiffPixelBufferWriter<TDestination> writer, bool allowInPlaceConvert)
        {
            ThrowHelper.ThrowIfNull(writer);
            _writer = writer;
            _canInPlaceConvert = allowInPlaceConvert && Unsafe.SizeOf<TSource>() == Unsafe.SizeOf<TDestination>();
        }

        /// <inheritdoc />
        public int Width => _writer?.Width ?? ThrowHelper.ThrowObjectDisposedException<int>(GetType().FullName);

        /// <inheritdoc />
        public int Height => _writer?.Height ?? ThrowHelper.ThrowObjectDisposedException<int>(GetType().FullName);

        /// <inheritdoc />
        public TiffPixelSpanHandle<TSource> GetRowSpan(int rowIndex, int start, int length)
        {
            if (_writer is null)
            {
                ThrowHelper.ThrowObjectDisposedException(GetType().FullName);
            }

            int width = _writer.Width;
            int height = _writer.Height;
            if ((uint)rowIndex >= (uint)height)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(rowIndex));
            }
            if ((uint)start > (uint)width)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(start));
            }
            if (length < 0 || (uint)(start + length) > (uint)width)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(length));
            }

            ConverterHandle? handle = Interlocked.Exchange(ref _cachedHandle, null);
            if (handle is null)
            {
                handle = new ConverterHandle();
            }
            handle.SetHandle(this, _writer.GetRowSpan(rowIndex, start, length), _canInPlaceConvert);
            return handle;
        }

        /// <inheritdoc />
        public TiffPixelSpanHandle<TSource> GetColumnSpan(int colIndex, int start, int length)
        {
            if (_writer is null)
            {
                ThrowHelper.ThrowObjectDisposedException(GetType().FullName);
            }

            int width = _writer.Width;
            int height = _writer.Height;
            if ((uint)colIndex >= (uint)width)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(colIndex));
            }
            if ((uint)start > (uint)height)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(start));
            }
            if (length < 0 || (uint)(start + length) > (uint)height)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(length));
            }

            ConverterHandle? handle = Interlocked.Exchange(ref _cachedHandle, null);
            if (handle is null)
            {
                handle = new ConverterHandle();
            }
            handle.SetHandle(this, _writer.GetColumnSpan(colIndex, start, length), _canInPlaceConvert);
            return handle;
        }

#pragma warning disable CA1816 // CA1816: Call GC.SuppressFinalize correctly
        /// <inheritdoc />
        public void Dispose()
#pragma warning restore CA1816 // CA1816: Call GC.SuppressFinalize correctly
        {
            if (_cachedHandle is not null)
            {
                _cachedHandle.ReleaseBuffer();
            }
            _cachedHandle = null;

            if (_writer is not null)
            {
                _writer.Dispose();
            }
            _writer = null;
        }

        /// <inheritdoc />
        public abstract void Convert(ReadOnlySpan<TSource> source, Span<TDestination> destination);

        #region ConverterHandle

        private class ConverterHandle : TiffPixelSpanHandle<TSource>
        {
            private TiffPixelConverter<TSource, TDestination>? _parent;
            private TiffPixelSpanHandle<TDestination>? _innerHandle;
            private byte[]? _buffer;
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
                if (_buffer is not null)
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
                    ThrowHelper.ThrowObjectDisposedException(GetType().FullName);
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

                Debug.Assert(_parent != null);
                Span<TDestination> destinationSpan = _innerHandle.GetSpan();
                if (_useInplaceConvert)
                {
                    Span<TSource> sourceSpan = MemoryMarshal.Cast<TDestination, TSource>(destinationSpan);
                    _parent!.Convert(sourceSpan, destinationSpan);
                }
                else
                {
                    Span<TSource> sourceSpan = MemoryMarshal.Cast<byte, TSource>(_buffer.AsSpan(0, _length * Unsafe.SizeOf<TSource>()));
                    _parent!.Convert(sourceSpan, destinationSpan);
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
