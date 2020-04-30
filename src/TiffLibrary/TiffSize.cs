using System;
using TiffLibrary.Utils;

namespace TiffLibrary
{
    /// <summary>
    /// A structure representing the size of a rectangle region, containing a width and a height field.
    /// </summary>
    public readonly struct TiffSize : IEquatable<TiffSize>
    {
        /// <summary>
        /// Initialize the instance with the specified width and height.
        /// </summary>
        /// <param name="width">The width of the region.</param>
        /// <param name="height">The height of the region.</param>
        public TiffSize(int width, int height)
        {
            if (width < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }
            if (height < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Gets the width of the region.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Gets the height of the region.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Is true if either Width or Height is zero.
        /// </summary>
        public bool IsAreaEmpty => Width == 0 || Height == 0;

        /// <inheritdoc />
        public bool Equals(TiffSize other)
        {
            return Width == other.Width && Height == other.Height;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is TiffSize size && Equals(size);
        }

        /// <summary>
        /// Compares two objects for equality.
        /// </summary>
        /// <param name="left">The object on the left side of the operand.</param>
        /// <param name="right">The object on the right side of the operand.</param>
        /// <returns>True if the <paramref name="left"/> parameter is equal to the <paramref name="right"/> parameter; otherwise, false.</returns>
        public static bool operator ==(TiffSize left, TiffSize right) => left.Equals(right);

        /// <summary>
        /// Compares two objects for equality.
        /// </summary>
        /// <param name="left">The object on the left side of the operand.</param>
        /// <param name="right">The object on the right side of the operand.</param>
        /// <returns>True if the <paramref name="left"/> parameter is not equal to the <paramref name="right"/> parameter; otherwise, false.</returns>
        public static bool operator !=(TiffSize left, TiffSize right) => !left.Equals(right);

        /// <inheritdoc />
        public override int GetHashCode()
        {
#if NO_HASHCODE
            return HashHelpers.Combine(Width.GetHashCode(), Height.GetHashCode());
#else
            return HashCode.Combine(Width, Height);
#endif
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({Width},{Height})";
        }
    }
}
