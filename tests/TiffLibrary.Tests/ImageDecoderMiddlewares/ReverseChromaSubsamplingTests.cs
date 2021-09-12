﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TiffLibrary.ImageDecoder;
using Xunit;

namespace TiffLibrary.Tests.ImageDecoderMiddlewares
{
    public class ReverseChromaSubsamplingTests
    {
        public static IEnumerable<object[]> GetChunkyTestData()
        {
            yield return new object[]
            {
                4, 4, // width, height
                1, 1, // subsampling,
                // input
                new byte[]
                {
                    0x1a, 0x1b, 0x1c, 0x2a, 0x2b, 0x2c, 0x3a, 0x3b, 0x3c, 0x4a, 0x4b, 0x4c,
                    0x5a, 0x5b, 0x5c, 0x6a, 0x6b, 0x6c, 0x7a, 0x7b, 0x7c, 0x8a, 0x8b, 0x8c,
                    0x9a, 0x9b, 0x9c, 0xaa, 0xab, 0xac, 0xba, 0xbb, 0xbc, 0xca, 0xcb, 0xcc,
                    0xda, 0xdb, 0xdc, 0xea, 0xeb, 0xec, 0xfa, 0xfb, 0xfc, 0x0a, 0x0b, 0x0c,
                },
                // output
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
                // input
                new byte[]
                {
                    0x1a, 0x2a, 0x1b, 0x1c, 0x3a, 0x4a, 0x3b, 0x3c,
                    0x5a, 0x6a, 0x5b, 0x5c, 0x7a, 0x8a, 0x7b, 0x7c,
                    0x9a, 0xaa, 0x9b, 0x9c, 0xba, 0xca, 0xbb, 0xbc,
                    0xda, 0xea, 0xdb, 0xdc, 0xfa, 0x0a, 0xfb, 0xfc,
                },
                // output
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
                // input
                new byte[]
                {
                    0x1a, 0x5a, 0x1b, 0x1c, 0x2a, 0x6a, 0x2b, 0x2c, 0x3a, 0x7a, 0x3b, 0x3c, 0x4a, 0x8a, 0x4b, 0x4c,
                    0x9a, 0xda, 0x9b, 0x9c, 0xaa, 0xea, 0xab, 0xac, 0xba, 0xfa, 0xbb, 0xbc, 0xca, 0x0a, 0xcb, 0xcc,
                },
                // output
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
                // input
                new byte[]
                {
                    0x1a, 0x2a, 0x5a, 0x6a, 0x1b, 0x1c, 0x3a, 0x4a, 0x7a, 0x8a, 0x3b, 0x3c,
                    0x9a, 0xaa, 0xda, 0xea, 0x9b, 0x9c, 0xba, 0xca, 0xfa, 0x0a, 0xbb, 0xbc,
                },
                // output
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
                // input
                new byte[]
                {
                    0x1a, 0x2a, 0x1b, 0x1c, 0x3a, 0x4a, 0x3b, 0x3c,
                    0x5a, 0x6a, 0x5b, 0x5c, 0x7a, 0x8a, 0x7b, 0x7c,
                    0x9a, 0xaa, 0x9b, 0x9c, 0xba, 0xca, 0xbb, 0xbc,
                    0xda, 0xea, 0xdb, 0xdc, 0xfa, 0x0a, 0xfb, 0xfc,
                },
                // output
                new byte[]
                {
                    0x1a, 0x1b, 0x1c, 0x2a, 0x1b, 0x1c, 0x3a, 0x3b, 0x3c,
                    0x5a, 0x5b, 0x5c, 0x6a, 0x5b, 0x5c, 0x7a, 0x7b, 0x7c,
                    0x9a, 0x9b, 0x9c, 0xaa, 0x9b, 0x9c, 0xba, 0xbb, 0xbc,
                }
            };


            yield return new object[]
            {
                4, 3, // width, height
                1, 2, // subsampling,
                // input
                new byte[]
                {
                    0x1a, 0x5a, 0x1b, 0x1c, 0x2a, 0x6a, 0x2b, 0x2c, 0x3a, 0x7a, 0x3b, 0x3c, 0x4a, 0x8a, 0x4b, 0x4c,
                    0x9a, 0xda, 0x9b, 0x9c, 0xaa, 0xea, 0xab, 0xac, 0xba, 0xfa, 0xbb, 0xbc, 0xca, 0x0a, 0xcb, 0xcc,
                },
                // output
                new byte[]
                {
                    0x1a, 0x1b, 0x1c, 0x2a, 0x2b, 0x2c, 0x3a, 0x3b, 0x3c, 0x4a, 0x4b, 0x4c,
                    0x5a, 0x1b, 0x1c, 0x6a, 0x2b, 0x2c, 0x7a, 0x3b, 0x3c, 0x8a, 0x4b, 0x4c,
                    0x9a, 0x9b, 0x9c, 0xaa, 0xab, 0xac, 0xba, 0xbb, 0xbc, 0xca, 0xcb, 0xcc,
                }
            };

            yield return new object[]
            {
                3, 3, // width, height
                2, 2, // subsampling,
                // input
                new byte[]
                {
                    0x1a, 0x2a, 0x5a, 0x6a, 0x1b, 0x1c, 0x3a, 0x4a, 0x7a, 0x8a, 0x3b, 0x3c,
                    0x9a, 0xaa, 0xda, 0xea, 0x9b, 0x9c, 0xba, 0xca, 0xfa, 0x0a, 0xbb, 0xbc,
                },
                // output
                new byte[]
                {
                    0x1a, 0x1b, 0x1c, 0x2a, 0x1b, 0x1c, 0x3a, 0x3b, 0x3c,
                    0x5a, 0x1b, 0x1c, 0x6a, 0x1b, 0x1c, 0x7a, 0x3b, 0x3c,
                    0x9a, 0x9b, 0x9c, 0xaa, 0x9b, 0x9c, 0xba, 0xbb, 0xbc,
                }
            };
        }

