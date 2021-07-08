using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using TiffLibrary.ImageSharpAdapter;
using TiffLibrary.PixelFormats;

namespace TiffLibrary
{
    /// <summary>
    /// Provides extension methods on <see cref="TiffImageDecoder"/> to work on ImageSharp image.
    /// </summary>
    public static class TiffImageDecoderImageSharpExtensions
    {
        /// <summary>
        /// Decode the image into the specified SixLabors.ImageSharp image buffer.
        /// </summary>
        /// <param name="decoder">The decoder instance.</param>
        /// <param name="destinationImage">The destination image to write pixels into.</param>
        public static void Decode(this TiffImageDecoder decoder, Image destinationImage)
            => Decode(decoder, default, destinationImage);

        /// <summary>
        /// Decode the image into the specified SixLabors.ImageSharp image buffer.
        /// </summary>
        /// <param name="decoder">The decoder instance.</param>
        /// <param name="offset">Number of columns and rows to skip in the source image.</param>
        /// <param name="destinationImage">The destination image to write pixels into.</param>
        public static void Decode(this TiffImageDecoder decoder, TiffPoint offset, Image destinationImage)
        {
            if (decoder is null)
            {
                throw new ArgumentNullException(nameof(decoder));
            }
            if (destinationImage is null)
            {
                throw new ArgumentNullException(nameof(destinationImage));
            }
            Decode(decoder, offset, new TiffSize(destinationImage.Width, destinationImage.Height), default, destinationImage);
        }

        /// <summary>
        /// Decode the image into the specified SixLabors.ImageSharp image buffer.
        /// </summary>
        /// <param name="decoder">The decoder instance.</param>
        /// <param name="offset">Number of columns and rows to skip in the source image.</param>
        /// <param name="readSize">Number of columns and rows to read from the source image.</param>
        /// <param name="destinationImage">The destination image to write pixels into.</param>
        public static void Decode(this TiffImageDecoder decoder, TiffPoint offset, TiffSize readSize, Image destinationImage)
            => Decode(decoder, offset, readSize, default, destinationImage);

        /// <summary>
        /// Decode the image into the specified SixLabors.ImageSharp image buffer.
        /// </summary>
        /// <param name="decoder">The decoder instance.</param>
        /// <param name="offset">Number of columns and rows to skip in the source image.</param>
        /// <param name="readSize">Number of columns and rows to read from the source image.</param>
        /// <param name="destinationOffset">Number of columns and rows to skip in the destination image.</param>
        /// <param name="destinationImage">The destination image to write pixels into.</param>
        public static void Decode(this TiffImageDecoder decoder, TiffPoint offset, TiffSize readSize, TiffPoint destinationOffset, Image destinationImage)
        {
            if (decoder is null)
            {
                throw new ArgumentNullException(nameof(decoder));
            }
            if (destinationImage is null)
            {
                throw new ArgumentNullException(nameof(destinationImage));
            }

            if (destinationImage is Image<L8> imageOfGray8)
            {
                decoder.Decode(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<L8, TiffGray8>(imageOfGray8.Frames.RootFrame));
            }
            else if (destinationImage is Image<L16> imageOfGray16)
            {
                decoder.Decode(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<L16, TiffGray16>(imageOfGray16.Frames.RootFrame));
            }
            else if (destinationImage is Image<A8> imageOfAlpha8)
            {
                decoder.Decode(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<A8, TiffMask>(imageOfAlpha8.Frames.RootFrame));
            }
            else if (destinationImage is Image<Rgb24> imageOfRgb24)
            {
                decoder.Decode(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<Rgb24, TiffRgb24>(imageOfRgb24.Frames.RootFrame));
            }
            else if (destinationImage is Image<Rgba32> imageOfRgba32)
            {
                decoder.Decode(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<Rgba32, TiffRgba32>(imageOfRgba32.Frames.RootFrame));
            }
            else if (destinationImage is Image<Rgba64> imageOfRgba64)
            {
                decoder.Decode(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<Rgba64, TiffRgba64>(imageOfRgba64.Frames.RootFrame));
            }
            else if (destinationImage is Image<Bgr24> imageOfBgr24)
            {
                decoder.Decode(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<Bgr24, TiffBgr24>(imageOfBgr24.Frames.RootFrame));
            }
            else if (destinationImage is Image<Bgra32> imageOfBgra32)
            {
                decoder.Decode(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<Bgra32, TiffBgra32>(imageOfBgra32.Frames.RootFrame));
            }
            else if (destinationImage is Image<Rgb48> imageOfRgb48)
            {
                var writer = new ImageSharpPixelBufferWriter<Rgb48, Rgb48>(imageOfRgb48.Frames.RootFrame);
                decoder.Decode(offset, readSize, destinationOffset, new ImageSharpConversionPixelBufferWriter2<TiffRgba64, Rgba64, Rgb48>(writer));
            }
            else
            {
                readSize = new TiffSize(Math.Max(0, Math.Min(destinationImage.Width - destinationOffset.X, readSize.Width)), Math.Max(0, Math.Min(destinationImage.Height - destinationOffset.Y, readSize.Height)));
                readSize = new TiffSize(Math.Max(0, Math.Min(decoder.Width - offset.X, readSize.Width)), Math.Max(0, Math.Min(decoder.Height - offset.Y, readSize.Height)));
                if (readSize.IsAreaEmpty)
                {
                    return;
                }

                using (var temp = new Image<Rgba32>(readSize.Width, readSize.Height))
                {
                    decoder.Decode(offset, readSize, default, new ImageSharpPixelBufferWriter<Rgba32, TiffRgba32>(temp.Frames.RootFrame));
                    destinationImage.Mutate(ctx => ctx.ApplyProcessor(new WriteRegionProcessor<Rgba32>(temp), new Rectangle(destinationOffset.X, destinationOffset.Y, readSize.Width, readSize.Height)));
                }
            }
        }

