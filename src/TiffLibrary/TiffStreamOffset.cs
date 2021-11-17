using System;

namespace TiffLibrary
{
    /// <summary>
    /// A value representing the offset from the begining of the TIFF file stream.
    /// </summary>
    public readonly struct TiffStreamOffset : IEquatable<TiffStreamOffset>, IComparable<TiffStreamOffset>, IFormattable
#if !NO_SPAN_FORMATTABLE
        , ISpanFormattable
#endif
    {
        /// <summary>
        /// Gets the offset as <see cref="long"/>.
        /// </summary>
        public long Offset { get; }

        /// <summary>
        /// Initial <see cref="TiffStreamOffset"/> with the specified offset.
        /// </summary>
        /// <param name="offset">The offset from the begining of the TIFF file stream.</param>
        public TiffStreamOffset(long offset)
        {
            Offset = offset;
        }

        /// <summary>
        /// Initial <see cref="TiffStreamOffset"/> with the specified offset.
        /// </summary>
        /// <param name="offset">The offset from the begining of the TIFF file stream.</param>
        public TiffStreamOffset(int offset)
        {
            Offset = offset;
        }

        /// <summary>
        /// Gets whether the offset is zero or not set.
        /// </summary>
        public bool IsZero => Offset == 0;

        /// <inheritdoc />
        public bool Equals(TiffStreamOffset other)
        {
            return other.Offset == Offset;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is TiffStreamOffset offset)
            {
                return offset.Offset == Offset;
            }
            return false;
        }

        /// <summary>
        /// Compares this instance to another offset and returns an indication of their relative values.
        /// </summary>
        /// <param name="other">An offset to compare.</param>
        /// <returns>A signed number indicating the relative values of this instance and <paramref name="other"/>.</returns>
        public int CompareTo(TiffStreamOffset other)
        {
            if (Offset > other.Offset)
            {
                return 1;
            }
            else if (Offset < other.Offset)
            {
                return -1;
            }
            return 0;
        }

        /// <summary>
        /// Compares two objects for equality.
        /// </summary>
        /// <param name="left">The object on the left side of the operand.</param>
        /// <param name="right">The object on the right side of the operand.</param>
        /// <returns>True if the <paramref name="left"/> parameter is equal to the <paramref name="right"/> parameter; otherwise, false.</returns>
        public static bool operator ==(TiffStreamOffset left, TiffStreamOffset right) => left.Offset == right.Offset;

        /// <summary>
        /// Compares two objects for equality.
        /// </summary>
        /// <param name="left">The object on the left side of the operand.</param>
        /// <param name="right">The object on the right side of the operand.</param>
        /// <returns>True if the <paramref name="left"/> parameter is not equal to the <paramref name="right"/> parameter; otherwise, false.</returns>
        public static bool operator !=(TiffStreamOffset left, TiffStreamOffset right) => left.Offset != right.Offset;

        /// <summary>
        /// Compares two objects.
        /// </summary>
        /// <param name="left">The object on the left side of the operand.</param>
        /// <param name="right">The object on the right side of the operand.</param>
        /// <returns>True if the <paramref name="left"/> parameter is greater than the <paramref name="right"/> parameter; otherwise, false.</returns>
        public static bool operator >(TiffStreamOffset left, TiffStreamOffset right) => left.Offset > right.Offset;

        /// <summary>
        /// Compares two objects.
        /// </summary>
        /// <param name="left">The object on the left side of the operand.</param>
        /// <param name="right">The object on the right side of the operand.</param>
        /// <returns>True if the <paramref name="left"/> parameter is less than the <paramref name="right"/> parameter; otherwise, false.</returns>
        public static bool operator <(TiffStreamOffset left, TiffStreamOffset right) => left.Offset < right.Offset;

        /// <summary>
        /// Compares two objects.
        /// </summary>
        /// <param name="left">The object on the left side of the operand.</param>
        /// <param name="right">The object on the right side of the operand.</param>
        /// <returns>True if the <paramref name="left"/> parameter is not less than the <paramref name="right"/> parameter; otherwise, false.</returns>
        public static bool operator >=(TiffStreamOffset left, TiffStreamOffset right) => left.Offset >= right.Offset;

        /// <summary>
        /// Compares two objects.
        /// </summary>
        /// <param name="left">The object on the left side of the operand.</param>
        /// <param name="right">The object on the right side of the operand.</param>
        /// <returns>True if the <paramref name="left"/> parameter is not greater than the <paramref name="right"/> parameter; otherwise, false.</returns>
        public static bool operator <=(TiffStreamOffset left, TiffStreamOffset right) => left.Offset <= right.Offset;

        /// <inheritdoc />
        public override int GetHashCode() => Offset.GetHashCode();


        /// <summary>
        /// Converts the offset into <see cref="long"/>.
        /// </summary>
        /// <param name="offset">The offset.</param>
        public static implicit operator long(TiffStreamOffset offset) => offset.Offset;

        /// <summary>
        /// Converts a <see cref="long"/> into <see cref="TiffStreamOffset"/>.
        /// </summary>
        /// <param name="offset">The offset from the begining of the TIFF file stream.</param>
        public static implicit operator TiffStreamOffset(long offset) => new TiffStreamOffset(offset);

        /// <summary>
        /// Converts the offset into <see cref="long"/>.
        /// </summary>
        /// <returns>The integer representing the offset.</returns>
        public long ToInt64() => Offset;

        /// <summary>
        /// Converts a <see cref="long"/> into <see cref="TiffStreamOffset"/>.
        /// </summary>
        /// <param name="offset">The offset from the begining of the TIFF file stream.</param>
        /// <returns>The created <see cref="TiffStreamOffset"/>.</returns>
        public static TiffStreamOffset FromInt64(long offset) => new TiffStreamOffset(offset);

        /// <inheritdoc />
        public override string ToString() => $"{Offset}";

        /// <inheritdoc />
        public string ToString(string? format, IFormatProvider? formatProvider) => Offset.ToString(format, formatProvider);

#if !NO_SPAN_FORMATTABLE
        /// <inheritdoc />
        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => Offset.TryFormat(destination, out charsWritten, format, provider);
#endif
    }

}
