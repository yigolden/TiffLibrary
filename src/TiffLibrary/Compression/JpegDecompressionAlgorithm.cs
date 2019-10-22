using System;
using System.IO;
using JpegLibrary;

namespace TiffLibrary.Compression
{
    /// <summary>
    /// Decompression support for JPEG.
    /// </summary>
    public sealed class JpegDecompressionAlgorithm : ITiffDecompressionAlgorithm
    {
        private readonly int _numberOfComponents;
        private JpegDecoder _decoder;
        private JpegBufferOutputWriter _outputWriter;

        /// <summary>
        /// Initialize the object.
        /// </summary>
        /// <param name="jpegTables">The JPEGTables field specified in the image file directory.</param>
        /// <param name="numberOfComponents">The number of component to be expected.</param>
        public JpegDecompressionAlgorithm(byte[] jpegTables, int numberOfComponents)
        {
            _decoder = new JpegDecoder();
            if (jpegTables?.Length > 0)
            {
                _decoder.LoadTables(jpegTables);
            }
            _outputWriter = new JpegBufferOutputWriter();
            _numberOfComponents = numberOfComponents;
        }

        /// <summary>
        /// Decompress the image data.
        /// </summary>
        /// <param name="context">Information about the TIFF file.</param>
        /// <param name="input">The input data.</param>
        /// <param name="output">The output data.</param>
        public void Decompress(TiffDecompressionContext context, ReadOnlyMemory<byte> input, Memory<byte> output)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // Identify this block
            JpegDecoder decoder = _decoder;
            decoder.SetInput(input);
            decoder.Identify();

            // Validate we are capable of decoding this.
            TiffSize outputBufferSize = context.ImageSize;
            if (decoder.Width < outputBufferSize.Width || decoder.Height < outputBufferSize.Height)
            {
                throw new InvalidDataException("Image dimension is too small.");
            }

            if (decoder.Precision != 8)
            {
                throw new InvalidDataException("Precision of 8 bit is expected.");
            }

            // Check number of components
            if (decoder.NumberOfComponents != _numberOfComponents)
            {
                throw new InvalidDataException($"Expect {_numberOfComponents} components, but got {decoder.NumberOfComponents} components in the JPEG stream.");
            }

            // Output writer 
            _outputWriter.Update(outputBufferSize.Width, context.SkippedScanlines, context.SkippedScanlines + context.RequestedScanlines, decoder.NumberOfComponents, output);

            // Decode
            decoder.SetOutputWriter(_outputWriter);
            decoder.Decode();

            // Reset state
            decoder.ResetInput();
            decoder.ResetHeader();
            decoder.ResetOutputWriter();
            _outputWriter.Reset();
        }
    }
}
