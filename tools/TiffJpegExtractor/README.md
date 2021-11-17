# TiffJpegExtractor
This tool extracts JPEG images from TIFF/JPEG file into a directory without decoding any image data. The prerequisite for the input TIFF file is:
* PhotometricInterpretation is YCbCr
* Compression is JPEG
* BitsPerSample is [8, 8, 8] or [8]

## Usage

```
dotnet run -- C:\Data\your-input.tif --output C:\Data\some-empty-directory
```

## Implementation
In most cases, TIFF/JPEG files contains raw JPEG stream for each tile/strip. This program reads the offsets and byte counts of each tile or strip, extract these bytes and save them into `.jpg` files. Occasionally, common JPEG tables (such as the quantization tables and the Huffman tables) for all the tiles/strips are stored separately in the JPEGTables tag. This program processes such TIFF files by concatenating these tables with the image stream to form valid JPEG streams and saving them to `.jpg` files.
