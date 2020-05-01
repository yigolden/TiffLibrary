using System;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TiffLibrary.ImageSharpAdapter;
using TiffLibrary.PixelFormats;

namespace TiffLibrary
{
    /// <summary>
    /// Provides extension methods on <see cref="TiffImageEncoder{TPixel}"/> to work on ImageSharp image.
    /// </summary>
    public static class TiffImageEncoderImageSharpExtensions
    {
        /// <summary>
        /// Build the <see cref="TiffImageEncoder{TPixel}"/> instance with the specified pixel format of input image.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type of the input image.</typeparam>
        /// <returns>The <see cref="TiffImageEncoder{TPixel}"/> instance.</returns>
        public static TiffImageEncoder<TPixel> BuildForImageSharp<TPixel>(this TiffImageEncoderBuilder builder) where TPixel : unmanaged, IPixel<TPixel>
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (typeof(TPixel) == typeof(L8))
            {
                return new TiffImageSharpEncoder<TPixel, L8, TiffGray8>(builder.Build<TiffGray8>());
            }
            else if (typeof(TPixel) == typeof(L16))
            {
                return new TiffImageSharpEncoder<TPixel, L16, TiffGray16>(builder.Build<TiffGray16>());
            }
            else if (typeof(TPixel) == typeof(A8))
            {
                return new TiffImageSharpEncoder<TPixel, A8, TiffMask>(builder.Build<TiffMask>());
            }
            else if (typeof(TPixel) == typeof(Rgb24))
            {
                return new TiffImageSharpEncoder<TPixel, Rgb24, TiffRgb24>(builder.Build<TiffRgb24>());
            }
            else if (typeof(TPixel) == typeof(Rgba32))
            {
                return new TiffImageSharpEncoder<TPixel, Rgba32, TiffRgba32>(builder.Build<TiffRgba32>());
            }
            else if (typeof(TPixel) == typeof(Rgba64))
            {
                return new TiffImageSharpEncoder<TPixel, Rgba64, TiffRgba64>(builder.Build<TiffRgba64>());
            }
            else if (typeof(TPixel) == typeof(Bgr24))
            {
                return new TiffImageSharpEncoder<TPixel, Bgr24, TiffBgr24>(builder.Build<TiffBgr24>());
            }
            else if (typeof(TPixel) == typeof(Bgra32))
            {
                return new TiffImageSharpEncoder<TPixel, Bgra32, TiffBgra32>(builder.Build<TiffBgra32>());
            }
            else
            {
                return new TiffImageSharpEncoder<TPixel, Rgba32, TiffRgba32>(builder.Build<TiffRgba32>());
            }
        }

        /// <summary>
        /// Encode a single image without writing any IFD tag fields.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="encoder">The image encoder.</param>
        /// <param name="writer">The <see cref="TiffFileWriter"/> object to write encoded image data to.</param>
        /// <param name="image">The image to read from.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort the encoding pipeline.</param>
        /// <returns>A <see cref="Task{TiffStreamRegion}"/> that completes and return the position and length written into the stream when the image has been encoded.</returns>
        public static Task<TiffStreamRegion> EncodeAsync<TPixel>(this TiffImageEncoder<TPixel> encoder, TiffFileWriter writer, Image<TPixel> image, CancellationToken cancellationToken = default) where TPixel : unmanaged, IPixel<TPixel>
        {
            if (encoder is null)
            {
                throw new ArgumentNullException(nameof(encoder));
            }
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }
            if (image is null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            return encoder.EncodeAsync(writer, default, new TiffSize(image.Width, image.Height), new ImageSharpPixelBufferReader<TPixel>(image), cancellationToken);
        }

        /// <summary>
        /// Encode an image as well as associated tag fields into TIFF stream.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="encoder">The image encoder.</param>
        /// <param name="writer">The <see cref="TiffImageFileDirectoryWriter"/> object to write encoded image data and fields to.</param>
        /// <param name="image">The image to read from.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort the encoding pipeline.</param>
        /// <returns>A <see cref="Task"/> that completes when the image and fields have been encoded.</returns>
        public static Task EncodeAsync<TPixel>(this TiffImageEncoder<TPixel> encoder, TiffImageFileDirectoryWriter writer, Image<TPixel> image, CancellationToken cancellationToken = default) where TPixel : unmanaged, IPixel<TPixel>
        {
            if (encoder is null)
            {
                throw new ArgumentNullException(nameof(encoder));
            }
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }
            if (image is null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            return encoder.EncodeAsync(writer, default, new TiffSize(image.Width, image.Height), new ImageSharpPixelBufferReader<TPixel>(image), cancellationToken);
        }

        /// <summary>
        /// Encode a single image without writing any IFD tag fields.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="encoder">The image encoder.</param>
        /// <param name="writer">The <see cref="TiffFileWriter"/> object to write encoded image data to.</param>
        /// <param name="offset">The number of columns and rows to skip in <paramref name="image"/>.</param>
        /// <param name="size">The number of columns and rows to encode in <paramref name="image"/>.</param>
        /// <param name="image">The image to read from.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort the encoding pipeline.</param>
        /// <returns>A <see cref="Task{TiffStreamRegion}"/> that completes and return the position and length written into the stream when the image has been encoded.</returns>
        public static Task<TiffStreamRegion> EncodeAsync<TPixel>(this TiffImageEncoder<TPixel> encoder, TiffFileWriter writer, TiffPoint offset, TiffSize size, Image<TPixel> image, CancellationToken cancellationToken = default) where TPixel : unmanaged, IPixel<TPixel>
        {
            if (encoder is null)
            {
                throw new ArgumentNullException(nameof(encoder));
            }
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }
            if (image is null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            return encoder.EncodeAsync(writer, offset, size, new ImageSharpPixelBufferReader<TPixel>(image), cancellationToken);
        }

        /// <summary>
        /// Encode an image as well as associated tag fields into TIFF stream.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <param name="encoder">The image encoder.</param>
        /// <param name="writer">The <see cref="TiffImageFileDirectoryWriter"/> object to write encoded image data and fields to.</param>
        /// <param name="offset">The number of columns and rows to skip in <paramref name="image"/>.</param>
        /// <param name="size">The number of columns and rows to encode in <paramref name="image"/>.</param>
        /// <param name="image">The image to read from.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user has requested to abort the encoding pipeline.</param>
        /// <returns>A <see cref="Task"/> that completes when the image and fields have been encoded.</returns>
        public static Task EncodeAsync<TPixel>(this TiffImageEncoder<TPixel> encoder, TiffImageFileDirectoryWriter writer, TiffPoint offset, TiffSize size, Image<TPixel> image, CancellationToken cancellationToken = default) where TPixel : unmanaged, IPixel<TPixel>
        {
            if (encoder is null)
            {
                throw new ArgumentNullException(nameof(encoder));
            }
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }
            if (image is null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            return encoder.EncodeAsync(writer, offset, size, new ImageSharpPixelBufferReader<TPixel>(image), cancellationToken);
        }

    }
}
