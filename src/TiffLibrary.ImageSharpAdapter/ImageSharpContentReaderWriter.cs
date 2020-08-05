using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;

namespace TiffLibrary.ImageSharpAdapter
{
    internal class ImageSharpContentReaderWriter : TiffFileContentReaderWriter
    {
        private readonly Stream _stream;

        public ImageSharpContentReaderWriter(Stream stream)
        {
            _stream = stream;
        }

        public override void Flush()
        {
            _stream.Flush();
        }

        public override int Read(TiffStreamOffset offset, Memory<byte> buffer)
        {
            Stream stream = _stream;
            if (stream is null)
            {
                throw new ObjectDisposedException(nameof(ImageSharpContentReaderWriter));
            }
            if (offset.Offset > stream.Length)
            {
                return 0;
            }

            stream.Seek(offset.Offset, SeekOrigin.Begin);

            if (MemoryMarshal.TryGetArray(buffer, out ArraySegment<byte> arraySegment))
            {
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

        public override void Write(TiffStreamOffset offset, ReadOnlyMemory<byte> buffer)
        {
            Stream stream = _stream;
            if (stream is null)
            {
                throw new ObjectDisposedException(nameof(ImageSharpContentReaderWriter));
            }

            stream.Seek(offset.Offset, SeekOrigin.Begin);

            if (MemoryMarshal.TryGetArray(buffer, out ArraySegment<byte> arraySegment))
            {
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

        protected override void Dispose(bool disposing) { }
    }
}
