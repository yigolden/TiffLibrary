namespace TiffLibrary
{
    /// <summary>
    /// The tiff tag.
    /// </summary>
    public enum TiffTag : ushort
    {
        /// <summary>
        /// None.
        /// </summary>
        None = 0,

        /// <summary>
        /// NewSubfileType.
        /// </summary>
        NewSubfileType = 0xFE,

        /// <summary>
        /// SubFileType.
        /// </summary>
        SubFileType = 0xFF,

        /// <summary>
        /// ImageWidth.
        /// </summary>
        ImageWidth = 0x100,

        /// <summary>
        /// ImageLength.
        /// </summary>
        ImageLength = 0x101,

        /// <summary>
        /// BitsPerSample.
        /// </summary>
        BitsPerSample = 0x102,

        /// <summary>
        /// Compression.
        /// </summary>
        Compression = 0x103,

        /// <summary>
        /// PhotometricInterpretation.
        /// </summary>
        PhotometricInterpretation = 0x106,

        /// <summary>
        /// Threshholding.
        /// </summary>
        Threshholding = 0x107,

        /// <summary>
        /// CellWidth.
        /// </summary>
        CellWidth = 0x108,

        /// <summary>
        /// CellLength.
        /// </summary>
        CellLength = 0x109,

        /// <summary>
        /// FillOrder.
        /// </summary>
        FillOrder = 0x10A,

        /// <summary>
        /// DocumentName.
        /// </summary>
        DocumentName = 0x10D,

        /// <summary>
        /// ImageDescription.
        /// </summary>
        ImageDescription = 0x10E,

        /// <summary>
        /// Make.
        /// </summary>
        Make = 0x10F,

        /// <summary>
        /// Model.
        /// </summary>
        Model = 0x110,

        /// <summary>
        /// StripOffsets.
        /// </summary>
        StripOffsets = 0x111,

        /// <summary>
        /// Orientation.
        /// </summary>
        Orientation = 0x112,

        /// <summary>
        /// SamplesPerPixel.
        /// </summary>
        SamplesPerPixel = 0x115,

        /// <summary>
        /// RowsPerStrip.
        /// </summary>
        RowsPerStrip = 0x116,

        /// <summary>
        /// StripByteCounts.
        /// </summary>
        StripByteCounts = 0x117,

        /// <summary>
        /// MinSampleValue.
        /// </summary>
        MinSampleValue = 0x118,

        /// <summary>
        /// MaxSampleValue.
        /// </summary>
        MaxSampleValue = 0x119,

        /// <summary>
        /// XResolution.
        /// </summary>
        XResolution = 0x11A,

        /// <summary>
        /// YResolution.
        /// </summary>
        YResolution = 0x11B,

        /// <summary>
        /// PlanarConfiguration.
        /// </summary>
        PlanarConfiguration = 0x11C,

        /// <summary>
        /// PageName.
        /// </summary>
        PageName = 0x11D,

        /// <summary>
        /// XPosition.
        /// </summary>
        XPosition = 0x11E,

        /// <summary>
        /// YPosition.
        /// </summary>
        YPosition = 0x11F,

        /// <summary>
        /// FreeOffsets.
        /// </summary>
        FreeOffsets = 0x120,

        /// <summary>
        /// FreeByteCounts.
        /// </summary>
        FreeByteCounts = 0x121,

        /// <summary>
        /// GrayResponseUnit.
        /// </summary>
        GrayResponseUnit = 0x122,

        /// <summary>
        /// GrayResponseCurve.
        /// </summary>
        GrayResponseCurve = 0x123,

        /// <summary>
        /// T4Options.
        /// </summary>
        T4Options = 0x124,

        /// <summary>
        /// T6Options.
        /// </summary>
        T6Options = 0x125,

        /// <summary>
        /// ResolutionUnit.
        /// </summary>
        ResolutionUnit = 0x128,

        /// <summary>
        /// PageNumber.
        /// </summary>
        PageNumber = 0x129,

        /// <summary>
        /// TransferFunction.
        /// </summary>
        TransferFunction = 0x12D,

        /// <summary>
        /// Software.
        /// </summary>
        Software = 0x131,

        /// <summary>
        /// DateTime.
        /// </summary>
        DateTime = 0x132,

        /// <summary>
        /// Artist.
        /// </summary>
        Artist = 0x13B,

        /// <summary>
        /// HostComputer.
        /// </summary>
        HostComputer = 0x13C,

        /// <summary>
        /// Predictor.
        /// </summary>
        Predictor = 0x13D,

        /// <summary>
        /// WhitePoint.
        /// </summary>
        WhitePoint = 0x13E,

        /// <summary>
        /// PrimaryChromaticities.
        /// </summary>
        PrimaryChromaticities = 0x13F,

        /// <summary>
        /// ColorMap.
        /// </summary>
        ColorMap = 0x140,

        /// <summary>
        /// HalftoneHints.
        /// </summary>
        HalftoneHints = 0x141,

        /// <summary>
        /// TileWidth.
        /// </summary>
        TileWidth = 0x142,

        /// <summary>
        /// TileLength.
        /// </summary>
        TileLength = 0x143,

        /// <summary>
        /// TileOffsets.
        /// </summary>
        TileOffsets = 0x144,

        /// <summary>
        /// TileByteCounts.
        /// </summary>
        TileByteCounts = 0x145,

        /// <summary>
        /// The BadFaxLines tag reports the number of scan lines with an incorrect number of pixels encountered by the facsimile during reception (but not necessarily in the file).
        /// </summary>
        BadFaxLines = 0x146,

        /// <summary>
        /// The CleanFaxData tag describes the error content of the data. That is, when the BadFaxLines and ImageLength tags indicate that the facsimile device encountered lines with an incorrect number of pixels during reception, the CleanFaxData tag indicates whether these lines are actually in the data or if the receiving facsimile device replaced them with regenerated lines.
        /// </summary>
        CleanFaxData = 0x147,

        /// <summary>
        /// The ConsecutiveBadFaxLines tag reports the maximum number of consecutive lines containing an incorrect number of pixels encountered by the facsimile device during reception (but not necessarily in the file).
        /// </summary>
        ConsecutiveBadFaxLines = 0x148,

        /// <summary>
        /// InkSet.
        /// </summary>
        InkSet = 0x14C,

        /// <summary>
        /// InkNames.
        /// </summary>
        InkNames = 0x14D,

        /// <summary>
        /// NumberOfInks.
        /// </summary>
        NumberOfInks = 0x14E,

        /// <summary>
        /// DotRange.
        /// </summary>
        DotRange = 0x150,

        /// <summary>
        /// TargetPrinter.
        /// </summary>
        TargetPrinter = 0x151,

        /// <summary>
        /// ExtraSamples.
        /// </summary>
        ExtraSamples = 0x152,

        /// <summary>
        /// SampleFormat.
        /// </summary>
        SampleFormat = 0x153,

        /// <summary>
        /// SMinSampleValue.
        /// </summary>
        SMinSampleValue = 0x154,

        /// <summary>
        /// SMaxSampleValue.
        /// </summary>
        SMaxSampleValue = 0x155,

        /// <summary>
        /// TransferRange.
        /// </summary>
        TransferRange = 0x156,

        /// <summary>
        /// JPEGProc.
        /// </summary>
        JPEGProc = 0x200,

        /// <summary>
        /// JPEGInterchangeFormat.
        /// </summary>
        JPEGInterchangeFormat = 0x201,

        /// <summary>
        /// JPEGInterchangeFormatLength.
        /// </summary>
        JPEGInterchangeFormatLength = 0x202,

        /// <summary>
        /// JPEGRestartInterval.
        /// </summary>
        JPEGRestartInterval = 0x203,

        /// <summary>
        /// JPEGLosslessPredictors.
        /// </summary>
        JPEGLosslessPredictors = 0x205,

        /// <summary>
        /// JPEGPointTransforms.
        /// </summary>
        JPEGPointTransforms = 0x206,

        /// <summary>
        /// JPEGQTables.
        /// </summary>
        JPEGQTables = 0x207,

        /// <summary>
        /// JPEGDCTables.
        /// </summary>
        JPEGDCTables = 0x208,

        /// <summary>
        /// JPEGACTables.
        /// </summary>
        JPEGACTables = 0x209,

        /// <summary>
        /// YCbCrCoefficients.
        /// </summary>
        YCbCrCoefficients = 0x211,

        /// <summary>
        /// YCbCrSubSampling.
        /// </summary>
        YCbCrSubSampling = 0x212,

        /// <summary>
        /// YCbCrPositioning.
        /// </summary>
        YCbCrPositioning = 0x213,

        /// <summary>
        /// ReferenceBlackWhite.
        /// </summary>
        ReferenceBlackWhite = 0x214,

        /// <summary>
        /// Copyright.
        /// </summary>
        Copyright = 0x8298,

        /// <summary>
        /// JPEGTables
        /// </summary>
        JPEGTables = 0x15B,

        /// <summary>
        /// ExifIfd
        /// </summary>
        ExifIfd = 0x8769,

        /// <summary>
        /// GpsIfd
        /// </summary>
        GpsIfd = 0x8825,

        /// <summary>
        /// InteroperabilityIfd
        /// </summary>
        InteroperabilityIfd = 0xA005,
    }
}
