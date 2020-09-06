using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace TiffLibrary
{
    internal sealed class TiffStreamContentReaderWriter : TiffFileContentReaderWriter
    {
        private Stream? _stream;
        private readonly bool _leaveOpen;

        public TiffStreamContentReaderWriter(Stream stream, bool leaveOpen)
        {
            _stream = stream;
            _leaveOpen = leaveOpen;
        }

        public override int Read(TiffStreamOffset offset, Memory<byte> buffer)
        {
            Stream? stream = _stream;
            if (stream is null)
            {
                throw new ObjectDisposedException(nameof(TiffStreamContentReaderWriter));
            }
            if (offset.Offset > stream.Length)
            {
                return 0;
            }

            stream.Seek(offset.Offset, SeekOrigin.Begin);

            if (MemoryMarshal.TryGetArray(buffer, out ArraySegment<byte> arraySegment))
            {
                if (arraySegment.Array is null)
                {
                    return 0;
                }

                return stream.Read(arraySegment.Array, arraySegment.Offset, arraySegment.Count);
            }

#if !NO_FAST_SPAN
            return stream.Read(buffer.Span);
#else
            // Slow path
            byte[] temp = ArrayPool<byte>.Shared.Rent(buffer.Length);
            try
            {
                int count = stream.Read(temp, 0, buffer.Length);
                temp.AsMemory(0, count).CopyTo(buffer);
                return count;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(temp);
            }
#endif
        }

        public override int Read(TiffStreamOffset offset, ArraySegment<byte> buffer)
        {
            Stream? stream = _stream;
            if (stream is null)
            {
                throw new ObjectDisposedException(nameof(TiffStreamContentReaderWriter));
            }
            if (offset.Offset > stream.Length)
            {
                return default;
            }
            if (buffer.Array is null)
            {
                return 0;
            }

            stream.Seek(offset.Offset, SeekOrigin.Begin);
            return stream.Read(buffer.Array, buffer.Offset, buffer.Count);
        }

        public override async ValueTask<int> ReadAsync(TiffStreamOffset offset, Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            Stream? stream = _stream;
            if (stream is null)
            {
                throw new ObjectDisposedException(nameof(TiffStreamContentReaderWriter));
            }
            if (offset.Offset > stream.Length)
            {
                return 0;
            }


            stream.Seek(offset.Offset, SeekOrigin.Begin);

#if !NO_FAST_SPAN
            return await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
#else
            if (MemoryMarshal.TryGetArray(buffer, out ArraySegment<byte> arraySegment))
            {
                if (arraySegment.Array is null)
                {
                    return 0;
                }

                return await stream.ReadAsync(arraySegment.Array, arraySegment.Offset, arraySegment.Count, cancellationToken).ConfigureAwait(false);
            }

            // Slow path
            byte[] temp = ArrayPool<byte>.Shared.Rent(buffer.Length);
            try
            {
                int count = await stream.ReadAsync(temp, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
                temp.AsMemory(0, count).CopyTo(buffer);
                return count;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(temp);
            }
#endif
        }

        public override async ValueTask<int> ReadAsync(TiffStreamOffset offset, ArraySegment<byte> buffer, CancellationToken cancellationToken = default)
        {
            Stream? stream = _stream;
            if (stream is null)
            {
                throw new ObjectDisposedException(nameof(TiffStreamContentReaderWriter));
            }
            if (offset.Offset > stream.Length)
            {
                return default;
            }
            if (buffer.Array is null)
            {
                return 0;
            }

            stream.Seek(offset.Offset, SeekOrigin.Begin);
#if !NO_FAST_SPAN
            return await stream.ReadAsync(buffer.AsMemory(), cancellationToken).ConfigureAwait(false);
#else
            return await stream.ReadAsync(buffer.Array, buffer.Offset, buffer.Count, cancellationToken).ConfigureAwait(false);
#endif
        }

        public override void Write(TiffStreamOffset offset, ArraySegment<byte> buffer)
        {
            Stream? stream = _stream;
            if (stream is null)
            {
                throw new ObjectDisposedException(nameof(TiffStreamContentReaderWriter));
            }
            if (buffer.Array is null)
            {
                return;
            }

            stream.Seek(offset, SeekOrigin.Begin);
            stream.Write(buffer.Array, buffer.Offset, buffer.Count);
        }

        public override void Write(TiffStreamOffset offset, ReadOnlyMemory<byte> buffer)
        {
            Stream? stream = _stream;
            if (stream is null)
            {
                throw new ObjectDisposedException(nameof(TiffStreamContentReaderWriter));
            }

            stream.Seek(offset, SeekOrigin.Begin);

            if (MemoryMarshal.TryGetArray(buffer, out ArraySegment<byte> arraySegment))
            {
                if (arraySegment.Array is null)
                {
                    return;
                }

                stream.Write(arraySegment.Array, arraySegment.Offset, arraySegment.Count);
                return;
            }

#if !NO_FAST_SPAN
            stream.Write(buffer.Span);
#else
            // Slow path
            byte[] temp = ArrayPool<byte>.Shared.Rent(buffer.Length);
            try
            {
                buffer.CopyTo(temp);
                stream.Write(temp, 0, buffer.Length);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(temp);
            }
#endif
        }

        public override async ValueTask WriteAsync(TiffStreamOffset offset, ArraySegment<byte> buffer, CancellationToken cancellationToken)
        {
            Stream? stream = _stream;
            if (stream is null)
            {
                throw new ObjectDisposedException(nameof(TiffStreamContentReaderWriter));
            }
            if (buffer.Array is null)
            {
                return;
            }

            stream.Seek(offset, SeekOrigin.Begin);
#if !NO_FAST_SPAN
            await stream.WriteAsync(buffer.AsMemory(), cancellationToken).ConfigureAwait(false);
#else
            await stream.WriteAsync(buffer.Array, buffer.Offset, buffer.Count, cancellationToken).ConfigureAwait(false);
#endif
        }


        public override async ValueTask WriteAsync(TiffStreamOffset offset, ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
        {
            Stream? stream = _stream;
            if (stream is null)
            {
                throw new ObjectDisposedException(nameof(TiffStreamContentReaderWriter));
            }

            stream.Seek(offset, SeekOrigin.Begin);

#if !NO_FAST_SPAN
            await stream.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
#else
            if (MemoryMarshal.TryGetArray(buffer, out ArraySegment<byte> arraySegment))
            {
                if (arraySegment.Array is null)
                {
                    return;
                }

                await stream.WriteAsync(arraySegment.Array, arraySegment.Offset, arraySegment.Count, cancellationToken).ConfigureAwait(false);
                return;
            }

            // Slow path
            byte[] temp = ArrayPool<byte>.Shared.Rent(buffer.Length);
            try
            {
                buffer.CopyTo(temp);
                await stream.WriteAsync(temp, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(temp);
            }
#endif
        }

        public override void Flush()
        {
            Stream? stream = _stream;
            if (stream is null)
            {
                throw new ObjectDisposedException(nameof(TiffStreamContentReaderWriter));
            }

            stream.Flush();
        }

        public override async ValueTask FlushAsync(CancellationToken cancellationToken)
        {
            Stream? stream = _stream;
            if (stream is null)
            {
                throw new ObjectDisposedException(nameof(TiffStreamContentReaderWriter));
            }

            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_leaveOpen && !(_stream is null))
            {
                if (disposing)
                {
                    _stream.Dispose();
                }
                _stream = null;
            }
        }

        public override async ValueTask DisposeAsync()
        {
            if (!_leaveOpen && !(_stream is null))
            {
                await _stream.DisposeAsync().ConfigureAwait(false);
                _stream = null;
            }
            await base.DisposeAsync().ConfigureAwait(false);
        }
    }
}
