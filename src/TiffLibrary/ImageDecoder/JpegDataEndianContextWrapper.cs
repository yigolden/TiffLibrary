using System;

namespace TiffLibrary.ImageDecoder
{
    internal sealed class JpegDataEndianContextWrapper : TiffDelegatingImageDecoderContext
    {
        public JpegDataEndianContextWrapper(TiffImageDecoderContext innerContext) : base(innerContext) { }

        public override bool IsLittleEndian => BitConverter.IsLittleEndian;
    }
}
