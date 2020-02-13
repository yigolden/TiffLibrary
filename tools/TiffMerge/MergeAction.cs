using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TiffLibrary;

namespace TiffMerge
{
    internal class MergeAction
    {
        public static async Task<int> Merge(FileInfo[] source, FileInfo output, CancellationToken cancellationToken)
        {
            if (source is null || source.Length == 0)
            {
                Console.WriteLine("Input TIFF file are not specified.");
                return 1;
            }
            if (output is null)
            {
                Console.WriteLine("Output TIFF file is not specified");
                return 1;
            }

            await using TiffFileWriter writer = await TiffFileWriter.OpenAsync(output.FullName);

            TiffStreamOffset fistIfdOffset = default;
            TiffStreamOffset previousIfdOffset = default;

            foreach (FileInfo sourceFile in source)
            {
                await using TiffFileReader reader = await TiffFileReader.OpenAsync(sourceFile.FullName);
                await using TiffFileContentReader contentReader = await reader.CreateContentReaderAsync();
                await using TiffFieldReader fieldReader = await reader.CreateFieldReaderAsync(cancellationToken);

                TiffStreamOffset inputIfdOffset = reader.FirstImageFileDirectoryOffset;
                while (!inputIfdOffset.IsZero)
                {
                    TiffImageFileDirectory ifd = await reader.ReadImageFileDirectoryAsync(inputIfdOffset, cancellationToken);

                    using (TiffImageFileDirectoryWriter ifdWriter = writer.CreateImageFileDirectory())
                    {
                        await CopyIfdAsync(contentReader, fieldReader, ifd, ifdWriter, cancellationToken);

                        previousIfdOffset = await ifdWriter.FlushAsync(previousIfdOffset);

                        if (fistIfdOffset.IsZero)
                        {
                            fistIfdOffset = previousIfdOffset;
                        }
                    }

                    inputIfdOffset = ifd.NextOffset;
                }

            }

            writer.SetFirstImageFileDirectoryOffset(fistIfdOffset);
            await writer.FlushAsync();

            return 0;
        }

        private static async Task CopyIfdAsync(TiffFileContentReader contentReader, TiffFieldReader fieldReader, TiffImageFileDirectory ifd, TiffImageFileDirectoryWriter dest, CancellationToken cancellationToken)
        {
            var tagReader = new TiffTagReader(fieldReader, ifd);
            foreach (TiffImageFileDirectoryEntry entry in ifd)
            {
                // Stripped image data
                if (entry.Tag == TiffTag.StripOffsets)
                {
                    await CopyStrippedImageAsync(contentReader, tagReader, dest, cancellationToken);
                }
                else if (entry.Tag == TiffTag.StripByteCounts)
                {
                    // Ignore this
                }

                // Tiled image data
                else if (entry.Tag == TiffTag.TileOffsets)
                {
                    await CopyTiledImageAsync(contentReader, tagReader, dest, cancellationToken);
                }
                else if (entry.Tag == TiffTag.TileByteCounts)
                {
                    // Ignore this
                }

                // Other fields
                else
                {
                    await CopyFieldValueAsync(fieldReader, entry, dest, cancellationToken);
                }
            }
        }

        private static async Task CopyStrippedImageAsync(TiffFileContentReader contentReader, TiffTagReader tagReader, TiffImageFileDirectoryWriter dest, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            TiffValueCollection<ulong> offsets = await tagReader.ReadStripOffsetsAsync(cancellationToken);
            TiffValueCollection<ulong> byteCounts = await tagReader.ReadStripByteCountsAsync(cancellationToken);

            if (offsets.Count != byteCounts.Count)
            {
                throw new InvalidDataException("Failed to copy stripped image data. StripOffsets and StripByteCounts don't have the same amount of elements.");
            }

            uint[] offsets32 = new uint[offsets.Count];
            uint[] byteCounts32 = new uint[offsets.Count];

            byte[]? buffer = null;
            try
            {
                for (int i = 0; i < offsets.Count; i++)
                {
                    int offset = checked((int)offsets[i]);
                    int byteCount = checked((int)byteCounts[i]);
                    byteCounts32[i] = checked((uint)byteCount);

                    if (buffer is null || byteCount > buffer.Length)
                    {
                        if (!(buffer is null))
                        {
                            ArrayPool<byte>.Shared.Return(buffer);
                        }
                        buffer = ArrayPool<byte>.Shared.Rent(byteCount);
                    }

                    if (await contentReader.ReadAsync(offset, new ArraySegment<byte>(buffer, 0, byteCount), cancellationToken) != byteCount)
                    {
                        throw new InvalidDataException("Invalid ByteCount field.");
                    }

                    TiffStreamOffset region = await dest.FileWriter.WriteAlignedBytesAsync(buffer, 0, byteCount);
                    offsets32[i] = checked((uint)region.Offset);
                }
            }
            finally
            {
                if (!(buffer is null))
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }

            await dest.WriteTagAsync(TiffTag.StripOffsets, TiffValueCollection.UnsafeWrap(offsets32));
            await dest.WriteTagAsync(TiffTag.StripByteCounts, TiffValueCollection.UnsafeWrap(byteCounts32));
        }

