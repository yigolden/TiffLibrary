using System;
using System.Threading.Tasks;

namespace TiffLibrary.ImageEncoder
{
    /// <summary>
    /// A middlewares that expand the input image to a specified size.
    /// </summary>
    /// <typeparam name="TPixel">The pixel type.</typeparam>
    public sealed class TiffImageEncoderPaddingMiddleware<TPixel> : ITiffImageEncoderMiddleware<TPixel> where TPixel : unmanaged
    {
        private readonly TiffSize _paddingSize;

        /// <summary>
        /// Initialize the middleware with the specified padding size.
        /// </summary>
        /// <param name="paddingSize">The padding size.</param>
        public TiffImageEncoderPaddingMiddleware(TiffSize paddingSize)
        {
            _paddingSize = paddingSize;
        }

        /// <summary>
        /// Wraps the <paramref name="context"/> in a new context that handles extending the input image if either width or height of the input image is less than those specified in the constructor. Then runs the next middleware with the wrapped context.
        /// </summary>
        /// <param name="context">The encoder context.</param>
        /// <param name="next">The next middleware.</param>
        /// <returns>A <see cref="Task"/> that completes when the image has been encoded.</returns>
        public ValueTask InvokeAsync(TiffImageEncoderContext<TPixel> context, ITiffImageEncoderPipelineNode<TPixel> next)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (next is null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            TiffSize size = context.ImageSize;
            if (size.Width < _paddingSize.Width || size.Height < _paddingSize.Height)
            {
                size = new TiffSize(Math.Max(size.Width, _paddingSize.Width), Math.Max(size.Height, _paddingSize.Height));
                return next.RunAsync(new PaddingContext(context, size));
            }

            return next.RunAsync(context);
        }

        internal sealed class PaddingContext : TiffDelegatingImageEncoderContext<TPixel>
        {
            private TiffSize _size;

            public PaddingContext(TiffImageEncoderContext<TPixel> context, TiffSize size) : base(context)
            {
                TiffSize originalSize = context.ImageSize;
                if (size.Width < originalSize.Width || size.Height < originalSize.Height)
                {
                    throw new ArgumentOutOfRangeException(nameof(context));
                }
                _size = size;
            }

            public override TiffSize ImageSize { get => _size; set => throw new NotSupportedException(); }

            public override TiffPixelBufferReader<TPixel> GetReader()
            {
                return new PaddedPixelBufferReader(base.GetReader(), _size).AsPixelBufferReader();
            }

        }

        internal class PaddedPixelBufferReader : ITiffPixelBufferReader<TPixel>
        {
            private readonly TiffPixelBufferReader<TPixel> _reader;
            private readonly TiffSize _size;

            public PaddedPixelBufferReader(TiffPixelBufferReader<TPixel> reader, TiffSize size)
            {
                _reader = reader;
                _size = size;
            }

            public int Width => _size.Width;

            public int Height => _size.Height;

            public async ValueTask ReadAsync(TiffPoint offset, TiffPixelBufferWriter<TPixel> destination)
            {
                int width = 0, height = 0;
                if (offset.Y < _reader.Height)
                {
                    height = Math.Min(_reader.Height - offset.Y, destination.Height);

                    if (offset.X < _reader.Width)
                    {
                        width = Math.Min(_reader.Width - offset.X, destination.Width);

                        await _reader.ReadAsync(offset, destination.Crop(default, new TiffSize(width, height))).ConfigureAwait(false);
                    }

                    FillEmpty(destination.Crop(new TiffPoint(width, 0), new TiffSize(destination.Width - width, height)));
                }

                FillEmpty(destination.Crop(new TiffPoint(0, height), new TiffSize(destination.Width, destination.Height - height)));
            }

            private static void FillEmpty(TiffPixelBufferWriter<TPixel> destination)
            {
                for (int i = 0; i < destination.Height; i++)
                {
                    using PixelBuffer.TiffPixelSpanHandle<TPixel> handle = destination.GetRowSpan(i);
                    handle.GetSpan().Clear();
                }
            }
        }
    }
}
