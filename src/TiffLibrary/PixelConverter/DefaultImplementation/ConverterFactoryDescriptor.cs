using System;

namespace TiffLibrary.PixelConverter
{
    internal class ConverterFactoryDescriptor
    {
        public Type SourceType { get; set; }
        public Type DestinationType { get; set; }
        public ITiffPixelConverterFactory ConverterFactory { get; set; }
    }
}