        /// <summary>
        /// Decode the image into the specified SixLabors.ImageSharp image buffer.
        /// </summary>
        /// <param name="decoder">The decoder instance.</param>
        /// <param name="destinationImage">The destination image to write pixels into.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort the decoding pipeline.</param>
        /// <returns>A <see cref="Task"/> that completes when the image has been decoded.</returns>
        public static Task DecodeAsync(this TiffImageDecoder decoder, Image destinationImage, CancellationToken cancellationToken = default)
            => DecodeAsync(decoder, default, destinationImage, cancellationToken);

        /// <summary>
        /// Decode the image into the specified SixLabors.ImageSharp image buffer.
        /// </summary>
        /// <param name="decoder">The decoder instance.</param>
        /// <param name="offset">Number of columns and rows to skip in the source image.</param>
        /// <param name="destinationImage">The destination image to write pixels into.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort the decoding pipeline.</param>
        /// <returns>A <see cref="Task"/> that completes when the image has been decoded.</returns>
        public static Task DecodeAsync(this TiffImageDecoder decoder, TiffPoint offset, Image destinationImage, CancellationToken cancellationToken = default)
        {
            if (decoder is null)
            {
                throw new ArgumentNullException(nameof(decoder));
            }
            if (destinationImage is null)
            {
                throw new ArgumentNullException(nameof(destinationImage));
            }

            return DecodeAsync(decoder, offset, new TiffSize(destinationImage.Width, destinationImage.Height), default, destinationImage, cancellationToken);
        }

        /// <summary>
        /// Decode the image into the specified SixLabors.ImageSharp image buffer.
        /// </summary>
        /// <param name="decoder">The decoder instance.</param>
        /// <param name="offset">Number of columns and rows to skip in the source image.</param>
        /// <param name="readSize">Number of columns and rows to read from the source image.</param>
        /// <param name="destinationImage">The destination image to write pixels into.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort the decoding pipeline.</param>
        /// <returns>A <see cref="Task"/> that completes when the image has been decoded.</returns>
        public static Task DecodeAsync(this TiffImageDecoder decoder, TiffPoint offset, TiffSize readSize, Image destinationImage, CancellationToken cancellationToken = default)
            => DecodeAsync(decoder, offset, readSize, default, destinationImage, cancellationToken);

