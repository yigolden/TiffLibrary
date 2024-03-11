﻿using System;
using System.Buffers;
using System.Threading;

namespace TiffLibrary.ImageEncoder
{
    internal class TiffDelegatingImageEncoderContext<TPixel> : TiffImageEncoderContext<TPixel> where TPixel : unmanaged
    {
        private TiffImageEncoderContext<TPixel> _innerContext;

        public TiffImageEncoderContext<TPixel> InnerContext => _innerContext;

        public TiffDelegatingImageEncoderContext(TiffImageEncoderContext<TPixel> innerContext)
        {
            _innerContext = innerContext;
        }

        protected void UpdateContext(TiffImageEncoderContext<TPixel> innerContext)
        {
            _innerContext = innerContext;
        }


        public override MemoryPool<byte>? MemoryPool { get => _innerContext.MemoryPool; set => _innerContext.MemoryPool = value; }
        public override CancellationToken CancellationToken { get => _innerContext.CancellationToken; set => _innerContext.CancellationToken = value; }
        public override TiffFileWriter? FileWriter { get => _innerContext.FileWriter; set => _innerContext.FileWriter = value; }
        public override TiffImageFileDirectoryWriter? IfdWriter { get => _innerContext.IfdWriter; set => _innerContext.IfdWriter = value; }
        public override TiffPhotometricInterpretation PhotometricInterpretation { get => _innerContext.PhotometricInterpretation; set => _innerContext.PhotometricInterpretation = value; }
        public override TiffValueCollection<ushort> BitsPerSample { get => _innerContext.BitsPerSample; set => _innerContext.BitsPerSample = value; }
        public override TiffSize ImageSize { get => _innerContext.ImageSize; set => _innerContext.ImageSize = value; }
        public override Memory<byte> UncompressedData { get => _innerContext.UncompressedData; set => _innerContext.UncompressedData = value; }

        public override TiffStreamRegion OutputRegion { get => _innerContext.OutputRegion; set => _innerContext.OutputRegion = value; }

        public override ITiffPixelBufferReader<TPixel> GetReader()
            => _innerContext.GetReader();

        public override TiffPixelBufferWriter<TPixel> ConvertWriter<TBuffer>(TiffPixelBufferWriter<TBuffer> writer)
            => _innerContext.ConvertWriter(writer);

        public override void RegisterService(Type serviceType, object? service)
            => _innerContext.RegisterService(serviceType, service);

        public override object? GetService(Type serviceType)
            => _innerContext.GetService(serviceType);

    }
}
