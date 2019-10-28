# Working with Pixel Buffer

## ITiffPixelBuffer<TPixel> Interface

`ITiffPixelBuffer` represents a 2-dimensional region of pixels in a contiguous memory buffer in row-major order. It can be used not only as the destination buffer when decoding an IFD from TIFF file, but also an image source when encoding an IFD into TIFF file.

TiffLibrary provides a simple implementation
called TiffMemoryPixelBuffer to wrap `Memory<TPixel>` as `ITiffPixelBuffer<TPixel>`. The following example shows how to create it.

``` csharp
Memory<TiffRgba32> pixels = new TiffRgba32[256 * 256];
var pixelBuffer = new TiffMemoryPixelBuffer<TPixel>(pixels, 256, 256);
```

Once you have created an `ITiffPixelBuffer<TPixel>` instance, you can use it as the image buffer when working with `TiffImageDecoder` or `TiffImageEncoder<TPixel>`.

``` csharp
// Decode an image into the pixel buffer.
TiffImageDecoder decoder = GetDecoder();
await decoder.DecodeAsync(pixelBuffer);

// Encode an image from the pixel buffer.
TiffImageFileDirectoryWriter ifdWriter = GetWriter();
TiffImageEncoder<TiffRgba32> encoder = GetEncoder();
await encoder.EncodeAsync(ifdWriter, pixelBuffer);
```

## Implementing Custom ITiffPixelBuffer

If the pixel buffer can not be accessed as `Memory<TPixel>`, it can not be wrapped in `TiffMemoryPixelBuffer<TPixel>`. Instead, you have to create your own implementation of `ITiffPixelBuffer<TPixel>` to access the pixels. A code snippet of `ITiffPixelBuffer<TPixel>` is shown below.

```csharp
public interface ITiffPixelBuffer<TPixel> where TPixel : unmanaged
{
    int Width { get; }
    int Height { get; }
    Span<TPixel> GetSpan();
}
```

The `Width` and `Height` properties are the number of columns and rows in this pixel buffer. The `GetSpan` method returns the memory of the entire pixel buffer in row-major order.

For example, if you want componenets of TiffLibrary to access pixels stored in `Image<Rgba32>` buffer from ImageSharp library, you can write a wrapper class that implements `ITiffPixelBuffer<TiffRgba32>` and pass the instance of that class to the encoder or the decoder. A simple implementation of the wrapper class is shown below. Note that the TPixel type parameter must be a type that TiffLibrary understands. In this case, `Rgba32` is reinterpreted as `TiffRgba32` so that TiffLibrary can correctly use the pixel buffer. For a list supported TPixel type parameter, see [Supported TIFF Features](./supported-tiff-features.md).

``` csharp
using System;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using TiffLibrary.PixelFormats;

namespace TiffLibrary.Tests
{
    class ImageSharpPixelBuffer : ITiffPixelBuffer<TiffRgba32>
    {
        private readonly Image<Rgba32> _image;

        public ImageSharpPixelBuffer(Image<Rgba32> image)
        {
            _image = image;
        }

        public int Width => _image.Width;

        public int Height => _image.Height;

        public Span<TiffRgba32> GetSpan()
        {
            return MemoryMarshal.Cast<Rgba32, TiffRgba32>(_image.GetPixelSpan());
        }
    }
}

```

## TiffPixelBuffer<TPixel> Struct

While `ITiffPixelBuffer<TPixel>` provides a machanism to access the entire region of the pixel buffer, sometimes we want to limit TiffLibrary to only access a sub-region of the entire pixel buffer. This is where the `TiffPixelBuffer<TPixel>` struct can come in handy. `TiffPixelBuffer<TPixel>` is a struct that represents a sub-region of the pixel buffer of `ITiffPixelBuffer<TPixel>`. It basically stores the reference to the pixel buffer itself, the starting point of the sub-region, and the size of the sub-region. The following code shows how to create a `TiffPixelBuffer<TPixel>` struct.

``` csharp
ITiffPixelBuffer<TiffGray8> pixelBuffer = new TiffMemoryPixelBuffer<TiffGray8>(new TiffGray8[1000 * 1000], 1000, 1000);
TiffPixelBuffer<TiffGray8> structBuffer;

// Create the struct using its constructor
structBuffer = new TiffPixelBuffer<TiffGray8>(pixelBuffer);

// Create the struct using extension methods on ITiffPixelBuffer<TPixel>
structBuffer = pixelBuffer.AsPixelBuffer();

// Create the struct using Crop extension methods on ITiffPixelBuffer<TPixel>
structBuffer = pixelBuffer.Crop(new TiffPoint(200, 100)); // A 800x900 region skipping the first 200 columns and 100 rows in the orinigal buffer
structBuffer = pixelBuffer.Crop(new TiffPoint(200, 100), new TiffSize(400, 300)); // A 400x300 region skipping the first 200 columns and 100 rows in the orinigal buffer

// You can even call Crop on the struct to further limit the area.
structBuffer = structBuffer.Crop(new TiffPoint(50, 50));
structBuffer = structBuffer.Crop(new TiffPoint(50, 50), new TiffSize(100, 100));
```

After `TiffPixelBuffer<TPixel>` is created, you can pass it to the encoder or the decoder to use. Apart from specifying the sub-region of the pixel buffer, you can also set a limit on the area to read when trying to decode images from TIFF files. This is a common technique when decoding images from massive-sized TIFF files. For more information, please see TODO.
