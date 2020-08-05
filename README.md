# TiffLibrary

A C# library for decoding and encoding images from and to TIFF files.

[![Build Status](https://dev.azure.com/jinyi0679/yigolden/_apis/build/status/yigolden.TiffLibrary?branchName=master)](https://dev.azure.com/jinyi0679/yigolden/_build/latest?definitionId=1&branchName=master)

## Features

* Implemented completely in C# code. No native dependencies.
* Runs on asynchronous APIs. Suitable for server-side applications.
* Supports both standard TIFF and BigTIFF format.
* Supports both stripped TIFF and tiled TIFF.
* Capable of reading and writing TIFF file of massive size.

## Minimum Supported Runtimes

* .NET Framework 4.6
* .NET Core 1.0
* Runtimes compatible with .NET Standard 1.3

## Recommanded Runtimes

* .NET Framework 4.7.2 and higher
* .NET Core 2.1 and higher
* Runtimes compatible with at least .NET Standard 2.0

## Getting Started

### **Install TiffLibrary Package**

To install this library, add this feed to your nuget.config: https://www.myget.org/F/yigolden/api/v3/index.json . An example of nuget.config file can be found at the root directory of this repository.

Install the latest version from the feed.

NuGet: [![NuGet](https://img.shields.io/nuget/v/TiffLibrary.svg)](https://www.nuget.org/packages/TiffLibrary/)

MyGet: [![MyGet](https://img.shields.io/myget/yigolden/v/TiffLibrary.svg)](https://www.myget.org/feed/yigolden/package/nuget/TiffLibrary)


```
dotnet add package TiffLibrary --version <VERSION> --source https://www.myget.org/F/yigolden/api/v3/index.json
```

Add the following using statement to your source files.
``` csharp
using TiffLibrary;
```

### **Opens a TIFF File for Reading**

Open a TIFF file using its file name:
``` csharp
using TiffFileReader tiff = await TiffFileReader.OpenAsync(@"C:\Data\test.tif");
```

Read from an existing `Stream`:
``` csharp
Stream stream = GetStream();
using TiffFileReader tiff = await TiffFileReader.OpenAsync(stream, leaveOpen: false);
```

Read from byte buffer:
``` csharp
byte[] buffer = GetBuffer();
using TiffFileReader tiff = await TiffFileReader.OpenAsync(buffer, 0, buffer.Length);
```

### **Read Image File Directory and Its Entries**

An image file directory (IFD) is a structure in TIFF file. It contains information about the image, as well as pointers to the actual image data. A single TIFF file can contains multiple IFDs (multiple images). They are stored in the TIFF file like a linked-list. Each IFD contains an pointer to the next, while the TIFF file header contains an pointer to the first IFD.

The following example shows how to read the first IFD and its entries.
``` csharp
using TiffFileReader tiff = await TiffFileReader.OpenAsync(@"C:\Data\test.tif");

// ReadImageFileDirectoryAsync method with no parameter will read the list of all the entries in the first IFD.
TiffImageFileDirectory ifd = await tiff.ReadImageFileDirectoryAsync();

// Tests whether an entry with the specified tag exists
bool exists = ifd.Contains(TiffTag.RowsPerStrip);

// Gets information of an entry.
TiffImageFileDirectoryEntry entry = ifd.FindEntry(TiffTag.RowsPerStrip);
TiffFieldType fieldType = entry.Type; // The data type of the values. In this case, it can be TiffFieldType.Short.
long count = entry.ValueCount; // Each entry contains an array of elements of the specific type. ValueCount is the length of the array.
// To read actual values from the entry, TiffFieldReader should be used, which will be covered later in this documentation.
```

The following example shows how to enumerate all the IFDs.
``` csharp
using TiffFileReader tiff = await TiffFileReader.OpenAsync(@"C:\Data\test.tif");

TiffStreamOffset ifdOffset = tiff.FirstImageFileDirectoryOffset;
while (!ifdOffset.IsZero)
{
    // Read the list of entries in this IFD.
    TiffImageFileDirectory ifd = await tiff.ReadImageFileDirectoryAsync(ifdOffset);

    // Operations on the IFD goes here.


    // Move to the next IFD.
    ifdOffset = ifd.NextOffset;
}
```

### **Read the Value of an IFD Entry**

To read actual values from the IFD Entry, a `TiffFieldReader` instance should be created. The following example shows how to read value from the specified tag.
``` csharp
using TiffFileReader tiff = await TiffFileReader.OpenAsync(@"C:\Data\test.tif");
using TiffFieldReader fieldReader = await tiff.CreateFieldReaderAsync();
TiffImageFileDirectory ifd = await tiff.ReadImageFileDirectoryAsync();

// Read the values of BitsPerSample tag.
TiffImageFileDirectoryEntry entry = ifd.FindEntry(TiffTag.BitsPerSample);
TiffValueCollection<ushort> bitsPerSample = await fieldReader.ReadShortFieldAsync(entry);
int count = bitsPerSample.Count;
ushort bitsPerSample0 = bitsPerSample.GetFirstOrDefault(); // or bitsPerSample[0]
ushort bitsPerSample1 = bitsPerSample[1];
ushort bitsPerSample2 = bitsPerSample[2];

// Alternatively, you can use the TiffTagReader helper if you are trying to read well-known tags.
TiffTagReader tagReader = new TiffTagReader(fieldReader, ifd);
TiffValueCollection<ushort> bitsPerSample = await tagReader.ReadBitsPerSampleAsync();

// TiffValueCollection<T> is a array-like structure for optimizing memory usage. It is allocation-free if the value collection contains no or only one elements.
```

Note that when the `TiffFieldReader` is created, a `TiffFileContentReader` instance is created (along with a `Stream` instance if we are reading from file) and kept alive in the life time of `TiffFieldReader` instance. As a result, to save system resources, reuse the `TiffFieldReader` instance if possible or cache the values read from the IFD so that `TiffFieldReader` is not created over and over again.

### **Decode an Image from an IFD**

Each IFD contains at most one image. The following code decode the image from the first IFD into an array.

``` csharp
using TiffLibray.PixelFormats;

using TiffFileReader tiff = await TiffFileReader.OpenAsync(stream, leaveOpen: false);
TiffImageFileDirectory ifd = await tiff.ReadImageFileDirectoryAsync();

// Create the decoder for the specified IFD.
TiffImageDecoder decoder = await tiff.CreateImageDecoderAsync(ifd);

// Create an array to store the pixels
TiffRgba32[] pixels = new TiffRgba32[decoder.Width * decoder.Height];
TiffMemoryPixelBuffer<TiffRgba32> pixelBuffer = new TiffMemoryPixelBuffer<TiffRgba32>(pixels, decoder.Width, decoder.Height, writable: true);

// Decode the image. Note that this call will create a `Stream` instance for reading pixel data if we are reading from file. It will be disposed before DecodeAsync completes.
await decoder.DecodeAsync(pixelBuffer);
```
When creating `TiffImageDecoder` instance, `TiffFieldReader` is used. Therefore, reuse the `TiffImageDecoder` instance if possible to save system resources.

For massive-sized TIFF, you may not allocate enough buffer for the entire image at once. You may want to read a sub-region from the entire region. The following code show how to do that.
``` csharp
...

TiffImageDecoder decoder = await tiff.CreateImageDecoderAsync(ifd);

// Buffer for a sub-region.
TiffRgba32[] pixels = new TiffRgba32[1024 * 1024];
TiffMemoryPixelBuffer<TiffRgba32> pixelBuffer = new TiffMemoryPixelBuffer<TiffRgba32>(pixels, 1024, 1024, writable: true);

// Read a sub-region starting from (2048, 2048).
await decoder.DecodeAsync(new TiffPoint(2048, 2048), pixelBuffer);
```

If you want to decode the image into pixel buffer other than `Memory<TPixel>` (eg. An `Image<Rgba32>` instance from ImageSharp library), you can refer to **[Working with Pixel Buffer](./Documentation/working-with-pixel-buffer.md)** page in advanced topics.

For other usages of `TiffImageDecoder`, see **[Using Image Decoder APIs](./Documentation/using-image-decoder-apis.md)** in advanced topics.

### **Encode an Image into TIFF file**

The following example shows how to encode an RGBA image into the first IFD of an TIFF file.

``` csharp
TiffRgba32[] pixels = new TiffRgba32[1024 * 1024];

// Operations to fill pixels.
...

// Build The image encoder.
var builder = new TiffImageEncoderBuilder();
builder.PhotometricInterpretation = TiffPhotometricInterpretation.RGB;
builder.EnableTransparencyForRgb = true;
builder.IsTiled = true;
builder.TileSize = new TiffSize(256, 256);
builder.Compression = TiffCompression.Deflate;
builder.ApplyPredictor = TiffPredictor.HorizontalDifferencing;
TiffImageEncoder<TiffRgba32> encoder = builder.Build<TiffRgba32>(); // The encoder instance can be reused.

// The pixel buffer to read from.
var pixelBuffer = new TiffMemoryPixelBuffer<TiffRgba32>(pixels, 1024, 1024, writable: false);

// Opens the file for writing.
using var writer = await TiffFileWriter.OpenAsync(@"C:\Data\test2.tif", useBigTiff: false);

// Encode the image into an IFD.
TiffStreamOffset ifdOffset;
using (var ifdWriter = writer.CreateImageFileDirectory())
{
    await encoder.EncodeAsync(ifdWriter, pixelBuffer);

    // Write other properties here (eg, XResolution, YResolution)
    await ifdWriter.WriteTagAsync(TiffTag.XResolution, TiffValueCollection.Single(new TiffRational(96, 1)));
    await ifdWriter.WriteTagAsync(TiffTag.YResolution, TiffValueCollection.Single(new TiffRational(96, 1)));
    await ifdWriter.WriteTagAsync(TiffTag.ResolutionUnit, TiffValueCollection.Single((ushort)TiffResolutionUnit.Inch));

    ifdOffset = await ifdWriter.FlushAsync();
}

// Set this IFD to be the first IFD and flush TIFF file header.
writer.SetFirstImageFileDirectoryOffset(ifdOffset);
await writer.FlushAsync();
```

## Adapter for SixLabors.ImageSharp

This project adds TIFF decoding and encoding supports into [SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp) library.

Add the following code to the start up process of your application (eg. Main method) to add TIFF support to SixLabors.ImageSharp.
``` csharp
SixLabors.ImageSharp.Configuration.Default.Configure(new TiffLibrary.ImageSharpAdapter.TiffConfigurationModule());
```

Now you should be able to decode or encode TIFF files using ImageSharp APIs.

``` csharp
using (Image<Rgba32> image = Image.Load<Rgba32>(@"C:\Data\test.tif"))
{
    image.Save(@"C:\Data\test2.tif");
    // Or
    image.Save(@"C:\Data\test2.tif", new TiffEncoder
    {
        PhotometricInterpretation = TiffPhotometricInterpretation.RGB,
        EnableTransparencyForRgb = true,
        Compression = TiffCompression.Deflate,
        Predictor = TiffPredictor.HorizontalDifferencing
    });
}
```

## Advanced Topics

* [Supported TIFF Features](./Documentation/supported-tiff-features.md)
* [Working with Pixel Buffer](./Documentation/working-with-pixel-buffer.md)
* [Using Image Decoder APIs](./Documentation/using-image-decoder-apis.md)
