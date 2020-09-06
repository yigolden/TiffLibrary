using System;
using System.Threading;

namespace TiffLibrary
{
    internal class TiffSyncFileContentSource : TiffFileContentSource
    {
        private ITiffFileContentSource? _contentSource;

        public TiffSyncFileContentSource(ITiffFileContentSource contentSource)
        {
            _contentSource = contentSource;
        }

        public override TiffFileContentReader OpenReader()
        {
            ITiffFileContentSource? contentSource = _contentSource;
            if (contentSource is null)
            {
                throw new ObjectDisposedException(nameof(TiffFileContentSource));
            }
            return new SyncContentReader(contentSource.OpenReader());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _contentSource?.Dispose();
            }
            _contentSource = null;
        }

        public static TiffFileContentSource WrapSource(ITiffFileContentSource source)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (source is TiffSyncFileContentSource syncSource)
            {
                return syncSource;
            }
            return new TiffSyncFileContentSource(source);
        }

        public static TiffFileContentReader WrapReader(TiffFileContentReader reader)
        {
            if (reader is null)
            {
                throw new ArgumentNullException(nameof(reader));
            }
            if (reader is SyncContentReader)
            {
                return reader;
            }
            return new SyncContentReader(reader);
        }

        private sealed class SyncContentReader : TiffFileContentReader
        {
            private TiffFileContentReader? _reader;

            public SyncContentReader(TiffFileContentReader reader)
            {
                _reader = reader;
            }

            public override int Read(TiffStreamOffset offset, Memory<byte> buffer)
            {
                TiffFileContentReader? reader = _reader;
                if (reader is null)
                {
                    throw new ObjectDisposedException(nameof(SyncContentReader));
                }
                return reader.Read(offset, buffer);
            }

            public override int Read(TiffStreamOffset offset, ArraySegment<byte> buffer)
            {
                TiffFileContentReader? reader = _reader;
                if (reader is null)
                {
                    throw new ObjectDisposedException(nameof(SyncContentReader));
                }
                return reader.Read(offset, buffer);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    Interlocked.Exchange(ref _reader, null)?.Dispose();
                }
                _reader = null;
            }
        }
    }
}