        /// <summary>
        /// Decode the image into the specified SixLabors.ImageSharp image buffer.
        /// </summary>
        /// <param name="decoder">The decoder instance.</param>
        /// <param name="offset">Number of columns and rows to skip in the source image.</param>
        /// <param name="readSize">Number of columns and rows to read from the source image.</param>
        /// <param name="destinationOffset">Number of columns and rows to skip in the destination image.</param>
        /// <param name="destinationImage">The destination image to write pixels into.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort the decoding pipeline.</param>
        /// <returns>A <see cref="Task"/> that completes when the image has been decoded.</returns>
        public static Task DecodeAsync(this TiffImageDecoder decoder, TiffPoint offset, TiffSize readSize, TiffPoint destinationOffset, Image destinationImage, CancellationToken cancellationToken = default)
        {
            if (decoder is null)
            {
                throw new ArgumentNullException(nameof(decoder));
            }
            if (destinationImage is null)
            {
                throw new ArgumentNullException(nameof(destinationImage));
            }

            if (destinationImage is Image<L8> imageOfGray8)
            {
                return decoder.DecodeAsync(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<L8, TiffGray8>(imageOfGray8.Frames.RootFrame), cancellationToken);
            }
            else if (destinationImage is Image<L16> imageOfGray16)
            {
                return decoder.DecodeAsync(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<L16, TiffGray16>(imageOfGray16.Frames.RootFrame), cancellationToken);
            }
            else if (destinationImage is Image<A8> imageOfAlpha8)
            {
                return decoder.DecodeAsync(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<A8, TiffMask>(imageOfAlpha8.Frames.RootFrame), cancellationToken);
            }
            else if (destinationImage is Image<Rgb24> imageOfRgb24)
            {
                return decoder.DecodeAsync(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<Rgb24, TiffRgb24>(imageOfRgb24.Frames.RootFrame), cancellationToken);
            }
            else if (destinationImage is Image<Rgba32> imageOfRgba32)
            {
                return decoder.DecodeAsync(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<Rgba32, TiffRgba32>(imageOfRgba32.Frames.RootFrame), cancellationToken);
            }
            else if (destinationImage is Image<Rgba64> imageOfRgba64)
            {
                return decoder.DecodeAsync(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<Rgba64, TiffRgba64>(imageOfRgba64.Frames.RootFrame), cancellationToken);
            }
            else if (destinationImage is Image<Bgr24> imageOfBgr24)
            {
                return decoder.DecodeAsync(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<Bgr24, TiffBgr24>(imageOfBgr24.Frames.RootFrame), cancellationToken);
            }
            else if (destinationImage is Image<Bgra32> imageOfBgra32)
            {
                return decoder.DecodeAsync(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<Bgra32, TiffBgra32>(imageOfBgra32.Frames.RootFrame), cancellationToken);
            }
            else if (destinationImage is Image<Rgb48> imageOfRgb48)
            {
                var writer = new ImageSharpPixelBufferWriter<Rgb48, Rgb48>(imageOfRgb48.Frames.RootFrame);
                return decoder.DecodeAsync(offset, readSize, destinationOffset, new ImageSharpConversionPixelBufferWriter2<TiffRgba64, Rgba64, Rgb48>(writer), cancellationToken);
            }
            else
            {
                return DecodeSlowAsync(decoder, offset, readSize, destinationOffset, destinationImage, cancellationToken);
            }

            static async Task DecodeSlowAsync(TiffImageDecoder decoder, TiffPoint offset, TiffSize readSize, TiffPoint destinationOffset, Image destinationImage, CancellationToken cancellationToken)
            {
                readSize = new TiffSize(Math.Max(0, Math.Min(destinationImage.Width - destinationOffset.X, readSize.Width)), Math.Max(0, Math.Min(destinationImage.Height - destinationOffset.Y, readSize.Height)));
                readSize = new TiffSize(Math.Max(0, Math.Min(decoder.Width - offset.X, readSize.Width)), Math.Max(0, Math.Min(decoder.Height - offset.Y, readSize.Height)));
                if (readSize.IsAreaEmpty)
                {
                    return;
                }

                using (var temp = new Image<Rgba32>(readSize.Width, readSize.Height))
                {
                    await decoder.DecodeAsync(offset, readSize, default, new ImageSharpPixelBufferWriter<Rgba32, TiffRgba32>(temp.Frames.RootFrame), cancellationToken).ConfigureAwait(false);
                    destinationImage.Mutate(ctx => ctx.ApplyProcessor(new WriteRegionProcessor<Rgba32>(temp), new Rectangle(destinationOffset.X, destinationOffset.Y, readSize.Width, readSize.Height)));
                }
            }
        }


