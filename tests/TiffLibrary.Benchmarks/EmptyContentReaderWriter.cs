using System;

namespace TiffLibrary.Benchmarks
{
    public class EmptyContentReaderWriter : TiffFileContentReaderWriter
    {
        public override int Read(TiffStreamOffset offset, Memory<byte> buffer) => 0;

        public override void Write(TiffStreamOffset offset, ReadOnlyMemory<byte> buffer) { }

        public override void Flush() { }

        protected override void Dispose(bool disposing) { }

    }
}
