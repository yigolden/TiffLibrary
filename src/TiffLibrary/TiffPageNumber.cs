using System;
using TiffLibrary.Utils;

namespace TiffLibrary
{
    /// <summary>
    /// Contains page number of a page in the TIFF as well as the total page count.
    /// </summary>
    public readonly struct TiffPageNumber : IEquatable<TiffPageNumber>
    {
        /// <summary>
        /// Gets the page number of the current page.
        /// </summary>
        public ushort PageNumber { get; }

        /// <summary>
        /// Gets the total page count.
        /// </summary>
        public ushort TotalPages { get; }

        /// <summary>
        /// Initialize the object with the specified page number and total page count.
        /// </summary>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="totalPages">Total page count.</param>
        public TiffPageNumber(ushort pageNumber, ushort totalPages)
        {
            PageNumber = pageNumber;
            TotalPages = totalPages;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The specified objects</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(TiffPageNumber other)
        {
            return PageNumber == other.PageNumber && TotalPages == other.TotalPages;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The specified objects</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return obj is TiffPageNumber other && Equals(other);
        }

        /// <summary>
        /// Compares two objects for equality.
        /// </summary>
        /// <param name="left">The object on the left side of the operand.</param>
        /// <param name="right">The object on the right side of the operand.</param>
        /// <returns>True if the <paramref name="left"/> parameter is equal to the <paramref name="right"/> parameter; otherwise, false.</returns>
        public static bool operator ==(TiffPageNumber left, TiffPageNumber right) => left.Equals(right);

        /// <summary>
        /// Compares two objects for equality.
        /// </summary>
        /// <param name="left">The object on the left side of the operand.</param>
        /// <param name="right">The object on the right side of the operand.</param>
        /// <returns>True if the <paramref name="left"/> parameter is not equal to the <paramref name="right"/> parameter; otherwise, false.</returns>
        public static bool operator !=(TiffPageNumber left, TiffPageNumber right) => !left.Equals(right);

        /// <summary>
        /// Gets a hash code for the current object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return HashHelpers.Combine(PageNumber.GetHashCode(), TotalPages.GetHashCode());
        }

        /// <summary>
        /// Converts this <see cref="TiffPoint"/> to a human-readable string.
        /// </summary>
        /// <returns>A human-readable string</returns>
        public override string ToString()
        {
            return $"({PageNumber}/{TotalPages})";
        }
    }
}
