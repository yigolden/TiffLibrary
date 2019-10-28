using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace TiffLibrary.ImageSharpAdapter
{
    internal class ImageSharpContentSource : TiffFileContentSource
    {
        private Stream _stream;
        private ContentReader _reader;

        public ImageSharpContentSource(Stream stream)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            _reader = new ContentReader(stream);
        }

        public override ValueTask<TiffFileContentReader> OpenReaderAsync()
        {
            return new ValueTask<TiffFileContentReader>(_reader);
        }

        public ValueTask<int> ReadAsync(long offset, ArraySegment<byte> buffer)
        {
            _stream.Seek(offset, SeekOrigin.Begin);
            return new ValueTask<int>(_stream.Read(buffer.Array, buffer.Offset, buffer.Count));
        }

        protected override void Dispose(bool disposing)
        {
            // Noop
        }

        internal sealed class ContentReader : TiffFileContentReader
        {
            private readonly Stream _stream;

            public ContentReader(Stream stream)
            {
                _stream = stream;
            }

            public override ValueTask DisposeAsync()
            {
                return default;
            }

            protected override void Dispose(bool disposing)
            {
                // Noop
            }

            public override ValueTask<int> ReadAsync(long offset, ArraySegment<byte> buffer, CancellationToken cancellationToken)
            {
                Stream stream = _stream;
                if (stream is null)
                {
                    throw new ObjectDisposedException(nameof(ContentReader));
                }
                stream.Seek(offset, SeekOrigin.Begin);
                return new ValueTask<int>(stream.ReadAsync(buffer.Array, buffer.Offset, buffer.Count, cancellationToken));
            }

            public override ValueTask<int> ReadAsync(long offset, Memory<byte> buffer, CancellationToken cancellationToken)
            {
#if !NO_FAST_SPAN
                Stream stream = _stream;
                if (stream is null)
                {
                    throw new ObjectDisposedException(nameof(ContentReader));
                }
                stream.Seek(offset, SeekOrigin.Begin);
                return stream.ReadAsync(buffer, cancellationToken);
#else
                return new ValueTask<int>(ReadFromStreamSlow());

                async Task<int> ReadFromStreamSlow()
                {
                    Stream stream = _stream;
                    if (stream is null)
                    {
                        throw new ObjectDisposedException(nameof(ContentReader));
                    }
                    stream.Seek(offset, SeekOrigin.Begin);
                    if (MemoryMarshal.TryGetArray(buffer, out ArraySegment<byte> arraySegment))
                    {
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
                }
#endif
            }

        }
    }
}
