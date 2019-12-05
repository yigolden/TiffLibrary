using System;
using System.Runtime.InteropServices;

namespace TiffLibrary.PixelFormats
{
    /// <summary>
    /// Pixel type containing a single 8 bit opacity value ranging from 0 to 255.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TiffMask : IEquatable<TiffMask>
    {
        /// <summary>
        /// The opacity of the pixel.
        /// </summary>
        public byte Opacity;

        /// <summary>
        /// Initialize the pixel with the specified opacity value.
        /// </summary>
        /// <param name="opacity">The opacity of the pixel.</param>
        public TiffMask(byte opacity)
        {
            Opacity = opacity;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The specified objects</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(TiffMask other)
        {
            return Opacity == other.Opacity;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The specified objects</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object? obj)
        {
            return obj is TiffMask other && Equals(other);
        }

        /// <summary>
        /// Gets a hash code for the current object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return Opacity.GetHashCode();
        }

        /// <summary>
        /// Compares two objects for equality.
        /// </summary>
        /// <param name="left">The object on the left side of the operand.</param>
        /// <param name="right">The object on the right side of the operand.</param>
        /// <returns>True if the <paramref name="left"/> parameter is equal to the <paramref name="right"/> parameter; otherwise, false.</returns>
        public static bool operator ==(TiffMask left, TiffMask right) => left.Equals(right);

        /// <summary>
        /// Compares two objects for equality.
        /// </summary>
        /// <param name="left">The object on the left side of the operand.</param>
        /// <param name="right">The object on the right side of the operand.</param>
        /// <returns>True if the <paramref name="left"/> parameter is not equal to the <paramref name="right"/> parameter; otherwise, false.</returns>
        public static bool operator !=(TiffMask left, TiffMask right) => !left.Equals(right);
    }

}
