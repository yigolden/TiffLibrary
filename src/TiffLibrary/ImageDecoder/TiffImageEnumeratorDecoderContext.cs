using System;

namespace TiffLibrary.ImageDecoder
{
    internal sealed class TiffImageEnumeratorDecoderContext : TiffDelegatingImageDecoderContext
    {
        public TiffImageEnumeratorDecoderContext(TiffImageDecoderContext innerContext) : base(innerContext) { }

        public override TiffValueCollection<TiffStreamRegion> PlanarRegions { get; set; }
        public override TiffSize SourceImageSize { get; set; }
        public override TiffPoint SourceReadOffset { get; set; }
        public override TiffSize ReadSize { get; set; }
        public TiffPoint CropOffset { get; set; }

        // Don't delegate UncompressedData into the underlying context.
        // We don't want it to be shared across differten strips/tiles when decoding in parallel.
        public override Memory<byte> UncompressedData { get; set; }

        public void SetCropSize(TiffPoint offset, TiffSize originalReadSize)
        {
            CropOffset = offset;

            int copySizeX = Math.Max(0, Math.Min(SourceImageSize.Width - SourceReadOffset.X, ReadSize.Width));
            int copySizeY = Math.Max(0, Math.Min(SourceImageSize.Height - SourceReadOffset.Y, ReadSize.Height));
            copySizeX = Math.Max(0, Math.Min(originalReadSize.Width - offset.X, copySizeX));
            copySizeY = Math.Max(0, Math.Min(originalReadSize.Height - offset.Y, copySizeY));

            ReadSize = new TiffSize(copySizeX, copySizeY);
        }

        public override TiffPixelBufferWriter<TPixel> GetWriter<TPixel>()
        {
            return InnerContext.GetWriter<TPixel>().Crop(CropOffset, ReadSize);
        }

    }
}