        public static IEnumerable<object[]> GetPlanarTestData()
        {
            yield return new object[]
            {
                4, 4, // width, height
                1, 1, // subsampling,
                // input
                new byte[]
                {
                    0x1a, 0x2a, 0x3a, 0x4a,
                    0x5a, 0x6a, 0x7a, 0x8a,
                    0x9a, 0xaa, 0xba, 0xca,
                    0xda, 0xea, 0xfa, 0x0a,

                    0x1b, 0x2b, 0x3b, 0x4b,
                    0x5b, 0x6b, 0x7b, 0x8b,
                    0x9b, 0xab, 0xbb, 0xcb,
                    0xdb, 0xeb, 0xfb, 0x0b,

                    0x1c, 0x2c, 0x3c, 0x4c,
                    0x5c, 0x6c, 0x7c, 0x8c,
                    0x9c, 0xac, 0xbc, 0xcc,
                    0xdc, 0xec, 0xfc, 0x0c,
                },
                // output
                new byte[]
                {
                    0x1a, 0x2a, 0x3a, 0x4a,
                    0x5a, 0x6a, 0x7a, 0x8a,
                    0x9a, 0xaa, 0xba, 0xca,
                    0xda, 0xea, 0xfa, 0x0a,

                    0x1b, 0x2b, 0x3b, 0x4b,
                    0x5b, 0x6b, 0x7b, 0x8b,
                    0x9b, 0xab, 0xbb, 0xcb,
                    0xdb, 0xeb, 0xfb, 0x0b,

                    0x1c, 0x2c, 0x3c, 0x4c,
                    0x5c, 0x6c, 0x7c, 0x8c,
                    0x9c, 0xac, 0xbc, 0xcc,
                    0xdc, 0xec, 0xfc, 0x0c,
                }
            };

            yield return new object[]
            {
                4, 4, // width, height
                2, 1, // subsampling,
                // input
                new byte[]
                {
                    0x1a, 0x2a, 0x3a, 0x4a,
                    0x5a, 0x6a, 0x7a, 0x8a,
                    0x9a, 0xaa, 0xba, 0xca,
                    0xda, 0xea, 0xfa, 0x0a,

                    0x1b, 0x3b,
                    0x5b, 0x7b,
                    0x9b, 0xbb,
                    0xdb, 0xfb,
                    0, 0, 0, 0, 0, 0, 0, 0,

                    0x1c, 0x3c,
                    0x5c, 0x7c,
                    0x9c, 0xbc,
                    0xdc, 0xfc,
                    0, 0, 0, 0, 0, 0, 0, 0,
                },
                // output
                new byte[]
                {
                    0x1a, 0x2a, 0x3a, 0x4a,
                    0x5a, 0x6a, 0x7a, 0x8a,
                    0x9a, 0xaa, 0xba, 0xca,
                    0xda, 0xea, 0xfa, 0x0a,

                    0x1b, 0x1b, 0x3b, 0x3b,
                    0x5b, 0x5b, 0x7b, 0x7b,
                    0x9b, 0x9b, 0xbb, 0xbb,
                    0xdb, 0xdb, 0xfb, 0xfb,

                    0x1c, 0x1c, 0x3c, 0x3c,
                    0x5c, 0x5c, 0x7c, 0x7c,
                    0x9c, 0x9c, 0xbc, 0xbc,
                    0xdc, 0xdc, 0xfc, 0xfc,
                }
            };

            yield return new object[]
            {
                4, 4, // width, height
                1, 2, // subsampling,
                // input
                new byte[]
                {
                    0x1a, 0x2a, 0x3a, 0x4a,
                    0x5a, 0x6a, 0x7a, 0x8a,
                    0x9a, 0xaa, 0xba, 0xca,
                    0xda, 0xea, 0xfa, 0x0a,

                    0x1b, 0x2b, 0x3b, 0x4b,
                    0x9b, 0xab, 0xbb, 0xcb,
                    0, 0, 0, 0, 0, 0, 0, 0,

                    0x1c, 0x2c, 0x3c, 0x4c,
                    0x9c, 0xac, 0xbc, 0xcc,
                    0, 0, 0, 0, 0, 0, 0, 0,
                },
                // output
                new byte[]
                {
                    0x1a, 0x2a, 0x3a, 0x4a,
                    0x5a, 0x6a, 0x7a, 0x8a,
                    0x9a, 0xaa, 0xba, 0xca,
                    0xda, 0xea, 0xfa, 0x0a,

                    0x1b, 0x2b, 0x3b, 0x4b,
                    0x1b, 0x2b, 0x3b, 0x4b,
                    0x9b, 0xab, 0xbb, 0xcb,
                    0x9b, 0xab, 0xbb, 0xcb,

                    0x1c, 0x2c, 0x3c, 0x4c,
                    0x1c, 0x2c, 0x3c, 0x4c,
                    0x9c, 0xac, 0xbc, 0xcc,
                    0x9c, 0xac, 0xbc, 0xcc,
                }
            };



            yield return new object[]
            {
                4, 4, // width, height
                2, 2, // subsampling,
                // input
                new byte[]
                {
                    0x1a, 0x2a, 0x3a, 0x4a,
                    0x5a, 0x6a, 0x7a, 0x8a,
                    0x9a, 0xaa, 0xba, 0xca,
                    0xda, 0xea, 0xfa, 0x0a,

                    0x1b, 0x3b,
                    0x9b, 0xbb,
                    0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,

                    0x1c, 0x3c,
                    0x9c, 0xbc,
                    0, 0, 0, 0,
                    0, 0, 0, 0, 0, 0, 0, 0,
                },
                // output
                new byte[]
                {
                    0x1a, 0x2a, 0x3a, 0x4a,
                    0x5a, 0x6a, 0x7a, 0x8a,
                    0x9a, 0xaa, 0xba, 0xca,
                    0xda, 0xea, 0xfa, 0x0a,

                    0x1b, 0x1b, 0x3b, 0x3b,
                    0x1b, 0x1b, 0x3b, 0x3b,
                    0x9b, 0x9b, 0xbb, 0xbb,
                    0x9b, 0x9b, 0xbb, 0xbb,

                    0x1c, 0x1c, 0x3c, 0x3c,
                    0x1c, 0x1c, 0x3c, 0x3c,
                    0x9c, 0x9c, 0xbc, 0xbc,
                    0x9c, 0x9c, 0xbc, 0xbc,
                }
            };
        }