        #region Frames

        /// <summary>
        /// Decode the image into the specified SixLabors.ImageSharp frame buffer.
        /// </summary>
        /// <param name="decoder">The decoder instance.</param>
        /// <param name="destinationFrame">The destination frame to write pixels into.</param>
        public static void Decode<TPixel>(this TiffImageDecoder decoder, ImageFrame<TPixel> destinationFrame) where TPixel : unmanaged, IPixel<TPixel>
            => Decode(decoder, default, destinationFrame);

        /// <summary>
        /// Decode the image into the specified SixLabors.ImageSharp frame buffer.
        /// </summary>
        /// <param name="decoder">The decoder instance.</param>
        /// <param name="offset">Number of columns and rows to skip in the source image.</param>
        /// <param name="destinationFrame">The destination frame to write pixels into.</param>
        public static void Decode<TPixel>(this TiffImageDecoder decoder, TiffPoint offset, ImageFrame<TPixel> destinationFrame) where TPixel : unmanaged, IPixel<TPixel>
        {
            if (decoder is null)
            {
                throw new ArgumentNullException(nameof(decoder));
            }
            if (destinationFrame is null)
            {
                throw new ArgumentNullException(nameof(destinationFrame));
            }
            Decode(decoder, offset, new TiffSize(destinationFrame.Width, destinationFrame.Height), default, destinationFrame);
        }

        /// <summary>
        /// Decode the image into the specified SixLabors.ImageSharp frame buffer.
        /// </summary>
        /// <param name="decoder">The decoder instance.</param>
        /// <param name="offset">Number of columns and rows to skip in the source image.</param>
        /// <param name="readSize">Number of columns and rows to read from the source image.</param>
        /// <param name="destinationFrame">The destination frame to write pixels into.</param>
        public static void Decode<TPixel>(this TiffImageDecoder decoder, TiffPoint offset, TiffSize readSize, ImageFrame<TPixel> destinationFrame) where TPixel : unmanaged, IPixel<TPixel>
            => Decode(decoder, offset, readSize, default, destinationFrame);

