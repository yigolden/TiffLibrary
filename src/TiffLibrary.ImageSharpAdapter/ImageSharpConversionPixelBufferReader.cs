using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp.PixelFormats;
using TiffLibrary.PixelBuffer;

namespace TiffLibrary.ImageSharpAdapter
{
    internal sealed class ImageSharpConversionPixelBufferReader<TImageSharpPixel, TIntermediate, TTiffPixel> : ITiffPixelBufferReader<TTiffPixel>
        where TImageSharpPixel : unmanaged, IPixel<TImageSharpPixel>
        where TIntermediate : unmanaged, IPixel<TIntermediate>
        where TTiffPixel : unmanaged
    {
        private readonly ITiffPixelBufferReader<TImageSharpPixel> _source;

        public ImageSharpConversionPixelBufferReader(ITiffPixelBufferReader<TImageSharpPixel> source)
        {
            Debug.Assert(Unsafe.SizeOf<TIntermediate>() == Unsafe.SizeOf<TTiffPixel>());
            _source = source;
        }

        public int Width => _source.Width;

        public int Height => _source.Height;

        public ValueTask ReadAsync(TiffPoint offset, TiffPixelBufferWriter<TTiffPixel> destination, CancellationToken cancellationToken)
        {
            ITiffPixelBufferWriter<TTiffPixel> destinationWriter = TiffPixelBufferUnsafeMarshal.GetBuffer(destination, out TiffPoint destinationOffset, out TiffSize size);
            TiffPixelBufferWriter<TImageSharpPixel> newDestination = new ImageSharpConversionPixelBufferWriter<TImageSharpPixel, TIntermediate, TTiffPixel>(destinationWriter).AsPixelBufferWriter().Crop(destinationOffset, size);
            return _source.ReadAsync(offset, newDestination, cancellationToken);
        }
    }
}
