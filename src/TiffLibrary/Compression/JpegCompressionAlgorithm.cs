using System;
using System.Buffers;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using JpegLibrary;
using TiffLibrary.ImageEncoder;

namespace TiffLibrary.Compression
{
    /// <summary>
    /// Compression support for JPEG.
    /// </summary>
    public sealed class JpegCompressionAlgorithm : ITiffCompressionAlgorithm
    {
        private const int MinimumBufferSegmentSize = 16384;

        private readonly TiffPhotometricInterpretation _photometricInterpretation;
        private readonly int _horizontalSubsampling;
        private readonly int _verticalSubsampling;
        private int _componentCount;
        private readonly bool _useSharedHuffmanTables;
        private readonly bool _useSharedQuantizationTables;
        private readonly bool _optimizeCoding;

        private TiffJpegEncoder? _encoder;
        private JpegBufferInputReader? _inputReader;

        /// <summary>
        /// Initialize the object.
        /// </summary>
        /// <param name="photometricInterpretation">The expected photometric interpretation.</param>
        /// <param name="quality">The quality factor to use when generating quantization table.</param>
        public JpegCompressionAlgorithm(TiffPhotometricInterpretation photometricInterpretation, int quality)
        {
            if ((uint)quality > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(quality));
            }
            _photometricInterpretation = photometricInterpretation;
            _horizontalSubsampling = 1;
            _verticalSubsampling = 1;
            _useSharedHuffmanTables = false;
            _useSharedQuantizationTables = false;
            _optimizeCoding = false;
            Initialize(quality, false);
        }

        /// <summary>
        /// Initialize the object.
        /// </summary>
        /// <param name="photometricInterpretation">The expected photometric interpretation.</param>
        /// <param name="horizontalSubsampling">The horizontal subsampling factor for YCbCr image.</param>
        /// <param name="verticalSubsampling">The vertical subsampling factor for YCbCr image.</param>
        /// <param name="options">Options to use when encoding with JPEG compression.</param>
        public JpegCompressionAlgorithm(TiffPhotometricInterpretation photometricInterpretation, int horizontalSubsampling, int verticalSubsampling, TiffJpegEncodingOptions? options)
        {
            options = options ?? TiffJpegEncodingOptions.Default;
            if ((uint)options.Quality > 100)
            {
                throw new ArgumentException("Quality should be between 0 and 100.");
            }
            _photometricInterpretation = photometricInterpretation;
            _horizontalSubsampling = horizontalSubsampling;
            _verticalSubsampling = verticalSubsampling;
            _useSharedHuffmanTables = options.UseSharedHuffmanTables && !options.OptimizeCoding;
            _useSharedQuantizationTables = options.UseSharedQuantizationTables;
            _optimizeCoding = options.OptimizeCoding;
            Initialize(options.Quality, options.OptimizeCoding);
        }

