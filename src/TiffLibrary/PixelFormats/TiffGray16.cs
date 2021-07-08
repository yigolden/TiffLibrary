using System;

namespace TiffLibrary.PixelFormats
{
    /// <summary>
    /// Pixel type containing a single 16 bit intensity value ranging from 0 to 255. The order of bytes is in machine-endian.
    /// </summary>
    [CLSCompliant(false)]
    public struct TiffGray16 : IEquatable<TiffGray16>
    {
        /// <summary>
        /// The intensity of the pixel.
        /// </summary>
        public ushort Intensity;

        /// <summary>
        /// Initialize the pixel with the specified intensity value.
        /// </summary>
        /// <param name="intensity">The intensity of the pixel.</param>
        public TiffGray16(ushort intensity)
        {
            Intensity = intensity;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The specified objects</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(TiffGray16 other)
        {
            return Intensity == other.Intensity;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The specified objects</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object? obj)
        {
            return obj is TiffGray16 other && Equals(other);
        }

        /// <summary>
        /// Gets a hash code for the current object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return Intensity.GetHashCode();
        }

        /// <summary>
        /// Compares two objects for equality.
        /// </summary>
        /// <param name="left">The object on the left side of the operand.</param>
        /// <param name="right">The object on the right side of the operand.</param>
        /// <returns>True if the <paramref name="left"/> parameter is equal to the <paramref name="right"/> parameter; otherwise, false.</returns>
        public static bool operator ==(TiffGray16 left, TiffGray16 right) => left.Equals(right);

        /// <summary>
        /// Compares two objects for equality.
        /// </summary>
        /// <param name="left">The object on the left side of the operand.</param>
        /// <param name="right">The object on the right side of the operand.</param>
        /// <returns>True if the <paramref name="left"/> parameter is not equal to the <paramref name="right"/> parameter; otherwise, false.</returns>
        public static bool operator !=(TiffGray16 left, TiffGray16 right) => !left.Equals(right);
    }

}
