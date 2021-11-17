using System;
using Xunit;

namespace TiffLibrary.Tests
{
    public class PrimitiveTests
    {
        [Fact]
        public void TestPoint()
        {
            var defaultPoint = new TiffPoint();
            Assert.Equal(0, defaultPoint.X);
            Assert.Equal(0, defaultPoint.Y);

            var point1 = new TiffPoint(1, 2);
            Assert.Equal(1, point1.X);
            Assert.Equal(2, point1.Y);
            Assert.False(point1.Equals(defaultPoint));
            Assert.False(defaultPoint.Equals(point1));
            Assert.False(point1 == defaultPoint);
            Assert.False(defaultPoint == point1);
            Assert.True(point1 != defaultPoint);
            Assert.True(defaultPoint != point1);
            Assert.False(point1.GetHashCode() == defaultPoint.GetHashCode());

            var point2 = new TiffPoint(1, 2);
            Assert.True(point1.Equals(point2));
            Assert.True(point2.Equals(point1));
            Assert.True(point1 == point2);
            Assert.True(point2 == point1);
            Assert.False(point1 != point2);
            Assert.False(point2 != point1);
            Assert.True(point1.GetHashCode() == point2.GetHashCode());

#if NET6_0_OR_GREATER
            Span<char> buffer = stackalloc char[64];
            int charsWritten;

            Assert.True(defaultPoint.TryFormat(buffer.Slice(0, 5), out charsWritten, default, default));
            Assert.Equal(5, charsWritten);
            Assert.Equal("(0,0)", buffer.Slice(0, 5).ToString());

            Assert.True(point1.TryFormat(buffer.Slice(0, 5), out charsWritten, default, default));
            Assert.Equal(5, charsWritten);
            Assert.Equal("(1,2)", buffer.Slice(0, 5).ToString());

            Assert.True(new TiffPoint(123, 4567).TryFormat(buffer.Slice(0, 10), out charsWritten, default, default));
            Assert.Equal(10, charsWritten);
            Assert.Equal("(123,4567)", buffer.Slice(0, 10).ToString());
#endif
        }

        [Fact]
        public void TestSize()
        {
            var defaultSize = new TiffSize();
            Assert.Equal(0, defaultSize.Width);
            Assert.Equal(0, defaultSize.Height);

            var size1 = new TiffSize(1, 2);
            Assert.Equal(1, size1.Width);
            Assert.Equal(2, size1.Height);
            Assert.False(size1.Equals(defaultSize));
            Assert.False(defaultSize.Equals(size1));
            Assert.False(size1 == defaultSize);
            Assert.False(defaultSize == size1);
            Assert.True(size1 != defaultSize);
            Assert.True(defaultSize != size1);
            Assert.False(size1.GetHashCode() == defaultSize.GetHashCode());

            var size2 = new TiffSize(1, 2);
            Assert.True(size1.Equals(size2));
            Assert.True(size2.Equals(size1));
            Assert.True(size1 == size2);
            Assert.True(size2 == size1);
            Assert.False(size1 != size2);
            Assert.False(size2 != size1);
            Assert.True(size1.GetHashCode() == size2.GetHashCode());

#if NET6_0_OR_GREATER
            Span<char> buffer = stackalloc char[64];
            int charsWritten;

            Assert.True(defaultSize.TryFormat(buffer.Slice(0, 5), out charsWritten, default, default));
            Assert.Equal(5, charsWritten);
            Assert.Equal("(0,0)", buffer.Slice(0, 5).ToString());

            Assert.True(size1.TryFormat(buffer.Slice(0, 5), out charsWritten, default, default));
            Assert.Equal(5, charsWritten);
            Assert.Equal("(1,2)", buffer.Slice(0, 5).ToString());

            Assert.True(new TiffSize(123, 4567).TryFormat(buffer.Slice(0, 10), out charsWritten, default, default));
            Assert.Equal(10, charsWritten);
            Assert.Equal("(123,4567)", buffer.Slice(0, 10).ToString());
#endif
        }

        [Fact]
        public void TestRational()
        {
            var defaultRational = new TiffRational();
            Assert.Equal((uint)0, defaultRational.Numerator);
            Assert.Equal((uint)0, defaultRational.Denominator);
            Assert.Equal(0 / 0f, defaultRational.ToSingle());
            Assert.Equal(0 / 0d, defaultRational.ToDouble());

            var rational1 = new TiffRational(1, 2);
            Assert.Equal((uint)1, rational1.Numerator);
            Assert.Equal((uint)2, rational1.Denominator);
            Assert.False(rational1.Equals(defaultRational));
            Assert.False(defaultRational.Equals(rational1));
            Assert.False(rational1 == defaultRational);
            Assert.False(defaultRational == rational1);
            Assert.True(rational1 != defaultRational);
            Assert.True(defaultRational != rational1);
            Assert.False(rational1.GetHashCode() == defaultRational.GetHashCode());
            Assert.Equal(1 / 2f, rational1.ToSingle());
            Assert.Equal(1 / 2d, rational1.ToDouble());

            var rational2 = new TiffRational(1, 2);
            Assert.True(rational1.Equals(rational2));
            Assert.True(rational2.Equals(rational1));
            Assert.True(rational1 == rational2);
            Assert.True(rational2 == rational1);
            Assert.False(rational1 != rational2);
            Assert.False(rational2 != rational1);
            Assert.True(rational1.GetHashCode() == rational2.GetHashCode());

#if NET6_0_OR_GREATER
            Span<char> buffer = stackalloc char[64];
            int charsWritten;

            Assert.True(defaultRational.TryFormat(buffer.Slice(0, 3), out charsWritten, default, default));
            Assert.Equal(3, charsWritten);
            Assert.Equal("0/0", buffer.Slice(0, 3).ToString());

            Assert.True(rational1.TryFormat(buffer.Slice(0, 3), out charsWritten, default, default));
            Assert.Equal(3, charsWritten);
            Assert.Equal("1/2", buffer.Slice(0, 3).ToString());

            Assert.True(new TiffRational(123, 4567).TryFormat(buffer.Slice(0, 8), out charsWritten, default, default));
            Assert.Equal(8, charsWritten);
            Assert.Equal("123/4567", buffer.Slice(0, 8).ToString());
#endif
        }

