using System;
using System.Buffers;
using System.Diagnostics;

namespace TiffLibrary
{
    internal sealed class ArrayPoolBufferWriter : IBufferWriter<byte>, IDisposable
    {
        private sealed class BufferSegment : ReadOnlySequenceSegment<byte>
        {
            private byte[] _array;
            private BufferSegment _next;
            private int _consumed;

            public int Length => _consumed;

            public int AvailableLength => _array.Length - _consumed;

            public Memory<byte> AvailableMemory => _array.AsMemory(_consumed);

            public Span<byte> AvailableSpan => _array.AsSpan(_consumed);

            public BufferSegment NextSegment => _next;

            public BufferSegment(long runningIndex, int sizeHint)
            {
                _array = ArrayPool<byte>.Shared.Rent(Math.Max(sizeHint, 16384));
                _consumed = 0;

                Memory = default;
                RunningIndex = runningIndex;
            }

            public void Advance(int count)
            {
                Debug.Assert(count <= AvailableLength);

                _consumed += count;
                Memory = _array.AsMemory(0, _consumed);
            }

            public void SetNext(BufferSegment segment)
            {
                _next = segment;

                Next = segment;
            }

            public void CopyTo(Span<byte> destination)
            {
                _array.AsSpan(0, _consumed).CopyTo(destination);
            }

            public BufferSegment ReturnArray()
            {
                BufferSegment next = _next;

                ArrayPool<byte>.Shared.Return(_array);

                _array = null;
                _next = null;

                Memory = default;
                Next = default;

                return next;
            }
        }

        private BufferSegment _head;
        private BufferSegment _current;
        private int _length;

        public int Length => _length;

        private BufferSegment GetBufferSegment(int sizeHint)
        {
            BufferSegment current = _current;
            if (current is null)
            {
                _head = _current = current = new BufferSegment(0, sizeHint);
            }
            if (sizeHint < current.AvailableLength)
            {
                return current;
            }

            current = new BufferSegment(_length, sizeHint);
            _current.SetNext(current);
            _current = current;

            return current;
        }

        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            return GetBufferSegment(sizeHint).AvailableMemory;
        }

        public Span<byte> GetSpan(int sizeHint = 0)
        {
            return GetBufferSegment(sizeHint).AvailableSpan;
        }

        public void Advance(int count)
        {
            BufferSegment current = _current;
            if (current is null)
            {
                throw new InvalidOperationException();
            }
            if (count > current.AvailableLength)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            current.Advance(count);
            _length += count;
        }

        public byte[] ToArray()
        {
            int totalLength = 0;
            BufferSegment segment = _head;
            while (segment != null)
            {
                totalLength += segment.Length;
                segment = segment.NextSegment;
            }

            byte[] destination = new byte[totalLength];
            int offset = 0;
            segment = _head;
            while (segment != null)
            {
                segment.CopyTo(destination.AsSpan(offset));
                offset += segment.Length;
                segment = segment.NextSegment;
            }

            return destination;
        }

        public void CopyTo(Span<byte> destination)
        {
            BufferSegment segment = _head;
            while (segment != null)
            {
                segment.CopyTo(destination);
                destination = destination.Slice(segment.Length);
                segment = segment.NextSegment;
            }
        }

        public ReadOnlySequence<byte> GetReadOnlySequence()
        {
            if (_current is null)
            {
                return ReadOnlySequence<byte>.Empty;
            }

            return new ReadOnlySequence<byte>(_head, 0, _current, _current.Length);
        }

        public void Dispose()
        {
            BufferSegment segment = _head;
            while (segment != null)
            {
                segment = segment.ReturnArray();
            }
            _head = _current = null;
            _length = 0;
        }
    }
}