        private void Initialize(int quality, bool optimizeCoding)
        {
            TiffJpegEncoder encoder;
            switch (_photometricInterpretation)
            {
                case TiffPhotometricInterpretation.BlackIsZero:
                case TiffPhotometricInterpretation.WhiteIsZero:
                    _componentCount = 1;
                    encoder = new TiffJpegEncoder(minimumBufferSegmentSize: MinimumBufferSegmentSize);
                    encoder.SetQuantizationTable(JpegStandardQuantizationTable.ScaleByQuality(JpegStandardQuantizationTable.GetLuminanceTable(JpegElementPrecision.Precision8Bit, 0), quality));
                    if (optimizeCoding)
                    {
                        encoder.SetHuffmanTable(true, 0);
                        encoder.SetHuffmanTable(false, 0);
                    }
                    else
                    {
                        encoder.SetHuffmanTable(true, 0, JpegStandardHuffmanEncodingTable.GetLuminanceDCTable());
                        encoder.SetHuffmanTable(false, 0, JpegStandardHuffmanEncodingTable.GetLuminanceACTable());
                    }
                    encoder.AddComponent(0, 0, 0, 0, 1, 1); // Y component
                    break;
                case TiffPhotometricInterpretation.RGB:
                    _componentCount = 3;
                    encoder = new TiffJpegEncoder(minimumBufferSegmentSize: MinimumBufferSegmentSize);
                    encoder.SetQuantizationTable(JpegStandardQuantizationTable.ScaleByQuality(JpegStandardQuantizationTable.GetLuminanceTable(JpegElementPrecision.Precision8Bit, 0), quality));
                    if (optimizeCoding)
                    {
                        encoder.SetHuffmanTable(true, 0);
                        encoder.SetHuffmanTable(false, 0);
                    }
                    else
                    {
                        encoder.SetHuffmanTable(true, 0, JpegStandardHuffmanEncodingTable.GetLuminanceDCTable());
                        encoder.SetHuffmanTable(false, 0, JpegStandardHuffmanEncodingTable.GetLuminanceACTable());
                    }
                    encoder.AddComponent(0, 0, 0, 0, 1, 1); // R component
                    encoder.AddComponent(1, 0, 0, 0, 1, 1); // G component
                    encoder.AddComponent(2, 0, 0, 0, 1, 1); // B component
                    break;
                case TiffPhotometricInterpretation.Seperated:
                    _componentCount = 4;
                    encoder = new TiffJpegEncoder(minimumBufferSegmentSize: MinimumBufferSegmentSize);
                    encoder.SetQuantizationTable(JpegStandardQuantizationTable.ScaleByQuality(JpegStandardQuantizationTable.GetLuminanceTable(JpegElementPrecision.Precision8Bit, 0), quality));
                    if (optimizeCoding)
                    {
                        encoder.SetHuffmanTable(true, 0);
                        encoder.SetHuffmanTable(false, 0);
                    }
                    else
                    {
                        encoder.SetHuffmanTable(true, 0, JpegStandardHuffmanEncodingTable.GetLuminanceDCTable());
                        encoder.SetHuffmanTable(false, 0, JpegStandardHuffmanEncodingTable.GetLuminanceACTable());
                    }
                    encoder.AddComponent(0, 0, 0, 0, 1, 1); // C component
                    encoder.AddComponent(1, 0, 0, 0, 1, 1); // M component
                    encoder.AddComponent(2, 0, 0, 0, 1, 1); // Y component
                    encoder.AddComponent(3, 0, 0, 0, 1, 1); // K component
                    break;
                case TiffPhotometricInterpretation.YCbCr:
                    _componentCount = 3;
                    encoder = new TiffJpegEncoder(minimumBufferSegmentSize: MinimumBufferSegmentSize);
                    encoder.SetQuantizationTable(JpegStandardQuantizationTable.ScaleByQuality(JpegStandardQuantizationTable.GetLuminanceTable(JpegElementPrecision.Precision8Bit, 0), quality));
                    encoder.SetQuantizationTable(JpegStandardQuantizationTable.ScaleByQuality(JpegStandardQuantizationTable.GetChrominanceTable(JpegElementPrecision.Precision8Bit, 1), quality));
                    if (optimizeCoding)
                    {
                        encoder.SetHuffmanTable(true, 0);
                        encoder.SetHuffmanTable(false, 0);
                        encoder.SetHuffmanTable(true, 1);
                        encoder.SetHuffmanTable(false, 1);
                    }
                    else
                    {
                        encoder.SetHuffmanTable(true, 0, JpegStandardHuffmanEncodingTable.GetLuminanceDCTable());
                        encoder.SetHuffmanTable(false, 0, JpegStandardHuffmanEncodingTable.GetLuminanceACTable());
                        encoder.SetHuffmanTable(true, 1, JpegStandardHuffmanEncodingTable.GetChrominanceDCTable());
                        encoder.SetHuffmanTable(false, 1, JpegStandardHuffmanEncodingTable.GetChrominanceACTable());
                    }
                    encoder.AddComponent(0, 0, 0, 0, (byte)_horizontalSubsampling, (byte)_verticalSubsampling); // Y component
                    encoder.AddComponent(1, 1, 1, 1, 1, 1); // Cb component
                    encoder.AddComponent(2, 1, 1, 1, 1, 1); // Cr component
                    break;
                default:
                    throw new NotSupportedException("JPEG compression only supports BlackIsZero, WhiteIsZero, RGB, YCbCr and CMYK photometric interpretation.");
            }
            _encoder = encoder;
        }

