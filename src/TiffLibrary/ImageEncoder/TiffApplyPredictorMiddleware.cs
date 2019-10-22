using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace TiffLibrary.ImageEncoder
{
    /// <summary>
    /// A middleware that apply predictor to the input image.
    /// </summary>
    /// <typeparam name="TPixel">The pixel type.</typeparam>
    public sealed class TiffApplyPredictorMiddleware<TPixel> : ITiffImageEncoderMiddleware<TPixel> where TPixel : unmanaged
    {
        private readonly TiffPredictor _predictor;

        /// <summary>
        /// Initialize the middleware with the specified predictor.
        /// </summary>
        /// <param name="predictor"></param>
        public TiffApplyPredictorMiddleware(TiffPredictor predictor)
        {
            _predictor = predictor;
        }

        /// <summary>
        /// Apply the predictor to <see cref="TiffImageEncoderContext{TPixel}.UncompressedData"/>, and runs the next middleware. Writes the <see cref="TiffTag.Predictor"/> tag to IFD writer.
        /// </summary>
        /// <param name="context">The encoder context.</param>
        /// <param name="next">The next middleware.</param>
        /// <returns>A <see cref="Task"/> that completes when the image has been encoded.</returns>
        public async Task InvokeAsync(TiffImageEncoderContext<TPixel> context, ITiffImageEncoderPipelineNode<TPixel> next)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (next is null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (_predictor == TiffPredictor.None)
            {
                await next.RunAsync(context).ConfigureAwait(false);
                return;
            }
            if (_predictor != TiffPredictor.HorizontalDifferencing)
            {
                throw new NotSupportedException("Predictor not supportted.");
            }

            TiffValueCollection<ushort> bitsPerSample = context.BitsPerSample;
            int totalBits = 0;
            for (int i = 0; i < bitsPerSample.Count; i++)
            {
                if (bitsPerSample[i] % 8 != 0)
                {
                    throw new InvalidOperationException("Horizontal differencing predictor can not be applied to this image.");
                }
                totalBits += bitsPerSample[i];
            }

            Memory<byte> pixelData = context.UncompressedData;

            int width = context.ImageSize.Width;
            int bytesPerScanlines = (totalBits / 8) * width;
            int height = context.ImageSize.Height;
            for (int row = 0; row < height; row++)
            {
                ApplyHorizontalDifferencingForScanline(pixelData.Slice(row * bytesPerScanlines, bytesPerScanlines), bitsPerSample, width);
            }

            await next.RunAsync(context).ConfigureAwait(false);

            TiffImageFileDirectoryWriter ifdWriter = context.IfdWriter;
            if (!(ifdWriter is null))
            {
                await ifdWriter.WriteTagAsync(TiffTag.Predictor, new TiffValueCollection<ushort>((ushort)_predictor)).ConfigureAwait(false);
            }
        }

        private static void ApplyHorizontalDifferencingForScanline(Memory<byte> scanline, TiffValueCollection<ushort> bitsPerSample, int width)
        {
            if (width <= 1)
            {
                return;
            }
            int sampleCount = bitsPerSample.Count;
            if (sampleCount > 8)
            {
                throw new NotSupportedException("Too many samples.");
            }

            Span<ushort> bitsPerSampleSpan = stackalloc ushort[8];
            ref ushort bitsPerSampleSpanRef = ref MemoryMarshal.GetReference(bitsPerSampleSpan);
            bitsPerSample.CopyTo(bitsPerSampleSpan);

            Span<uint> samples = stackalloc uint[8];
            ref uint samplesRef = ref MemoryMarshal.GetReference(samples);

            Span<byte> scanlineSpan = scanline.Span;
            var reader = new BitReader(scanlineSpan);
            var writer = new BitWriter(scanlineSpan);

            for (int sampleIndex = 0; sampleIndex < sampleCount; sampleIndex++)
            {
                int bits = Unsafe.Add(ref bitsPerSampleSpanRef, sampleIndex);
                if (bits > 32)
                {
                    throw new NotSupportedException("Bits too large.");
                }
                uint value = reader.Read(bits);
                Unsafe.Add(ref samplesRef, sampleIndex) = value;
                writer.Write(value, bits);
            }

            for (int col = 1; col < width; col++)
            {
                for (int sampleIndex = 0; sampleIndex < sampleCount; sampleIndex++)
                {
                    int bits = Unsafe.Add(ref bitsPerSampleSpanRef, sampleIndex);
                    uint value = reader.Read(bits);
                    writer.Write(value - Unsafe.Add(ref samplesRef, sampleIndex), bits);
                    Unsafe.Add(ref samplesRef, sampleIndex) = value;
                }
            }

            writer.Flush();
        }
    }
}
