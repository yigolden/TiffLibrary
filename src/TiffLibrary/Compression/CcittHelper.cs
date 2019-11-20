namespace TiffLibrary.Compression
{
    internal static class CcittHelper
    {
        internal static void SwapTable<T>(ref T table1, ref T table2)
        {
            T temp = table1;
            table1 = table2;
            table2 = temp;
        }

    }
}
