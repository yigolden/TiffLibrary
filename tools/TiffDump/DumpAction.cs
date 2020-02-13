using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TiffLibrary;

namespace TiffDump
{
    internal class DumpAction
    {
        public static async Task<int> Dump(FileInfo file, CancellationToken cancellationToken)
        {
            await using TiffFileReader tiff = await TiffFileReader.OpenAsync(file.FullName);
            await using TiffFieldReader fieldReader = await tiff.CreateFieldReaderAsync(cancellationToken);

            Console.WriteLine("Input File: " + file.FullName);
            Console.WriteLine($"StandardTIFF: {tiff.IsStandardTiff}, BigTIFF: {tiff.IsBigTiff}, IsLittleEndian: {tiff.IsLittleEndian}");
            Console.WriteLine("First IFD: " + tiff.FirstImageFileDirectoryOffset);
            Console.WriteLine();

            int ifdIndex = 0;
            TiffStreamOffset ifdOffset = tiff.FirstImageFileDirectoryOffset;
            while (!ifdOffset.IsZero)
            {
                TiffImageFileDirectory ifd = await tiff.ReadImageFileDirectoryAsync(ifdOffset, cancellationToken);

                Console.WriteLine($"IFD #{ifdIndex++} (Offset = {ifdOffset})");

                Console.WriteLine("  Well-known Tags:");
                await DumpWellKnownTagsAsync(fieldReader, ifd);
                Console.WriteLine();

                Console.WriteLine("  Entries:");
                for (int i = 0; i < ifd.Count; i++)
                {
                    TiffImageFileDirectoryEntry entry = ifd[i];
                    await DumpIfdEntryAsync(i, fieldReader, entry);
                }
                if (ifd.Count == 0)
                {
                    Console.WriteLine("    No entries found.");
                }

                Console.WriteLine();
                Console.WriteLine();

                ifdOffset = ifd.NextOffset;
            }

            return 0;
        }

        private static async Task DumpWellKnownTagsAsync(TiffFieldReader fieldReader, TiffImageFileDirectory ifd)
        {
            int count = 0;
            var tagReader = new TiffTagReader(fieldReader, ifd);

            if (ifd.Contains(TiffTag.PhotometricInterpretation))
            {
                Console.WriteLine("    PhotometricInterpretation = " + (await tagReader.ReadPhotometricInterpretationAsync()));
                count++;
            }
            if (ifd.Contains(TiffTag.SamplesPerPixel))
            {
                Console.WriteLine("    SamplesPerPixel = " + (await tagReader.ReadSamplesPerPixelAsync()));
                count++;
            }
            if (ifd.Contains(TiffTag.BitsPerSample))
            {
                Console.Write("    BitsPerSample = ");
                DumpValueCollecionSimple(await tagReader.ReadBitsPerSampleAsync());
                Console.WriteLine();
                count++;
            }
            if (ifd.Contains(TiffTag.ImageWidth))
            {
                Console.WriteLine("    ImageWidth = " + (await tagReader.ReadImageWidthAsync()));
                count++;
            }
            if (ifd.Contains(TiffTag.ImageLength))
            {
                Console.WriteLine("    ImageLength = " + (await tagReader.ReadImageLengthAsync()));
                count++;
            }
            if (ifd.Contains(TiffTag.Compression))
            {
                Console.WriteLine("    Compression = " + (await tagReader.ReadCompressionAsync()));
                count++;
            }
            if (ifd.Contains(TiffTag.FillOrder))
            {
                Console.WriteLine("    FillOrder = " + (await tagReader.ReadFillOrderAsync()));
                count++;
            }
            if (ifd.Contains(TiffTag.Predictor))
            {
                Console.WriteLine("    Predictor = " + (await tagReader.ReadPredictorAsync()));
                count++;
            }
            if (ifd.Contains(TiffTag.Orientation))
            {
                Console.WriteLine("    Orientation = " + (await tagReader.ReadOrientationAsync()));
                count++;
            }
            if (ifd.Contains(TiffTag.RowsPerStrip))
            {
                Console.WriteLine("    RowsPerStrip = " + (await tagReader.ReadRowsPerStripAsync()));
                count++;
            }
            if (ifd.Contains(TiffTag.TileWidth))
            {
                Console.WriteLine("    TileWidth = " + (await tagReader.ReadTileWidthAsync()));
                count++;
            }
            if (ifd.Contains(TiffTag.TileLength))
            {
                Console.WriteLine("    TileLength = " + (await tagReader.ReadTileLengthAsync()));
                count++;
            }

            if (count == 0)
            {
                Console.WriteLine("    No well-known tags found.");
            }
        }

