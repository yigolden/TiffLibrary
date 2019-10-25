using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace TiffLibrary.Tests
{
    public class FileContentSourceTests
    {
        public static IEnumerable<object[]> GetContentSources()
        {
            // Reference content
            string file = @"Assets/PhotometricInterpretation/flower-minisblack-big-endian.tif";
            byte[] referenceContent = File.ReadAllBytes(file);
            byte[] clonedContent = new byte[2000 + referenceContent.Length];
            referenceContent.CopyTo(clonedContent, 1000);

            // File source
            yield return new object[]
            {
                referenceContent,
                TiffFileContentSource.Create(file)
            };

            // Stream source
            yield return new object[]
            {
                referenceContent,
                TiffFileContentSource.Create(new MemoryStream(clonedContent, 1000, referenceContent.Length, writable: false), leaveOpen: true)
            };

            // ReadOnlyMemory source
            yield return new object[]
            {
                referenceContent,
                TiffFileContentSource.Create(clonedContent.AsMemory(1000, referenceContent.Length))
            };

            // byte[] source
            yield return new object[]
            {
                referenceContent,
                TiffFileContentSource.Create(clonedContent, 1000, referenceContent.Length)
            };
        }

        [Theory]
        [MemberData(nameof(GetContentSources))]
        public async Task TestRead(byte[] referenceContent, TiffFileContentSource source)
        {
            var rand = new Random(42);
            try
            {
                TiffFileContentReader reader = await source.OpenReaderAsync();

                // Random read within the range of the file.
                for (int i = 0; i < 100; i++)
                {
                    int offset = rand.Next(0, referenceContent.Length);
                    int count = 1;
                    if (offset + 1 < referenceContent.Length)
                    {
                        count = rand.Next(1, referenceContent.Length - offset);
                    }

                    await AssertReadAsync(reader, offset, count, count);
                }

                // Read on the edge of the file
                await AssertReadAsync(reader, referenceContent.Length - 2048, 4096, 2048);

                // Read past the end of the file.
                await AssertReadAsync(reader, referenceContent.Length, 4096, 0);
                await AssertReadAsync(reader, referenceContent.Length + 2048, 4096, 0);
            }
            finally
            {
                await source.DisposeAsync();
            }

            async Task AssertReadAsync(TiffFileContentReader reader, int fileOffset, int count, int expectedCount)
            {
                int readCount;
                byte[] buffer = ArrayPool<byte>.Shared.Rent(count);
                try
                {
                    // Use Memory API
                    Array.Clear(buffer, 0, count);
                    readCount = await reader.ReadAsync(fileOffset, new Memory<byte>(buffer, 0, count));
                    Assert.Equal(expectedCount, readCount);
                    Assert.True(buffer.AsSpan(0, readCount).SequenceEqual(referenceContent.AsSpan(Math.Min(referenceContent.Length, fileOffset), expectedCount)));

                    // Use ArraySegment API
                    Array.Clear(buffer, 0, count);
                    readCount = await reader.ReadAsync(fileOffset, new ArraySegment<byte>(buffer, 0, count));
                    Assert.Equal(expectedCount, readCount);
                    Assert.True(buffer.AsSpan(0, readCount).SequenceEqual(referenceContent.AsSpan(Math.Min(referenceContent.Length, fileOffset), expectedCount)));
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
        }

    }
}
