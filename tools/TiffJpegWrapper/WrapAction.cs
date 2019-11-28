using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JpegLibrary;
using TiffLibrary;

namespace TiffJpegWrapper
{
    internal class WrapAction
    {

        public static async Task<int> Wrap(FileInfo source, FileInfo output, CancellationToken cancellationToken)
        {
            if (source is null || !source.Exists)
            {
                Console.WriteLine(source is null ? "Input JPEG image is not specified." : "File not found: " + source.FullName);
                return 1;
            }
            if (output is null)
            {
                Console.WriteLine("Output TIFF file is not specified.");
                return 1;
            }

            byte[] jpegFile = await File.ReadAllBytesAsync(source.FullName, cancellationToken);
            var decoder = new JpegDecoder();
            decoder.SetInput(jpegFile);
            decoder.Identify(loadQuantizationTables: false);

            ushort[] bitsPerSample = new ushort[decoder.NumberOfComponents];
            ushort bits = decoder.IsExtendedJpeg ? (ushort)12 : (ushort)8;
            for (int i = 0; i < bitsPerSample.Length; i++)
            {
                bitsPerSample[i] = bits;
            }
            TiffPhotometricInterpretation photometricInterpretation =
                bitsPerSample.Length == 1 ? TiffPhotometricInterpretation.BlackIsZero :
                bitsPerSample.Length == 3 ? TiffPhotometricInterpretation.YCbCr :
                throw new InvalidDataException("Photometric interpretation not supported.");

            using (var writer = await TiffFileWriter.OpenAsync(output.FullName, useBigTiff: false))
            {
                TiffStreamOffset imageOffset = await writer.WriteAlignedBytesAsync(jpegFile);

                TiffStreamOffset ifdOffset;
                using (var ifdWriter = writer.CreateImageFileDirectory())
                {
                    await ifdWriter.WriteTagAsync(TiffTag.ImageWidth, TiffValueCollection.Single((ushort)decoder.Width));
                    await ifdWriter.WriteTagAsync(TiffTag.ImageLength, TiffValueCollection.Single((ushort)decoder.Height));
                    await ifdWriter.WriteTagAsync(TiffTag.BitsPerSample, TiffValueCollection.UnsafeWrap(bitsPerSample));
                    await ifdWriter.WriteTagAsync(TiffTag.Compression, TiffValueCollection.Single((ushort)TiffCompression.Jpeg));
                    await ifdWriter.WriteTagAsync(TiffTag.PhotometricInterpretation, TiffValueCollection.Single((ushort)photometricInterpretation));
                    await ifdWriter.WriteTagAsync(TiffTag.SamplesPerPixel, TiffValueCollection.Single((ushort)bitsPerSample.Length));
                    await ifdWriter.WriteTagAsync(TiffTag.PlanarConfiguration, TiffValueCollection.Single((ushort)TiffPlanarConfiguration.Chunky));

                    await ifdWriter.WriteTagAsync(TiffTag.RowsPerStrip, TiffValueCollection.Single((ushort)decoder.Height));
                    await ifdWriter.WriteTagAsync(TiffTag.StripOffsets, TiffValueCollection.Single((uint)imageOffset.Offset));
                    await ifdWriter.WriteTagAsync(TiffTag.StripByteCounts, TiffValueCollection.Single((uint)jpegFile.Length));

                    if (photometricInterpretation == TiffPhotometricInterpretation.YCbCr)
                    {
                        int maxHorizontalSampling = decoder.GetMaximumHorizontalSampling();
                        int maxVerticalSampling = decoder.GetMaximumVerticalSampling();
                        int yHorizontalSubSampling = maxHorizontalSampling / decoder.GetHorizontalSampling(0);
                        int yVerticalSubSampling = maxVerticalSampling / decoder.GetVerticalSampling(0);
                        int cbHorizontalSubSampling = maxHorizontalSampling / decoder.GetHorizontalSampling(1) / yHorizontalSubSampling;
                        int cbVerticalSubSampling = maxVerticalSampling / decoder.GetVerticalSampling(1) / yVerticalSubSampling;
                        int crHorizontalSubSampling = maxHorizontalSampling / decoder.GetHorizontalSampling(2) / yHorizontalSubSampling;
                        int crVerticalSubSampling = maxVerticalSampling / decoder.GetVerticalSampling(2) / yVerticalSubSampling;

                        if (cbHorizontalSubSampling != crHorizontalSubSampling || cbVerticalSubSampling != crVerticalSubSampling)
                        {
                            throw new InvalidDataException("Unsupported JPEG image.");
                        }

                        await ifdWriter.WriteTagAsync(TiffTag.YCbCrSubSampling, TiffValueCollection.UnsafeWrap(new ushort[] { (ushort)cbHorizontalSubSampling, (ushort)cbVerticalSubSampling }));
                    }

                    // Write other properties here (eg, XResolution, YResolution)
                    await ifdWriter.WriteTagAsync(TiffTag.XResolution, TiffValueCollection.Single(new TiffRational(96, 1)));
                    await ifdWriter.WriteTagAsync(TiffTag.YResolution, TiffValueCollection.Single(new TiffRational(96, 1)));
                    await ifdWriter.WriteTagAsync(TiffTag.ResolutionUnit, TiffValueCollection.Single((ushort)TiffResolutionUnit.Inch));

                    ifdOffset = await ifdWriter.FlushAsync();
                }

                writer.SetFirstImageFileDirectoryOffset(ifdOffset);
                await writer.FlushAsync();
            }

            return 0;
        }
    }
}
