using System;

namespace TiffLibrary.PixelConverter
{
    internal class ConverterFactoryDescriptor
    {
        public ConverterFactoryDescriptor(Type sourceType, Type destinationType, ITiffPixelConverterFactory converterFactory)
        {
            SourceType = sourceType;
            DestinationType = destinationType;
            ConverterFactory = converterFactory;
        }

        public Type SourceType { get; }
        public Type DestinationType { get; }
        public ITiffPixelConverterFactory ConverterFactory { get; }
    }
}
