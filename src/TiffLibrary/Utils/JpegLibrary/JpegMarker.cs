#nullable enable

namespace JpegLibrary
{
    /// <summary>
    /// JPEG markers
    /// </summary>
    internal enum JpegMarker : byte
    {
        /// <summary>
        /// Padding
        /// </summary>
        Padding = 0xFF,

        /// <summary>
        /// Start of image
        /// </summary>
        StartOfImage = 0xD8,

        /// <summary>
        /// Reserved for application segments
        /// </summary>
        App0 = 0xE0,

        /// <summary>
        /// Reserved for application segments
        /// </summary>
        App1 = 0xE1,

        /// <summary>
        /// Reserved for application segments
        /// </summary>
        App2 = 0xE2,

        /// <summary>
        /// Reserved for application segments
        /// </summary>
        App3 = 0xE3,

        /// <summary>
        /// Reserved for application segments
        /// </summary>
        App4 = 0xE4,

        /// <summary>
        /// Reserved for application segments
        /// </summary>
        App5 = 0xE5,

        /// <summary>
        /// Reserved for application segments
        /// </summary>
        App6 = 0xE6,

        /// <summary>
        /// Reserved for application segments
        /// </summary>
        App7 = 0xE7,

        /// <summary>
        /// Reserved for application segments
        /// </summary>
        App8 = 0xE8,

        /// <summary>
        /// Reserved for application segments
        /// </summary>
        App9 = 0xE9,

        /// <summary>
        /// Reserved for application segments
        /// </summary>
        App10 = 0xEA,

        /// <summary>
        /// Reserved for application segments
        /// </summary>
        App11 = 0xEB,

        /// <summary>
        /// Reserved for application segments
        /// </summary>
        App12 = 0xEC,

        /// <summary>
        /// Reserved for application segments
        /// </summary>
        App13 = 0xED,

        /// <summary>
        /// Reserved for application segments
        /// </summary>
        App14 = 0xEE,

        /// <summary>
        /// Reserved for application segments
        /// </summary>
        App15 = 0xEF,

        /// <summary>
        /// Start of Frame marker, non-differential, Huffman coding, Baseline DCT
        /// </summary>
        StartOfFrame0 = 0xC0,

        /// <summary>
        /// Start of Frame marker, non-differential, Huffman coding, Extended sequential DCT
        /// </summary>
        StartOfFrame1 = 0xC1,

        /// <summary>
        /// Start of Frame marker, non-differential, Huffman coding, Progressive DCT
        /// </summary>
        StartOfFrame2 = 0xC2,

        /// <summary>
        /// Start of Frame marker, non-differential, Huffman coding, Lossless (sequential)
        /// </summary>
        StartOfFrame3 = 0xC3,

        /// <summary>
        /// Start of Frame marker, differential, Huffman coding, Differential sequential DCT
        /// </summary>
        StartOfFrame5 = 0xC5,

        /// <summary>
        /// 
        /// Start of Frame marker, differential, Huffman coding, Differential progressive DCT
        /// </summary>
        StartOfFrame6 = 0xC6,

        /// <summary>
        /// Start of Frame marker, differential, Huffman coding, Differential lossless (sequential)
        /// </summary>
        StartOfFrame7 = 0xC7,

        /// <summary>
        /// Start of Frame marker, non-differential, arithmetic coding, Extended sequential DCT
        /// </summary>
        StartOfFrame9 = 0xC9,

        /// <summary>
        /// Start of Frame marker, non-differential, arithmetic coding, Progressive DCT
        /// </summary>
        StartOfFrame10 = 0xCA,

        /// <summary>
        /// Start of Frame marker, non-differential, arithmetic coding, Lossless (sequential)
        /// </summary>
        StartOfFrame11 = 0xCB,

        /// <summary>
        /// Start of Frame marker, differential, arithmetic coding, Differential sequential DCT
        /// </summary>
        StartOfFrame13 = 0xCD,

        /// <summary>
        /// Start of Frame marker, differential, arithmetic coding, Differential progressive DCT
        /// </summary>
        StartOfFrame14 = 0xCE,

        /// <summary>
        /// Start of Frame marker, differential, arithmetic coding, Differential lossless (sequential)
        /// </summary>
        StartOfFrame15 = 0xCF,

        /// <summary>
        ///  Define Huffman table(s)
        /// </summary>
        DefineHuffmanTable = 0xC4,

        /// <summary>
        /// Define arithmetic coding conditioning(s)
        /// </summary>
        DefineArithmeticCodingConditioning = 0xCC,

        /// <summary>
        /// Define quantization table(s)
        /// </summary>
        DefineQuantizationTable = 0xDB,

        /// <summary>
        /// Define number of lines
        /// </summary>
        DefineNumberOfLines = 0xDC,

        /// <summary>
        /// Define restart interval
        /// </summary>
        DefineRestartInterval = 0xDD,

        /// <summary>
        /// Start of scan
        /// </summary>
        StartOfScan = 0xDA,

        /// <summary>
        ///  Restart with modulo 8 count 0
        /// </summary>
        DefineRestart0 = 0xD0,

        /// <summary>
        ///  Restart with modulo 8 count 1
        /// </summary>
        DefineRestart1 = 0xD1,

        /// <summary>
        ///  Restart with modulo 8 count 2
        /// </summary>
        DefineRestart2 = 0xD2,

        /// <summary>
        ///  Restart with modulo 8 count 3
        /// </summary>
        DefineRestart3 = 0xD3,

        /// <summary>
        ///  Restart with modulo 8 count 4
        /// </summary>
        DefineRestart4 = 0xD4,

        /// <summary>
        ///  Restart with modulo 8 count 5
        /// </summary>
        DefineRestart5 = 0xD5,

        /// <summary>
        ///  Restart with modulo 8 count 6
        /// </summary>
        DefineRestart6 = 0xD6,

        /// <summary>
        ///  Restart with modulo 8 count 7
        /// </summary>
        DefineRestart7 = 0xD7,

        /// <summary>
        /// Comment
        /// </summary>
        Comment = 0xFE,

        /// <summary>
        /// End of image
        /// </summary>
        EndOfImage = 0xD9,
    }
}
