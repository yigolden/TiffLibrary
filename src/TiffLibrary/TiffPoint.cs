using System;

namespace TiffLibrary
{
    /// <summary>
    /// Represents an ordered pair of integer x- and y-coordinates that defines a point in a two-dimensional plane.
    /// </summary>
    public readonly struct TiffPoint : IEquatable<TiffPoint>, IFormattable
#if !NO_SPAN_FORMATTABLE
        , ISpanFormattable
#endif
    {
        /// <summary>
        /// Initializes a new instance with the specified coordinates.
        /// </summary>
        /// <param name="x">The horizontal position of the point.</param>
        /// <param name="y">The vertical position of the point.</param>
        public TiffPoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Gets the horizontal position of the point.
        /// </summary>
        public int X { get; }

        /// <summary>
        /// Gets the vertical position of the point.
        /// </summary>
        public int Y { get; }

        /// <inheritdoc />
        public bool Equals(TiffPoint other)
        {
            return X == other.X && Y == other.Y;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is TiffPoint point && Equals(point);
        }

        /// <summary>
        /// Compares two objects for equality.
        /// </summary>
        /// <param name="left">The object on the left side of the operand.</param>
        /// <param name="right">The object on the right side of the operand.</param>
        /// <returns>True if the <paramref name="left"/> parameter is equal to the <paramref name="right"/> parameter; otherwise, false.</returns>
        public static bool operator ==(TiffPoint left, TiffPoint right) => left.Equals(right);

        /// <summary>
        /// Compares two objects for equality.
        /// </summary>
        /// <param name="left">The object on the left side of the operand.</param>
        /// <param name="right">The object on the right side of the operand.</param>
        /// <returns>True if the <paramref name="left"/> parameter is not equal to the <paramref name="right"/> parameter; otherwise, false.</returns>
        public static bool operator !=(TiffPoint left, TiffPoint right) => !left.Equals(right);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({X},{Y})";
        }

        /// <inheritdoc />
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return $"({X.ToString(format, formatProvider)},{Y.ToString(format, formatProvider)})";
        }

#if !NO_SPAN_FORMATTABLE
        /// <inheritdoc />
        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            if (destination.Length < 5)
            {
                goto TryFormatError;
            }

            destination[0] = '(';
            Span<char> remaining = destination.Slice(1);

            if (!X.TryFormat(remaining, out int c, format, provider))
            {
                goto TryFormatError;
            }
            remaining = remaining.Slice(c);

            if (remaining.Length < 3)
            {
                goto TryFormatError;
            }

            remaining[0] = ',';
            remaining = remaining.Slice(1);

            if (!Y.TryFormat(remaining, out c, format, provider))
            {
                goto TryFormatError;
            }
            remaining = remaining.Slice(c);

            if (remaining.IsEmpty)
            {
                goto TryFormatError;
            }
            remaining[0] = ')';
            remaining = remaining.Slice(1);

            charsWritten = destination.Length - remaining.Length;
            return true;

        TryFormatError:
            charsWritten = 0;
            return false;
        }
#endif
    }
}
