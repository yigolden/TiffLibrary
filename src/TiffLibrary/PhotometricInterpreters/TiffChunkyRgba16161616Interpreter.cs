using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TiffLibrary.ImageDecoder;
using TiffLibrary.PixelBuffer;
using TiffLibrary.PixelFormats;

namespace TiffLibrary.PhotometricInterpreters
{
    /// <summary>
    /// A middleware to read 16-bit RGBA pixels from uncompressed data to destination buffer writer.
    /// </summary>
    public sealed class TiffChunkyRgba16161616Interpreter : ITiffImageDecoderMiddleware
    {
        private readonly bool _isAlphaAssociated;
        private readonly bool _undoColorPreMultiplying;

        /// <summary>
        /// Initialize the middleware.
        /// </summary>
        /// <param name="isAlphaAssociated">Whether the alpha channel is associated.</param>
        /// <param name="undoColorPreMultiplying">Whether to undo color pre-multiplying.</param>
        public TiffChunkyRgba16161616Interpreter(bool isAlphaAssociated, bool undoColorPreMultiplying)
        {
            _isAlphaAssociated = isAlphaAssociated;
            _undoColorPreMultiplying = undoColorPreMultiplying;
        }

        /// <summary>
        /// Run this middleware.
        /// </summary>
        /// <param name="context">Information of the current decoding process.</param>
        /// <param name="next">The next middleware in the decoder pipeline.</param>
        /// <returns>A <see cref="Task"/> that completes when this middleware completes running.</returns>
        public ValueTask InvokeAsync(TiffImageDecoderContext context, ITiffImageDecoderPipelineNode next)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (next is null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (!_isAlphaAssociated)
            {
                ProcessUnassociated(context);
            }
            else if (_undoColorPreMultiplying)
            {
                ProcessAssociatedWithUndoColorPreMultiplying(context);
            }
            else
            {
                ProcessAssociatedPreservingColorPreMultiplying(context);
            }

            return next.RunAsync(context);
        }


        private static void ProcessUnassociated(TiffImageDecoderContext context)
        {
            int bytesPerScanline = 8 * context.SourceImageSize.Width;
            Memory<byte> source = context.UncompressedData.Slice(context.SourceReadOffset.Y * bytesPerScanline);
            ReadOnlySpan<byte> sourceSpan = source.Span;

            using TiffPixelBufferWriter<TiffBgra64> writer = context.GetWriter<TiffBgra64>();

            TiffOperationContext? operationContext = context.OperationContext;
            if (operationContext is null)
            {
                throw new InvalidOperationException("Failed to acquire OperationContext.");
            }

            int rows = context.ReadSize.Height;
            for (int row = 0; row < rows; row++)
            {
                using TiffPixelSpanHandle<TiffBgra64> pixelSpanHandle = writer.GetRowSpan(row);
                Span<byte> rowDestinationSpan = MemoryMarshal.Cast<TiffBgra64, byte>(pixelSpanHandle.GetSpan());
                CopyScanlineRgbaToBgra(sourceSpan.Slice(8 * context.SourceReadOffset.X, 8 * context.ReadSize.Width), rowDestinationSpan, context.ReadSize.Width, operationContext.IsLittleEndian == BitConverter.IsLittleEndian);
                sourceSpan = sourceSpan.Slice(bytesPerScanline);
            }
        }

        private static void CopyScanlineRgbaToBgra(ReadOnlySpan<byte> source, Span<byte> destination, int count, bool endiannessMatches)
        {
            if (source.Length < 8 * count)
            {
                throw new ArgumentException("source too short.", nameof(source));
            }
            if (destination.Length < 8 * count)
            {
                throw new ArgumentException("destination too short.", nameof(destination));
            }

            if (endiannessMatches)
            {
                for (int i = 0; i < count; i++)
                {
                    ulong value = (uint)(source[7] << 8) | source[6]; // a
                    value = (value << 16) | (uint)(source[1] << 8) | source[0]; // r
                    value = (value << 16) | (uint)(source[3] << 8) | source[2]; // g
                    value = (value << 16) | (uint)(source[5] << 8) | source[4]; // b

                    BinaryPrimitives.WriteUInt64LittleEndian(destination, value);

                    source = source.Slice(8);
                    destination = destination.Slice(8);
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    ulong value = (uint)(source[6] << 8) | source[7]; // a
                    value = (value << 16) | (uint)(source[0] << 8) | source[1]; // r
                    value = (value << 16) | (uint)(source[2] << 8) | source[3]; // g
                    value = (value << 16) | (uint)(source[4] << 8) | source[5]; // b

                    BinaryPrimitives.WriteUInt64LittleEndian(destination, value);

                    source = source.Slice(8);
                    destination = destination.Slice(8);
                }
            }
        }

