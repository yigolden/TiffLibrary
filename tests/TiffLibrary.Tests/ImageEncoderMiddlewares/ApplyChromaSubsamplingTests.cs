﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TiffLibrary.ImageEncoder;
using Xunit;

namespace TiffLibrary.Tests.ImageEncoderMiddlewares
{
    public class ApplyChromaSubsamplingTests
    {
        public static IEnumerable<object[]> GetChunkyTestData()
        {
            yield return new object[]
            {
                4, 4, // width, height
                1, 1, // subsampling,
                // subsampled
                new byte[]
                {
                    0x1a, 0x1b, 0x1c, 0x2a, 0x2b, 0x2c, 0x3a, 0x3b, 0x3c, 0x4a, 0x4b, 0x4c,
                    0x5a, 0x5b, 0x5c, 0x6a, 0x6b, 0x6c, 0x7a, 0x7b, 0x7c, 0x8a, 0x8b, 0x8c,
                    0x9a, 0x9b, 0x9c, 0xaa, 0xab, 0xac, 0xba, 0xbb, 0xbc, 0xca, 0xcb, 0xcc,
                    0xda, 0xdb, 0xdc, 0xea, 0xeb, 0xec, 0xfa, 0xfb, 0xfc, 0x0a, 0x0b, 0x0c,
                },
                // original
                new byte[]
                {
                    0x1a, 0x1b, 0x1c, 0x2a, 0x2b, 0x2c, 0x3a, 0x3b, 0x3c, 0x4a, 0x4b, 0x4c,
                    0x5a, 0x5b, 0x5c, 0x6a, 0x6b, 0x6c, 0x7a, 0x7b, 0x7c, 0x8a, 0x8b, 0x8c,
                    0x9a, 0x9b, 0x9c, 0xaa, 0xab, 0xac, 0xba, 0xbb, 0xbc, 0xca, 0xcb, 0xcc,
                    0xda, 0xdb, 0xdc, 0xea, 0xeb, 0xec, 0xfa, 0xfb, 0xfc, 0x0a, 0x0b, 0x0c,
                }
            };

            yield return new object[]
            {
                4, 4, // width, height
                2, 1, // subsampling,
                // subsampled
                new byte[]
                {
                    0x1a, 0x2a, 0x1b, 0x1c, 0x3a, 0x4a, 0x3b, 0x3c,
                    0x5a, 0x6a, 0x5b, 0x5c, 0x7a, 0x8a, 0x7b, 0x7c,
                    0x9a, 0xaa, 0x9b, 0x9c, 0xba, 0xca, 0xbb, 0xbc,
                    0xda, 0xea, 0xdb, 0xdc, 0xfa, 0x0a, 0xfb, 0xfc,
                },
                // original
                new byte[]
                {
                    0x1a, 0x1b, 0x1c, 0x2a, 0x1b, 0x1c, 0x3a, 0x3b, 0x3c, 0x4a, 0x3b, 0x3c,
                    0x5a, 0x5b, 0x5c, 0x6a, 0x5b, 0x5c, 0x7a, 0x7b, 0x7c, 0x8a, 0x7b, 0x7c,
                    0x9a, 0x9b, 0x9c, 0xaa, 0x9b, 0x9c, 0xba, 0xbb, 0xbc, 0xca, 0xbb, 0xbc,
                    0xda, 0xdb, 0xdc, 0xea, 0xdb, 0xdc, 0xfa, 0xfb, 0xfc, 0x0a, 0xfb, 0xfc,
                }
            };


            yield return new object[]
            {
                4, 4, // width, height
                1, 2, // subsampling,
                // subsampled
                new byte[]
                {
                    0x1a, 0x5a, 0x1b, 0x1c, 0x2a, 0x6a, 0x2b, 0x2c, 0x3a, 0x7a, 0x3b, 0x3c, 0x4a, 0x8a, 0x4b, 0x4c,
                    0x9a, 0xda, 0x9b, 0x9c, 0xaa, 0xea, 0xab, 0xac, 0xba, 0xfa, 0xbb, 0xbc, 0xca, 0x0a, 0xcb, 0xcc,
                },
                // original
                new byte[]
                {
                    0x1a, 0x1b, 0x1c, 0x2a, 0x2b, 0x2c, 0x3a, 0x3b, 0x3c, 0x4a, 0x4b, 0x4c,
                    0x5a, 0x1b, 0x1c, 0x6a, 0x2b, 0x2c, 0x7a, 0x3b, 0x3c, 0x8a, 0x4b, 0x4c,
                    0x9a, 0x9b, 0x9c, 0xaa, 0xab, 0xac, 0xba, 0xbb, 0xbc, 0xca, 0xcb, 0xcc,
                    0xda, 0x9b, 0x9c, 0xea, 0xab, 0xac, 0xfa, 0xbb, 0xbc, 0x0a, 0xcb, 0xcc,
                }
            };

            yield return new object[]
            {
                4, 4, // width, height
                2, 2, // subsampling,
                // subsampled
                new byte[]
                {
                    0x1a, 0x2a, 0x5a, 0x6a, 0x1b, 0x1c, 0x3a, 0x4a, 0x7a, 0x8a, 0x3b, 0x3c,
                    0x9a, 0xaa, 0xda, 0xea, 0x9b, 0x9c, 0xba, 0xca, 0xfa, 0x0a, 0xbb, 0xbc,
                },
                // original
                new byte[]
                {
                    0x1a, 0x1b, 0x1c, 0x2a, 0x1b, 0x1c, 0x3a, 0x3b, 0x3c, 0x4a, 0x3b, 0x3c,
                    0x5a, 0x1b, 0x1c, 0x6a, 0x1b, 0x1c, 0x7a, 0x3b, 0x3c, 0x8a, 0x3b, 0x3c,
                    0x9a, 0x9b, 0x9c, 0xaa, 0x9b, 0x9c, 0xba, 0xbb, 0xbc, 0xca, 0xbb, 0xbc,
                    0xda, 0x9b, 0x9c, 0xea, 0x9b, 0x9c, 0xfa, 0xbb, 0xbc, 0x0a, 0xbb, 0xbc,
                }
            };

            // If ImageWidth and ImageLength are not multiples of
            // ChromaSubsampleHoriz and ChromaSubsampleVert respectively, then the
            // source data shall be padded to the next integer multiple of these
            // values before downsampling.

            yield return new object[]
            {
                3, 3, // width, height
                2, 1, // subsampling,
                // subsampled
                new byte[]
                {
                    0x1a, 0x2a, 0x1b, 0x1c, 0x3a, 0x3a, 0x3b, 0x3c,
                    0x5a, 0x6a, 0x5b, 0x5c, 0x7a, 0x7a, 0x7b, 0x7c,
                    0x9a, 0xaa, 0x9b, 0x9c, 0xba, 0xba, 0xbb, 0xbc,
                },
                // original
                new byte[]
                {
                    0x1a, 0x1b, 0x1c, 0x2a, 0x1b, 0x1c, 0x3a, 0x3b, 0x3c,
                    0x5a, 0x5b, 0x5c, 0x6a, 0x5b, 0x5c, 0x7a, 0x7b, 0x7c,
                    0x9a, 0x9b, 0x9c, 0xaa, 0x9b, 0x9c, 0xba, 0xbb, 0xbc,
                }
            };

            yield return new object[]
            {
                3, 3, // width, height
                1, 2, // subsampling,
                // subsampled
                new byte[]
                {
                    0x1a, 0x5a, 0x1b, 0x1c, 0x2a, 0x6a, 0x2b, 0x2c, 0x3a, 0x7a, 0x3b, 0x3c,
                    0x9a, 0x9a, 0x9b, 0x9c, 0xaa, 0xaa, 0xab, 0xac, 0xba, 0xba, 0xbb, 0xbc,
                },
                // original
                new byte[]
                {
                    0x1a, 0x1b, 0x1c, 0x2a, 0x2b, 0x2c, 0x3a, 0x3b, 0x3c,
                    0x5a, 0x1b, 0x1c, 0x6a, 0x2b, 0x2c, 0x7a, 0x3b, 0x3c,
                    0x9a, 0x9b, 0x9c, 0xaa, 0xab, 0xac, 0xba, 0xbb, 0xbc,
                }
            };

            yield return new object[]
            {
                3, 3, // width, height
                2, 2, // subsampling,
                // subsampled
                new byte[]
                {
                    0x1a, 0x2a, 0x5a, 0x6a, 0x1b, 0x1c, 0x3a, 0x3a, 0x7a, 0x7a, 0x3b, 0x3c,
                    0x9a, 0xaa, 0xaa, 0xaa, 0x9b, 0x9c, 0xba, 0xba, 0xba, 0xba, 0xbb, 0xbc,
                },
                // original
                new byte[]
                {
                    0x1a, 0x1b, 0x1c, 0x2a, 0x1b, 0x1c, 0x3a, 0x3b, 0x3c,
                    0x5a, 0x1b, 0x1c, 0x6a, 0x1b, 0x1c, 0x7a, 0x3b, 0x3c,
                    0x9a, 0x9b, 0x9c, 0xaa, 0x9b, 0x9c, 0xba, 0xbb, 0xbc,
                }
            };

        }

