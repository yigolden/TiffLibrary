using System;
using System.Buffers;

namespace JpegLibrary
{
    internal readonly struct JpegScanHeader
    {
        public JpegScanHeader(byte numberOfComponents, JpegScanComponentSpecificationParameters[] components, byte startOfSpectralSelection, byte endOfSpectralSelection, byte successiveApproximationBitPositionHigh, byte successiveApproximationBitPositionLow)
        {
            NumberOfComponents = numberOfComponents;
            Components = components;
            StartOfSpectralSelection = startOfSpectralSelection;
            EndOfSpectralSelection = endOfSpectralSelection;
            SuccessiveApproximationBitPositionHigh = successiveApproximationBitPositionHigh;
            SuccessiveApproximationBitPositionLow = successiveApproximationBitPositionLow;
        }

        public byte NumberOfComponents { get; }
        public JpegScanComponentSpecificationParameters[] Components { get; }
        public byte StartOfSpectralSelection { get; }
        public byte EndOfSpectralSelection { get; }
        public byte SuccessiveApproximationBitPositionHigh { get; }
        public byte SuccessiveApproximationBitPositionLow { get; }

        public byte BytesRequired => (byte)(4 + 2 * NumberOfComponents);

        internal bool ShadowEquals(JpegScanHeader other)
        {
            return NumberOfComponents == other.NumberOfComponents && StartOfSpectralSelection == other.StartOfSpectralSelection && EndOfSpectralSelection == other.EndOfSpectralSelection &&
                SuccessiveApproximationBitPositionHigh == other.SuccessiveApproximationBitPositionHigh && SuccessiveApproximationBitPositionLow == other.SuccessiveApproximationBitPositionLow;
        }

        public static bool TryParse(ReadOnlySequence<byte> buffer, bool metadataOnly, out JpegScanHeader scanHeader, out int bytesConsumed)
        {
            if (buffer.IsSingleSegment)
            {
#if NO_READONLYSEQUENCE_FISTSPAN
                return TryParse(buffer.First.Span, metadataOnly, out scanHeader, out bytesConsumed);
#else
                return TryParse(buffer.FirstSpan, metadataOnly, out scanHeader, out bytesConsumed);
#endif
            }

            bytesConsumed = 0;

            if (buffer.IsEmpty)
            {
                scanHeader = default;
                return false;
            }

#if NO_READONLYSEQUENCE_FISTSPAN
            byte numberOfComponents = buffer.First.Span[0];
#else
            byte numberOfComponents = buffer.FirstSpan[0];
#endif
            buffer = buffer.Slice(1);
            bytesConsumed++;

            if (buffer.Length < (2 * numberOfComponents + 3))
            {
                scanHeader = default;
                return false;
            }

            JpegScanComponentSpecificationParameters[] components;
            if (metadataOnly)
            {
                components = null;
                buffer = buffer.Slice(2 * numberOfComponents);
                bytesConsumed += 2 * numberOfComponents;
            }
            else
            {
                components = new JpegScanComponentSpecificationParameters[numberOfComponents];
                for (int i = 0; i < components.Length; i++)
                {
#pragma warning disable CA1806
                    JpegScanComponentSpecificationParameters.TryParse(buffer, out components[i]);
#pragma warning restore CA1806 
                    buffer = buffer.Slice(2);
                    bytesConsumed += 2;
                }
            }

            Span<byte> local = stackalloc byte[4];
            buffer.Slice(0, 3).CopyTo(local);

            byte successiveApproximationBitPosition = local[2];
            byte endOfSpectralSelection = local[1];
            byte startOfSpectralSelection = local[0];
            bytesConsumed += 3;

            scanHeader = new JpegScanHeader(numberOfComponents, components, startOfSpectralSelection, endOfSpectralSelection, (byte)(successiveApproximationBitPosition >> 4), (byte)(successiveApproximationBitPosition & 0xf));
            return true;

        }

        public static bool TryParse(ReadOnlySpan<byte> buffer, bool metadataOnly, out JpegScanHeader scanHeader, out int bytesConsumed)
        {
            bytesConsumed = 0;

            if (buffer.IsEmpty)
            {
                scanHeader = default;
                return false;
            }

            byte numberOfComponents = buffer[0];
            buffer = buffer.Slice(1);
            bytesConsumed++;

            if (buffer.Length < (2 * numberOfComponents + 3))
            {
                scanHeader = default;
                return false;
            }

            JpegScanComponentSpecificationParameters[] components;
            if (metadataOnly)
            {
                components = null;
                buffer = buffer.Slice(2 * numberOfComponents);
                bytesConsumed += 2 * numberOfComponents;
            }
            else
            {
                components = new JpegScanComponentSpecificationParameters[numberOfComponents];
                for (int i = 0; i < components.Length; i++)
                {
#pragma warning disable CA1806
                    JpegScanComponentSpecificationParameters.TryParse(buffer, out components[i]);
#pragma warning restore CA1806
                    buffer = buffer.Slice(2);
                    bytesConsumed += 2;
                }
            }

            byte successiveApproximationBitPosition = buffer[2];
            byte endOfSpectralSelection = buffer[1];
            byte startOfSpectralSelection = buffer[0];
            bytesConsumed += 3;

            scanHeader = new JpegScanHeader(numberOfComponents, components, startOfSpectralSelection, endOfSpectralSelection, (byte)(successiveApproximationBitPosition >> 4), (byte)(successiveApproximationBitPosition & 0xf));
            return true;
        }

        public bool TryWrite(Span<byte> buffer, out int bytesWritten)
        {
            if (buffer.IsEmpty)
            {
                bytesWritten = 0;
                return false;
            }

            buffer[0] = NumberOfComponents;
            buffer = buffer.Slice(1);
            bytesWritten = 1;

            JpegScanComponentSpecificationParameters[] components = Components;
            if (components is null || components.Length < NumberOfComponents)
            {
                throw new InvalidOperationException("Components are not specified.");
            }

            for (int i = 0; i < NumberOfComponents; i++)
            {
                if (!components[i].TryWrite(buffer, out int bytes))
                {
                    return false;
                }
                buffer = buffer.Slice(bytes);
                bytesWritten += bytes;
            }

            if (buffer.Length < 3)
            {
                return false;
            }

            buffer[0] = StartOfSpectralSelection;
            buffer[1] = EndOfSpectralSelection;
            buffer[2] = (byte)((SuccessiveApproximationBitPositionHigh << 4) | (SuccessiveApproximationBitPositionLow & 0xf));
            bytesWritten += 3;

            return true;
        }
    }

