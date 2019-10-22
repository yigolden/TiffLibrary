using System;
using System.Runtime.InteropServices;

namespace TiffLibrary.PixelFormats
{
    /// <summary>
    /// Pixel type containing three 8-bit unsigned normalized values ranging from 0 to 255.
    /// The color components are stored in blue, green, red order (least significant to most significant byte).
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct TiffBgr24 : IEquatable<TiffBgr24>
    {
        /// <summary>
        /// The blue component.
        /// </summary>
        [FieldOffset(0)]
        public byte B;

        /// <summary>
        /// The green component.
        /// </summary>
        [FieldOffset(1)]
        public byte G;

        /// <summary>
        /// The red component.
        /// </summary>
        [FieldOffset(2)]
        public byte R;

        /// <summary>
        /// Initialize the pixel with the specified blue, green and red values.
        /// </summary>
        /// <param name="b">The blue component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="r">The red component.</param>
        public TiffBgr24(byte b, byte g, byte r)
        {
            B = b;
            G = g;
            R = r;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The specified objects</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(TiffBgr24 other)
        {
            return B == other.B && G == other.G && R == other.R;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The specified objects</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return obj is TiffBgr24 other && Equals(other);
        }

        /// <summary>
        /// Gets a hash code for the current object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return B << 16 | G << 8 | R;
        }

        /// <summary>
        /// Compares two objects for equality.
        /// </summary>
        /// <param name="left">The object on the left side of the operand.</param>
        /// <param name="right">The object on the right side of the operand.</param>
        /// <returns>True if the <paramref name="left"/> parameter is equal to the <paramref name="right"/> parameter; otherwise, false.</returns>
        public static bool operator ==(TiffBgr24 left, TiffBgr24 right) => left.Equals(right);

        /// <summary>
        /// Compares two objects for equality.
        /// </summary>
        /// <param name="left">The object on the left side of the operand.</param>
        /// <param name="right">The object on the right side of the operand.</param>
        /// <returns>True if the <paramref name="left"/> parameter is not equal to the <paramref name="right"/> parameter; otherwise, false.</returns>
        public static bool operator !=(TiffBgr24 left, TiffBgr24 right) => !left.Equals(right);
    }
}
