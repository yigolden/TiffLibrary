using SixLabors.ImageSharp;

namespace TiffLibrary.ImageSharpAdapter
{
    /// <summary>
    /// Registers the iamge encoders, decoders and mime type detectors for the TIFF format.
    /// </summary>
    public sealed class TiffConfigurationModule : IConfigurationModule
    {
        /// <inheritdoc/>
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
