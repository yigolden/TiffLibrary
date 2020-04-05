# Supported TIFF Features

This page lists features of TIFF format supported by TiffLibrary.

## File Format

* Both Little-endian and Big-endian TIFF.
* Both Standard TIFF and BigTIFF format.
* Both Stripped TIFF and tiled TIFF.

## Pixel Formats

The following pixel formats are supported by the decoder and the encoder. The decoder and the encoder can accept buffers of these types and automatically convert from and to the photometric interpretations used in the TIFF files. These types resides in TiffLibrary.PixelFormats namespace.

* TiffGray8
* TiffGray16
* TiffRgba32
* TiffBgra32
* TiffRgba64
* TiffBgra64
* TiffRgb24
* TiffBgr24

## Photometric Interpretations for Decoding

The following photometric interpretations in the TIFF files are supported by the image decoder.

* TransparencyMask (1 bit)
* WhiteIsZero (1 to 32 bits)
* BlackIsZero (1 to 32 bits)
* Chunky/Planar RGB/RGBA (1 to 32 bits)
* PaletteColor (1 to 8 bits)
* Chunky/Planar CMYK (8/16 bits)
* Chunky/Planar YCbCr (8/16 bits)

Note that although files with bits per sample greater than 16 bits can be read by the decoder, the pixel formats of TiffLibrary only support up to 16 bits. Therefore, the lower bits will be truncated when reading such files.

## Compression for Decoding

* NoCompression (CompressionTag=1)
* ModifiedHuffmanCompression (CompressionTag=2)
* T4 (CompressionTag=3)
* T6 (CompressionTag=4)
* LZW (CompressionTag=5)
* Old-style JPEG (CompressionTag=6)
* JPEG (CompressionTag=7)
* Deflate (CompressionTag=8/32946)
* PackBits (CompressionTag=32773)
* ThunderScan (CompressionTag=32809)

Note that support for old-style JPEG is not thoroughly tested and will not be actively developed because it is deprecated in favor of the "new" JPEG compression method (CompressionTag=7).

## Predictor for Decoding

* None (PredictorTag=1)
* Horizontal Predictor (PredictorTag=2)

Horizontal predictor is usually used along with LZW and Deflate compression to further reduce file size while preserving image quality.

## Orientation for Decoding

TiffLibrary supports decoding of the image in any of the following orientation.

* TopLeft (OrientationTag=1)
* TopRight (OrientationTag=2)
* BottomRight (OrientationTag=3)
* BottomLeft (OrientationTag=4)
* LeftTop (OrientationTag=5)
* RightTop (OrientationTag=6)
* RightBottom (OrientationTag=7)
* LeftBottom (OrientationTag=8)

## Photometric Interpretations for Encoding

* TransparencyMask (1 bit)
* WhiteIsZero (8 bits)
* BlackIsZero (8 bits)
* Chunky RGB/RGBA (8 bits)
* Chunky CMYK (8 bits)
* Chunky YCbCr (8 bits)

## Compression for Encoding

* NoCompression (CompressionTag=1)
* ModifiedHuffmanCompression (CompressionTag=2)
* T6 (CompressionTag=4)
* LZW (CompressionTag=5)
* JPEG (CompressionTag=7)
* Deflate (CompressionTag=8)
* PackBits (CompressionTag=32773)
* ThunderScan (CompressionTag=32809)

Notes about JPEG: JPEG compression only supports 8-bit BlackIsZero, WhiteIaZero, RGB, CMYK and YCbCr photometric interpretations.

## Predictor for Encoding

* None (PredictorTag=1)
* Horizontal Predictor (PredictorTag=2)

It is not recommended to apply horizontal predictor to image compressed with compression methods other than LZW and Deflate, because such TIFF files may confuse other TIFF readers.

## Orientation for Encoding

TiffLibrary supports encoding of the image in any of the following orientation.

* TopLeft (OrientationTag=1)
* TopRight (OrientationTag=2)
* BottomRight (OrientationTag=3)
* BottomLeft (OrientationTag=4)
* LeftTop (OrientationTag=5)
* RightTop (OrientationTag=6)
* RightBottom (OrientationTag=7)
* LeftBottom (OrientationTag=8)
