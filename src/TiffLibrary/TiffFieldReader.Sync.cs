using System;

namespace TiffLibrary
{
#pragma warning disable CA2012
    public partial class TiffFieldReader
    {
        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Byte"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<byte> ReadByteField(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadByteFieldAsync(GetSyncReader(), entry, int.MaxValue, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Byte"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="sizeLimit">The maximum number of bytes to read from the IFD</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<byte> ReadByteField(TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false)
            => ReadByteFieldAsync(GetSyncReader(), entry, sizeLimit, skipTypeValidation).GetAwaiter().GetResult();


        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Byte"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="offset">The number of elements to skip reading.</param>
        /// <param name="destination">The destination buffer to store the values.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public void ReadByteField(TiffImageFileDirectoryEntry entry, int offset, Memory<byte> destination, bool skipTypeValidation = false)
            => ReadByteFieldAsync(GetSyncReader(), entry, offset, destination, skipTypeValidation);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SByte"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        [CLSCompliant(false)]
        public TiffValueCollection<sbyte> ReadSByteField(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadSByteFieldAsync(GetSyncReader(), entry, int.MaxValue, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SByte"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="sizeLimit">The maximum number of bytes to read from the IFD</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        [CLSCompliant(false)]
        public TiffValueCollection<sbyte> ReadSByteField(TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false)
            => ReadSByteFieldAsync(GetSyncReader(), entry, sizeLimit, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SByte"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="offset">The number of elements to skip reading.</param>
        /// <param name="destination">The destination buffer to store the values.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        [CLSCompliant(false)]
        public void ReadSByteField(TiffImageFileDirectoryEntry entry, int offset, Memory<sbyte> destination, bool skipTypeValidation = false)
            => ReadSByteFieldAsync(GetSyncReader(), entry, offset, destination, skipTypeValidation);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.ASCII"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<string> ReadASCIIField(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadASCIIFieldAsync(GetSyncReader(), entry, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read the first string value of type <see cref="TiffFieldType.ASCII"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="sizeLimit">The maximum number of bytes to read from the IFD</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The first string int the IFD.</returns>
        public string ReadASCIIFieldFirstString(TiffImageFileDirectoryEntry entry, int sizeLimit = -1, bool skipTypeValidation = false)
                => ReadASCIIFieldFirstStringAsync(GetSyncReader(), entry, sizeLimit, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Short"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        [CLSCompliant(false)]
        public TiffValueCollection<ushort> ReadShortField(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadShortFieldAsync(GetSyncReader(), entry, int.MaxValue, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Short"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        [CLSCompliant(false)]
        public TiffValueCollection<ushort> ReadShortField(TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false)
            => ReadShortFieldAsync(GetSyncReader(), entry, sizeLimit, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Short"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="offset">The number of elements to skip reading.</param>
        /// <param name="destination">The destination buffer to store the values.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        [CLSCompliant(false)]
        public void ReadShortField(TiffImageFileDirectoryEntry entry, int offset, Memory<ushort> destination, bool skipTypeValidation = false)
            => ReadShortFieldAsync(GetSyncReader(), entry, offset, destination, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SShort"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<short> ReadSShortField(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadSShortFieldAsync(GetSyncReader(), entry, int.MaxValue, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SShort"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<short> ReadSShortField(TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false)
            => ReadSShortFieldAsync(GetSyncReader(), entry, sizeLimit, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SShort"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="offset">The number of elements to skip reading.</param>
        /// <param name="destination">The destination buffer to store the values.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public void ReadSShortField(TiffImageFileDirectoryEntry entry, int offset, Memory<short> destination, bool skipTypeValidation = false)
            => ReadSShortFieldAsync(GetSyncReader(), entry, offset, destination, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Long"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        [CLSCompliant(false)]
        public TiffValueCollection<uint> ReadLongField(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadLongFieldAsync(GetSyncReader(), entry, int.MaxValue, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Long"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        [CLSCompliant(false)]
        public TiffValueCollection<uint> ReadLongField(TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false)
            => ReadLongFieldAsync(GetSyncReader(), entry, sizeLimit, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Long"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="offset">The number of elements to skip reading.</param>
        /// <param name="destination">The destination buffer to store the values.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        [CLSCompliant(false)]
        public void ReadLongField(TiffImageFileDirectoryEntry entry, int offset, Memory<uint> destination, bool skipTypeValidation = false)
            => ReadLongFieldAsync(GetSyncReader(), entry, offset, destination, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SLong"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<int> ReadSLongField(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadSLongFieldAsync(GetSyncReader(), entry, int.MaxValue, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SLong"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<int> ReadSLongField(TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false)
            => ReadSLongFieldAsync(GetSyncReader(), entry, sizeLimit, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SLong"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="offset">The number of elements to skip reading.</param>
        /// <param name="destination">The destination buffer to store the values.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public void ReadSLongField(TiffImageFileDirectoryEntry entry, int offset, Memory<int> destination, bool skipTypeValidation = false)
            => ReadSLongFieldAsync(GetSyncReader(), entry, offset, destination, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Long8"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        [CLSCompliant(false)]
        public TiffValueCollection<ulong> ReadLong8Field(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadLong8FieldAsync(GetSyncReader(), entry, int.MaxValue, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Long8"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        [CLSCompliant(false)]
        public TiffValueCollection<ulong> ReadLong8Field(TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false)
            => ReadLong8FieldAsync(GetSyncReader(), entry, sizeLimit, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Long8"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="offset">The number of elements to skip reading.</param>
        /// <param name="destination">The destination buffer to store the values.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        [CLSCompliant(false)]
        public void ReadLong8Field(TiffImageFileDirectoryEntry entry, int offset, Memory<ulong> destination, bool skipTypeValidation = false)
            => ReadLong8FieldAsync(GetSyncReader(), entry, offset, destination, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SLong8"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<long> ReadSLong8Field(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadSLong8FieldAsync(GetSyncReader(), entry, int.MaxValue, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SLong8"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<long> ReadSLong8Field(TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false)
            => ReadSLong8FieldAsync(GetSyncReader(), entry, sizeLimit, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SLong8"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="offset">The number of elements to skip reading.</param>
        /// <param name="destination">The destination buffer to store the values.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public void ReadSLong8Field(TiffImageFileDirectoryEntry entry, int offset, Memory<long> destination, bool skipTypeValidation = false)
            => ReadSLong8FieldAsync(GetSyncReader(), entry, offset, destination, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Float"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<float> ReadFloatField(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadFloatFieldAsync(GetSyncReader(), entry, int.MaxValue, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Float"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<float> ReadFloatField(TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false)
            => ReadFloatFieldAsync(GetSyncReader(), entry, sizeLimit, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Float"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="offset">The number of elements to skip reading.</param>
        /// <param name="destination">The destination buffer to store the values.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public void ReadFloatField(TiffImageFileDirectoryEntry entry, int offset, Memory<float> destination, bool skipTypeValidation = false)
            => ReadFloatFieldAsync(GetSyncReader(), entry, offset, destination, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Double"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<double> ReadDoubleField(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadDoubleFieldAsync(GetSyncReader(), entry, int.MaxValue, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Double"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<double> ReadDoubleField(TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false)
            => ReadDoubleFieldAsync(GetSyncReader(), entry, sizeLimit, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Double"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="offset">The number of elements to skip reading.</param>
        /// <param name="destination">The destination buffer to store the values.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public void ReadDoubleField(TiffImageFileDirectoryEntry entry, int offset, Memory<double> destination, bool skipTypeValidation = false)
            => ReadDoubleFieldAsync(GetSyncReader(), entry, offset, destination, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Rational"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        [CLSCompliant(false)]
        public TiffValueCollection<TiffRational> ReadRationalField(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadRationalFieldAsync(GetSyncReader(), entry, int.MaxValue, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Rational"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        [CLSCompliant(false)]
        public TiffValueCollection<TiffRational> ReadRationalField(TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false)
            => ReadRationalFieldAsync(GetSyncReader(), entry, sizeLimit, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Rational"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="offset">The number of elements to skip reading.</param>
        /// <param name="destination">The destination buffer to store the values.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        [CLSCompliant(false)]
        public void ReadRationalField(TiffImageFileDirectoryEntry entry, int offset, Memory<TiffRational> destination, bool skipTypeValidation = false)
            => ReadRationalFieldAsync(GetSyncReader(), entry, offset, destination, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SRational"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<TiffSRational> ReadSRationalField(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadSRationalFieldAsync(GetSyncReader(), entry, int.MaxValue, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SRational"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<TiffSRational> ReadSRationalField(TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false)
            => ReadSRationalFieldAsync(GetSyncReader(), entry, sizeLimit, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SRational"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="offset">The number of elements to skip reading.</param>
        /// <param name="destination">The destination buffer to store the values.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public void ReadSRationalField(TiffImageFileDirectoryEntry entry, int offset, Memory<TiffSRational> destination, bool skipTypeValidation = false)
            => ReadSRationalFieldAsync(GetSyncReader(), entry, offset, destination, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.IFD"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<TiffStreamOffset> ReadIFDField(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadIFDFieldAsync(GetSyncReader(), entry, int.MaxValue, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.IFD"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<TiffStreamOffset> ReadIFDField(TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false)
            => ReadIFDFieldAsync(GetSyncReader(), entry, sizeLimit, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.IFD"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="offset">The number of elements to skip reading.</param>
        /// <param name="destination">The destination buffer to store the values.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public void ReadIFDField(TiffImageFileDirectoryEntry entry, int offset, Memory<TiffStreamOffset> destination, bool skipTypeValidation = false)
            => ReadIFDFieldAsync(GetSyncReader(), entry, offset, destination, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.IFD8"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<TiffStreamOffset> ReadIFD8Field(TiffImageFileDirectoryEntry entry, bool skipTypeValidation = false)
            => ReadIFD8FieldAsync(GetSyncReader(), entry, int.MaxValue, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.IFD8"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<TiffStreamOffset> ReadIFD8Field(TiffImageFileDirectoryEntry entry, int sizeLimit, bool skipTypeValidation = false)
            => ReadIFD8FieldAsync(GetSyncReader(), entry, sizeLimit, skipTypeValidation).GetAwaiter().GetResult();

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.IFD8"/> from the specified IFD entry.
        /// </summary>
        /// <param name="entry">The IFD entry.</param>
        /// <param name="offset">The number of elements to skip reading.</param>
        /// <param name="destination">The destination buffer to store the values.</param>
        /// <param name="skipTypeValidation">Set to true to skip validation of the type field.</param>
        /// <returns>The values read.</returns>
        public void ReadIFD8Field(TiffImageFileDirectoryEntry entry, int offset, Memory<TiffStreamOffset> destination, bool skipTypeValidation = false)
            => ReadIFD8FieldAsync(GetSyncReader(), entry, offset, destination, skipTypeValidation).GetAwaiter().GetResult();

    }
#pragma warning restore CA2012
}
