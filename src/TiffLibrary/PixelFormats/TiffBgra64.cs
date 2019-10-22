using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TiffLibrary.Utils;

namespace TiffLibrary.PixelFormats
{
    /// <summary>
    /// Pixel type containing four 16-bit unsigned normalized values ranging from 0 to 65535.
    /// The color components are stored in blue, green, red, alpha order (least significant to most significant byte).
    /// Each component is stored in machine-endian.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TiffBgra64 : IEquatable<TiffBgra64>
    {
        /// <summary>
        /// The blue component.
        /// </summary>
        public ushort B;

        /// <summary>
        /// The green component.
        /// </summary>
        public ushort G;

        /// <summary>
        /// The red component.
        /// </summary>
        public ushort R;

        /// <summary>
        /// The alpha component.
        /// </summary>
        public ushort A;

        /// <summary>
        /// Initialize the pixel with the specified blue, green, red and alpha values.
        /// </summary>
        /// <param name="b">The blue component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="r">The red component.</param>
        /// <param name="a">The alpha component.</param>
        public TiffBgra64(ushort b, ushort g, ushort r, ushort a)
        {
            B = b;
            G = g;
            R = r;
            A = a;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The specified objects</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(TiffBgra64 other)
        {
            return B == other.B && G == other.G && R == other.R && A == other.A;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The specified objects</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return obj is TiffBgra64 other && Equals(other);
        }

        /// <summary>
        /// Gets a hash code for the current object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return HashHelpers.Combine((B << 16 | G).GetHashCode(), (R << 16 | A).GetHashCode());
        }

        /// <summary>
        /// Compares two objects for equality.
        /// </summary>
        /// <param name="left">The object on the left side of the operand.</param>
        /// <param name="right">The object on the right side of the operand.</param>
        /// <returns>True if the <paramref name="left"/> parameter is equal to the <paramref name="right"/> parameter; otherwise, false.</returns>
        public static bool operator ==(TiffBgra64 left, TiffBgra64 right) => left.Equals(right);

        /// <summary>
        /// Compares two objects for equality.
        /// </summary>
        /// <param name="left">The object on the left side of the operand.</param>
        /// <param name="right">The object on the right side of the operand.</param>
        /// <returns>True if the <paramref name="left"/> parameter is not equal to the <paramref name="right"/> parameter; otherwise, false.</returns>
        public static bool operator !=(TiffBgra64 left, TiffBgra64 right) => !left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal readonly TiffBgra64 Inverse()
        {
            ulong reversed = ~Unsafe.As<TiffBgra64, ulong>(ref Unsafe.AsRef(in this));
            return Unsafe.As<ulong, TiffBgra64>(ref reversed);
        }
    }

}
