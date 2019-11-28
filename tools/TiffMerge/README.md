# TiffMerge

Merge multiple TIFF files into a single file by copying IFDs into the new file.

TiffMerge takes one or more TIFF files as input, creates a new TIFF file, and copies IFDs from the inpt TIFF files into the output TIFF files. The output TIFF files should contain all the IFDs from the input TIFF files.

Limitaitons: The endianness of input files are assumed to be the same. Copying sub-IFDs is not supported.

This tool is only used to test some functionalities of TiffLibrary.
