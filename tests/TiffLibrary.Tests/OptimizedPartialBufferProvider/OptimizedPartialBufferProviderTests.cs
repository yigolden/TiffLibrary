using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using TiffLibrary.PixelBuffer;
using TiffLibrary.PixelFormats;
using Xunit;
using Xunit.Abstractions;

namespace TiffLibrary.Tests.OptimizedPartialBufferProvider
{
    public class OptimizedPartialBufferProviderTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public OptimizedPartialBufferProviderTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task WritePyramidTiledTiffUsingOptimizedPartialBufferProvider()
        {
            string tmpFilePath = Path.Combine(Path.GetTempPath(), "tmp.tiff");

            try
            {
                await using var writer = await TiffFileWriter.OpenAsync(tmpFilePath, useBigTiff: true);

                var baseImageSize = new Size(5316, 5316);
                int tileSize = 256;

                // Build The image encoder.
                var encoderBuilder = new TiffImageEncoderBuilder
                {
                    PhotometricInterpretation = TiffPhotometricInterpretation.BlackIsZero,
                    IsTiled = true,
                    TileSize = new TiffSize(tileSize, tileSize),
                    Compression = TiffCompression.NoCompression
                };

                // We just build the pyramid to the last until one tile
                var numberOfPyramidLevelsForHeight = (int)Math.Ceiling(Math.Log2(baseImageSize.Height / (double)tileSize));
                var numberOfPyramidLevelsForWidth = (int)Math.Ceiling(Math.Log2(baseImageSize.Width / (double)tileSize));
                int numberOfPyramidLevels = Math.Min(numberOfPyramidLevelsForHeight, numberOfPyramidLevelsForWidth);

                TiffStreamOffset lastIfdOffset = -1;

                var encoder = encoderBuilder.Build<TiffGray8>();
                var bufferProvider = new OptimizedPartialBufferProvider(new TiffSize(baseImageSize.Width, baseImageSize.Height));

                List<ulong> subIfdStreamOffsets = await WriteSubIfds(bufferProvider, writer, encoder,
                    numberOfPyramidLevels).ConfigureAwait(false);
                if (lastIfdOffset == -1)
                {
                    lastIfdOffset = await WriteMainIfd(bufferProvider, writer, encoder, subIfdStreamOffsets, null).ConfigureAwait(false);
                    // Set this IFD to be the first IFD.
                    writer.SetFirstImageFileDirectoryOffset(lastIfdOffset);
                }
                else
                {
                    lastIfdOffset = await WriteMainIfd(bufferProvider, writer, encoder, subIfdStreamOffsets, lastIfdOffset).ConfigureAwait(false);
                }

                // Flush TIFF file header.
                await writer.FlushAsync().ConfigureAwait(false);

            }
            catch (Exception e)
            {
                _testOutputHelper.WriteLine(e.ToString());
                throw;
            }
            finally
            {
                if (File.Exists(tmpFilePath))
                {
                    File.Delete(tmpFilePath);
                }
            }
        }

        private async Task<TiffStreamOffset> WriteMainIfd(OptimizedPartialBufferProvider optimizedBufferProvider, TiffFileWriter writer,
            TiffImageEncoder<TiffGray8> encoder, List<ulong> subIfdOffsets, TiffStreamOffset? lastMainIfdOffset)
        {
            using TiffImageFileDirectoryWriter ifdWriter = writer.CreateImageFileDirectory();

            optimizedBufferProvider.SetLayer(0);
            var optimizedTiledPixelReader = new TiffOptimizedPartialPixelBufferReaderAdapter<TiffGray8>(optimizedBufferProvider);
            await encoder.EncodeAsync(ifdWriter, optimizedTiledPixelReader).ConfigureAwait(false);

            await ifdWriter.WriteTagAsync(TiffTag.SamplesPerPixel, TiffValueCollection.Single((short)1)).ConfigureAwait(false);
            await ifdWriter.WriteTagAsync(TiffTag.PlanarConfiguration, TiffValueCollection.Single((short)1)).ConfigureAwait(false);
            await ifdWriter.WriteTagAsync(TiffTag.SampleFormat, TiffValueCollection.Single((short)1)).ConfigureAwait(false);

            await ifdWriter.WriteTagAsync(TiffTag.SubIFDs, TiffValueCollection.UnsafeWrap(subIfdOffsets.ToArray())).ConfigureAwait(false);

            TiffStreamOffset nextIfdOffset;
            if (lastMainIfdOffset != null)
            {
                nextIfdOffset = await ifdWriter.FlushAsync(lastMainIfdOffset.Value).ConfigureAwait(false);
            }
            else
            {
                nextIfdOffset = await ifdWriter.FlushAsync().ConfigureAwait(false);
            }

            return nextIfdOffset;
        }

        private async Task<List<ulong>> WriteSubIfds(OptimizedPartialBufferProvider optimizedBufferProvider, TiffFileWriter writer,
            TiffImageEncoder<TiffGray8> encoder, int numberOfPyramidLevels)
        {
            var subIfdOffsets = new List<ulong>();

            for (int level = numberOfPyramidLevels; level >= 1; level--)
            {
                using TiffImageFileDirectoryWriter ifdWriter = writer.CreateImageFileDirectory();

                optimizedBufferProvider.SetLayer(level);

                var optimizedTiledPixelReader = new TiffOptimizedPartialPixelBufferReaderAdapter<TiffGray8>(optimizedBufferProvider);
                await encoder.EncodeAsync(ifdWriter, optimizedTiledPixelReader).ConfigureAwait(false);

                await ifdWriter.WriteTagAsync(TiffTag.SamplesPerPixel, TiffValueCollection.Single((short)1)).ConfigureAwait(false);
                await ifdWriter.WriteTagAsync(TiffTag.PlanarConfiguration, TiffValueCollection.Single((short)1)).ConfigureAwait(false);
                await ifdWriter.WriteTagAsync(TiffTag.SampleFormat, TiffValueCollection.Single((short)1)).ConfigureAwait(false);

                await ifdWriter.WriteTagAsync(TiffTag.NewSubfileType, TiffValueCollection.Single((short)1)).ConfigureAwait(false);
                TiffStreamOffset ifdOffset = await ifdWriter.FlushAsync().ConfigureAwait(false);
                subIfdOffsets.Add((ulong) ifdOffset.ToInt64());
            }

            return subIfdOffsets;
        }
    }
}
