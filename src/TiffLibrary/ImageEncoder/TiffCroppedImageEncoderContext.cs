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
        public override TiffImageFileDirectoryWriter? IfdWriter { get => ExposeIfdWriter ? InnerContext.IfdWriter : null; set => ThrowHelper.ThrowNotSupportedException(); }

        // Don't delegate UncompressedData into the underlying context.
        // We don't want it to be shared across differten strips/tiles when decoding in parallel.
        public override Memory<byte> UncompressedData { get; set; }

        // Don't delegate OutputRegion into the underlying context.
        // We don't want it to be shared across differten strips/tiles when decoding in parallel.
        public override TiffStreamRegion OutputRegion { get; set; }

        // Don't delegate BitsPerSample into the underlying context.
        // We don't want it to be shared across differten strips/tiles when decoding in parallel.
        public override TiffValueCollection<ushort> BitsPerSample { get; set; }

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
