using System;

namespace TiffLibrary.ImageDecoder
{
    internal sealed class TiffImageEnumeratorDecoderContext : TiffImageDecoderContext
    {
        private readonly TiffImageDecoderContext _innerContext;

        public TiffImageEnumeratorDecoderContext(TiffImageDecoderContext innerContext)
        {
            _innerContext = innerContext;
        }

        public override TiffOperationContext OperationContext { get => _innerContext.OperationContext; set => _innerContext.OperationContext = value; }
        public override TiffFileContentReader ContentReader { get => _innerContext.ContentReader; set => _innerContext.ContentReader = value; }
        public override Memory<byte> UncompressedData { get => _innerContext.UncompressedData; set => _innerContext.UncompressedData = value; }

        public override TiffValueCollection<TiffStreamRegion> PlanarRegions { get; set; }
        public override TiffSize SourceImageSize { get; set; }
        public override TiffPoint SourceReadOffset { get; set; }
        public override TiffSize ReadSize { get; set; }
        public TiffPoint CropOffset { get; set; }

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
            return _innerContext.GetWriter<TPixel>().Crop(CropOffset, ReadSize);
        }

    }
}