        private static async Task DumpIfdEntryAsync(int index, TiffFieldReader fieldReader, TiffImageFileDirectoryEntry entry)
        {
            string tagName = Enum.IsDefined(typeof(TiffTag), entry.Tag) ? $"{entry.Tag} ({(int)entry.Tag})" : ((int)entry.Tag).ToString();
            string typeName = Enum.IsDefined(typeof(TiffFieldType), entry.Type) ? entry.Type.ToString() : "Unknown";

            Console.Write($"    Entry #{index}: {tagName}, {typeName}[{entry.ValueCount}].");

            switch (entry.Type)
            {
                case TiffFieldType.Byte:
                    Console.Write(" Binary data not shown.");
                    break;
                case TiffFieldType.ASCII:
                    TiffValueCollection<string> valuesAscii = await fieldReader.ReadASCIIFieldAsync(entry, skipTypeValidation: true);
                    if (valuesAscii.IsEmpty)
                    {
                        // Do nothing
                    }
                    else if (valuesAscii.Count == 1)
                    {
                        Console.Write(" Value = " + valuesAscii.GetFirstOrDefault());
                    }
                    else
                    {
                        Console.WriteLine();
                        for (int i = 0; i < valuesAscii.Count; i++)
                        {
                            Console.Write($"      [{i}] = {valuesAscii[i]}");
                        }
                    }
                    break;
                case TiffFieldType.Short:
                    TiffValueCollection<ushort> valuesShort = await fieldReader.ReadShortFieldAsync(entry, skipTypeValidation: true);
                    DumpValueCollecion(valuesShort);
                    break;
                case TiffFieldType.Long:
                    TiffValueCollection<uint> valuesLong = await fieldReader.ReadLongFieldAsync(entry, skipTypeValidation: true);
                    DumpValueCollecion(valuesLong);
                    break;
                case TiffFieldType.Rational:
                    TiffValueCollection<TiffRational> valuesRational = await fieldReader.ReadRationalFieldAsync(entry, skipTypeValidation: true);
                    DumpValueCollecion(valuesRational);
                    break;
                case TiffFieldType.SByte:
                    Console.Write(" Binary data not shown.");
                    break;
                case TiffFieldType.Undefined:
                    Console.Write(" Binary data not shown.");
                    break;
                case TiffFieldType.SShort:
                    TiffValueCollection<short> valuesSShort = await fieldReader.ReadSShortFieldAsync(entry, skipTypeValidation: true);
                    DumpValueCollecion(valuesSShort);
                    break;
                case TiffFieldType.SLong:
                    TiffValueCollection<int> valuesSLong = await fieldReader.ReadSLongFieldAsync(entry, skipTypeValidation: true);
                    DumpValueCollecion(valuesSLong);
                    break;
                case TiffFieldType.SRational:
                    TiffValueCollection<TiffSRational> valuesSRational = await fieldReader.ReadSRationalFieldAsync(entry, skipTypeValidation: true);
                    DumpValueCollecion(valuesSRational);
                    break;
                case TiffFieldType.Float:
                    TiffValueCollection<float> valuesFloat = await fieldReader.ReadFloatFieldAsync(entry, skipTypeValidation: true);
                    DumpValueCollecion(valuesFloat);
                    break;
                case TiffFieldType.Double:
                    TiffValueCollection<double> valuesDouble = await fieldReader.ReadDoubleFieldAsync(entry, skipTypeValidation: true);
                    DumpValueCollecion(valuesDouble);
                    break;
                case TiffFieldType.IFD:
                    TiffValueCollection<TiffStreamOffset> valuesIfd = await fieldReader.ReadIFDFieldAsync(entry, skipTypeValidation: true);
                    DumpValueCollecion(valuesIfd);
                    break;
                case TiffFieldType.Long8:
                    TiffValueCollection<ulong> valuesLong8 = await fieldReader.ReadLong8FieldAsync(entry, skipTypeValidation: true);
                    DumpValueCollecion(valuesLong8);
                    break;
                case TiffFieldType.SLong8:
                    TiffValueCollection<long> valuesSLong8 = await fieldReader.ReadSLong8FieldAsync(entry, skipTypeValidation: true);
                    DumpValueCollecion(valuesSLong8);
                    break;
                case TiffFieldType.IFD8:
                    TiffValueCollection<TiffStreamOffset> valuesIfd8 = await fieldReader.ReadIFD8FieldAsync(entry, skipTypeValidation: true);
                    DumpValueCollecion(valuesIfd8);
                    break;
                default:
                    Console.Write(" Unsupported field type.");
                    break;
            }

            Console.WriteLine();
        }

        private static void DumpValueCollecion<T>(TiffValueCollection<T> values)
        {
            if (values.IsEmpty)
            {
                // Do nothing
            }
            else if (values.Count == 1)
            {
                Console.Write(" Value = " + values.GetFirstOrDefault());
            }
            else
            {
                Console.Write(" Values = [");
                for (int i = 0; i < values.Count; i++)
                {
                    Console.Write(values[i]);
                    if (i != values.Count - 1)
                    {
                        Console.Write(", ");
                    }
                }
                Console.Write("]");
            }
        }

        private static void DumpValueCollecionSimple<T>(TiffValueCollection<T> values)
        {
            Console.Write("[");
            if (values.IsEmpty)
            {
                // Do nothing
            }
            else if (values.Count == 1)
            {
                Console.Write(values.GetFirstOrDefault());
            }
            else
            {
                for (int i = 0; i < values.Count; i++)
                {
                    Console.Write(values[i]);
                    if (i != values.Count - 1)
                    {
                        Console.Write(", ");
                    }
                }
            }
            Console.Write("]");
        }
    }
}
