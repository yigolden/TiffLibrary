using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TiffLibrary;

namespace TiffJpegExtractor
{
    internal static class ExtractAction
    {
        public static async Task Extract(FileInfo input, DirectoryInfo output, CancellationToken cancellationToken)
        {
            await using TiffFileReader reader = await TiffFileReader.OpenAsync(input.FullName, cancellationToken);
            if (reader.FirstImageFileDirectoryOffset.IsZero)
            {
                Console.WriteLine("No IFD is preset.");
                return;
            }

            await using TiffFileContentReader contentReader = await reader.CreateContentReaderAsync(cancellationToken);
            await using TiffFieldReader fieldReader = await reader.CreateFieldReaderAsync(cancellationToken);

            TiffStreamOffset ifdOffset = reader.FirstImageFileDirectoryOffset;
            while (!ifdOffset.IsZero)
            {
                TiffImageFileDirectory ifd = await reader.ReadImageFileDirectoryAsync(ifdOffset, cancellationToken);

                Console.WriteLine($"Processing IFD {ifdOffset.ToInt64()}");
                await ExtractImageFileDirectory(contentReader, new TiffTagReader(fieldReader, ifd), output.CreateSubdirectory($"ifd_{ifdOffset.ToInt64()}"), cancellationToken);

                ifdOffset = ifd.NextOffset;
            }
        }

        public static async Task ExtractImageFileDirectory(TiffFileContentReader contentReader, TiffTagReader tagReader, DirectoryInfo output, CancellationToken cancellationToken)
        {
            if (await tagReader.ReadPhotometricInterpretationAsync(cancellationToken) != TiffPhotometricInterpretation.YCbCr)
            {
                Console.WriteLine("ERROR: invalid photometric interpretation.");
                return;
            }
            if (await tagReader.ReadCompressionAsync(cancellationToken) != TiffCompression.Jpeg)
            {
                Console.WriteLine("ERROR: invalid compression.");
                return;
            }
            TiffValueCollection<ushort> bitsPerSample = await tagReader.ReadBitsPerSampleAsync(cancellationToken);
            if (bitsPerSample.Count == 1)
            {
                if (bitsPerSample.GetFirstOrDefault() != 8)
                {
                    Console.WriteLine("ERROR: invalid bits per sample.");
                    return;
                }
            }
            else if (bitsPerSample.Count == 3)
            {
                if (bitsPerSample.GetFirstOrDefault() != 8 || bitsPerSample[1] != 8 || bitsPerSample[2] != 8)
                {
                    Console.WriteLine("ERROR: invalid bits per sample.");
                    return;
                }
            }
            else
            {
                Console.WriteLine("ERROR: invalid bits per sample.");
            }

            byte[] jpegTables = await tagReader.ReadJPEGTablesAsync(cancellationToken);

            foreach ((string identifier, TiffStreamRegion region) in await EnumerateStrilesAsync(tagReader, cancellationToken))
            {
                await using var fs = new FileStream(Path.Join(output.FullName, identifier + ".jpg"), FileMode.Create);
                await WriteJpegStreamAsync(fs, jpegTables, contentReader, region, cancellationToken);
            }
        }

