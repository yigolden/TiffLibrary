using System;
using System.Threading;
using JpegLibrary;

namespace TiffLibrary.Compression
{
    /// <summary>
    /// Decompression support for JPEG.
    /// </summary>
    public sealed class JpegDecompressionAlgorithm : ITiffDecompressionAlgorithm
    {
        private readonly int _numberOfComponents;
        private readonly byte[] _jpegTables;
        private JpegDecoder? _decoder;

        /// <summary>
        /// Initialize the object.
        /// </summary>
        /// <param name="jpegTables">The JPEGTables field specified in the image file directory.</param>
        /// <param name="numberOfComponents">The number of component to be expected.</param>
        public JpegDecompressionAlgorithm(byte[] jpegTables, int numberOfComponents)
        {
            _jpegTables = jpegTables;
            _decoder = LoadJpegDecoder();
            _numberOfComponents = numberOfComponents;
        }

        private JpegDecoder LoadJpegDecoder()
        {
            var decoder = new JpegDecoder();
            if (_jpegTables?.Length > 0)
            {
                decoder.LoadTables(_jpegTables);
            }
            return decoder;
        }

        /// <inheritdoc />
        public int Decompress(TiffDecompressionContext context, ReadOnlyMemory<byte> input, Memory<byte> output)
        {
            ThrowHelper.ThrowIfNull(context);

            // Identify this block
            JpegDecoder decoder = Interlocked.Exchange(ref _decoder, null) ?? LoadJpegDecoder();
            decoder.MemoryPool = context.MemoryPool;
            decoder.SetInput(input);
            decoder.Identify();

            // Validate we are capable of decoding this.
            TiffSize outputBufferSize = context.ImageSize;
            if (decoder.Width < outputBufferSize.Width || decoder.Height < outputBufferSize.Height)
            {
                ThrowHelper.ThrowInvalidDataException("Image dimension is too small.");
            }

            // Check number of components
            if (decoder.NumberOfComponents != _numberOfComponents)
            {
                ThrowHelper.ThrowInvalidDataException($"Expect {_numberOfComponents} components, but got {decoder.NumberOfComponents} components in the JPEG stream.");
            }

            JpegBlockOutputWriter outputWriter;
            if (decoder.Precision == 8)
            {
                if (context.BitsPerSample.GetFirstOrDefault() != 8)
                {
                    ThrowHelper.ThrowInvalidDataException("Precision of 8 bit is not expected.");
                }
                outputWriter = new JpegBuffer8BitOutputWriter(outputBufferSize.Width, context.SkippedScanlines, context.SkippedScanlines + context.RequestedScanlines, decoder.NumberOfComponents, output);
            }
            else if (decoder.Precision < 8)
            {
                if (context.BitsPerSample.GetFirstOrDefault() != 8)
                {
                    ThrowHelper.ThrowInvalidDataException($"Precision of {decoder.Precision} bit is not expected.");
                }
                outputWriter = new JpegBufferAny8BitOutputWriter(outputBufferSize.Width, context.SkippedScanlines, context.SkippedScanlines + context.RequestedScanlines, decoder.NumberOfComponents, decoder.Precision, output);
            }
            else if (decoder.Precision <= 16)
            {
                if (context.BitsPerSample.GetFirstOrDefault() != 16)
                {
                    ThrowHelper.ThrowInvalidDataException($"Precision of {decoder.Precision} bit is not expected.");
                }
                outputWriter = new JpegBufferAny16BitOutputWriter(outputBufferSize.Width, context.SkippedScanlines, context.SkippedScanlines + context.RequestedScanlines, decoder.NumberOfComponents, decoder.Precision, output);
            }
            else
            {
                ThrowHelper.ThrowInvalidDataException($"Precision of {decoder.Precision} bit is not expected.");
                return default;
            }

            // Decode
            decoder.SetOutputWriter(outputWriter);
            decoder.Decode();

            // Reset state
            decoder.ResetInput();
            decoder.ResetHeader();
            decoder.ResetOutputWriter();

            // Cache the instances
            Interlocked.CompareExchange(ref _decoder, decoder, null);

            return context.BytesPerScanline * context.ImageSize.Height;
        }
    }
}
