using System;
using System.Buffers;
using System.Diagnostics;
using System.Threading;

namespace TiffLibrary.ImageDecoder
{
    internal class TiffDelegatingImageDecoderContext : TiffImageDecoderContext
    {
        private readonly TiffImageDecoderContext _innerContext;

        protected TiffImageDecoderContext InnerContext => _innerContext;

        public TiffDelegatingImageDecoderContext(TiffImageDecoderContext innerContext)
        {
            Debug.Assert(innerContext != null);
            _innerContext = innerContext!;
        }

        public override MemoryPool<byte>? MemoryPool { get => _innerContext.MemoryPool; set => _innerContext.MemoryPool = value; }
        public override CancellationToken CancellationToken { get => _innerContext.CancellationToken; set => _innerContext.CancellationToken = value; }
        public override TiffOperationContext? OperationContext { get => _innerContext.OperationContext; set => _innerContext.OperationContext = value; }
        public override TiffFileContentReader? ContentReader { get => _innerContext.ContentReader; set => _innerContext.ContentReader = value; }
        public override TiffValueCollection<TiffStreamRegion> PlanarRegions { get => _innerContext.PlanarRegions; set => _innerContext.PlanarRegions = value; }
        public override Memory<byte> UncompressedData { get => _innerContext.UncompressedData; set => _innerContext.UncompressedData = value; }
        public override TiffSize SourceImageSize { get => _innerContext.SourceImageSize; set => _innerContext.SourceImageSize = value; }
        public override TiffPoint SourceReadOffset { get => _innerContext.SourceReadOffset; set => _innerContext.SourceReadOffset = value; }
        public override TiffSize ReadSize { get => _innerContext.ReadSize; set => _innerContext.ReadSize = value; }

        public override TiffPixelBufferWriter<TPixel> GetWriter<TPixel>()
        {
            return _innerContext.GetWriter<TPixel>();
        }
    }
}