        [Theory]
        [MemberData(nameof(GetChunkyTestData))]
        public async Task TestChunkyData(int width, int height, ushort horizontalSubsampling, ushort verticalSubsampling, byte[] input, byte[] output)
        {
            var middleware = new TiffReverseChromaSubsampling8Middleware(horizontalSubsampling, verticalSubsampling, false);
            byte[] buffer = new byte[Math.Max(input.Length, output.Length)];
            input.AsSpan().CopyTo(buffer);
            var context = new TestDecoderContext
            {
                MemoryPool = MemoryPool<byte>.Shared,
                UncompressedData = buffer,
                SourceImageSize = new TiffSize(width, height)
            };
            await middleware.InvokeAsync(context, new EmptyPipelineNode());
            Assert.True(buffer.AsSpan(0, output.Length).SequenceEqual(output));
        }



        [Theory]
        [MemberData(nameof(GetPlanarTestData))]
        public async Task TestPlanarData(int width, int height, ushort horizontalSubsampling, ushort verticalSubsampling, byte[] input, byte[] output)
        {
            var middleware = new TiffReverseChromaSubsampling8Middleware(horizontalSubsampling, verticalSubsampling, true);
            byte[] buffer = new byte[output.Length];
            input.AsSpan().CopyTo(buffer);
            var context = new TestDecoderContext
            {
                UncompressedData = buffer,
                SourceImageSize = new TiffSize(width, height)
            };
            await middleware.InvokeAsync(context, new EmptyPipelineNode());
            Assert.True(buffer.AsSpan().SequenceEqual(output));
        }

        internal class TestDecoderContext : TiffImageDecoderContext
        {
            public override MemoryPool<byte> MemoryPool { get; set; }
            public override CancellationToken CancellationToken { get; set; }
            public override TiffOperationContext OperationContext { get; set; }
            public override TiffFileContentReader ContentReader { get; set; }
            public override TiffValueCollection<TiffStreamRegion> PlanarRegions { get; set; }
            public override Memory<byte> UncompressedData { get; set; }
            public override TiffSize SourceImageSize { get; set; }
            public override TiffPoint SourceReadOffset { get; set; }
            public override TiffSize ReadSize { get; set; }

            public override TiffPixelBufferWriter<TPixel> GetWriter<TPixel>()
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

        internal class EmptyPipelineNode : ITiffImageDecoderPipelineNode
        {
            public ValueTask RunAsync(TiffImageDecoderContext context)
            {
                return default;
            }
        }
    }
}