        private static async Task CopyTiledImageAsync(TiffFileContentReader contentReader, TiffTagReader tagReader, TiffImageFileDirectoryWriter dest, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            TiffValueCollection<ulong> offsets = await tagReader.ReadTileOffsetsAsync(cancellationToken);
            TiffValueCollection<ulong> byteCounts = await tagReader.ReadTileByteCountsAsync(cancellationToken);

            if (offsets.Count != byteCounts.Count)
            {
                throw new InvalidDataException("Failed to copy stripped image data. TileOffsets and TileByteCounts don't have the same amount of elements.");
            }

            uint[] offsets32 = new uint[offsets.Count];
            uint[] byteCounts32 = new uint[offsets.Count];

            byte[]? buffer = null;
            try
            {
                for (int i = 0; i < offsets.Count; i++)
                {
                    int offset = checked((int)offsets[i]);
                    int byteCount = checked((int)byteCounts[i]);
                    byteCounts32[i] = checked((uint)byteCount);

                    if (buffer is null || byteCount > buffer.Length)
                    {
                        if (!(buffer is null))
                        {
                            ArrayPool<byte>.Shared.Return(buffer);
                        }
                        buffer = ArrayPool<byte>.Shared.Rent(byteCount);
                    }

                    if (await contentReader.ReadAsync(offset, new ArraySegment<byte>(buffer, 0, byteCount), cancellationToken) != byteCount)
                    {
                        throw new InvalidDataException("Invalid ByteCount field.");
                    }

                    TiffStreamOffset region = await dest.FileWriter.WriteAlignedBytesAsync(buffer, 0, byteCount);
                    offsets32[i] = checked((uint)region.Offset);
                }
            }
            finally
            {
                if (!(buffer is null))
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }

            await dest.WriteTagAsync(TiffTag.TileOffsets, TiffValueCollection.UnsafeWrap(offsets32));
            await dest.WriteTagAsync(TiffTag.TileByteCounts, TiffValueCollection.UnsafeWrap(byteCounts32));
        }

        private static async Task CopyFieldValueAsync(TiffFieldReader reader, TiffImageFileDirectoryEntry entry, TiffImageFileDirectoryWriter ifdWriter, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            switch (entry.Type)
            {
                case TiffFieldType.Byte:
                    await ifdWriter.WriteTagAsync(entry.Tag, await reader.ReadByteFieldAsync(entry));
                    break;
                case TiffFieldType.ASCII:
                    await ifdWriter.WriteTagAsync(entry.Tag, await reader.ReadASCIIFieldAsync(entry));
                    break;
                case TiffFieldType.Short:
                    await ifdWriter.WriteTagAsync(entry.Tag, await reader.ReadShortFieldAsync(entry));
                    break;
                case TiffFieldType.Long:
                    await ifdWriter.WriteTagAsync(entry.Tag, await reader.ReadLongFieldAsync(entry));
                    break;
                case TiffFieldType.Rational:
                    await ifdWriter.WriteTagAsync(entry.Tag, await reader.ReadRationalFieldAsync(entry));
                    break;
                case TiffFieldType.SByte:
                    await ifdWriter.WriteTagAsync(entry.Tag, entry.Type, await reader.ReadByteFieldAsync(entry));
                    break;
                case TiffFieldType.Undefined:
                    await ifdWriter.WriteTagAsync(entry.Tag, entry.Type, await reader.ReadByteFieldAsync(entry));
                    break;
                case TiffFieldType.SShort:
                    await ifdWriter.WriteTagAsync(entry.Tag, await reader.ReadSShortFieldAsync(entry));
                    break;
                case TiffFieldType.SLong:
                    await ifdWriter.WriteTagAsync(entry.Tag, await reader.ReadSLongFieldAsync(entry));
                    break;
                case TiffFieldType.SRational:
                    await ifdWriter.WriteTagAsync(entry.Tag, await reader.ReadSRationalFieldAsync(entry));
                    break;
                case TiffFieldType.Float:
                    await ifdWriter.WriteTagAsync(entry.Tag, await reader.ReadFloatFieldAsync(entry));
                    break;
                case TiffFieldType.Double:
                    await ifdWriter.WriteTagAsync(entry.Tag, await reader.ReadDoubleFieldAsync(entry));
                    break;
                case TiffFieldType.IFD:
                    throw new NotSupportedException("This IFD type is not supported yet.");
                case TiffFieldType.Long8:
                    await ifdWriter.WriteTagAsync(entry.Tag, await reader.ReadLong8FieldAsync(entry));
                    break;
                case TiffFieldType.SLong8:
                    await ifdWriter.WriteTagAsync(entry.Tag, await reader.ReadSLong8FieldAsync(entry));
                    break;
                case TiffFieldType.IFD8:
                    throw new NotSupportedException("This IFD type is not supported yet.");
                default:
                    throw new NotSupportedException($"Unsupported IFD field type: {entry.Type}.");
            }

        }
    }
}
