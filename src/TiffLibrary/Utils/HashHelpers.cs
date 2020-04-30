#if NO_HASHCODE
namespace TiffLibrary.Utils
{
    internal static class HashHelpers
    {
        public static int Combine(int h1, int h2)
        {
            uint rol5 = ((uint)h1 << 5) | ((uint)h1 >> 27);
            return ((int)rol5 + h1) ^ h2;
        }
    }
}
#endif
