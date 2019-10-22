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

If the pixel buffer can not be accessed as `Memory<TPixel>`, it can not be wrapped in `TiffMemoryPixelBuffer<TPixel>`. Instead, you can create your own implementation of `ITiffPixelBuffer<TPixel>` to access pixel buffer. A code snippet of `ITiffPixelBuffer<TPixel>` is shown below.

```csharp
public interface ITiffPixelBuffer<TPixel> where TPixel : unmanaged
{
    int Width { get; }
    int Height { get; }
    Span<TPixel> GetSpan();
}
```

The `Width` and `Height` properties are the number of columns and rows in this pixel buffer. The `GetSpan` method returns the memory of the entire pixel buffer in row-major order.
