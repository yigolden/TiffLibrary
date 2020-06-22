using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TiffLibrary.ImageSharpAdapter
{
    internal sealed class SyncStreamWrapper : Stream
    {
        private readonly Stream _stream;

        public SyncStreamWrapper(Stream stream)
        {
            _stream = stream;
        }

        public override bool CanRead => _stream.CanRead;

        public override bool CanSeek => _stream.CanSeek;

        public override bool CanWrite => _stream.CanWrite;

        public override long Length => _stream.Length;

        public override long Position { get => _stream.Position; set => _stream.Position = Position; }

        public override bool CanTimeout => _stream.CanTimeout;

        public override int ReadTimeout { get => _stream.ReadTimeout; set => _stream.ReadTimeout = value; }

        public override int WriteTimeout { get => _stream.WriteTimeout; set => _stream.WriteTimeout = value; }

        public override void Flush() => _stream.Flush();

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            _stream.Flush();
            return Task.CompletedTask;
        }

        public override int Read(byte[] buffer, int offset, int count)
            => _stream.Read(buffer, offset, count);

        public override int ReadByte()
            => _stream.ReadByte();

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => Task.FromResult(_stream.Read(buffer, offset, count));

        public override long Seek(long offset, SeekOrigin origin)
            => _stream.Seek(offset, origin);

        public override void SetLength(long value)
            => _stream.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count)
            => _stream.Write(buffer, offset, count);

        public override void WriteByte(byte value)
            => _stream.WriteByte(value);

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            _stream.Write(buffer, offset, count);
            return Task.CompletedTask;
        }

#if !NO_FAST_SPAN

        public override int Read(Span<byte> buffer)
            => _stream.Read(buffer);

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return new ValueTask<int>(_stream.Read(buffer.Span));
        }

        public override void Write(ReadOnlySpan<byte> buffer)
            => _stream.Write(buffer);

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            _stream.Write(buffer.Span);
            return default;
        }

#endif

    }
}
