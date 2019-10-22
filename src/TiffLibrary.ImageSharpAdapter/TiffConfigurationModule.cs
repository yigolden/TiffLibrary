using SixLabors.ImageSharp;

namespace TiffLibrary.ImageSharpAdapter
{
    public sealed class TiffConfigurationModule : IConfigurationModule
    {
        public void Configure(Configuration configuration)
        {
            if (configuration is null)
            {
                throw new System.ArgumentNullException(nameof(configuration));
            }

            configuration.ImageFormatsManager.SetEncoder(TiffFormat.Instance, new TiffEncoder());
            configuration.ImageFormatsManager.SetDecoder(TiffFormat.Instance, new TiffDecoder());
            configuration.ImageFormatsManager.AddImageFormatDetector(new TiffImageFormatDetector());
        }
    }
}
