using System;

namespace TiffLibrary
{
#pragma warning disable CA2012
    public partial class TiffFieldReader
    {

        /// <summary>
        /// Read bytes from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="destination">The destination buffer.</param>
        /// <param name="sizePerElement">Byte count per element.</param>
        public void ReadByteField(TiffImageFileDirectoryEntry entry, Memory<byte> destination, int sizePerElement = 0)
            => ReadByteFieldAsync(GetSyncReader(), entry, destination, sizePerElement).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Byte"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<byte> ReadByteField(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadByteFieldAsync(GetSyncReader(), entry, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SByte"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<sbyte> ReadSByteField(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadSByteFieldAsync(GetSyncReader(), entry, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.ASCII"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<string> ReadASCIIField(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadASCIIFieldAsync(GetSyncReader(), entry, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Short"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<ushort> ReadShortField(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadShortFieldAsync(GetSyncReader(), entry, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SShort"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<short> ReadSShortField(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadSShortFieldAsync(GetSyncReader(), entry, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Long"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<uint> ReadLongField(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadLongFieldAsync(GetSyncReader(), entry, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SLong"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<int> ReadSLongField(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadSLongFieldAsync(GetSyncReader(), entry, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Long8"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<ulong> ReadLong8Field(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadLong8FieldAsync(GetSyncReader(), entry, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SLong8"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<long> ReadSLong8Field(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadSLong8FieldAsync(GetSyncReader(), entry, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Float"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<float> ReadFloatField(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadFloatFieldAsync(GetSyncReader(), entry, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Double"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<double> ReadDoubleField(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadDoubleFieldAsync(GetSyncReader(), entry, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Rational"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<TiffRational> ReadRationalField(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadRationalFieldAsync(GetSyncReader(), entry, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SRational"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<TiffSRational> ReadSRationalField(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadSRationalFieldAsync(GetSyncReader(), entry, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.IFD"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<TiffStreamOffset> ReadIFDField(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadIFDFieldAsync(GetSyncReader(), entry, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.IFD8"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<TiffStreamOffset> ReadIFD8Field(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadIFD8FieldAsync(GetSyncReader(), entry, skipTypeValidation).GetAwaiter().GetResult();

    }
#pragma warning restore CA2012
}
