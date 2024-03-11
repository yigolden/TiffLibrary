using System.Drawing;

namespace TiffLibrary.Utils;

/// <summary>
///
/// </summary>
/// <typeparam name="TPixel"></typeparam>
public interface IPartialBufferProvider<TPixel> where TPixel : unmanaged
{
    /// <summary>
    /// The size of the image that is being processed. If it is multi-resolution tiff. Please extend the class to provide dynamic values here.
    /// </summary>
    TiffSize ImageSize { get; }

    /// <summary>
    /// The return the memory buffer. Given the region on the image. If it is multi-resolution tiff. Please optimize extend the class to manage different levels.
    /// </summary>
    public TiffMemoryPixelBuffer<TPixel> GetMemoryBuffer(Rectangle imageRegion);
}
