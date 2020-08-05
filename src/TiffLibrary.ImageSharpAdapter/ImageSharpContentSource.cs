using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TiffLibrary.ImageSharpAdapter
{
    internal class ImageSharpContentSource : TiffFileContentSource
    {
        private ImageSharpContentReaderWriter _reader;

        public ImageSharpContentSource(Stream stream)
        {
            _reader = new ImageSharpContentReaderWriter(stream);
        }

        public override TiffFileContentReader OpenReader()
        {
            return _reader;
        }

        public override ValueTask<TiffFileContentReader> OpenReaderAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return new ValueTask<TiffFileContentReader>(_reader);
        }

        protected override void Dispose(bool disposing) { }

    }
}
