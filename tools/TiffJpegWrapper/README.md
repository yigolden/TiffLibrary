# TiffJpegWrapper

Creates a single-strip TIFF file that wraps the specified JPEG image.

TiffJpegWrapper takes a JPEG file as input, appends a TIFF header before it and an IFD definition after it to form a readable TIFF file. The Compression tag of the IFD is set to 7 (JPEG). The file only contains one strip. Its RowsPerStrip is equal to ImageHeight. The content of the strip is the JPEG file itself.

This tool is only used to test some functionalities of TiffLibrary.
