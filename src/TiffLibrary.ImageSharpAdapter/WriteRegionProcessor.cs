using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.Primitives;

namespace TiffLibrary.ImageSharpAdapter
{
    internal class WriteRegionProcessor<TSource> : IImageProcessor where TSource : struct, IPixel<TSource>
    {
        private readonly Image<TSource> _image;

        public WriteRegionProcessor(Image<TSource> image)
        {
            _image = image;
        }

        public IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Image<TPixel> source, Rectangle sourceRectangle) where TPixel : struct, IPixel<TPixel>
        {
            return new WriteRegionProcessor<TSource, TPixel>(_image, source, sourceRectangle);
        }
    }

    internal class WriteRegionProcessor<TSource, TDest> : IImageProcessor<TDest> where TSource : struct, IPixel<TSource> where TDest : struct, IPixel<TDest>
    {
        private readonly Image<TSource> _source;
        private readonly Image<TDest> _destination;
        private readonly Rectangle _copyRectange;


        public WriteRegionProcessor(Image<TSource> source, Image<TDest> destination, Rectangle copyRectange)
        {
            _source = source;
            _destination = destination;
            _copyRectange = copyRectange;
        }

        public void Apply()
        {
            Image<TSource> source = _source;
            Image<TDest> destination = _destination;
            Point sourceOffset = default;
            Point destinationOffset = new Point(_copyRectange.X, _copyRectange.Y);
            Size copySize = new Size(_copyRectange.Width, _copyRectange.Height);

            copySize = new Size(MathHelper.Clamp(copySize.Width, 0, source.Width), MathHelper.Clamp(copySize.Height, 0, source.Height));
            copySize = new Size(MathHelper.Clamp(copySize.Width, 0, destination.Width - destinationOffset.X), MathHelper.Clamp(copySize.Height, 0, destination.Height - destinationOffset.Y));

            if (destinationOffset.X < 0)
            {
                sourceOffset = new Point(sourceOffset.X - destinationOffset.X, sourceOffset.Y);
                copySize = new Size(copySize.Width + destinationOffset.X, copySize.Height);
                destinationOffset = new Point(0, destinationOffset.Y);
            }
            if (destinationOffset.Y < 0)
            {
                sourceOffset = new Point(sourceOffset.X, sourceOffset.Y - destinationOffset.Y);
                copySize = new Size(copySize.Width, copySize.Height + destinationOffset.Y);
                destinationOffset = new Point(destinationOffset.X, 0);
            }
            if (sourceOffset.X >= source.Width || sourceOffset.Y >= source.Height)
            {
                return;
            }
            if (copySize.Width <= 0 || copySize.Height <= 0)
            {
                return;
            }

            for (int row = 0; row < copySize.Height; row++)
            {
                int width = copySize.Width;
                ref TSource sourceSpan = ref Unsafe.Add(ref MemoryMarshal.GetReference(source.GetPixelRowSpan(sourceOffset.Y + row)), sourceOffset.X);
                ref TDest destinationSpan = ref Unsafe.Add(ref MemoryMarshal.GetReference(destination.GetPixelRowSpan(destinationOffset.Y + row)), destinationOffset.X);

                for (int i = 0; i < width; i++)
                {
                    Unsafe.Add(ref destinationSpan, i).FromScaledVector4(Unsafe.Add(ref sourceSpan, i).ToScaledVector4());
                }
            }
        }

        public void Dispose() { }
    }
}