        /// <summary>
        /// Decode the image into the specified SixLabors.ImageSharp frame buffer.
        /// </summary>
        /// <param name="decoder">The decoder instance.</param>
        /// <param name="offset">Number of columns and rows to skip in the source image.</param>
        /// <param name="readSize">Number of columns and rows to read from the source image.</param>
        /// <param name="destinationOffset">Number of columns and rows to skip in the destination frame.</param>
        /// <param name="destinationFrame">The destination frame to write pixels into.</param>
        public static void Decode<TPixel>(this TiffImageDecoder decoder, TiffPoint offset, TiffSize readSize, TiffPoint destinationOffset, ImageFrame<TPixel> destinationFrame) where TPixel : unmanaged, IPixel<TPixel>
        {
            if (decoder is null)
            {
                throw new ArgumentNullException(nameof(decoder));
            }
            if (destinationFrame is null)
            {
                throw new ArgumentNullException(nameof(destinationFrame));
            }

            if (typeof(TPixel) == typeof(L8))
            {
                decoder.Decode(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<L8, TiffGray8>(Unsafe.As<ImageFrame<L8>>(destinationFrame)));
            }
            else if (typeof(TPixel) == typeof(L16))
            {
                decoder.Decode(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<L16, TiffGray16>(Unsafe.As<ImageFrame<L16>>(destinationFrame)));
            }
            else if (typeof(TPixel) == typeof(A8))
            {
                decoder.Decode(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<A8, TiffMask>(Unsafe.As<ImageFrame<A8>>(destinationFrame)));
            }
            else if (typeof(TPixel) == typeof(Rgb24))
            {
                decoder.Decode(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<Rgb24, TiffRgb24>(Unsafe.As<ImageFrame<Rgb24>>(destinationFrame)));
            }
            else if (typeof(TPixel) == typeof(Rgba32))
            {
                decoder.Decode(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<Rgba32, TiffRgba32>(Unsafe.As<ImageFrame<Rgba32>>(destinationFrame)));
            }
            else if (typeof(TPixel) == typeof(Rgba64))
            {
                decoder.Decode(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<Rgba64, TiffRgba64>(Unsafe.As<ImageFrame<Rgba64>>(destinationFrame)));
            }
            else if (typeof(TPixel) == typeof(Bgr24))
            {
                decoder.Decode(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<Bgr24, TiffBgr24>(Unsafe.As<ImageFrame<Bgr24>>(destinationFrame)));
            }
            else if (typeof(TPixel) == typeof(Bgra32))
            {
                decoder.Decode(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<Bgra32, TiffBgra32>(Unsafe.As<ImageFrame<Bgra32>>(destinationFrame)));
            }
            else if (typeof(TPixel) == typeof(Rgb48))
            {
                var writer = new ImageSharpPixelBufferWriter<Rgb48, Rgb48>(Unsafe.As<ImageFrame<Rgb48>>(destinationFrame));
                decoder.Decode(offset, readSize, destinationOffset, new ImageSharpConversionPixelBufferWriter2<TiffRgba64, Rgba64, Rgb48>(writer));
            }
            else
            {
                var writer = new ImageSharpPixelBufferWriter<TPixel, TPixel>(destinationFrame);
                decoder.Decode(offset, readSize, destinationOffset, new ImageSharpConversionPixelBufferWriter2<TiffRgba32, Rgba32, TPixel>(writer));
            }
        }

        /// <summary>
        /// Decode the image into the specified SixLabors.ImageSharp frame buffer.
        /// </summary>
        /// <param name="decoder">The decoder instance.</param>
        /// <param name="destinationFrame">The destination frame to write pixels into.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort the decoding pipeline.</param>
        /// <returns>A <see cref="Task"/> that completes when the image has been decoded.</returns>
        public static Task DecodeAsync<TPixel>(this TiffImageDecoder decoder, ImageFrame<TPixel> destinationFrame, CancellationToken cancellationToken = default) where TPixel : unmanaged, IPixel<TPixel>
            => DecodeAsync(decoder, default, destinationFrame, cancellationToken);

        /// <summary>
        /// Decode the image into the specified SixLabors.ImageSharp frame buffer.
        /// </summary>
        /// <param name="decoder">The decoder instance.</param>
        /// <param name="offset">Number of columns and rows to skip in the source image.</param>
        /// <param name="destinationFrame">The destination frame to write pixels into.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort the decoding pipeline.</param>
        /// <returns>A <see cref="Task"/> that completes when the image has been decoded.</returns>
        public static Task DecodeAsync<TPixel>(this TiffImageDecoder decoder, TiffPoint offset, ImageFrame<TPixel> destinationFrame, CancellationToken cancellationToken = default) where TPixel : unmanaged, IPixel<TPixel>
        {
            if (decoder is null)
            {
                throw new ArgumentNullException(nameof(decoder));
            }
            if (destinationFrame is null)
            {
                throw new ArgumentNullException(nameof(destinationFrame));
            }

            return DecodeAsync(decoder, offset, new TiffSize(destinationFrame.Width, destinationFrame.Height), default, destinationFrame, cancellationToken);
        }

        /// <summary>
        /// Decode the image into the specified SixLabors.ImageSharp frame buffer.
        /// </summary>
        /// <param name="decoder">The decoder instance.</param>
        /// <param name="offset">Number of columns and rows to skip in the source image.</param>
        /// <param name="readSize">Number of columns and rows to read from the source image.</param>
        /// <param name="destinationFrame">The destination frame to write pixels into.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort the decoding pipeline.</param>
        /// <returns>A <see cref="Task"/> that completes when the image has been decoded.</returns>
        public static Task DecodeAsync<TPixel>(this TiffImageDecoder decoder, TiffPoint offset, TiffSize readSize, ImageFrame<TPixel> destinationFrame, CancellationToken cancellationToken = default) where TPixel : unmanaged, IPixel<TPixel>
            => DecodeAsync(decoder, offset, readSize, default, destinationFrame, cancellationToken);

