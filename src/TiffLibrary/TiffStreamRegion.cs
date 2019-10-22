﻿using System;
using TiffLibrary.Utils;

namespace TiffLibrary
{
    /// <summary>
    /// Represents a region in the TIFF file stream.
    /// </summary>
    public readonly struct TiffStreamRegion : IEquatable<TiffStreamRegion>
    {
        private readonly long _offset;
        private readonly int _length;

        /// <summary>
        /// Gets the start position of the region in the stream.
        /// </summary>
        public TiffStreamOffset Offset => new TiffStreamOffset(_offset);

        /// <summary>
        /// Gets the length of the region.
        /// </summary>
        public int Length => _length;

        /// <summary>
        /// Initialize the object with the specified offset and length.
        /// </summary>
        /// <param name="offset">The start position of the region in the stream.</param>
        /// <param name="length">The length of the region.</param>
        public TiffStreamRegion(long offset, int length)
        {
            _offset = offset;
            _length = length;
        }

        /// <summary>
        /// Initialize the object with the specified offset and length.
        /// </summary>
        /// <param name="offset">The start position of the region in the stream.</param>
        /// <param name="length">The length of the region.</param>
        public TiffStreamRegion(int offset, int length)
        {
            _offset = offset;
            _length = length;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The specified objects</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(TiffStreamRegion other)
        {
            return other._offset == _offset && other._length == _length;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The specified objects</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (obj is TiffStreamRegion other)
            {
                return other._offset == _offset && other._length == _length;
            }
            return false;
        }

        /// <summary>
        /// Compares two objects for equality.
        /// </summary>
        /// <param name="left">The object on the left side of the operand.</param>
        /// <param name="right">The object on the right side of the operand.</param>
        /// <returns>True if the <paramref name="left"/> parameter is equal to the <paramref name="right"/> parameter; otherwise, false.</returns>
        public static bool operator ==(TiffStreamRegion left, TiffStreamRegion right) => left.Equals(right);

        /// <summary>
        /// Compares two objects for equality.
        /// </summary>
        /// <param name="left">The object on the left side of the operand.</param>
        /// <param name="right">The object on the right side of the operand.</param>
        /// <returns>True if the <paramref name="left"/> parameter is not equal to the <paramref name="right"/> parameter; otherwise, false.</returns>
        public static bool operator !=(TiffStreamRegion left, TiffStreamRegion right) => !left.Equals(right);

        /// <summary>
        /// Gets a hash code for the current object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return HashHelpers.Combine(_offset.GetHashCode(), _length.GetHashCode());
        }
    }
}
