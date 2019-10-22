using System;
using System.IO;
using System.Threading.Tasks;

namespace TiffLibrary
{
    internal sealed class TiffFileStreamContentSource : TiffFileContentSource
    {
        private readonly string _filename;

        public TiffFileStreamContentSource(string filename)
        {
            _filename = filename ?? throw new ArgumentNullException(nameof(filename));
        }

        public override ValueTask<TiffFileContentReader> OpenReaderAsync()
        {
            return new ValueTask<TiffFileContentReader>(new TiffStreamContentSource.ContentReader(new FileStream(_filename, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true), leaveOpen: false));
        }

        protected override void Dispose(bool disposing)
        {
            // Noop
        }

    }
}
