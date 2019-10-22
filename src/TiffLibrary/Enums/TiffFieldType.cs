namespace TiffLibrary
{
    /// <summary>
    /// TIFF IFD Field Type
    /// </summary>
    public enum TiffFieldType : short
    {
        /// <summary>
        /// 8-bit unsigned integer.
        /// </summary>
        Byte = 1,

        /// <summary>
        /// 8-bit byte that contains a 7-bit ASCII code; the last byte must be NUL (binary zero).
        /// The value of the Count part of an ASCII field entry includes the NUL. If padding is necessary, the Count does not include the pad byte. Note that there is no initial “count byte” as in Pascal-style strings.
        /// Any ASCII field can contain multiple strings, each terminated with a NUL. A single string is preferred whenever possible. The Count for multi-string fields is the number of bytes in all the strings in that field plus their terminating NUL bytes. Only one NUL is allowed between strings, so that the strings following the first string will often begin on an odd byte.
        /// </summary>
        ASCII = 2,

        /// <summary>
        /// 16-bit (2-byte) unsigned integer.
        /// </summary>
        Short = 3,

        /// <summary>
        /// 32-bit (4-byte) unsigned integer.
        /// </summary>
        Long = 4,

        /// <summary>
        /// Two LONGs:  the first represents the numerator of a fraction; the second, the denominator.
        /// </summary>
        Rational = 5,

        /// <summary>
        /// An 8-bit signed (twos-complement) integer.
        /// </summary>
        SByte = 6,

        /// <summary>
        /// An 8-bit byte that may contain anything, depending on the definition of the field.
        /// </summary>
        Undefined = 7,

        /// <summary>
        /// A 16-bit (2-byte) signed (twos-complement) integer.
        /// </summary>
        SShort = 8,

        /// <summary>
        /// A 32-bit (4-byte) signed (twos-complement) integer.
        /// </summary>
        SLong = 9,

        /// <summary>
        /// Two SLONG’s:  the first represents the numerator of a fraction, the second the denominator. 
        /// </summary>
        SRational = 10,

        /// <summary>
        /// Single precision (4-byte) IEEE format.
        /// </summary>
        Float = 11,

        /// <summary>
        /// Double precision (8-byte) IEEE format.
        /// </summary>
        Double = 12,

        /// <summary>
        /// 32-bit unsigned integer (offset)
        /// </summary>
        IFD = 13,

        /// <summary>
        /// BigTiff: 64-bit unsigned integer
        /// </summary>
        Long8 = 16,

        /// <summary>
        /// BigTiff: 64-bit signed integer
        /// </summary>
        SLong8 = 17,

        /// <summary>
        /// BigTiff: 64-bit IFD offset
        /// </summary>
        IFD8 = 18,
    }
}
