using System;
using System.Threading.Tasks;

namespace TiffLibrary
{
    internal sealed class TiffMemoryContentSource : TiffFileContentSource
    {
        private ContentReader _reader;

        public TiffMemoryContentSource(ReadOnlyMemory<byte> memory)
        {
            _reader = new ContentReader(memory);
        }

        public override ValueTask<TiffFileContentReader> OpenReaderAsync()
        {
            if (_reader is null)
            {
                throw new ObjectDisposedException(nameof(TiffMemoryContentSource));
            }
            return new ValueTask<TiffFileContentReader>(_reader);
        }

        protected override void Dispose(bool disposing)
        {
            _reader = null;
        }

        internal sealed class ContentReader : TiffFileContentReader
        {
            private ReadOnlyMemory<byte> _memory;

            public ContentReader(ReadOnlyMemory<byte> memory)
            {
                _memory = memory;
            }

            public override ValueTask DisposeAsync()
            {
                // Noop
                return default;
            }

            protected override void Dispose(bool disposing)
            {
                // Noop
            }

            public override ValueTask<int> ReadAsync(long offset, ArraySegment<byte> buffer)
            {
                int offset32 = checked((int)offset);
                ReadOnlySpan<byte> span = _memory.Span.Slice(offset32);
                span = span.Slice(0, Math.Min(buffer.Count, span.Length));
                span.CopyTo(buffer.AsSpan());
                return new ValueTask<int>(span.Length);
            }

            public override ValueTask<int> ReadAsync(long offset, Memory<byte> buffer)
            {
                int offset32 = checked((int)offset);
                ReadOnlySpan<byte> span = _memory.Span.Slice(offset32);
                span = span.Slice(0, Math.Min(buffer.Length, span.Length));
                span.CopyTo(buffer.Span);
                return new ValueTask<int>(span.Length);
            }
        }
    }
}
