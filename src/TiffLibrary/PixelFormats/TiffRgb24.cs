using System;
using System.Runtime.InteropServices;

namespace TiffLibrary.PixelFormats
{
    /// <summary>
    /// Pixel type containing three 8-bit unsigned normalized values ranging from 0 to 255.
    /// The color components are stored in red, green, blue order (least significant to most significant byte).
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct TiffRgb24 : IEquatable<TiffRgb24>
    {
        /// <summary>
        /// The red component.
        /// </summary>
        [FieldOffset(0)]
        public byte R;

        /// <summary>
        /// The green component.
        /// </summary>
        [FieldOffset(1)]
        public byte G;

        /// <summary>
        /// The blue component.
        /// </summary>
        [FieldOffset(2)]
        public byte B;

        /// <summary>
        /// Initialize the pixel with the specified red, green and blue values.
        /// </summary>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        public TiffRgb24(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The specified objects</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(TiffRgb24 other)
        {
            return R == other.R && G == other.G && B == other.B;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The specified objects</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object? obj)
        {
            return obj is TiffRgb24 other && Equals(other);
        }

        /// <summary>
        /// Gets a hash code for the current object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
#if NO_HASHCODE
            return R << 16 | G << 8 | B;
#else
            return HashCode.Combine(R, G, B);
#endif
        }

        /// <summary>
        /// Compares two objects for equality.
        /// </summary>
        /// <param name="left">The object on the left side of the operand.</param>
        /// <param name="right">The object on the right side of the operand.</param>
        /// <returns>True if the <paramref name="left"/> parameter is equal to the <paramref name="right"/> parameter; otherwise, false.</returns>
        public static bool operator ==(TiffRgb24 left, TiffRgb24 right) => left.Equals(right);

        /// <summary>
        /// Compares two objects for equality.
        /// </summary>
        /// <param name="left">The object on the left side of the operand.</param>
        /// <param name="right">The object on the right side of the operand.</param>
        /// <returns>True if the <paramref name="left"/> parameter is not equal to the <paramref name="right"/> parameter; otherwise, false.</returns>
        public static bool operator !=(TiffRgb24 left, TiffRgb24 right) => !left.Equals(right);

    }
}
