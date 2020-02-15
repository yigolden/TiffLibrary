# Using Image Decoder APIs

`TiffImageDecoder` base class defines the basic form of the methods used to decode image from an IFD. An instance of `TiffImageDecoder` corresponds to a specific IFD which is specified when `TiffImageDecoder` is created. The APIs of the image decoder are shown below.

``` csharp
public abstract class TiffImageDecoder
{
    public abstract int Width { get; }
    public abstract int Height { get; }
    public abstract Task DecodeAsync<TPixel>(TiffPoint offset, TiffSize readSize, TiffPoint destinationOffset, ITiffPixelBufferWriter<TPixel> writer, CancellationToken cancellationToken = default) where TPixel : unmanaged;
}
```

The `Width` and the `Height` property represents the width and the height of the image encoded in this IFD. The `DecodeAsync` methods decodes the image and writes the pixels into the `writer` specified. To support reading a sub-region of the entire image in the IFD, This method accepts `offset` and `readSize` which can be used to specify where the region starts and how big the region is. If you want the decoder to write pixels into a specific region in the pixel buffer, You can specify `destinationOffset` to be the offset of the destination region. The `TPixel` type parameter can be any of the supported pixel format documented in [Supported TIFF Features](./supported-tiff-features.md). The decoder will automatically converts pixels in the IFD into pixels whose format is specified by the `TPixel` type parameter.

The most simple example of decoding the entire region is shown below.
``` csharp
TiffImageDecoder decoder = GetDecoder();
ITiffPixelBufferWriter<TPixel> writer = GetWriter(decoder.Width, decoder.Height);
await decoder.DecodeAsync(offset: default, readSize: new TiffSize(decoder.Width, decoder.Height), destinationOffset: default, writer: writer);
```

TiffLibrary provides a serious of extension methods on `TiffImageDeocder` to help you simplify you code when decoding images. The extension methods are located in `TiffImageDecoderExtensions` class. They can work on four types of buffer. Below is the full list of the decoding APIs available.

``` csharp
// APIs that works on ITiffPixelBufferWriter<TPixel> interface
Task DecodeAsync<TPixel>(ITiffPixelBufferWriter<TPixel> writer, CancellationToken cancellationToken);
Task DecodeAsync<TPixel>(TiffPoint offset, ITiffPixelBufferWriter<TPixel> writer, CancellationToken cancellationToken);
Task DecodeAsync<TPixel>(TiffPoint offset, TiffSize readSize, ITiffPixelBufferWriter<TPixel> writer, CancellationToken cancellationToken);
Task DecodeAsync<TPixel>(TiffPoint offset, TiffSize readSize, TiffPoint destinationOffset, ITiffPixelBufferWriter<TPixel> writer, CancellationToken cancellationToken); // This is the instance method on TiffImageDecoder

// APIs that works on TiffPixelBufferWriter<TPixel> struct
Task DecodeAsync<TPixel>(TiffPixelBufferWriter<TPixel> writer, CancellationToken cancellationToken);
Task DecodeAsync<TPixel>(TiffPoint offset, TiffPixelBufferWriter<TPixel> writer, CancellationToken cancellationToken);
Task DecodeAsync<TPixel>(TiffPoint offset, TiffSize readSize, TiffPixelBufferWriter<TPixel> writer, CancellationToken cancellationToken);
Task DecodeAsync<TPixel>(TiffPoint offset, TiffSize readSize, TiffPoint destinationOffset, TiffPixelBufferWriter<TPixel> writer, CancellationToken cancellationToken);

// APIs that works on ITiffPixelBuffer<TPixel> interface
Task DecodeAsync<TPixel>(ITiffPixelBuffer<TPixel> buffer, CancellationToken cancellationToken);
Task DecodeAsync<TPixel>(TiffPoint offset, ITiffPixelBuffer<TPixel> buffer, CancellationToken cancellationToken);
Task DecodeAsync<TPixel>(TiffPoint offset, TiffSize readSize, ITiffPixelBuffer<TPixel> buffer, CancellationToken cancellationToken);
Task DecodeAsync<TPixel>(TiffPoint offset, TiffSize readSize, TiffPoint destinationOffset, ITiffPixelBuffer<TPixel> buffer, CancellationToken cancellationToken);

// APIs that works on TiffPixelBuffer<TPixel> struct
Task DecodeAsync<TPixel>(TiffPixelBuffer<TPixel> buffer, CancellationToken cancellationToken);
Task DecodeAsync<TPixel>(TiffPoint offset, TiffPixelBuffer<TPixel> buffer, CancellationToken cancellationToken);
Task DecodeAsync<TPixel>(TiffPoint offset, TiffSize readSize, TiffPixelBuffer<TPixel> buffer, CancellationToken cancellationToken);
Task DecodeAsync<TPixel>(TiffPoint offset, TiffSize readSize, TiffPoint destinationOffset, TiffPixelBuffer<TPixel> buffer, CancellationToken cancellationToken);
```
