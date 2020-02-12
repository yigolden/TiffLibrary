# Working with Pixel Buffer

## ITiffPixelBuffer&lt;TPixel&gt; Interface

`ITiffPixelBuffer<TPixel>` represents a 2-dimensional region of pixels in a contiguous memory buffer in row-major order. It can be used not only as the destination buffer when decoding an IFD from TIFF file, but also an image source when encoding an IFD into TIFF file.

TiffLibrary provides a simple implementation
called TiffMemoryPixelBuffer to wrap `Memory<TPixel>` as `ITiffPixelBuffer<TPixel>`. The following example shows how to create it.

``` csharp
Memory<TiffRgba32> pixels = new TiffRgba32[256 * 256];
var pixelBuffer = new TiffMemoryPixelBuffer<TPixel>(pixels, 256, 256, writable: true);
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

## Implementing Custom ITiffPixelBuffer&lt;TPixel&gt;

If the pixel buffer can not be accessed as `Memory<TPixel>`, it can not be wrapped in `TiffMemoryPixelBuffer<TPixel>`. Instead, you have to create your own implementation of `ITiffPixelBuffer<TPixel>` to access the pixels. A code snippet of `ITiffPixelBuffer<TPixel>` is shown below.

``` csharp
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

## TiffPixelBuffer&lt;TPixel&gt; Struct

While `ITiffPixelBuffer<TPixel>` provides a machanism to access the entire region of the pixel buffer, sometimes we want to limit TiffLibrary to only access a sub-region of the entire pixel buffer. This is where the `TiffPixelBuffer<TPixel>` struct can come in handy. `TiffPixelBuffer<TPixel>` is a struct that represents a sub-region of the pixel buffer of `ITiffPixelBuffer<TPixel>`. It basically stores the reference to the pixel buffer itself, the starting point of the sub-region, and the size of the sub-region. The following code shows how to create a `TiffPixelBuffer<TPixel>` struct.

