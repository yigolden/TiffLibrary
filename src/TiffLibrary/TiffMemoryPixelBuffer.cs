using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TiffLibrary
{
    /// <summary>
    /// A 2-dimensional region of pixels in a contiguous memory buffer in row-major order.
    /// </summary>
    /// <typeparam name="TPixel"></typeparam>
    public sealed class TiffMemoryPixelBuffer<TPixel> : ITiffPixelBuffer<TPixel> where TPixel : unmanaged
    {
        private readonly Memory<TPixel> _buffer;
        private readonly int _width;
        private readonly int _height;
        private readonly bool _readonly;

        /// <summary>
        /// Initialize the region with the specified <see cref="ReadOnlyMemory{TPixel}"/>.
        /// </summary>
        /// <param name="buffer">The memory buffer.</param>
        /// <param name="width">The width of the region.</param>
        /// <param name="height">The height of the region.</param>
        public TiffMemoryPixelBuffer(ReadOnlyMemory<TPixel> buffer, int width, int height)
        {
            if (width < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(width));
            }
            if (height < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(height));
            }
            if (buffer.Length < width * height)
            {
                ThrowHelper.ThrowArgumentException("buffer is too small.");
            }
            _buffer = MemoryMarshal.AsMemory(buffer).Slice(0, width * height);
            _width = width;
            _height = height;
            _readonly = true;
        }

        /// <summary>
        /// Initialize the region with the specified <see cref="Memory{TPixel}"/>.
        /// </summary>
        /// <param name="buffer">The memory buffer.</param>
        /// <param name="width">The width of the region.</param>
        /// <param name="height">The height of the region.</param>
        /// <param name="writable">Whether this pixel buffer is writable.</param>
        public TiffMemoryPixelBuffer(Memory<TPixel> buffer, int width, int height, bool writable)
        {
            if (width < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(width));
            }
            if (height < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(height));
            }
            if (buffer.Length < width * height)
            {
                ThrowHelper.ThrowArgumentException("buffer is too small.");
            }
            _buffer = buffer.Slice(0, width * height);
            _width = width;
            _height = height;
            _readonly = !writable;
        }

        /// <inheritdoc />
        public int Width => _width;

        /// <inheritdoc />
        public int Height => _height;

        /// <inheritdoc />
        public Span<TPixel> GetSpan()
        {
            if (_readonly)
            {
                ThrowWriteToReadOnlyPixelBuffer();
            }
            return _buffer.Span.Slice(0, _width * _height);
        }

        /// <inheritdoc />
        public ReadOnlySpan<TPixel> GetReadOnlySpan()
        {
            return _buffer.Span.Slice(0, _width * _height);
        }

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowWriteToReadOnlyPixelBuffer()
        {
            ThrowHelper.ThrowInvalidOperationException("Can not write to a read-only pixel buffer.");
        }
    }
}
