using System;
using System.Globalization;

namespace TiffLibrary
{
    /// <summary>
    /// Represents a signed rational.
    /// </summary>
    public readonly struct TiffSRational : IEquatable<TiffSRational>, IEquatable<TiffRational>
    {
        /// <summary>
        /// The numerator of the rational.
        /// </summary>
        public int Numerator { get; }

        /// <summary>
        /// The denominator of the rational.
        /// </summary>
        public int Denominator { get; }

        /// <summary>
        /// Creates a rational value with the specified numerator and denominator.
        /// </summary>
        /// <param name="numerator">The numerator of the rational.</param>
        /// <param name="denominator">The denominator of the rational.</param>
        public TiffSRational(int numerator, int denominator)
        {
            Numerator = numerator;
            Denominator = denominator;
        }

        /// <summary>
        /// Converts the rational value to <see cref="float"/>.
        /// </summary>
        /// <returns>The rational value in <see cref="float"/>.</returns>
        public double ToSingle()
        {
            return Numerator / (double)Denominator;
        }

        /// <summary>
        /// Converts the rational value to <see cref="double"/>.
        /// </summary>
        /// <returns>The rational value in <see cref="double"/>.</returns>
        public double ToDouble()
        {
            return Numerator / (double)Denominator;
        }

        /// <inheritdoc />
        public bool Equals(TiffSRational other)
        {
            return Numerator == other.Numerator && Denominator == other.Denominator;
        }

        /// <inheritdoc />
        [CLSCompliant(false)]
        public bool Equals(TiffRational other)
        {
            return Numerator == other.Numerator && Denominator == other.Denominator;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return (obj is TiffSRational other && Equals(other)) || (obj is TiffRational other2 && Equals(other2));
        }

        /// <summary>
        /// Compares two objects for equality.
        /// </summary>
        /// <param name="left">The object on the left side of the operand.</param>
        /// <param name="right">The object on the right side of the operand.</param>
        /// <returns>True if the <paramref name="left"/> parameter is equal to the <paramref name="right"/> parameter; otherwise, false.</returns>
        public static bool operator ==(TiffSRational left, TiffSRational right) => left.Equals(right);

        /// <summary>
        /// Compares two objects for equality.
        /// </summary>
        /// <param name="left">The object on the left side of the operand.</param>
        /// <param name="right">The object on the right side of the operand.</param>
        /// <returns>True if the <paramref name="left"/> parameter is not equal to the <paramref name="right"/> parameter; otherwise, false.</returns>
        public static bool operator !=(TiffSRational left, TiffSRational right) => !left.Equals(right);

        /// <summary>
        /// Compares two objects for equality.
        /// </summary>
        /// <param name="left">The object on the left side of the operand.</param>
        /// <param name="right">The object on the right side of the operand.</param>
        /// <returns>True if the <paramref name="left"/> parameter is equal to the <paramref name="right"/> parameter; otherwise, false.</returns>
        [CLSCompliant(false)]
        public static bool operator ==(TiffSRational left, TiffRational right) => left.Equals(right);

        /// <summary>
        /// Compares two objects for equality.
        /// </summary>
        /// <param name="left">The object on the left side of the operand.</param>
        /// <param name="right">The object on the right side of the operand.</param>
        /// <returns>True if the <paramref name="left"/> parameter is not equal to the <paramref name="right"/> parameter; otherwise, false.</returns>
        [CLSCompliant(false)]
        public static bool operator !=(TiffSRational left, TiffRational right) => !left.Equals(right);

        /// <inheritdoc />
        public override int GetHashCode()
        {
#if NO_HASHCODE
            return HashHelpers.Combine(Numerator.GetHashCode(), Denominator.GetHashCode());
#else
            return HashCode.Combine(Numerator, Denominator);
#endif
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Numerator.ToString("G", CultureInfo.InvariantCulture) + "/" + Denominator.ToString("G", CultureInfo.InvariantCulture);
        }
    }
}
