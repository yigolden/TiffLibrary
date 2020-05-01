using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp.PixelFormats;

namespace TiffLibrary.ImageSharpAdapter
{
    internal sealed class TiffImageSharpEncoder<TExposedPixel, TIntermediate, TTiffPixel> : TiffImageEncoder<TExposedPixel>
        where TExposedPixel : unmanaged, IPixel<TExposedPixel>
        where TIntermediate : unmanaged, IPixel<TIntermediate>
        where TTiffPixel : unmanaged
    {
        private readonly TiffImageEncoder<TTiffPixel> _encoder;

        public TiffImageSharpEncoder(TiffImageEncoder<TTiffPixel> encoder)
        {
            Debug.Assert(Unsafe.SizeOf<TIntermediate>() == Unsafe.SizeOf<TTiffPixel>());
            _encoder = encoder ?? throw new ArgumentNullException(nameof(encoder));
        }

        public override Task<TiffStreamRegion> EncodeAsync(TiffFileWriter writer, TiffPoint offset, TiffSize size, ITiffPixelBufferReader<TExposedPixel> reader, CancellationToken cancellationToken = default)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }
            if (reader is null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (typeof(TExposedPixel) == typeof(TIntermediate))
            {
                return _encoder.EncodeAsync(writer, offset, size, new TiffPassthroughPixelBufferReader<TExposedPixel, TTiffPixel>(reader), cancellationToken);
            }

            return _encoder.EncodeAsync(writer, offset, size, new ImageSharpConversionPixelBufferReader<TExposedPixel, TIntermediate, TTiffPixel>(reader), cancellationToken);
        }

        public override Task EncodeAsync(TiffImageFileDirectoryWriter writer, TiffPoint offset, TiffSize size, ITiffPixelBufferReader<TExposedPixel> reader, CancellationToken cancellationToken = default)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }
            if (reader is null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (typeof(TExposedPixel) == typeof(TIntermediate))
            {
                return _encoder.EncodeAsync(writer, offset, size, new TiffPassthroughPixelBufferReader<TExposedPixel, TTiffPixel>(reader), cancellationToken);
            }

            return _encoder.EncodeAsync(writer, offset, size, new ImageSharpConversionPixelBufferReader<TExposedPixel, TIntermediate, TTiffPixel>(reader), cancellationToken);
        }
    }
}
