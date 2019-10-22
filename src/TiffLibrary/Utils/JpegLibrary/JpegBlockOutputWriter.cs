namespace JpegLibrary
{
    internal abstract partial class JpegBlockOutputWriter
    {
        public abstract void WriteBlock(in JpegBlock8x8 block, int componentIndex, int x, int y, int horizontalSamplingFactor, int verticalSamplingFactor);
    }
}
