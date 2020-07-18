using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace TiffLibrary.Tests.FieldReader
{
    public class CompatibilityTests
    {
        public static IEnumerable<object[]> GetTestIterations()
        {
            yield return new object[] { 1, false };
            yield return new object[] { 2, false };
            yield return new object[] { 4, false };
            yield return new object[] { 8, false };
            yield return new object[] { 16, false };
            yield return new object[] { 199, false };
            yield return new object[] { 1, true };
            yield return new object[] { 2, true };
            yield return new object[] { 4, true };
            yield return new object[] { 8, true };
            yield return new object[] { 16, true };
            yield return new object[] { 199, true };
        }

        private static async Task<Stream> GenerateTiffAsync(bool bigTiff, Func<TiffImageFileDirectoryWriter, Task> ifdAction)
        {
            var ms = new MemoryStream();
            using (TiffFileWriter writer = await TiffFileWriter.OpenAsync(ms, leaveOpen: true, useBigTiff: bigTiff))
            {
                using (TiffImageFileDirectoryWriter ifd = writer.CreateImageFileDirectory())
                {
                    await ifdAction(ifd);
                    writer.SetFirstImageFileDirectoryOffset(await ifd.FlushAsync());
                }
                await writer.FlushAsync();
            }

            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        private static async Task TestValidConversionAsync<T>(TiffFieldReader fieldReader, TiffImageFileDirectoryEntry entry, string methodName, T[] refData) where T : unmanaged
        {
            MethodInfo method1 = typeof(TiffFieldReader).GetMethod(methodName, new Type[] { typeof(TiffImageFileDirectoryEntry), typeof(bool), typeof(CancellationToken) });
            MethodInfo method2 = typeof(TiffFieldReader).GetMethod(methodName, new Type[] { typeof(TiffImageFileDirectoryEntry), typeof(int), typeof(bool), typeof(CancellationToken) });
            MethodInfo method3 = typeof(TiffFieldReader).GetMethod(methodName, new Type[] { typeof(TiffImageFileDirectoryEntry), typeof(int), typeof(Memory<T>), typeof(bool), typeof(CancellationToken) });
            Assert.NotNull(method1);
            Assert.NotNull(method2);
            Assert.NotNull(method3);
            var delegate1 = (Func<TiffImageFileDirectoryEntry, bool, CancellationToken, ValueTask<TiffValueCollection<T>>>)method1.CreateDelegate(typeof(Func<TiffImageFileDirectoryEntry, bool, CancellationToken, ValueTask<TiffValueCollection<T>>>), fieldReader);
            var delegate2 = (Func<TiffImageFileDirectoryEntry, int, bool, CancellationToken, ValueTask<TiffValueCollection<T>>>)method2.CreateDelegate(typeof(Func<TiffImageFileDirectoryEntry, int, bool, CancellationToken, ValueTask<TiffValueCollection<T>>>), fieldReader);
            var delegate3 = (Func<TiffImageFileDirectoryEntry, int, Memory<T>, bool, CancellationToken, ValueTask>)method3.CreateDelegate(typeof(Func<TiffImageFileDirectoryEntry, int, Memory<T>, bool, CancellationToken, ValueTask>), fieldReader);

            // Test basic overload
            TiffValueCollection<T> testData = await delegate1(entry, false, default);
            Assert.True(MemoryMarshal.AsBytes(refData.AsSpan()).SequenceEqual(MemoryMarshal.AsBytes(testData.ToArray().AsSpan())));

            // Test overload with sizeLimit argument
            int sizeLimit = refData.Length / 2;
            testData = await delegate2(entry, sizeLimit, false, default);
            Assert.Equal(sizeLimit, testData.Count);
            Assert.True(MemoryMarshal.AsBytes(refData.AsSpan(0, sizeLimit)).SequenceEqual(MemoryMarshal.AsBytes(testData.ToArray().AsSpan())));

            // Test overload with external buffer
            int offset = refData.Length / 4;
            T[] testArray = new T[sizeLimit];
            await delegate3(entry, offset, testArray, false, default);
            Assert.True(MemoryMarshal.AsBytes(refData.AsSpan(offset, sizeLimit)).SequenceEqual(MemoryMarshal.AsBytes(testArray.AsSpan())));

            // Test invalid parameter
            Assert.Throws<ArgumentOutOfRangeException>("offset", () => delegate3(entry, -1, new T[entry.ValueCount], false, default));
            Assert.Throws<ArgumentOutOfRangeException>("offset", () => delegate3(entry, (int)(entry.ValueCount + 1), new T[1], false, default));
            Assert.Throws<ArgumentOutOfRangeException>("destination", () => delegate3(entry, 0, new T[entry.ValueCount + 1], false, default));
        }

        private static async Task TestInvalidConversionAsync<T>(TiffFieldReader fieldReader, TiffImageFileDirectoryEntry entry, string methodName, int length) where T : unmanaged
        {
            MethodInfo method1 = typeof(TiffFieldReader).GetMethod(methodName, new Type[] { typeof(TiffImageFileDirectoryEntry), typeof(bool), typeof(CancellationToken) });
            MethodInfo method2 = typeof(TiffFieldReader).GetMethod(methodName, new Type[] { typeof(TiffImageFileDirectoryEntry), typeof(int), typeof(bool), typeof(CancellationToken) });
            MethodInfo method3 = typeof(TiffFieldReader).GetMethod(methodName, new Type[] { typeof(TiffImageFileDirectoryEntry), typeof(int), typeof(Memory<T>), typeof(bool), typeof(CancellationToken) });
            Assert.NotNull(method1);
            Assert.NotNull(method2);
            Assert.NotNull(method3);
            var delegate1 = (Func<TiffImageFileDirectoryEntry, bool, CancellationToken, ValueTask<TiffValueCollection<T>>>)method1.CreateDelegate(typeof(Func<TiffImageFileDirectoryEntry, bool, CancellationToken, ValueTask<TiffValueCollection<T>>>), fieldReader);
            var delegate2 = (Func<TiffImageFileDirectoryEntry, int, bool, CancellationToken, ValueTask<TiffValueCollection<T>>>)method2.CreateDelegate(typeof(Func<TiffImageFileDirectoryEntry, int, bool, CancellationToken, ValueTask<TiffValueCollection<T>>>), fieldReader);
            var delegate3 = (Func<TiffImageFileDirectoryEntry, int, Memory<T>, bool, CancellationToken, ValueTask>)method3.CreateDelegate(typeof(Func<TiffImageFileDirectoryEntry, int, Memory<T>, bool, CancellationToken, ValueTask>), fieldReader);

            // Test basic overload
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await delegate1(entry, false, default));

            // Test overload with sizeLimit argument
            int sizeLimit = length / 2;
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await delegate2(entry, sizeLimit, false, default));

            // Test overload with external buffer
            int offset = length / 4;
            T[] testArray = new T[sizeLimit];
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await delegate3(entry, offset, testArray, false, default));
        }

        [Theory]
        [MemberData(nameof(GetTestIterations))]
        public async Task TestByteCompatibility(int length, bool bigTiff)
        {
            byte[] refData = new byte[length];
            new Random(42).NextBytes(refData);

            using Stream stream = await GenerateTiffAsync(bigTiff, async ifd =>
            {
                await ifd.WriteTagAsync((TiffTag)0x1234, TiffValueCollection.UnsafeWrap(refData));
            });

            await using (TiffFileReader reader = await TiffFileReader.OpenAsync(stream, leaveOpen: true))
            {
                TiffImageFileDirectory ifd = await reader.ReadImageFileDirectoryAsync();
                TiffFieldReader fieldReader = await reader.CreateFieldReaderAsync();
                TiffImageFileDirectoryEntry entry = ifd.FindEntry((TiffTag)0x1234);
                Assert.Equal((TiffTag)0x1234, entry.Tag);

                // Byte
                await TestValidConversionAsync(fieldReader, entry, nameof(fieldReader.ReadByteFieldAsync), refData);

                // SSbyte
                await TestValidConversionAsync(fieldReader, entry, nameof(fieldReader.ReadSByteFieldAsync), Array.ConvertAll(refData, v => (sbyte)v));

                // Short
                await TestValidConversionAsync(fieldReader, entry, nameof(fieldReader.ReadShortFieldAsync), Array.ConvertAll(refData, v => (ushort)v));

                // SShort
                await TestValidConversionAsync(fieldReader, entry, nameof(fieldReader.ReadSShortFieldAsync), Array.ConvertAll(refData, v => (short)(ushort)v));

                // Long
                await TestValidConversionAsync(fieldReader, entry, nameof(fieldReader.ReadLongFieldAsync), Array.ConvertAll(refData, v => (uint)v));

                // SLong
                await TestValidConversionAsync(fieldReader, entry, nameof(fieldReader.ReadSLongFieldAsync), Array.ConvertAll(refData, v => (int)(uint)v));

                // Float
                await TestInvalidConversionAsync<float>(fieldReader, entry, nameof(fieldReader.ReadFloatFieldAsync), refData.Length);

                // Double
                await TestInvalidConversionAsync<double>(fieldReader, entry, nameof(fieldReader.ReadDoubleFieldAsync), refData.Length);

                // Rational
                await TestValidConversionAsync(fieldReader, entry, nameof(fieldReader.ReadRationalFieldAsync), Array.ConvertAll(refData, v => new TiffRational((uint)v, 1)));

                // SRational
                await TestValidConversionAsync(fieldReader, entry, nameof(fieldReader.ReadSRationalFieldAsync), Array.ConvertAll(refData, v => new TiffSRational((int)(uint)v, 1)));
            }
        }

        [Theory]
        [MemberData(nameof(GetTestIterations))]
        public async Task TestSByteCompatibility(int length, bool bigTiff)
        {
            sbyte[] refData = new sbyte[length];
            new Random(42).NextBytes(MemoryMarshal.AsBytes(refData.AsSpan()));

            using Stream stream = await GenerateTiffAsync(bigTiff, async ifd =>
            {
                await ifd.WriteTagAsync((TiffTag)0x1234, TiffFieldType.SByte, TiffValueCollection.UnsafeWrap(Array.ConvertAll(refData, b => (byte)b)));
            });

            await using (TiffFileReader reader = await TiffFileReader.OpenAsync(stream, leaveOpen: true))
            {
                TiffImageFileDirectory ifd = await reader.ReadImageFileDirectoryAsync();
                TiffFieldReader fieldReader = await reader.CreateFieldReaderAsync();
                TiffImageFileDirectoryEntry entry = ifd.FindEntry((TiffTag)0x1234);
                Assert.Equal((TiffTag)0x1234, entry.Tag);

                // Byte
                await TestValidConversionAsync(fieldReader, entry, nameof(fieldReader.ReadByteFieldAsync), Array.ConvertAll(refData, v => (byte)v));

                // SSbyte
                await TestValidConversionAsync(fieldReader, entry, nameof(fieldReader.ReadSByteFieldAsync), refData);

                // Short
                await TestInvalidConversionAsync<ushort>(fieldReader, entry, nameof(fieldReader.ReadShortFieldAsync), refData.Length);

                // SShort
                await TestValidConversionAsync(fieldReader, entry, nameof(fieldReader.ReadSShortFieldAsync), Array.ConvertAll(refData, v => (short)v));

                // Long
                await TestInvalidConversionAsync<uint>(fieldReader, entry, nameof(fieldReader.ReadLongFieldAsync), refData.Length);

                // SLong
                await TestValidConversionAsync(fieldReader, entry, nameof(fieldReader.ReadSLongFieldAsync), Array.ConvertAll(refData, v => (int)v));

                // Float
                await TestInvalidConversionAsync<float>(fieldReader, entry, nameof(fieldReader.ReadFloatFieldAsync), refData.Length);

                // Double
                await TestInvalidConversionAsync<double>(fieldReader, entry, nameof(fieldReader.ReadDoubleFieldAsync), refData.Length);

                // Rational
                await TestInvalidConversionAsync<TiffRational>(fieldReader, entry, nameof(fieldReader.ReadRationalFieldAsync), refData.Length);

                // SRational
                await TestValidConversionAsync(fieldReader, entry, nameof(fieldReader.ReadSRationalFieldAsync), Array.ConvertAll(refData, v => new TiffSRational((int)v, 1)));
            }
        }

        [Theory]
        [MemberData(nameof(GetTestIterations))]
        public async Task TestShortCompatibility(int length, bool bigTiff)
        {
            ushort[] refData = new ushort[length];
            new Random(42).NextBytes(MemoryMarshal.AsBytes(refData.AsSpan()));

            using Stream stream = await GenerateTiffAsync(bigTiff, async ifd =>
            {
                await ifd.WriteTagAsync((TiffTag)0x1234, TiffValueCollection.UnsafeWrap(refData));
            });

            await using (TiffFileReader reader = await TiffFileReader.OpenAsync(stream, leaveOpen: true))
            {
                TiffImageFileDirectory ifd = await reader.ReadImageFileDirectoryAsync();
                TiffFieldReader fieldReader = await reader.CreateFieldReaderAsync();
                TiffImageFileDirectoryEntry entry = ifd.FindEntry((TiffTag)0x1234);
                Assert.Equal((TiffTag)0x1234, entry.Tag);

                // Byte
                await TestInvalidConversionAsync<byte>(fieldReader, entry, nameof(fieldReader.ReadByteFieldAsync), refData.Length);

                // SSbyte
                await TestInvalidConversionAsync<sbyte>(fieldReader, entry, nameof(fieldReader.ReadSByteFieldAsync), refData.Length);

                // Short
                await TestValidConversionAsync(fieldReader, entry, nameof(fieldReader.ReadShortFieldAsync), refData);

                // SShort
                await TestValidConversionAsync(fieldReader, entry, nameof(fieldReader.ReadSShortFieldAsync), Array.ConvertAll(refData, v => (short)v));

                // Long
                await TestValidConversionAsync(fieldReader, entry, nameof(fieldReader.ReadLongFieldAsync), Array.ConvertAll(refData, v => (uint)v));

                // SLong
                await TestValidConversionAsync(fieldReader, entry, nameof(fieldReader.ReadSLongFieldAsync), Array.ConvertAll(refData, v => (int)(uint)v));

                // Float
                await TestInvalidConversionAsync<float>(fieldReader, entry, nameof(fieldReader.ReadFloatFieldAsync), refData.Length);

                // Double
                await TestInvalidConversionAsync<double>(fieldReader, entry, nameof(fieldReader.ReadDoubleFieldAsync), refData.Length);

                // Rational
                await TestValidConversionAsync(fieldReader, entry, nameof(fieldReader.ReadRationalFieldAsync), Array.ConvertAll(refData, v => new TiffRational(v, 1)));

                // SRational
                await TestValidConversionAsync(fieldReader, entry, nameof(fieldReader.ReadSRationalFieldAsync), Array.ConvertAll(refData, v => new TiffSRational((int)(uint)v, 1)));
            }
        }

        [Theory]
        [MemberData(nameof(GetTestIterations))]
        public async Task TestSShortCompatibility(int length, bool bigTiff)
        {
            short[] refData = new short[length];
            new Random(42).NextBytes(MemoryMarshal.AsBytes(refData.AsSpan()));

            using Stream stream = await GenerateTiffAsync(bigTiff, async ifd =>
            {
                await ifd.WriteTagAsync((TiffTag)0x1234, TiffValueCollection.UnsafeWrap(refData));
            });

            await using (TiffFileReader reader = await TiffFileReader.OpenAsync(stream, leaveOpen: true))
            {
                TiffImageFileDirectory ifd = await reader.ReadImageFileDirectoryAsync();
                TiffFieldReader fieldReader = await reader.CreateFieldReaderAsync();
                TiffImageFileDirectoryEntry entry = ifd.FindEntry((TiffTag)0x1234);
                Assert.Equal((TiffTag)0x1234, entry.Tag);

                // Byte
                await TestInvalidConversionAsync<byte>(fieldReader, entry, nameof(fieldReader.ReadByteFieldAsync), refData.Length);

                // SSbyte
                await TestInvalidConversionAsync<sbyte>(fieldReader, entry, nameof(fieldReader.ReadSByteFieldAsync), refData.Length);

                // Short
                await TestValidConversionAsync(fieldReader, entry, nameof(fieldReader.ReadShortFieldAsync), Array.ConvertAll(refData, v => (ushort)v));

                // SShort
                await TestValidConversionAsync(fieldReader, entry, nameof(fieldReader.ReadSShortFieldAsync), refData);

                // Long
                await TestInvalidConversionAsync<uint>(fieldReader, entry, nameof(fieldReader.ReadLongFieldAsync), refData.Length);

                // SLong
                await TestValidConversionAsync(fieldReader, entry, nameof(fieldReader.ReadSLongFieldAsync), Array.ConvertAll(refData, v => (int)v));

                // Float
                await TestInvalidConversionAsync<float>(fieldReader, entry, nameof(fieldReader.ReadFloatFieldAsync), refData.Length);

                // Double
                await TestInvalidConversionAsync<double>(fieldReader, entry, nameof(fieldReader.ReadDoubleFieldAsync), refData.Length);

                // Rational
                await TestInvalidConversionAsync<TiffRational>(fieldReader, entry, nameof(fieldReader.ReadRationalFieldAsync), refData.Length);

                // SRational
                await TestValidConversionAsync(fieldReader, entry, nameof(fieldReader.ReadSRationalFieldAsync), Array.ConvertAll(refData, v => new TiffSRational((int)v, 1)));
            }
        }

        [Theory]
        [MemberData(nameof(GetTestIterations))]
        public async Task TestLongCompatibility(int length, bool bigTiff)
        {
            uint[] refData = new uint[length];
            new Random(42).NextBytes(MemoryMarshal.AsBytes(refData.AsSpan()));

            using Stream stream = await GenerateTiffAsync(bigTiff, async ifd =>
            {
                await ifd.WriteTagAsync((TiffTag)0x1234, TiffValueCollection.UnsafeWrap(refData));
            });

            await using (TiffFileReader reader = await TiffFileReader.OpenAsync(stream, leaveOpen: true))
            {
                TiffImageFileDirectory ifd = await reader.ReadImageFileDirectoryAsync();
                TiffFieldReader fieldReader = await reader.CreateFieldReaderAsync();
                TiffImageFileDirectoryEntry entry = ifd.FindEntry((TiffTag)0x1234);
                Assert.Equal((TiffTag)0x1234, entry.Tag);

                // Byte
                await TestInvalidConversionAsync<byte>(fieldReader, entry, nameof(fieldReader.ReadByteFieldAsync), refData.Length);

                // SSbyte
                await TestInvalidConversionAsync<sbyte>(fieldReader, entry, nameof(fieldReader.ReadSByteFieldAsync), refData.Length);

                // Short
                await TestInvalidConversionAsync<ushort>(fieldReader, entry, nameof(fieldReader.ReadShortFieldAsync), refData.Length);

                // SShort
                await TestInvalidConversionAsync<short>(fieldReader, entry, nameof(fieldReader.ReadSShortFieldAsync), refData.Length);

                // Long
                await TestValidConversionAsync(fieldReader, entry, nameof(fieldReader.ReadLongFieldAsync), refData);

                // SLong
                await TestValidConversionAsync(fieldReader, entry, nameof(fieldReader.ReadSLongFieldAsync), Array.ConvertAll(refData, v => (int)v));

                // Float
                await TestInvalidConversionAsync<float>(fieldReader, entry, nameof(fieldReader.ReadFloatFieldAsync), refData.Length);

                // Double
                await TestInvalidConversionAsync<double>(fieldReader, entry, nameof(fieldReader.ReadDoubleFieldAsync), refData.Length);

                // Rational
                await TestValidConversionAsync(fieldReader, entry, nameof(fieldReader.ReadRationalFieldAsync), Array.ConvertAll(refData, v => new TiffRational(v, 1)));

                // SRational
                await TestValidConversionAsync(fieldReader, entry, nameof(fieldReader.ReadSRationalFieldAsync), Array.ConvertAll(refData, v => new TiffSRational((int)v, 1)));
            }
        }


        [Theory]
        [MemberData(nameof(GetTestIterations))]
        public async Task TestSLongCompatibility(int length, bool bigTiff)
        {
            int[] refData = new int[length];
            new Random(42).NextBytes(MemoryMarshal.AsBytes(refData.AsSpan()));

            using Stream stream = await GenerateTiffAsync(bigTiff, async ifd =>
            {
                await ifd.WriteTagAsync((TiffTag)0x1234, TiffValueCollection.UnsafeWrap(refData));
            });

            await using (TiffFileReader reader = await TiffFileReader.OpenAsync(stream, leaveOpen: true))
            {
                TiffImageFileDirectory ifd = await reader.ReadImageFileDirectoryAsync();
                TiffFieldReader fieldReader = await reader.CreateFieldReaderAsync();
                TiffImageFileDirectoryEntry entry = ifd.FindEntry((TiffTag)0x1234);
                Assert.Equal((TiffTag)0x1234, entry.Tag);

                // Byte
                await TestInvalidConversionAsync<byte>(fieldReader, entry, nameof(fieldReader.ReadByteFieldAsync), refData.Length);

                // SSbyte
                await TestInvalidConversionAsync<sbyte>(fieldReader, entry, nameof(fieldReader.ReadSByteFieldAsync), refData.Length);

                // Short
                await TestInvalidConversionAsync<ushort>(fieldReader, entry, nameof(fieldReader.ReadShortFieldAsync), refData.Length);

                // SShort
                await TestInvalidConversionAsync<short>(fieldReader, entry, nameof(fieldReader.ReadSShortFieldAsync), refData.Length);

                // Long
                await TestValidConversionAsync(fieldReader, entry, nameof(fieldReader.ReadLongFieldAsync), Array.ConvertAll(refData, v => (uint)v));

                // SLong
                await TestValidConversionAsync(fieldReader, entry, nameof(fieldReader.ReadSLongFieldAsync), refData);

                // Float
                await TestInvalidConversionAsync<float>(fieldReader, entry, nameof(fieldReader.ReadFloatFieldAsync), refData.Length);

                // Double
                await TestInvalidConversionAsync<double>(fieldReader, entry, nameof(fieldReader.ReadDoubleFieldAsync), refData.Length);

                // Rational
                await TestInvalidConversionAsync<TiffRational>(fieldReader, entry, nameof(fieldReader.ReadRationalFieldAsync), refData.Length);

                // SRational
                await TestValidConversionAsync(fieldReader, entry, nameof(fieldReader.ReadSRationalFieldAsync), Array.ConvertAll(refData, v => new TiffSRational(v, 1)));
            }
        }

        [Theory]
        [MemberData(nameof(GetTestIterations))]
        public async Task TestFloatCompatibility(int length, bool bigTiff)
        {
            float[] refData = new float[length];
            var rand = new Random();
            for (int i = 0; i < refData.Length; i++)
            {
                refData[i] = (float)rand.NextDouble();
            }

            using Stream stream = await GenerateTiffAsync(bigTiff, async ifd =>
            {
                await ifd.WriteTagAsync((TiffTag)0x1234, TiffValueCollection.UnsafeWrap(refData));
            });

            await using (TiffFileReader reader = await TiffFileReader.OpenAsync(stream, leaveOpen: true))
            {
                TiffImageFileDirectory ifd = await reader.ReadImageFileDirectoryAsync();
                TiffFieldReader fieldReader = await reader.CreateFieldReaderAsync();
                TiffImageFileDirectoryEntry entry = ifd.FindEntry((TiffTag)0x1234);
                Assert.Equal((TiffTag)0x1234, entry.Tag);

                // Byte
                await TestInvalidConversionAsync<byte>(fieldReader, entry, nameof(fieldReader.ReadByteFieldAsync), refData.Length);

                // SSbyte
                await TestInvalidConversionAsync<sbyte>(fieldReader, entry, nameof(fieldReader.ReadSByteFieldAsync), refData.Length);

                // Short
                await TestInvalidConversionAsync<ushort>(fieldReader, entry, nameof(fieldReader.ReadShortFieldAsync), refData.Length);

                // SShort
                await TestInvalidConversionAsync<short>(fieldReader, entry, nameof(fieldReader.ReadSShortFieldAsync), refData.Length);

                // Long
                await TestInvalidConversionAsync<uint>(fieldReader, entry, nameof(fieldReader.ReadLongFieldAsync), refData.Length);

                // SLong
                await TestInvalidConversionAsync<int>(fieldReader, entry, nameof(fieldReader.ReadSLongFieldAsync), refData.Length);

                // Float
                await TestValidConversionAsync(fieldReader, entry, nameof(fieldReader.ReadFloatFieldAsync), refData);

                // Double
                await TestValidConversionAsync(fieldReader, entry, nameof(fieldReader.ReadDoubleFieldAsync), Array.ConvertAll(refData, v => (double)v));

                // Rational
                await TestInvalidConversionAsync<TiffRational>(fieldReader, entry, nameof(fieldReader.ReadRationalFieldAsync), refData.Length);

                // SRational
                await TestInvalidConversionAsync<TiffSRational>(fieldReader, entry, nameof(fieldReader.ReadSRationalFieldAsync), refData.Length);
            }
        }


        [Theory]
        [MemberData(nameof(GetTestIterations))]
        public async Task TestDoubleCompatibility(int length, bool bigTiff)
        {
            double[] refData = new double[length];
            var rand = new Random();
            for (int i = 0; i < refData.Length; i++)
            {
                refData[i] = rand.NextDouble();
            }

            using Stream stream = await GenerateTiffAsync(bigTiff, async ifd =>
            {
                await ifd.WriteTagAsync((TiffTag)0x1234, TiffValueCollection.UnsafeWrap(refData));
            });

            await using (TiffFileReader reader = await TiffFileReader.OpenAsync(stream, leaveOpen: true))
            {
                TiffImageFileDirectory ifd = await reader.ReadImageFileDirectoryAsync();
                TiffFieldReader fieldReader = await reader.CreateFieldReaderAsync();
                TiffImageFileDirectoryEntry entry = ifd.FindEntry((TiffTag)0x1234);
                Assert.Equal((TiffTag)0x1234, entry.Tag);

                // Byte
                await TestInvalidConversionAsync<byte>(fieldReader, entry, nameof(fieldReader.ReadByteFieldAsync), refData.Length);

                // SSbyte
                await TestInvalidConversionAsync<sbyte>(fieldReader, entry, nameof(fieldReader.ReadSByteFieldAsync), refData.Length);

                // Short
                await TestInvalidConversionAsync<ushort>(fieldReader, entry, nameof(fieldReader.ReadShortFieldAsync), refData.Length);

                // SShort
                await TestInvalidConversionAsync<short>(fieldReader, entry, nameof(fieldReader.ReadSShortFieldAsync), refData.Length);

                // Long
                await TestInvalidConversionAsync<uint>(fieldReader, entry, nameof(fieldReader.ReadLongFieldAsync), refData.Length);

                // SLong
                await TestInvalidConversionAsync<int>(fieldReader, entry, nameof(fieldReader.ReadSLongFieldAsync), refData.Length);

                // Float
                await TestInvalidConversionAsync<float>(fieldReader, entry, nameof(fieldReader.ReadFloatFieldAsync), refData.Length);

                // Double
                await TestValidConversionAsync(fieldReader, entry, nameof(fieldReader.ReadDoubleFieldAsync), refData);

                // Rational
                await TestInvalidConversionAsync<TiffRational>(fieldReader, entry, nameof(fieldReader.ReadRationalFieldAsync), refData.Length);

                // SRational
                await TestInvalidConversionAsync<TiffSRational>(fieldReader, entry, nameof(fieldReader.ReadSRationalFieldAsync), refData.Length);
            }
        }

        [Theory]
        [MemberData(nameof(GetTestIterations))]
        public async Task TestRationalCompatibility(int length, bool bigTiff)
        {
            TiffRational[] refData = new TiffRational[length];
            new Random(42).NextBytes(MemoryMarshal.AsBytes(refData.AsSpan()));

            using Stream stream = await GenerateTiffAsync(bigTiff, async ifd =>
            {
                await ifd.WriteTagAsync((TiffTag)0x1234, TiffValueCollection.UnsafeWrap(refData));
            });

            await using (TiffFileReader reader = await TiffFileReader.OpenAsync(stream, leaveOpen: true))
            {
                TiffImageFileDirectory ifd = await reader.ReadImageFileDirectoryAsync();
                TiffFieldReader fieldReader = await reader.CreateFieldReaderAsync();
                TiffImageFileDirectoryEntry entry = ifd.FindEntry((TiffTag)0x1234);
                Assert.Equal((TiffTag)0x1234, entry.Tag);

                // Byte
                await TestInvalidConversionAsync<byte>(fieldReader, entry, nameof(fieldReader.ReadByteFieldAsync), refData.Length);

                // SSbyte
                await TestInvalidConversionAsync<sbyte>(fieldReader, entry, nameof(fieldReader.ReadSByteFieldAsync), refData.Length);

                // Short
                await TestInvalidConversionAsync<ushort>(fieldReader, entry, nameof(fieldReader.ReadShortFieldAsync), refData.Length);

                // SShort
                await TestInvalidConversionAsync<short>(fieldReader, entry, nameof(fieldReader.ReadSShortFieldAsync), refData.Length);

                // Long
                await TestInvalidConversionAsync<uint>(fieldReader, entry, nameof(fieldReader.ReadLongFieldAsync), refData.Length);

                // SLong
                await TestInvalidConversionAsync<int>(fieldReader, entry, nameof(fieldReader.ReadSLongFieldAsync), refData.Length);

                // Float
                await TestInvalidConversionAsync<float>(fieldReader, entry, nameof(fieldReader.ReadFloatFieldAsync), refData.Length);

                // Double
                await TestInvalidConversionAsync<double>(fieldReader, entry, nameof(fieldReader.ReadDoubleFieldAsync), refData.Length);

                // Rational
                await TestValidConversionAsync(fieldReader, entry, nameof(fieldReader.ReadRationalFieldAsync), refData);

                // SRational
                await TestInvalidConversionAsync<TiffSRational>(fieldReader, entry, nameof(fieldReader.ReadSRationalFieldAsync), refData.Length);
            }
        }

        [Theory]
        [MemberData(nameof(GetTestIterations))]
        public async Task TestSRationalCompatibility(int length, bool bigTiff)
        {
            TiffSRational[] refData = new TiffSRational[length];
            new Random(42).NextBytes(MemoryMarshal.AsBytes(refData.AsSpan()));

            using Stream stream = await GenerateTiffAsync(bigTiff, async ifd =>
            {
                await ifd.WriteTagAsync((TiffTag)0x1234, TiffValueCollection.UnsafeWrap(refData));
            });

            await using (TiffFileReader reader = await TiffFileReader.OpenAsync(stream, leaveOpen: true))
            {
                TiffImageFileDirectory ifd = await reader.ReadImageFileDirectoryAsync();
                TiffFieldReader fieldReader = await reader.CreateFieldReaderAsync();
                TiffImageFileDirectoryEntry entry = ifd.FindEntry((TiffTag)0x1234);
                Assert.Equal((TiffTag)0x1234, entry.Tag);

                // Byte
                await TestInvalidConversionAsync<byte>(fieldReader, entry, nameof(fieldReader.ReadByteFieldAsync), refData.Length);

                // SSbyte
                await TestInvalidConversionAsync<sbyte>(fieldReader, entry, nameof(fieldReader.ReadSByteFieldAsync), refData.Length);

                // Short
                await TestInvalidConversionAsync<ushort>(fieldReader, entry, nameof(fieldReader.ReadShortFieldAsync), refData.Length);

                // SShort
                await TestInvalidConversionAsync<short>(fieldReader, entry, nameof(fieldReader.ReadSShortFieldAsync), refData.Length);

                // Long
                await TestInvalidConversionAsync<uint>(fieldReader, entry, nameof(fieldReader.ReadLongFieldAsync), refData.Length);

                // SLong
                await TestInvalidConversionAsync<int>(fieldReader, entry, nameof(fieldReader.ReadSLongFieldAsync), refData.Length);

                // Float
                await TestInvalidConversionAsync<float>(fieldReader, entry, nameof(fieldReader.ReadFloatFieldAsync), refData.Length);

                // Double
                await TestInvalidConversionAsync<double>(fieldReader, entry, nameof(fieldReader.ReadDoubleFieldAsync), refData.Length);

                // Rational
                await TestInvalidConversionAsync<TiffRational>(fieldReader, entry, nameof(fieldReader.ReadRationalFieldAsync), refData.Length);

                // SRational
                await TestValidConversionAsync(fieldReader, entry, nameof(fieldReader.ReadSRationalFieldAsync), refData);

            }
        }

    }
}
