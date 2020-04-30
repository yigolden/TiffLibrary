using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TiffLibrary.PixelFormats
{
    /// <summary>
    /// Pixel type containing three 8-bit unsigned normalized values ranging from 0 to 255.
    /// The color components are stored in red, green, blue, alpha order (least significant to most significant byte).
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TiffRgba32 : IEquatable<TiffRgba32>
    {
        /// <summary>
        /// The red component.
        /// </summary>
        public byte R;

        /// <summary>
        /// The green component.
        /// </summary>
        public byte G;

        /// <summary>
        /// The blue component.
        /// </summary>
        public byte B;

        /// <summary>
        /// The alpha component.
        /// </summary>
        public byte A;

        /// <summary>
        /// Initialize the pixel with the specified red, green, blue and alpha values.
        /// </summary>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        /// <param name="a">The alpha component.</param>
        public TiffRgba32(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The specified objects</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(TiffRgba32 other)
        {
            return R == other.R && G == other.G && B == other.B && A == other.A;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The specified objects</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object? obj)
        {
            return obj is TiffRgba32 other && Equals(other);
        }

        /// <summary>
        /// Gets a hash code for the current object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
#if NO_HASHCODE
            return R << 24 | G << 16 | B << 8 | A;
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
        public static bool operator ==(TiffRgba32 left, TiffRgba32 right) => left.Equals(right);

        /// <summary>
        /// Compares two objects for equality.
        /// </summary>
        /// <param name="left">The object on the left side of the operand.</param>
        /// <param name="right">The object on the right side of the operand.</param>
        /// <returns>True if the <paramref name="left"/> parameter is not equal to the <paramref name="right"/> parameter; otherwise, false.</returns>
        public static bool operator !=(TiffRgba32 left, TiffRgba32 right) => !left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal readonly TiffRgba32 Inverse()
        {
            uint reversed = ~Unsafe.As<TiffRgba32, uint>(ref Unsafe.AsRef(in this));
            return Unsafe.As<uint, TiffRgba32>(ref reversed);
        }
    }

}
