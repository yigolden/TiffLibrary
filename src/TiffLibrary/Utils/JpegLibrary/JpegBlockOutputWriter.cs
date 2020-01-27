#nullable enable

namespace JpegLibrary
{
    internal abstract partial class JpegBlockOutputWriter
    {
        public abstract void WriteBlock(ref short blockRef, int componentIndex, int x, int y);
    }
}