        private static readonly byte[] s_startOfImageMarker = new byte[] { 0xff, 0xd8 };
        private static async Task WriteJpegStreamAsync(Stream stream, byte[] jpegTables, TiffFileContentReader contentReader, TiffStreamRegion region, CancellationToken cancellationToken)
        {
            await stream.WriteAsync(s_startOfImageMarker, cancellationToken);
            int skip = FindStartOfImage(jpegTables);
            if (skip < jpegTables.Length)
            {
                int length = FindEndOfImage(jpegTables.AsSpan(skip));
                await stream.WriteAsync(jpegTables.AsMemory(skip, length), cancellationToken);
            }
            bool startOfImageProcessed = false;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(16384);
            try
            {
                while (region.Length != 0)
                {
                    int readSize = Math.Min(region.Length, buffer.Length);
                    if (await contentReader.ReadAsync(region.Offset, buffer.AsMemory(0, readSize), cancellationToken) != readSize)
                    {
                        throw new InvalidDataException("EOF reached.");
                    }
                    region = new TiffStreamRegion(region.Offset + readSize, region.Length - readSize);
                    skip = 0;
                    if (!startOfImageProcessed)
                    {
                        skip = FindStartOfImage(buffer.AsSpan(0, readSize));
                        startOfImageProcessed = true;
                    }
                    await stream.WriteAsync(buffer.AsMemory(skip, readSize - skip), cancellationToken);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        private static int FindStartOfImage(ReadOnlySpan<byte> data)
        {
            if (data.Length < 2)
            {
                return 0;
            }
            int initialLength = data.Length;
            while (data.Length >= 2)
            {
                int b1 = data[0];
                int b2 = data[1];
                if (b1 == 0xff)
                {
                    if (b2 == 0xff)
                    {
                        data = data.Slice(1);
                        continue;
                    }
                    if (b2 == 0xd8)
                    {
                        return initialLength - data.Length + 2;
                    }
                    return initialLength - data.Length;
                }

                return 0;
            }
            return 0;
        }

        private static int FindEndOfImage(ReadOnlySpan<byte> data)
        {
            if (data.Length < 2)
            {
                return data.Length;
            }
            if (data[data.Length - 2] != 0xff || data[data.Length - 1] != 0xd9)
            {
                return data.Length;
            }
            data = data.Slice(0, data.Length - 2);
            while (!data.IsEmpty)
            {
                if (data[data.Length - 1] != 0xff)
                {
                    return data.Length;
                }
                data = data.Slice(0, data.Length - 1);
            }
            return 0;
        }

        private static async Task<IEnumerable<(string Identifier, TiffStreamRegion Region)>> EnumerateStrilesAsync(TiffTagReader tagReader, CancellationToken cancellationToken)
        {
            ulong width = await tagReader.ReadImageWidthAsync(cancellationToken);
            ulong height = await tagReader.ReadImageLengthAsync(cancellationToken);
            if (width == 0 || height == 0)
            {
                Console.WriteLine("ERROR: this image is empty.");
                return Array.Empty<(string, TiffStreamRegion)>();
            }

            if (tagReader.ImageFileDirectory.Contains(TiffTag.TileWidth) && tagReader.ImageFileDirectory.Contains(TiffTag.TileLength))
            {
                uint tileWidth = (await tagReader.ReadTileWidthAsync(cancellationToken)).GetValueOrDefault();
                uint tileLength = (await tagReader.ReadTileLengthAsync(cancellationToken)).GetValueOrDefault();
                if (tileWidth < 16 || tileLength < 16)
                {
                    Console.WriteLine("ERROR: invalid tiled TIFF.");
                    return Array.Empty<(string, TiffStreamRegion)>();
                }
                if (tileWidth % 16 != 0 || tileLength % 16 != 0)
                {
                    Console.WriteLine("ERROR: invalid tiled TIFF.");
                    return Array.Empty<(string, TiffStreamRegion)>();
                }

                ulong[] offsets = (await tagReader.ReadTileOffsetsAsync(cancellationToken)).ToArray();
                ulong[] byteCounts = (await tagReader.ReadTileByteCountsAsync(cancellationToken)).ToArray();

                ulong expectedCount = ((width + tileWidth - 1) / tileWidth) * ((height + tileLength - 1) / tileLength);
                if ((uint)offsets.Length < expectedCount || (uint)byteCounts.Length < expectedCount)
                {
                    Console.WriteLine("ERROR: Some tiles are missing.");
                    return Array.Empty<(string, TiffStreamRegion)>();
                }

                return EnuemrateTiles(width, height, tileWidth, tileLength, offsets, byteCounts);
            }
            else if (tagReader.ImageFileDirectory.Contains(TiffTag.StripOffsets) && tagReader.ImageFileDirectory.Contains(TiffTag.StripByteCounts))
            {
                ulong rowsPerStrip = await tagReader.ReadRowsPerStripAsync(cancellationToken);
                if (rowsPerStrip == 0)
                {
                    rowsPerStrip = height;
                }

                ulong[] offsets = (await tagReader.ReadStripOffsetsAsync(cancellationToken)).ToArray();
                ulong[] byteCounts = (await tagReader.ReadStripByteCountsAsync(cancellationToken)).ToArray();

                ulong expectedCount = (height + rowsPerStrip - 1) / rowsPerStrip;
                if ((uint)offsets.Length < expectedCount || (uint)byteCounts.Length < expectedCount)
                {
                    Console.WriteLine("ERROR: Some strips are missing.");
                    return Array.Empty<(string, TiffStreamRegion)>();
                }

                return EnuemrateStrips(height, rowsPerStrip, offsets, byteCounts);
            }
            else
            {
                Console.WriteLine("ERROR: IFD is corrupted.");
                return Array.Empty<(string, TiffStreamRegion)>();
            }
        }

        private static IEnumerable<(string Identifier, TiffStreamRegion Region)> EnuemrateTiles(ulong width, ulong height, uint tileWidth, uint tileHeight, ulong[] offsets, ulong[] byteCounts)
        {
            int rowCount = (int)((height + tileHeight - 1) / tileHeight);
            int colCount = (int)((width + tileWidth - 1) / tileWidth);

            int index = 0;
            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < colCount; col++)
                {
                    yield return ($"{row}_{col}", new TiffStreamRegion((long)offsets[index], (int)byteCounts[index]));
                    index++;
                }
            }
        }

        private static IEnumerable<(string Identifier, TiffStreamRegion Region)> EnuemrateStrips(ulong height, ulong rowsPerStrip, ulong[] offsets, ulong[] byteCounts)
        {
            int stripCount = (int)((height + rowsPerStrip - 1) / rowsPerStrip);

            for (int i = 0; i < stripCount; i++)
            {
                yield return (i.ToString(CultureInfo.InvariantCulture), new TiffStreamRegion((long)offsets[i], (int)byteCounts[i]));
            }
        }

    }
}
