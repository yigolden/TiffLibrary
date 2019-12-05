using System;
using TiffLibrary.Utils;

namespace TiffLibrary
{
    /// <summary>
    /// Represents an ordered pair of integer x- and y-coordinates that defines a point in a two-dimensional plane.
    /// </summary>
    public readonly struct TiffPoint : IEquatable<TiffPoint>
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

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The specified objects</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(TiffPoint other)
        {
            return X == other.X && Y == other.Y;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The specified objects</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
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

        /// <summary>
        /// Gets a hash code for the current object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return HashHelpers.Combine(X.GetHashCode(), Y.GetHashCode());
        }

        /// <summary>
        /// Converts this <see cref="TiffPoint"/> to a human-readable string.
        /// </summary>
        /// <returns>A human-readable string</returns>
        public override string ToString()
        {
            return $"({X},{Y})";
        }
    }
}