    internal readonly struct JpegScanComponentSpecificationParameters
    {
        public JpegScanComponentSpecificationParameters(byte scanComponentSelector, byte dcEntropyCodingTableSelector, byte acEntropyCodingTableSelector)
        {
            ScanComponentSelector = scanComponentSelector;
            DcEntropyCodingTableSelector = dcEntropyCodingTableSelector;
            AcEntropyCodingTableSelector = acEntropyCodingTableSelector;
        }

        public byte ScanComponentSelector { get; }
        public byte DcEntropyCodingTableSelector { get; }
        public byte AcEntropyCodingTableSelector { get; }

        public static bool TryParse(ReadOnlySequence<byte> buffer, out JpegScanComponentSpecificationParameters component)
        {
            if (buffer.IsSingleSegment)
            {
#if NO_READONLYSEQUENCE_FISTSPAN
                return TryParse(buffer.First.Span, out component);
#else
                return TryParse(buffer.FirstSpan, out component);
#endif
            }

            if (buffer.Length < 2)
            {
                component = default;
                return false;
            }

            Span<byte> local = stackalloc byte[2];
            buffer.Slice(0, 2).CopyTo(local);

            byte entropyCodingTableSelector = local[1];
            byte scanComponentSelector = local[0];

            component = new JpegScanComponentSpecificationParameters(scanComponentSelector, (byte)(entropyCodingTableSelector >> 4), (byte)(entropyCodingTableSelector & 0xf));
            return true;
        }

        public static bool TryParse(ReadOnlySpan<byte> buffer, out JpegScanComponentSpecificationParameters component)
        {
            if (buffer.Length < 2)
            {
                component = default;
                return false;
            }

            byte entropyCodingTableSelector = buffer[1];
            byte scanComponentSelector = buffer[0];

            component = new JpegScanComponentSpecificationParameters(scanComponentSelector, (byte)(entropyCodingTableSelector >> 4), (byte)(entropyCodingTableSelector & 0xf));
            return true;
        }

        public bool TryWrite(Span<byte> buffer, out int bytesWritten)
        {
            if (buffer.Length < 2)
            {
                bytesWritten = 0;
                return false;
            }

            buffer[0] = ScanComponentSelector;
            buffer[1] = (byte)((DcEntropyCodingTableSelector << 4) | (AcEntropyCodingTableSelector & 0xf));
            bytesWritten = 2;
            return true;
        }
    }
}