        [Theory]
        [MemberData(nameof(GetChunkyTestData))]
        public async Task TestChunkyData(int width, int height, ushort horizontalSubsampling, ushort verticalSubsampling, byte[] subsampled, byte[] original)
        {
            var middleware = new TiffApplyChromaSubsamplingMiddleware<byte>(horizontalSubsampling, verticalSubsampling);
            byte[] buffer = new byte[original.Length];
            original.AsSpan().CopyTo(buffer);
            var context = new TestEncoderContext<byte>
            {
                MemoryPool = MemoryPool<byte>.Shared,
                BitsPerSample = TiffValueCollection.UnsafeWrap(new ushort[] { 8, 8, 8 }),
                UncompressedData = buffer,
                ImageSize = new TiffSize(width, height)
            };
            await middleware.InvokeAsync(context, new ValidationPipelineNode<byte>(subsampled));
        }


        internal class TestEncoderContext<TPixel> : TiffImageEncoderContext<TPixel> where TPixel : unmanaged
        {
            public override MemoryPool<byte> MemoryPool { get; set; }
            public override CancellationToken CancellationToken { get; set; }
            public override TiffFileWriter FileWriter { get; set; }
            public override TiffImageFileDirectoryWriter IfdWriter { get; set; }
            public override TiffPhotometricInterpretation PhotometricInterpretation { get; set; }
            public override TiffValueCollection<ushort> BitsPerSample { get; set; }
            public override TiffSize ImageSize { get; set; }
            public override Memory<byte> UncompressedData { get; set; }
            public override TiffStreamRegion OutputRegion { get; set; }

            public override TiffPixelBufferWriter<TPixel> ConvertWriter<TBuffer>(TiffPixelBufferWriter<TBuffer> writer)
            {
                throw new NotSupportedException();
            }

            public override TiffPixelBufferReader<TPixel> GetReader()
            {
                throw new NotSupportedException();
            }

            public override void RegisterService(Type serviceType, object service)
            {
                throw new NotSupportedException();
            }

            public override object GetService(Type serviceType)
            {
                throw new NotSupportedException();
            }
        }


        internal class ValidationPipelineNode<TPixel> : ITiffImageEncoderPipelineNode<TPixel> where TPixel : unmanaged
        {
            private readonly byte[] _expected;

            public ValidationPipelineNode(byte[] expected)
            {
                _expected = expected;
            }

            public ValueTask RunAsync(TiffImageEncoderContext<TPixel> context)
            {
                Assert.True(context.UncompressedData.Span.SequenceEqual(_expected));
                return default;
            }
        }
    }
}
