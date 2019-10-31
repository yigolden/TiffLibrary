using System;
using Xunit;

namespace TiffLibrary.Tests
{
    public class TiffValueCollectionTest
    {

        [Fact]
        public void TestEmpty()
        {
            TiffValueCollection<int> emptyVal = TiffValueCollection.Empty<int>();
            Assert.Empty(emptyVal);
            Assert.True(emptyVal.IsEmpty);
            Assert.True(emptyVal.Count == 0);

            var emptyVal2 = new TiffValueCollection<int>(Array.Empty<int>());
            Assert.Empty(emptyVal2);
            Assert.True(emptyVal2.IsEmpty);
            Assert.True(emptyVal2.Count == 0);

            var emptyVal3 = new TiffValueCollection<int>(Array.Empty<int>().AsSpan());
            Assert.Empty(emptyVal3);
            Assert.True(emptyVal3.IsEmpty);
            Assert.True(emptyVal3.Count == 0);

            var emptyVal4 = new TiffValueCollection<int>(null);
            Assert.Empty(emptyVal4);
            Assert.True(emptyVal4.IsEmpty);
            Assert.True(emptyVal4.Count == 0);

            TiffValueCollection<string> emptyRef = TiffValueCollection.Empty<string>();
            Assert.Empty(emptyVal);
            Assert.True(emptyVal.IsEmpty);
            Assert.True(emptyVal.Count == 0);

            int count = 0;
            foreach (int v in emptyVal)
            {
                count++;
            }
            Assert.Equal(0, count);

            count = 0;
            foreach (int v in emptyVal2)
            {
                count++;
            }
            Assert.Equal(0, count);

            count = 0;
            foreach (int v in emptyVal3)
            {
                count++;
            }
            Assert.Equal(0, count);

            foreach (string v in emptyRef)
            {
                count++;
            }
            Assert.Equal(0, count);
        }

        [Fact]
        public void TestSingle()
        {
            var singleVal = new TiffValueCollection<int>(42);
            Assert.Single(singleVal);
            Assert.False(singleVal.IsEmpty);
            Assert.True(singleVal.Count == 1);
            Assert.Equal(42, singleVal[0]);
            Assert.Throws<IndexOutOfRangeException>(() => singleVal[-1]);
            Assert.Throws<IndexOutOfRangeException>(() => singleVal[2]);

            var singleValZero = new TiffValueCollection<int>(0);
            Assert.Single(singleVal);
            Assert.False(singleVal.IsEmpty);
            Assert.True(singleVal.Count == 1);
            Assert.Equal(0, singleValZero[0]);
            Assert.Throws<IndexOutOfRangeException>(() => singleValZero[-1]);
            Assert.Throws<IndexOutOfRangeException>(() => singleValZero[2]);

            var singleRef = new TiffValueCollection<string>("hello world.");
            Assert.Single(singleRef);
            Assert.False(singleRef.IsEmpty);
            Assert.True(singleRef.Count == 1);
            Assert.Equal("hello world.", singleRef[0]);
            Assert.Throws<IndexOutOfRangeException>(() => singleValZero[-1]);
            Assert.Throws<IndexOutOfRangeException>(() => singleValZero[2]);

            var singleRefNull = new TiffValueCollection<string>((string)null);
            Assert.Single(singleRefNull);
            Assert.False(singleRefNull.IsEmpty);
            Assert.True(singleRefNull.Count == 1);
            Assert.Null(singleRefNull[0]);
            Assert.Throws<IndexOutOfRangeException>(() => singleRefNull[-1]);
            Assert.Throws<IndexOutOfRangeException>(() => singleRefNull[2]);
        }

        [Fact]
        public void TestMultiValue()
        {
            int[] values = new int[] { 42, 88, 100 };

            var multiVal = TiffValueCollection.UnsafeWrap(values);
            Assert.Equal(3, multiVal.Count);
            Assert.False(multiVal.IsEmpty);
            Assert.Equal(42, multiVal[0]);
            Assert.Equal(88, multiVal[1]);
            Assert.Equal(100, multiVal[2]);
            Assert.Throws<IndexOutOfRangeException>(() => multiVal[-1]);
            Assert.Throws<IndexOutOfRangeException>(() => multiVal[3]);

            int count = 0;
            foreach (int v in multiVal)
            {
                switch (count)
                {
                    case 0:
                        Assert.Equal(42, v);
                        break;
                    case 1:
                        Assert.Equal(88, v);
                        break;
                    case 2:
                        Assert.Equal(100, v);
                        break;
                }

                count++;
            }
            Assert.Equal(3, count);

            // TiffValueCollection contains the reference to the original array.
            values[1] = 128;
            Assert.Equal(128, multiVal[1]);

            // When using the ReadOnlySpan<T> constructor, TiffValueCollection creates an array internally and copys all the values from the span to the array.
            var multiVal2 = new TiffValueCollection<int>(values.AsSpan());
            Assert.Equal(3, multiVal2.Count);
            Assert.False(multiVal2.IsEmpty);
            Assert.Equal(42, multiVal2[0]);
            Assert.Equal(128, multiVal2[1]);
            Assert.Equal(100, multiVal2[2]);
            Assert.Throws<IndexOutOfRangeException>(() => multiVal2[-1]);
            Assert.Throws<IndexOutOfRangeException>(() => multiVal2[3]);

            values[1] = 256;
            Assert.Equal(128, multiVal2[1]);

            count = 0;
            foreach (int v in multiVal2)
            {
                switch (count)
                {
                    case 0:
                        Assert.Equal(42, v);
                        break;
                    case 1:
                        Assert.Equal(128, v);
                        break;
                    case 2:
                        Assert.Equal(100, v);
                        break;
                }

                count++;
            }
            Assert.Equal(3, count);

            string[] strings = new string[] { "hello", "world", "." };
            var multiRef = new TiffValueCollection<string>(strings);
            Assert.Equal(3, multiRef.Count);
            Assert.False(multiRef.IsEmpty);
            Assert.Equal("hello", multiRef[0]);
            Assert.Equal("world", multiRef[1]);
            Assert.Equal(".", multiRef[2]);
            Assert.Throws<IndexOutOfRangeException>(() => multiRef[-1]);
            Assert.Throws<IndexOutOfRangeException>(() => multiRef[3]);

            count = 0;
            foreach (string v in multiRef)
            {
                switch (count)
                {
                    case 0:
                        Assert.Equal("hello", v);
                        break;
                    case 1:
                        Assert.Equal("world", v);
                        break;
                    case 2:
                        Assert.Equal(".", v);
                        break;
                }

                count++;
            }
            Assert.Equal(3, count);
        }
    }
}
