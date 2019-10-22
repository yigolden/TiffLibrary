namespace JpegLibrary
{
    internal abstract class JpegBlockInputReader
    {
        public abstract int Width { get; }
        public abstract int Height { get; }
        public abstract void ReadBlock(ref JpegBlock8x8 block, int componentIndex, int x, int y);
    }
}
