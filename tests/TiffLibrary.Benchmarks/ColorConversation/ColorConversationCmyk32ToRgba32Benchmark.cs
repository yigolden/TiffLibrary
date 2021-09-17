using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using TiffLibrary.ImageEncoder.PhotometricEncoder;
using TiffLibrary.PixelBuffer;
using TiffLibrary.PixelConverter;
using TiffLibrary.PixelFormats;

namespace TiffLibrary.Benchmarks.ColorConversation
{
    public class ColorConversationCmyk32ToRgba32Benchmark
    {
        private TiffCmyk32[] _source;
        private byte[] _destination;
        private ITiffPixelBufferWriter<TiffRgba32> _destinationWriter;

        public const int Length = 2000 * 8;

        [Params(10, 100, 500, 2000)]
        public int PixelsPerRow { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            _source = new TiffCmyk32[PixelsPerRow];
            new Random(42).NextBytes(MemoryMarshal.AsBytes(_source.AsSpan()));
            _destination = new byte[Length * Unsafe.SizeOf<TiffRgba32>()];
            _destinationWriter = new TiffMemoryPixelBufferWriter<TiffRgba32>(MemoryPool<byte>.Shared, _destination, PixelsPerRow, Length / PixelsPerRow);
        }

        [Benchmark]
        public void Run()
        {
            int width = PixelsPerRow;
            TiffCmyk32[] source = _source;
            var factory = new TiffDefaultPixelConverterFactory();
            ITiffPixelBufferWriter<TiffCmyk32> converter = factory.CreateConverter<TiffCmyk32, TiffRgba32>(_destinationWriter);
            for (int i = 0; i < converter.Height; i++)
            {
                using TiffPixelSpanHandle<TiffCmyk32> handle = converter.GetRowSpan(i, 0, width);
                source.CopyTo(handle.GetSpan());
            }
        }
    }
}
