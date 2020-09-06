using System;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TiffLibrary.PixelBuffer;

namespace TiffLibrary.ImageSharpAdapter
{
    internal sealed class ImageSharpPixelBufferReader<TPixel> : ITiffPixelBufferReader<TPixel> where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly ImageFrame<TPixel> _image;
        private readonly TiffPoint _offset;
        private readonly TiffSize _size;

        public ImageSharpPixelBufferReader(ImageFrame<TPixel> image)
        {
            if (image is null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            _image = image;
            _offset = default;
            _size = new TiffSize(image.Width, image.Height);
        }

        public ImageSharpPixelBufferReader(ImageFrame<TPixel> image, TiffPoint offset, TiffSize size)
        {
            if (image is null)
            {
                throw new ArgumentNullException(nameof(image));
            }
            if (offset.X < 0)
            {
                size = new TiffSize(size.Width + offset.X, size.Height);
                offset = new TiffPoint(0, offset.Y);
            }
            if (offset.Y < 0)
            {
                size = new TiffSize(size.Width, size.Height + offset.Y);
                offset = new TiffPoint(offset.X, 0);
            }

            _image = image;
            _offset = offset;
            _size = new TiffSize(MathHelper.Clamp(size.Width, 0, image.Width), MathHelper.Clamp(size.Height, 0, image.Height));
        }


        public int Width => _size.Width;

        public int Height => _size.Height;

        public ValueTask ReadAsync(TiffPoint offset, TiffPixelBufferWriter<TPixel> destination, CancellationToken cancellationToken)
        {
            if (offset.X >= (uint)_size.Width || offset.Y >= (uint)_size.Height)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            ImageFrame<TPixel> image = _image;
            int offsetX = _offset.X + offset.X;
            int offsetY = _offset.Y + offset.Y;
            int width = Math.Min(_size.Width - offset.X, destination.Width);
            int height = Math.Min(_size.Height - offset.Y, destination.Height);

            for (int row = 0; row < height; row++)
            {
                Span<TPixel> sourceSpan = image.GetPixelRowSpan(offsetY + row).Slice(offsetX, width);
                using TiffPixelSpanHandle<TPixel> destinationHandle = destination.GetRowSpan(row);
                sourceSpan.CopyTo(destinationHandle.GetSpan());
            }

            return default;
        }
    }
}
