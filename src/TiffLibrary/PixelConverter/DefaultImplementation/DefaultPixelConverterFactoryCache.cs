namespace TiffLibrary.PixelConverter
{
    internal static class DefaultPixelConverterFactoryCache<TSource, TDestination> where TSource : unmanaged where TDestination : unmanaged
    {
        public static ITiffPixelConverterFactory Factory { get; set; }
    }
}
