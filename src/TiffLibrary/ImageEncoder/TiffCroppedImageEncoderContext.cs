using System;

namespace TiffLibrary.ImageEncoder
{
    internal sealed class TiffCroppedImageEncoderContext<TPixel> : TiffDelegatingImageEncoderContext<TPixel> where TPixel : unmanaged
    {

        public TiffCroppedImageEncoderContext(TiffImageEncoderContext<TPixel> innerContext) : base(innerContext) { }

        public TiffSize OriginalSize => InnerContext.ImageSize;

        public TiffPoint ImageOffset { get; set; }
        public override TiffSize ImageSize { get; set; }

        public bool ExposeIfdWriter { get; set; }
        public override TiffImageFileDirectoryWriter IfdWriter { get => ExposeIfdWriter ? InnerContext.IfdWriter : null; set => throw new NotSupportedException(); }

        public void Crop(TiffPoint offset, TiffSize size)
        {
            ImageOffset = offset;
            ImageSize = size;
        }

        public override TiffPixelBufferReader<TPixel> GetReader()
        {
            return InnerContext.GetReader().Crop(ImageOffset, ImageSize);
        }
    }
}