        /// <summary>
        /// Decode the image into the specified SixLabors.ImageSharp frame buffer.
        /// </summary>
        /// <param name="decoder">The decoder instance.</param>
        /// <param name="offset">Number of columns and rows to skip in the source image.</param>
        /// <param name="readSize">Number of columns and rows to read from the source image.</param>
        /// <param name="destinationOffset">Number of columns and rows to skip in the destination frame.</param>
        /// <param name="destinationFrame">The destination frame to write pixels into.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort the decoding pipeline.</param>
        /// <returns>A <see cref="Task"/> that completes when the image has been decoded.</returns>
        public static Task DecodeAsync<TPixel>(this TiffImageDecoder decoder, TiffPoint offset, TiffSize readSize, TiffPoint destinationOffset, ImageFrame<TPixel> destinationFrame, CancellationToken cancellationToken = default) where TPixel : unmanaged, IPixel<TPixel>
        {
            if (decoder is null)
            {
                throw new ArgumentNullException(nameof(decoder));
            }
            if (destinationFrame is null)
            {
                throw new ArgumentNullException(nameof(destinationFrame));
            }

            if (typeof(TPixel) == typeof(L8))
            {
                return decoder.DecodeAsync(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<L8, TiffGray8>(Unsafe.As<ImageFrame<L8>>(destinationFrame)), cancellationToken);
            }
            else if (typeof(TPixel) == typeof(L16))
            {
                return decoder.DecodeAsync(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<L16, TiffGray16>(Unsafe.As<ImageFrame<L16>>(destinationFrame)), cancellationToken);
            }
            else if (typeof(TPixel) == typeof(A8))
            {
                return decoder.DecodeAsync(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<A8, TiffMask>(Unsafe.As<ImageFrame<A8>>(destinationFrame)), cancellationToken);
            }
            else if (typeof(TPixel) == typeof(Rgb24))
            {
                return decoder.DecodeAsync(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<Rgb24, TiffRgb24>(Unsafe.As<ImageFrame<Rgb24>>(destinationFrame)), cancellationToken);
            }
            else if (typeof(TPixel) == typeof(Rgba32))
            {
                return decoder.DecodeAsync(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<Rgba32, TiffRgba32>(Unsafe.As<ImageFrame<Rgba32>>(destinationFrame)), cancellationToken);
            }
            else if (typeof(TPixel) == typeof(Rgba64))
            {
                return decoder.DecodeAsync(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<Rgba64, TiffRgba64>(Unsafe.As<ImageFrame<Rgba64>>(destinationFrame)), cancellationToken);
            }
            else if (typeof(TPixel) == typeof(Bgr24))
            {
                return decoder.DecodeAsync(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<Bgr24, TiffBgr24>(Unsafe.As<ImageFrame<Bgr24>>(destinationFrame)), cancellationToken);
            }
            else if (typeof(TPixel) == typeof(Bgra32))
            {
                return decoder.DecodeAsync(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<Bgra32, TiffBgra32>(Unsafe.As<ImageFrame<Bgra32>>(destinationFrame)), cancellationToken);
            }
            else if (typeof(TPixel) == typeof(Rgb48))
            {
                var writer = new ImageSharpPixelBufferWriter<Rgb48, Rgb48>(Unsafe.As<ImageFrame<Rgb48>>(destinationFrame));
                return decoder.DecodeAsync(offset, readSize, destinationOffset, new ImageSharpConversionPixelBufferWriter2<TiffRgba64, Rgba64, Rgb48>(writer), cancellationToken);
            }
            else
            {
                var writer = new ImageSharpPixelBufferWriter<TPixel, TPixel>(destinationFrame);
                return decoder.DecodeAsync(offset, readSize, destinationOffset, new ImageSharpConversionPixelBufferWriter2<TiffRgba32, Rgba32, TPixel>(writer), cancellationToken);
            }
        }

        #endregion
    }
}