        private void CheckBitsPerSample(TiffValueCollection<ushort> bitsPerSample)
        {
            if (bitsPerSample.Count != _componentCount)
            {
                throw new InvalidOperationException();
            }
            // Currently only 8 bit is supported.
            foreach (ushort item in bitsPerSample)
            {
                if (item != 8)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        /// <inheritdoc />
        public void Compress(TiffCompressionContext context, ReadOnlyMemory<byte> input, IBufferWriter<byte> outputWriter)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (outputWriter is null)
            {
                throw new ArgumentNullException(nameof(outputWriter));
            }

            if (_encoder is null)
            {
                throw new InvalidOperationException("JPEG encoder is not initialized.");
            }

            if (context.PhotometricInterpretation != _photometricInterpretation)
            {
                throw new InvalidOperationException();
            }
            CheckBitsPerSample(context.BitsPerSample);

            TiffJpegEncoder encoder = _encoder.CloneParameter();
            encoder.MemoryPool = context.MemoryPool;

            // Input
            JpegBufferInputReader inputReader = Interlocked.Exchange(ref _inputReader, null) ?? new JpegBufferInputReader();
            encoder.SetInputReader(inputReader);

            // Update InputReader
            inputReader.Update(context.ImageSize.Width, context.ImageSize.Height, _componentCount, input);

            // Output
            encoder.SetOutput(outputWriter);

            // Encoder
            encoder.Encode(!_useSharedHuffmanTables, !_useSharedQuantizationTables, _optimizeCoding);

            // Reset state
            inputReader.Reset();

            // Cache the input reader instance
            Interlocked.CompareExchange(ref _inputReader, inputReader, null);
        }

        /// <summary>
        /// The middleware that can be used to write JPEGTables field. It should be added to the encoding pipeline if useSharedJpegTables is set to true in the constructor.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type.</typeparam>
        /// <returns>The middleware to write JPEGTables field</returns>
        public ITiffImageEncoderMiddleware<TPixel> GetTableWriter<TPixel>() where TPixel : unmanaged
        {
            if (_encoder is null)
            {
                throw new InvalidOperationException("JPEG encoder is not initialized.");
            }

            return new JpegTableWriter<TPixel>(_encoder, _useSharedHuffmanTables, _useSharedQuantizationTables);
        }

        class JpegTableWriter<TPixel> : ITiffImageEncoderMiddleware<TPixel> where TPixel : unmanaged
        {
            private readonly TiffJpegEncoder _encoder;
            private readonly bool _useSharedHuffmanTables;
            private readonly bool _useSharedQuantizationTables;
            private byte[]? _jpegTables;

            public JpegTableWriter(TiffJpegEncoder encoder, bool useSharedHuffmanTables, bool useSharedQuantizationTables)
            {
                _encoder = encoder;
                _useSharedHuffmanTables = useSharedHuffmanTables;
                _useSharedQuantizationTables = useSharedQuantizationTables;
            }

            public ValueTask InvokeAsync(TiffImageEncoderContext<TPixel> context, ITiffImageEncoderPipelineNode<TPixel> next)
            {
                TiffImageFileDirectoryWriter? ifdWriter = context.IfdWriter;
                if (!(ifdWriter is null) && (_useSharedHuffmanTables || _useSharedQuantizationTables))
                {
                    if (_jpegTables is null)
                    {
                        InitializeTables();
                    }
                    return new ValueTask(WriteTablesAndContinueAsync(context, next));
                }

                return next.RunAsync(context);
            }

            private async Task WriteTablesAndContinueAsync(TiffImageEncoderContext<TPixel> context, ITiffImageEncoderPipelineNode<TPixel> next)
            {
                TiffImageFileDirectoryWriter? ifdWriter = context.IfdWriter;
                byte[]? jpegTables = _jpegTables;

                Debug.Assert(ifdWriter != null);
                Debug.Assert(jpegTables != null);
                await ifdWriter!.WriteTagAsync(TiffTag.JPEGTables, TiffFieldType.Undefined, TiffValueCollection.UnsafeWrap(jpegTables!)).ConfigureAwait(false);

                await next.RunAsync(context).ConfigureAwait(false);
            }

            private void InitializeTables()
            {
                using var buffer = new MemoryPoolBufferWriter();
                _encoder.WriteTables(buffer, _useSharedHuffmanTables, _useSharedQuantizationTables);
                _jpegTables = buffer.ToArray();
            }
        }

        class TiffJpegEncoder : JpegEncoder
        {
            public TiffJpegEncoder() { }
            public TiffJpegEncoder(int minimumBufferSegmentSize) : base(minimumBufferSegmentSize) { }

            public void WriteTables(IBufferWriter<byte> buffer, bool writeHuffmanTables, bool writeQuantizationTables)
            {
                if (buffer is null)
                {
                    throw new ArgumentNullException(nameof(buffer));
                }

                var writer = new JpegWriter(buffer, minimumBufferSize: MinimumBufferSegmentSize);

                WriteStartOfImage(ref writer);
                if (writeQuantizationTables)
                {
                    WriteQuantizationTables(ref writer);
                }
                if (writeHuffmanTables)
                {
                    WriteHuffmanTables(ref writer);
                }
                WriteEndOfImage(ref writer);

                writer.Flush();
            }

            public void Encode(bool writeHuffmanTables, bool writeQuantizationTables, bool optimizeCoding)
            {
                JpegWriter writer = CreateJpegWriter();

                WriteStartOfImage(ref writer);
                if (writeQuantizationTables)
                {
                    WriteQuantizationTables(ref writer);
                }
                JpegFrameHeader frameHeader = WriteStartOfFrame(ref writer);
                JpegBlockAllocator? allocator = optimizeCoding ? new JpegBlockAllocator(MemoryPool) : null;
                try
                {
                    if (!(allocator is null))
                    {
                        allocator.Allocate(frameHeader);
                        TransformBlocks(allocator);
                        BuildHuffmanTables(frameHeader, allocator, false);
                        WriteHuffmanTables(ref writer);
                        WriteStartOfScan(ref writer);
                        WritePreparedScanData(frameHeader, allocator, ref writer);
                    }
                    else
                    {
                        if (writeHuffmanTables)
                        {
                            WriteHuffmanTables(ref writer);
                        }
                        WriteStartOfScan(ref writer);
                        WriteScanData(ref writer);
                    }
                }
                finally
                {
                    allocator?.Dispose();
                }
                WriteEndOfImage(ref writer);

                writer.Flush();
            }

            public TiffJpegEncoder CloneParameter()
            {
                return CloneParameters<TiffJpegEncoder>();
            }
        }
    }
}