        [Fact]
        public void TestSRational()
        {
            var defaultSRational = new TiffSRational();
            Assert.Equal(0, defaultSRational.Numerator);
            Assert.Equal(0, defaultSRational.Denominator);
            Assert.Equal(0 / 0f, defaultSRational.ToSingle());
            Assert.Equal(0 / 0d, defaultSRational.ToDouble());

            var srational1 = new TiffSRational(1, -2);
            Assert.Equal(1, srational1.Numerator);
            Assert.Equal(-2, srational1.Denominator);
            Assert.False(srational1.Equals(defaultSRational));
            Assert.False(defaultSRational.Equals(srational1));
            Assert.False(srational1 == defaultSRational);
            Assert.False(defaultSRational == srational1);
            Assert.True(srational1 != defaultSRational);
            Assert.True(defaultSRational != srational1);
            Assert.False(srational1.GetHashCode() == defaultSRational.GetHashCode());
            Assert.Equal(-1 / 2f, srational1.ToSingle());
            Assert.Equal(-1 / 2d, srational1.ToDouble());

            var srational2 = new TiffSRational(1, -2);
            Assert.True(srational1.Equals(srational2));
            Assert.True(srational2.Equals(srational1));
            Assert.True(srational1 == srational2);
            Assert.True(srational2 == srational1);
            Assert.False(srational1 != srational2);
            Assert.False(srational2 != srational1);
            Assert.True(srational1.GetHashCode() == srational2.GetHashCode());

#if NET6_0_OR_GREATER
            Span<char> buffer = stackalloc char[64];
            int charsWritten;

            Assert.True(defaultSRational.TryFormat(buffer.Slice(0, 3), out charsWritten, default, default));
            Assert.Equal(3, charsWritten);
            Assert.Equal("0/0", buffer.Slice(0, 3).ToString());

            Assert.True(srational1.TryFormat(buffer.Slice(0, 4), out charsWritten, default, default));
            Assert.Equal(4, charsWritten);
            Assert.Equal("1/-2", buffer.Slice(0, 4).ToString());

            Assert.True(new TiffSRational(123, 4567).TryFormat(buffer.Slice(0, 8), out charsWritten, default, default));
            Assert.Equal(8, charsWritten);
            Assert.Equal("123/4567", buffer.Slice(0, 8).ToString());
#endif
        }

        [Fact]
        public void TestPageNumber()
        {
            var defaultPageNumber = new TiffPageNumber();
            Assert.Equal(0, defaultPageNumber.PageNumber);
            Assert.Equal(0, defaultPageNumber.TotalPages);

            var pageNumber1 = new TiffPageNumber(1, 2);
            Assert.Equal(1, pageNumber1.PageNumber);
            Assert.Equal(2, pageNumber1.TotalPages);
            Assert.False(pageNumber1.Equals(defaultPageNumber));
            Assert.False(defaultPageNumber.Equals(pageNumber1));
            Assert.False(pageNumber1 == defaultPageNumber);
            Assert.False(defaultPageNumber == pageNumber1);
            Assert.True(pageNumber1 != defaultPageNumber);
            Assert.True(defaultPageNumber != pageNumber1);
            Assert.False(pageNumber1.GetHashCode() == defaultPageNumber.GetHashCode());

            var pageNumber2 = new TiffPageNumber(1, 2);
            Assert.True(pageNumber1.Equals(pageNumber2));
            Assert.True(pageNumber2.Equals(pageNumber1));
            Assert.True(pageNumber1 == pageNumber2);
            Assert.True(pageNumber2 == pageNumber1);
            Assert.False(pageNumber1 != pageNumber2);
            Assert.False(pageNumber2 != pageNumber1);
            Assert.True(pageNumber1.GetHashCode() == pageNumber2.GetHashCode());

#if NET6_0_OR_GREATER
            Span<char> buffer = stackalloc char[64];
            int charsWritten;

            Assert.True(defaultPageNumber.TryFormat(buffer.Slice(0, 5), out charsWritten, default, default));
            Assert.Equal(5, charsWritten);
            Assert.Equal("(0/0)", buffer.Slice(0, 5).ToString());

            Assert.True(pageNumber1.TryFormat(buffer.Slice(0, 5), out charsWritten, default, default));
            Assert.Equal(5, charsWritten);
            Assert.Equal("(1/2)", buffer.Slice(0, 5).ToString());

            Assert.True(new TiffPageNumber(123, 4567).TryFormat(buffer.Slice(0, 10), out charsWritten, default, default));
            Assert.Equal(10, charsWritten);
            Assert.Equal("(123/4567)", buffer.Slice(0, 10).ToString());
#endif
        }

    }
}
