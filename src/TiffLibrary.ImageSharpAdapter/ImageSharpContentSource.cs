using System;
using System.IO;
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

            public override ValueTask<int> ReadAsync(long offset, ArraySegment<byte> buffer)
            {
                Stream stream = _stream;
                if (stream is null)
                {
                    throw new ObjectDisposedException(nameof(ContentReader));
                }
                stream.Seek(offset, SeekOrigin.Begin);
                return new ValueTask<int>(stream.ReadAsync(buffer.Array, buffer.Offset, buffer.Count));
            }

        }
    }
}
