#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace JpegLibrary.ScanDecoder
{
    internal abstract class JpegScanDecoder : IDisposable
    {
        public abstract void ProcessScan(ref JpegReader reader, JpegScanHeader scanHeader);

        public abstract void Dispose();

        public static JpegScanDecoder? Create(JpegMarker sofMarker, JpegDecoder decoder, JpegFrameHeader header)
        {
            switch (sofMarker)
            {
                case JpegMarker.StartOfFrame0:
                    return new JpegHuffmanBaselineScanDecoder(decoder, header);
                case JpegMarker.StartOfFrame1:
                    return new JpegHuffmanBaselineScanDecoder(decoder, header);
                case JpegMarker.StartOfFrame2:
                    return new JpegHuffmanProgressiveScanDecoder(decoder, header);
                case JpegMarker.StartOfFrame3:
                    return new JpegHuffmanLosslessScanDecoder(decoder, header);
                default:
                    return null;
            }
        }

        [DoesNotReturn]
        protected static void ThrowInvalidDataException(string? message = null)
        {
            if (message is null)
            {
                throw new InvalidDataException();
            }
            else
            {
                throw new InvalidDataException(message);
            }
        }

        [DoesNotReturn]
        protected static void ThrowInvalidDataException(int offset, string message)
        {
            throw new InvalidDataException($"Failed to decode JPEG data at offset {offset}. {message}");
        }
    }
}
