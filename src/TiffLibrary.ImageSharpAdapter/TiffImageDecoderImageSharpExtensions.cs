using System;
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
        /// Decode the image into the specified pixel buffer writer.
        /// </summary>
        /// <param name="decoder">The decoder instance.</param>
        /// <param name="destinationImage">The destination image to write pixels into.</param>
        public static void Decode(this TiffImageDecoder decoder, Image destinationImage)
            => Decode(decoder, default, destinationImage);

        /// <summary>
        /// Decode the image into the specified pixel buffer writer.
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
        /// Decode the image into the specified pixel buffer writer.
        /// </summary>
        /// <param name="decoder">The decoder instance.</param>
        /// <param name="offset">Number of columns and rows to skip in the source image.</param>
        /// <param name="readSize">Number of columns and rows to read from the source image.</param>
        /// <param name="destinationImage">The destination image to write pixels into.</param>
        public static void Decode(this TiffImageDecoder decoder, TiffPoint offset, TiffSize readSize, Image destinationImage)
            => Decode(decoder, offset, readSize, default, destinationImage);

        /// <summary>
        /// Decode the image into the specified pixel buffer writer.
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
                decoder.Decode(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<L8, TiffGray8>(imageOfGray8));
            }
            else if (destinationImage is Image<L16> imageOfGray16)
            {
                decoder.Decode(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<L16, TiffGray16>(imageOfGray16));
            }
            else if (destinationImage is Image<A8> imageOfAlpha8)
            {
                decoder.Decode(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<A8, TiffMask>(imageOfAlpha8));
            }
            else if (destinationImage is Image<Rgb24> imageOfRgb24)
            {
                decoder.Decode(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<Rgb24, TiffRgb24>(imageOfRgb24));
            }
            else if (destinationImage is Image<Rgba32> imageOfRgba32)
            {
                decoder.Decode(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<Rgba32, TiffRgba32>(imageOfRgba32));
            }
            else if (destinationImage is Image<Rgba64> imageOfRgba64)
            {
                decoder.Decode(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<Rgba64, TiffRgba64>(imageOfRgba64));
            }
            else if (destinationImage is Image<Bgr24> imageOfBgr24)
            {
                decoder.Decode(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<Bgr24, TiffBgr24>(imageOfBgr24));
            }
            else if (destinationImage is Image<Bgra32> imageOfBgra32)
            {
                decoder.Decode(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<Bgra32, TiffBgra32>(imageOfBgra32));
            }
            else if (destinationImage is Image<Rgb48> imageOfRgb48)
            {
                var writer = new ImageSharpPixelBufferWriter<Rgb48, Rgb48>(imageOfRgb48);
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
                    decoder.Decode(offset, readSize, default, new ImageSharpPixelBufferWriter<Rgba32, TiffRgba32>(temp));
                    destinationImage.Mutate(ctx => ctx.ApplyProcessor(new WriteRegionProcessor<Rgba32>(temp), new Rectangle(destinationOffset.X, destinationOffset.Y, readSize.Width, readSize.Height)));
                }
            }
        }

        /// <summary>
        /// Decode the image into the specified pixel buffer writer.
        /// </summary>
        /// <param name="decoder">The decoder instance.</param>
        /// <param name="destinationImage">The destination image to write pixels into.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort the decoding pipeline.</param>
        /// <returns>A <see cref="Task"/> that completes when the image has been decoded.</returns>
        public static Task DecodeAsync(this TiffImageDecoder decoder, Image destinationImage, CancellationToken cancellationToken = default)
            => DecodeAsync(decoder, default, destinationImage, cancellationToken);

        /// <summary>
        /// Decode the image into the specified pixel buffer writer.
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
        /// Decode the image into the specified pixel buffer writer.
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
        /// Decode the image into the specified pixel buffer writer.
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
                return decoder.DecodeAsync(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<L8, TiffGray8>(imageOfGray8));
            }
            else if (destinationImage is Image<L16> imageOfGray16)
            {
                return decoder.DecodeAsync(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<L16, TiffGray16>(imageOfGray16));
            }
            else if (destinationImage is Image<A8> imageOfAlpha8)
            {
                return decoder.DecodeAsync(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<A8, TiffMask>(imageOfAlpha8));
            }
            else if (destinationImage is Image<Rgb24> imageOfRgb24)
            {
                return decoder.DecodeAsync(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<Rgb24, TiffRgb24>(imageOfRgb24));
            }
            else if (destinationImage is Image<Rgba32> imageOfRgba32)
            {
                return decoder.DecodeAsync(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<Rgba32, TiffRgba32>(imageOfRgba32));
            }
            else if (destinationImage is Image<Rgba64> imageOfRgba64)
            {
                return decoder.DecodeAsync(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<Rgba64, TiffRgba64>(imageOfRgba64));
            }
            else if (destinationImage is Image<Bgr24> imageOfBgr24)
            {
                return decoder.DecodeAsync(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<Bgr24, TiffBgr24>(imageOfBgr24));
            }
            else if (destinationImage is Image<Bgra32> imageOfBgra32)
            {
                return decoder.DecodeAsync(offset, readSize, destinationOffset, new ImageSharpPixelBufferWriter<Bgra32, TiffBgra32>(imageOfBgra32));
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
                    await decoder.DecodeAsync(offset, readSize, default, new ImageSharpPixelBufferWriter<Rgba32, TiffRgba32>(temp), cancellationToken).ConfigureAwait(false);
                    destinationImage.Mutate(ctx => ctx.ApplyProcessor(new WriteRegionProcessor<Rgba32>(temp), new Rectangle(destinationOffset.X, destinationOffset.Y, readSize.Width, readSize.Height)));
                }
            }
        }
    }
}
