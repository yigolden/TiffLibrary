using System;
using System.Runtime.InteropServices;

namespace TiffLibrary.PixelFormats
{
    /// <summary>
    /// Pixel type containing four 8-bit unsigned normalized values ranging from 0 to 255.
    /// The color components are stored in cyan, magenta, yellow, black order (least significant to most significant byte).
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TiffCmyk32 : IEquatable<TiffCmyk32>
    {
        /// <summary>
        /// The cyan component.
        /// </summary>
        public byte C;

        /// <summary>
        /// The magenta component.
        /// </summary>
        public byte M;

        /// <summary>
        /// The yellow component.
        /// </summary>
        public byte Y;

        /// <summary>
        /// The black component.
        /// </summary>
        public byte K;

        /// <summary>
        /// Initialize the pixel with the specified cyan, magenta, yellow and black values.
        /// </summary>
        /// <param name="c">The cyan component.</param>
        /// <param name="m">The magenta component.</param>
        /// <param name="y">The yellow component.</param>
        /// <param name="k">The black component.</param>
        public TiffCmyk32(byte c, byte m, byte y, byte k)
        {
            C = c;
            M = m;
            Y = y;
            K = k;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The specified objects</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(TiffCmyk32 other)
        {
            return C == other.C && M == other.M && Y == other.Y && K == other.K;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The specified objects</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object? obj)
        {
            return obj is TiffCmyk32 other && Equals(other);
        }

        /// <summary>
        /// Gets a hash code for the current object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return C << 24 | M << 16 | Y << 8 | K;
        }

        /// <summary>
        /// Compares two objects for equality.
        /// </summary>
        /// <param name="left">The object on the left side of the operand.</param>
        /// <param name="right">The object on the right side of the operand.</param>
        /// <returns>True if the <paramref name="left"/> parameter is equal to the <paramref name="right"/> parameter; otherwise, false.</returns>
        public static bool operator ==(TiffCmyk32 left, TiffCmyk32 right) => left.Equals(right);

        /// <summary>
        /// Compares two objects for equality.
        /// </summary>
        /// <param name="left">The object on the left side of the operand.</param>
        /// <param name="right">The object on the right side of the operand.</param>
        /// <returns>True if the <paramref name="left"/> parameter is not equal to the <paramref name="right"/> parameter; otherwise, false.</returns>
        public static bool operator !=(TiffCmyk32 left, TiffCmyk32 right) => !left.Equals(right);
    }


}