``` csharp
ITiffPixelBuffer<TiffGray8> pixelBuffer = new TiffMemoryPixelBuffer<TiffGray8>(new TiffGray8[1000 * 1000], 1000, 1000, writable: true);
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

After `TiffPixelBuffer<TPixel>` is created, you can pass it to the encoder or the decoder to use. Apart from specifying the sub-region of the pixel buffer, you can also set a limit on the area to read when trying to decode images from TIFF files. This is a common technique when decoding images from massive-sized TIFF files. For more information, please see [Using Image Decoder APIs](./using-image-decoder-apis.md).


## ITiffPixelBufferWriter&lt;TPixel&gt; Interface

`ITiffPixelBufferWriter<TPixel>` represents a write-only 2-dimensional region of pixel buffer. Like `ITiffPixelBuffer<TPixel>` interface, it provides `Width` and `Height` properties for querying the number of columns and rows of the pixel buffer. Apart from that, it provides the following two APIs for writing pixels into the buffer.

``` csharp
public interface ITiffPixelBufferWriter<TPixel> : IDisposable where TPixel : unmanaged
{
    TiffPixelSpanHandle<TPixel> GetRowSpan(int rowIndex, int start, int length);
    TiffPixelSpanHandle<TPixel> GetColumnSpan(int colIndex, int start, int length);
}
```

The `GetRowSpan` and `GetColumnSpan` methods return a writable buffer for the specified 1-dimensional span of pixels in the pixel buffer. The `rowIndex` and the `colIndex` parameters specify the row or columns the user want to write pixels into. The `start` and the `length` parameters specify the span of pixels within the selected row or column. If users want to write a 2-dimensional region to the pixel buffer, they will have to iterate through all the rows or columns of the region, call `GetRowSpan` or `GetColumnSpan` for each row or column, and write the pixels into the buffer for each span.

`TiffPixelSpanHandle<TPixel>` represents a temporary buffer for a 1-dimensional span of pixels.  The APIs is shown below. The `GetSpan` method returns the buffer you should fill for this span of pixels. After the buffer is filled, call `Dispose` on the instance to flush the pixels in the temporary buffer to the actual pixel buffer.

``` csharp
public abstract class TiffPixelSpanHandle<TPixel> : IDisposable where TPixel : unmanaged
{
    public abstract Span<TPixel> GetSpan();
    public abstract void Dispose();
}
```

To reduce allocation, implementations of `TiffPixelSpanHandle<TPixel>` may reuse the instance itself, and may or may not reuse the buffer of the writer. Therefore, make sure `Dispose` is called after you are done with the current row or column, and never use the instance again.

The following example shows how to fill a pixel buffer represented as `ITiffPixelBufferWriter<TiffGray8>` with black color.

``` csharp
void FillBlack(ITiffPixelBufferWriter<TiffGray8> writer)
{
    int width = writer.Width;
    int height = writer.Height;
    TiffGray8 blackPixel = default;
    for (int i = 0; i < height; i++)
    {
        using TiffPixelSpanHandle<TiffGray8> handle = writer.GetRowSpan(i, 0, width);
        handle.GetSpan().Fill(blackPixel);
    }
}
```

When you no longer use the pixel buffer writer instance, call `Dispose` on the instance to make sure all the temporary resources are released.

Typically, an implementation of `ITiffPixelBufferWriter<TPixel>` can be complex due to the requirement to handle buffer for pixel span in both directions. If you only want to wrap an existing pixel buffer as a pixel buffer writer, please use `TiffPixelBufferWriterAdapter<TPixel>` in `TiffLibrary.PixelBuffer` namespace.

Like the `TiffPixelBuffer<TPixel>` struct, these is also a `TiffPixelBufferWriter<TPixel>` struct to represents a sub-region of pixel buffer writer. Its usage is very similar to `TiffPixelBuffer<TPixel>`. Except it also has `GetRowSpan` and `GetColumnSpan` methods, and can be used like a regular pixel buffer writer.

## ITiffPixelBufferReader&lt;TPixel&gt; Interface

`ITiffPixelBufferReader<TPixel>` represents a read-only 2-dimensional region of pixel buffer. Like `ITiffPixelBuffer<TPixel>` interface, it provides `Width` and `Height` properties for querying the number of columns and rows of the pixel buffer. Apart from that, it provides the following API for reading pixels from the buffer.

``` csharp
public interface ITiffPixelBufferReader<TPixel> : IDisposable where TPixel : unmanaged
{
    ValueTask ReadAsync(TiffPoint offset, TiffPixelBufferWriter<TPixel> destination, CancellationToken cancellationToken);
}
```

To read a region from the reader, the caller will need to prepare the offset of the region and a pixel buffer writer object to let `ITiffPixelBufferReader<TPixel>` to write pixels to, and call `ReadAsync` method to start copying pixels to the writer. The caller should also `await` on the `ValueTask` returned by `ReadAsync` in case that the pixels of the region is not immediately available. The following example shows how to read a sub-region from a pixel buffer reader instance.

``` csharp
public async Task ReadRegionAsync(ITiffPixelBufferReader<TiffGray8> reader, ITiffPixelBuffer<TiffGray8> destination)
{
    // Prepare the writer
    var writer = new TiffPixelBufferWriterAdapter<TiffGray8>(destination);
    // The offset of the region
    var offset = new TiffPoint(512, 512);
    // Read pixels
    await reader.ReadAsync(offset, writer.AsPixelBufferWriter(), CancellationToken.None);
}
```

If you only want to wrap an existing pixel buffer as a pixel buffer reader, please use `TiffPixelBufferReaderAdapter<TPixel>` in `TiffLibrary.PixelBuffer` namespace. If you want to use a massive image source where a particular region in the image is not immediately available in the memory, you can perform necessary IO operations in `ReadAsync` methods to retrieve the pixels and write to `TiffPixelBufferWriter<TPixel>`.

Like the `TiffPixelBuffer<TPixel>` struct, these is also a `TiffPixelBufferReader<TPixel>` struct to represents a sub-region of pixel buffer reader. Its usage is very similar to `TiffPixelBuffer<TPixel>`. Except it also has the `ReadAsync` method, and can be used like a regular pixel buffer reader.
