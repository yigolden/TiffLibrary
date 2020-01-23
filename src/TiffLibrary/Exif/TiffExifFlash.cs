using System;

namespace TiffLibrary.Exif
{
    /// <summary>
    /// Indicates the status of flash when the image was shot.
    /// </summary>
    public readonly struct TiffExifFlash : IEquatable<TiffExifFlash>
    {
        /// <summary>
        /// The value of the Flash tag.
        /// </summary>
        public ushort Value { get; }

        /// <summary>
        /// Initialize with the specified Flash tag value.
        /// </summary>
        /// <param name="value">The Flash tag value.</param>
        public TiffExifFlash(ushort value)
        {
            Value = value;
        }

        /// <summary>
        /// Values for bit 0 indicating whether the flash fired.
        /// </summary>
        public bool FlashFired => (Value & 0b1) != 0;

        /// <summary>
        /// Values for bits 1 and 2 indicating the status of returned light.
        /// 00 = No strobe return detection function
        /// 01 = reserved
        /// 10 = Strobe return light not detected
        /// 11 = Strobe return light detected
        /// </summary>
        public int StatusOfReturnedLight => (Value >> 1) & 0b11;

        /// <summary>
        /// Values for bits 3 and 4 indicating the camera's flash mode.
        /// 00 = unknown
        /// 01 = Compulsory flash firing
        /// 10 = Compulsory flash suppression
        /// 11 = Auto mode
        /// </summary>
        public int FlashMode => (Value >> 3) & 0b11;

        /// <summary>
        /// Values for bit 5 indicating the presence of a flash function.
        /// </summary>
        public bool FlashFunctionPresent => (Value & 0b100000) != 0;

        /// <summary>
        /// Values for bit 6 indicating the camera's red-eye mode.
        /// </summary>
        public bool RedEyeReductionSupported => (Value & 0b1000000) != 0;

        /// <summary>
        /// Gets the underlying value。
        /// </summary>
        /// <param name="flash"></param>
        public static implicit operator ushort(TiffExifFlash flash) => flash.Value;

        /// <summary>
        /// Gets the underlying value。
        /// </summary>
        /// <returns>The value of the Flash tag.</returns>
        public ushort ToUInt16() => Value;

        /// <inheritdoc />
        public bool Equals(TiffExifFlash other)
        {
            return Value == other.Value && Value == other.Value;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is TiffExifFlash flash && Equals(flash);
        }

        /// <summary>
        /// Compares two objects for equality.
        /// </summary>
        /// <param name="left">The object on the left side of the operand.</param>
        /// <param name="right">The object on the right side of the operand.</param>
        /// <returns>True if the <paramref name="left"/> parameter is equal to the <paramref name="right"/> parameter; otherwise, false.</returns>
        public static bool operator ==(TiffExifFlash left, TiffExifFlash right) => left.Equals(right);

        /// <summary>
        /// Compares two objects for equality.
        /// </summary>
        /// <param name="left">The object on the left side of the operand.</param>
        /// <param name="right">The object on the right side of the operand.</param>
        /// <returns>True if the <paramref name="left"/> parameter is not equal to the <paramref name="right"/> parameter; otherwise, false.</returns>
        public static bool operator !=(TiffExifFlash left, TiffExifFlash right) => !left.Equals(right);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Value switch
            {
                0x0000 => "Flash did not fire.",
                0x0001 => "Flash fired.",
                0x0005 => "Strobe return light not detected.",
                0x0007 => "Strobe return light detected.",
                0x0009 => "Flash fired, compulsory flash mode.",
                0x000D => "Flash fired, compulsory flash mode, return light not detected",
                0x000F => "Flash fired, compulsory flash mode, return light detected.",
                0x0010 => "Flash did not fire, compulsory flash mode.",
                0x0018 => "Flash did not fire, auto mode.",
                0x0019 => "Flash fired, auto mode.",
                0x001D => "Flash fired, auto mode, return light not detected.",
                0x001F => "Flash fired, auto mode, return light detected.",
                0x0020 => "No flash function.",
                0x0041 => "Flash fired, red-eye reduction mode.",
                0x0045 => "Flash fired, red-eye reduction mode, return light not detected.",
                0x0047 => "Flash fired, red-eye reduction mode, return light detected.",
                0x0049 => "Flash fired, compulsory flash mode, red-eye reduction mode.",
                0x004D => "Flash fired, compulsory flash mode, red-eye reduction mode, return light not detected.",
                0x004F => "Flash fired, compulsory flash mode, red-eye reduction mode, return light detected.",
                0x0059 => "Flash fired, auto mode, red-eye reduction mode.",
                0x005D => "Flash fired, auto mode, return light not detected, red-eye reduction mode.",
                0x005F => "Flash fired, auto mode, return light detected, red-eye reduction mode.",
                _ => "Unknown.",
            };
        }
    }
}
