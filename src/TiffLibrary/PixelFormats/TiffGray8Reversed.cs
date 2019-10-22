using System;
using System.Runtime.InteropServices;

namespace TiffLibrary.PixelFormats
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct TiffGray8Reversed : IEquatable<TiffGray8Reversed>
    {
        public byte ReversedIntensity;

        public TiffGray8Reversed(byte intensity)
        {
            ReversedIntensity = intensity;
        }

        public bool Equals(TiffGray8Reversed other)
        {
            return ReversedIntensity == other.ReversedIntensity;
        }

        public override bool Equals(object obj)
        {
            return obj is TiffGray8Reversed other && Equals(other);
        }

        public override int GetHashCode()
        {
            return ReversedIntensity.GetHashCode();
        }

        public static bool operator ==(TiffGray8Reversed op1, TiffGray8Reversed op2) => op1.Equals(op2);
        public static bool operator !=(TiffGray8Reversed op1, TiffGray8Reversed op2) => !op1.Equals(op2);
    }
}
