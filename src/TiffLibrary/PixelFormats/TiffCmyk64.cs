using System;
using System.Runtime.InteropServices;
using TiffLibrary.Utils;

namespace TiffLibrary.PixelFormats
{
    /// <summary>
    /// Pixel type containing four 16-bit unsigned normalized values ranging from 0 to 65535.
    /// The color components are stored in cyan, magenta, yellow, black order (least significant to most significant byte).
    /// Each component is stored in machine-endian.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TiffCmyk64 : IEquatable<TiffCmyk64>
    {
        /// <summary>
        /// The cyan component.
        /// </summary>
        public ushort C;

        /// <summary>
        /// The magenta component.
        /// </summary>
        public ushort M;

        /// <summary>
        /// The yellow component.
        /// </summary>
        public ushort Y;

        /// <summary>
        /// The black component.
        /// </summary>
        public ushort K;

        /// <summary>
        /// Initialize the pixel with the specified cyan, magenta, yellow and black values.
        /// </summary>
        /// <param name="c">The cyan component.</param>
        /// <param name="m">The magenta component.</param>
        /// <param name="y">The yellow component.</param>
        /// <param name="k">The black component.</param>
        public TiffCmyk64(ushort c, ushort m, ushort y, ushort k)
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
        public bool Equals(TiffCmyk64 other)
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
            return obj is TiffCmyk64 other && Equals(other);
        }

        /// <summary>
        /// Gets a hash code for the current object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
#if NO_HASHCODE
            return HashHelpers.Combine((C << 16 | M).GetHashCode(), (Y << 16 | K).GetHashCode());
#else
            return HashCode.Combine(C, M, Y, K);
#endif
        }

        /// <summary>
        /// Compares two objects for equality.
        /// </summary>
        /// <param name="left">The object on the left side of the operand.</param>
        /// <param name="right">The object on the right side of the operand.</param>
        /// <returns>True if the <paramref name="left"/> parameter is equal to the <paramref name="right"/> parameter; otherwise, false.</returns>
        public static bool operator ==(TiffCmyk64 left, TiffCmyk64 right) => left.Equals(right);

        /// <summary>
        /// Compares two objects for equality.
        /// </summary>
        /// <param name="left">The object on the left side of the operand.</param>
        /// <param name="right">The object on the right side of the operand.</param>
        /// <returns>True if the <paramref name="left"/> parameter is not equal to the <paramref name="right"/> parameter; otherwise, false.</returns>
        public static bool operator !=(TiffCmyk64 left, TiffCmyk64 right) => !left.Equals(right);

    }


}
