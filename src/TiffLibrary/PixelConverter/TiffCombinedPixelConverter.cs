using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TiffLibrary.PixelConverter
{
    internal class TiffCombinedPixelConverter<TSource, TIntermediate, TDestination> : TiffPixelConverter<TSource, TDestination>
        where TSource : unmanaged where TIntermediate : unmanaged where TDestination : unmanaged
    {
        private readonly ITiffPixelSpanConverter<TSource, TIntermediate> _converter1;
        private readonly ITiffPixelSpanConverter<TIntermediate, TDestination> _converter2;
        private readonly bool _canInPlaceConvert;

        private byte[] _writer;

        public TiffCombinedPixelConverter(ITiffPixelBufferWriter<TDestination> writer, ITiffPixelSpanConverter<TSource, TIntermediate> converter1, ITiffPixelSpanConverter<TIntermediate, TDestination> converter2, bool allowInPlaceConvert = true)
            : base(writer, allowInPlaceConvert && Unsafe.SizeOf<TSource>() == Unsafe.SizeOf<TIntermediate>())
        {
            ThrowHelper.ThrowIfNull(converter1);
            ThrowHelper.ThrowIfNull(converter2);
            _converter1 = converter1;
            _converter2 = converter2;
            _canInPlaceConvert = allowInPlaceConvert && Unsafe.SizeOf<TSource>() == Unsafe.SizeOf<TIntermediate>() && Unsafe.SizeOf<TIntermediate>() == Unsafe.SizeOf<TDestination>();

            // Temparary buffer
            _writer = ArrayPool<byte>.Shared.Rent(writer.Width * Unsafe.SizeOf<TIntermediate>());
        }

        public override void Convert(ReadOnlySpan<TSource> source, Span<TDestination> destination)
        {
            if (_writer is null)
            {
                ThrowHelper.ThrowObjectDisposedException(GetType().FullName);
            }

            if (_canInPlaceConvert)
            {
                _converter1.Convert(source, MemoryMarshal.Cast<TDestination, TIntermediate>(destination));
                _converter2.Convert(MemoryMarshal.Cast<TDestination, TIntermediate>(destination), destination);
                return;
            }

            int bufferLength = source.Length * Unsafe.SizeOf<TIntermediate>();
            if (_writer.Length >= bufferLength)
            {
                Convert(source, MemoryMarshal.Cast<byte, TIntermediate>(_writer.AsSpan(0, bufferLength)), destination);
                return;
            }

            byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferLength);
            try
            {
                Convert(source, MemoryMarshal.Cast<byte, TIntermediate>(buffer.AsSpan(0, bufferLength)), destination);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        protected void Convert(ReadOnlySpan<TSource> source, Span<TIntermediate> intermediateBuffer, Span<TDestination> destination)
        {
            _converter1.Convert(source, intermediateBuffer);
            _converter2.Convert(intermediateBuffer, destination);
        }

    }

}