        private static void ProcessAssociatedWithUndoColorPreMultiplying(TiffImageDecoderContext context)
        {
            int bytesPerScanline = 8 * context.SourceImageSize.Width;
            Memory<byte> source = context.UncompressedData.Slice(context.SourceReadOffset.Y * bytesPerScanline);
            ReadOnlySpan<byte> sourceSpan = source.Span;

            using TiffPixelBufferWriter<TiffBgra64> writer = context.GetWriter<TiffBgra64>();

            TiffOperationContext? operationContext = context.OperationContext;
            if (operationContext is null)
            {
                throw new InvalidOperationException("Failed to acquire OperationContext.");
            }

            int rows = context.ReadSize.Height;
            for (int row = 0; row < rows; row++)
            {
                using TiffPixelSpanHandle<TiffBgra64> pixelSpanHandle = writer.GetRowSpan(row);
                Span<byte> rowDestinationSpan = MemoryMarshal.Cast<TiffBgra64, byte>(pixelSpanHandle.GetSpan());
                CopyScanlineRgbaToBgra(sourceSpan.Slice(8 * context.SourceReadOffset.X, 8 * context.ReadSize.Width), rowDestinationSpan, context.ReadSize.Width, operationContext.IsLittleEndian == BitConverter.IsLittleEndian);
                sourceSpan = sourceSpan.Slice(bytesPerScanline);

                UndoColorPreMultiplying(rowDestinationSpan, context.ReadSize.Width);
            }
        }

        private static void ProcessAssociatedPreservingColorPreMultiplying(TiffImageDecoderContext context)
        {
            int bytesPerScanline = 8 * context.SourceImageSize.Width;
            Memory<byte> source = context.UncompressedData.Slice(context.SourceReadOffset.Y * bytesPerScanline);
            ReadOnlySpan<byte> sourceSpan = source.Span;

            using TiffPixelBufferWriter<TiffBgra64> writer = context.GetWriter<TiffBgra64>();

            TiffOperationContext? operationContext = context.OperationContext;
            if (operationContext is null)
            {
                throw new InvalidOperationException("Failed to acquire OperationContext.");
            }

            int rows = context.ReadSize.Height;
            for (int row = 0; row < rows; row++)
            {
                using TiffPixelSpanHandle<TiffBgra64> pixelSpanHandle = writer.GetRowSpan(row);
                Span<byte> rowDestinationSpan = MemoryMarshal.Cast<TiffBgra64, byte>(pixelSpanHandle.GetSpan());
                CopyScanlineRgbaToBgra(sourceSpan.Slice(8 * context.SourceReadOffset.X, 8 * context.ReadSize.Width), rowDestinationSpan, context.ReadSize.Width, operationContext.IsLittleEndian == BitConverter.IsLittleEndian);
                sourceSpan = sourceSpan.Slice(bytesPerScanline);

                WipeAlphaChanel(rowDestinationSpan, context.ReadSize.Width);
            }
        }

        private static void WipeAlphaChanel(Span<byte> data, int count)
        {
            ref ushort refData = ref MemoryMarshal.GetReference(MemoryMarshal.Cast<byte, ushort>(data));
            for (int i = 0; i < count; i++)
            {
                Unsafe.Add(ref refData, 3) = 0xffff;
                refData = ref Unsafe.Add(ref refData, 4);
            }
        }

        private static void UndoColorPreMultiplying(Span<byte> data, int count)
        {
            ref ushort refData = ref MemoryMarshal.GetReference(MemoryMarshal.Cast<byte, ushort>(data));
            for (int i = 0; i < count; i++)
            {
                ushort a = Unsafe.Add(ref refData, 3);
                if (a == 0)
                {
                    Unsafe.WriteUnaligned(ref Unsafe.As<ushort, byte>(ref refData), (long)0);
                }
                else
                {
                    ushort b = refData;
                    ushort g = Unsafe.Add(ref refData, 1);
                    ushort r = Unsafe.Add(ref refData, 2);
                    b = (ushort)(b * 0xffff / a);
                    g = (ushort)(g * 0xffff / a);
                    r = (ushort)(r * 0xffff / a);
                    refData = b;
                    Unsafe.Add(ref refData, 1) = g;
                    Unsafe.Add(ref refData, 2) = r;
                }

                refData = ref Unsafe.Add(ref refData, 4);
            }
        }
    }
}
