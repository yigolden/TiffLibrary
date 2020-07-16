using System;
using System.Threading;
using System.Threading.Tasks;

namespace TiffLibrary
{
#pragma warning disable CA1815 // CA1815: Override equals and operator equals on value types
    /// <summary>
    /// The helper structure to read tag data from IFD.
    /// </summary>
    public readonly struct TiffTagReader
#pragma warning restore CA1815 // CA1815: Override equals and operator equals on value types
    {
        /// <summary>
        /// Create the <see cref="TiffTagReader"/> structure with the specified <see cref="TiffFieldReader"/> and <see cref="TiffImageFileDirectory"/>.
        /// </summary>
        /// <param name="reader">The field reader.</param>
        /// <param name="imageFileDirectory">The IFD to read.</param>
        public TiffTagReader(TiffFieldReader reader, TiffImageFileDirectory imageFileDirectory)
        {
            Reader = reader ?? throw new ArgumentNullException(nameof(reader));
            ImageFileDirectory = imageFileDirectory ?? throw new ArgumentNullException(nameof(imageFileDirectory));
        }

        /// <summary>
        /// Gets the associated field reader instance.
        /// </summary>
        public TiffFieldReader Reader { get; }

        /// <summary>
        /// Gets the associated IFD.
        /// </summary>
        public TiffImageFileDirectory ImageFileDirectory { get; }

        #region Byte

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Byte"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<byte>> ReadByteFieldAsync(TiffTag tag, CancellationToken cancellationToken = default)
            => ReadByteFieldAsync(tag, int.MaxValue, cancellationToken);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Byte"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <param name="sizeLimit">The maximum number of bytes to read from the IFD</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<byte>> ReadByteFieldAsync(TiffTag tag, int sizeLimit, CancellationToken cancellationToken = default)
        {
            if (Reader is null)
            {
                throw new InvalidOperationException();
            }

            if (ImageFileDirectory is null)
            {
                throw new InvalidOperationException();
            }

            TiffImageFileDirectoryEntry entry = ImageFileDirectory.FindEntry(tag);
            if (entry.Tag == TiffTag.None)
            {
                return new ValueTask<TiffValueCollection<byte>>(TiffValueCollection.Empty<byte>());
            }

            return Reader.ReadByteFieldAsync(entry, sizeLimit, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Byte"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<byte> ReadByteField(TiffTag tag)
            => ReadByteField(tag, int.MaxValue);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Byte"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <param name="sizeLimit">The maximum number of bytes to read from the IFD</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<byte> ReadByteField(TiffTag tag, int sizeLimit)
        {
            if (Reader is null)
            {
                throw new InvalidOperationException();
            }

            if (ImageFileDirectory is null)
            {
                throw new InvalidOperationException();
            }

            TiffImageFileDirectoryEntry entry = ImageFileDirectory.FindEntry(tag);
            if (entry.Tag == TiffTag.None)
            {
                return TiffValueCollection.Empty<byte>();
            }

            return Reader.ReadByteField(entry, sizeLimit);
        }

        #endregion

        #region SByte

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SByte"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<sbyte>> ReadSByteFieldAsync(TiffTag tag, CancellationToken cancellationToken = default)
            => ReadSByteFieldAsync(tag, int.MaxValue, cancellationToken);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SByte"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <param name="sizeLimit">The maximum number of bytes to read from the IFD</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<sbyte>> ReadSByteFieldAsync(TiffTag tag, int sizeLimit, CancellationToken cancellationToken = default)
        {
            if (Reader is null)
            {
                throw new InvalidOperationException();
            }

            if (ImageFileDirectory is null)
            {
                throw new InvalidOperationException();
            }

            TiffImageFileDirectoryEntry entry = ImageFileDirectory.FindEntry(tag);
            if (entry.Tag == TiffTag.None)
            {
                return new ValueTask<TiffValueCollection<sbyte>>(TiffValueCollection.Empty<sbyte>());
            }

            return Reader.ReadSByteFieldAsync(entry, sizeLimit, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SByte"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<sbyte> ReadSByteField(TiffTag tag)
            => ReadSByteField(tag, int.MaxValue);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SByte"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <param name="sizeLimit">The maximum number of bytes to read from the IFD</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<sbyte> ReadSByteField(TiffTag tag, int sizeLimit)
        {
            if (Reader is null)
            {
                throw new InvalidOperationException();
            }

            if (ImageFileDirectory is null)
            {
                throw new InvalidOperationException();
            }

            TiffImageFileDirectoryEntry entry = ImageFileDirectory.FindEntry(tag);
            if (entry.Tag == TiffTag.None)
            {
                return TiffValueCollection.Empty<sbyte>();
            }

            return Reader.ReadSByteField(entry, sizeLimit);
        }

        #endregion

        #region ASCII

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.ASCII"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<string>> ReadASCIIFieldAsync(TiffTag tag, CancellationToken cancellationToken = default)
        {
            if (Reader is null)
            {
                throw new InvalidOperationException();
            }

            if (ImageFileDirectory is null)
            {
                throw new InvalidOperationException();
            }

            TiffImageFileDirectoryEntry entry = ImageFileDirectory.FindEntry(tag);
            if (entry.Tag == TiffTag.None)
            {
                return new ValueTask<TiffValueCollection<string>>(TiffValueCollection.Empty<string>());
            }

            return Reader.ReadASCIIFieldAsync(entry, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.ASCII"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<string> ReadASCIIField(TiffTag tag)
        {
            if (Reader is null)
            {
                throw new InvalidOperationException();
            }

            if (ImageFileDirectory is null)
            {
                throw new InvalidOperationException();
            }

            TiffImageFileDirectoryEntry entry = ImageFileDirectory.FindEntry(tag);
            if (entry.Tag == TiffTag.None)
            {
                return TiffValueCollection.Empty<string>();
            }

            return Reader.ReadASCIIField(entry);
        }

        /// <summary>
        /// Read the first string value of type <see cref="TiffFieldType.ASCII"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <param name="sizeLimit">The maximum number of bytes to read from the IFD</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the string value.</returns>
        public async ValueTask<string?> ReadASCIIFieldFirstStringAsync(TiffTag tag, int sizeLimit = int.MaxValue, CancellationToken cancellationToken = default)
        {
            if (Reader is null)
            {
                throw new InvalidOperationException();
            }

            if (ImageFileDirectory is null)
            {
                throw new InvalidOperationException();
            }

            TiffImageFileDirectoryEntry entry = ImageFileDirectory.FindEntry(tag);
            if (entry.Tag == TiffTag.None)
            {
                return null;
            }

            return await Reader.ReadASCIIFieldFirstStringAsync(entry, sizeLimit, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Read the first string value of type <see cref="TiffFieldType.ASCII"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <param name="sizeLimit">The maximum number of bytes to read from the IFD</param>
        /// <returns>The string value.</returns>
        public string? ReadASCIIFieldFirstString(TiffTag tag, int sizeLimit = -1)
        {
            if (Reader is null)
            {
                throw new InvalidOperationException();
            }

            if (ImageFileDirectory is null)
            {
                throw new InvalidOperationException();
            }

            TiffImageFileDirectoryEntry entry = ImageFileDirectory.FindEntry(tag);
            if (entry.Tag == TiffTag.None)
            {
                return null;
            }

            return Reader.ReadASCIIFieldFirstString(entry, sizeLimit);
        }

        #endregion

        #region Short

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Short"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<ushort>> ReadShortFieldAsync(TiffTag tag, CancellationToken cancellationToken = default)
            => ReadShortFieldAsync(tag, int.MaxValue, cancellationToken);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Short"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<ushort>> ReadShortFieldAsync(TiffTag tag, int sizeLimit, CancellationToken cancellationToken = default)
        {
            if (Reader is null)
            {
                throw new InvalidOperationException();
            }

            if (ImageFileDirectory is null)
            {
                throw new InvalidOperationException();
            }

            TiffImageFileDirectoryEntry entry = ImageFileDirectory.FindEntry(tag);
            if (entry.Tag == TiffTag.None)
            {
                return new ValueTask<TiffValueCollection<ushort>>(TiffValueCollection.Empty<ushort>());
            }

            return Reader.ReadShortFieldAsync(entry, sizeLimit, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Short"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<ushort> ReadShortField(TiffTag tag)
            => ReadShortField(tag, int.MaxValue);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Short"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<ushort> ReadShortField(TiffTag tag, int sizeLimit)
        {
            if (Reader is null)
            {
                throw new InvalidOperationException();
            }

            if (ImageFileDirectory is null)
            {
                throw new InvalidOperationException();
            }

            TiffImageFileDirectoryEntry entry = ImageFileDirectory.FindEntry(tag);
            if (entry.Tag == TiffTag.None)
            {
                return TiffValueCollection.Empty<ushort>();
            }

            return Reader.ReadShortField(entry, sizeLimit);
        }

        #endregion

        #region SShort

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SShort"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<short>> ReadSShortFieldAsync(TiffTag tag, CancellationToken cancellationToken = default)
            => ReadSShortFieldAsync(tag, int.MaxValue, cancellationToken);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SShort"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<short>> ReadSShortFieldAsync(TiffTag tag, int sizeLimit, CancellationToken cancellationToken = default)
        {
            if (Reader is null)
            {
                throw new InvalidOperationException();
            }

            if (ImageFileDirectory is null)
            {
                throw new InvalidOperationException();
            }

            TiffImageFileDirectoryEntry entry = ImageFileDirectory.FindEntry(tag);
            if (entry.Tag == TiffTag.None)
            {
                return new ValueTask<TiffValueCollection<short>>(TiffValueCollection.Empty<short>());
            }

            return Reader.ReadSShortFieldAsync(entry, sizeLimit, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SShort"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<short> ReadSShortField(TiffTag tag)
            => ReadSShortField(tag, int.MaxValue);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SShort"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<short> ReadSShortField(TiffTag tag, int sizeLimit)
        {
            if (Reader is null)
            {
                throw new InvalidOperationException();
            }

            if (ImageFileDirectory is null)
            {
                throw new InvalidOperationException();
            }

            TiffImageFileDirectoryEntry entry = ImageFileDirectory.FindEntry(tag);
            if (entry.Tag == TiffTag.None)
            {
                return TiffValueCollection.Empty<short>();
            }

            return Reader.ReadSShortField(entry, sizeLimit);
        }

        #endregion

        #region Long

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Long"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<uint>> ReadLongFieldAsync(TiffTag tag, CancellationToken cancellationToken = default)
            => ReadLongFieldAsync(tag, int.MaxValue, cancellationToken);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Long"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<uint>> ReadLongFieldAsync(TiffTag tag, int sizeLimit, CancellationToken cancellationToken = default)
        {
            if (Reader is null)
            {
                throw new InvalidOperationException();
            }

            if (ImageFileDirectory is null)
            {
                throw new InvalidOperationException();
            }

            TiffImageFileDirectoryEntry entry = ImageFileDirectory.FindEntry(tag);
            if (entry.Tag == TiffTag.None)
            {
                return new ValueTask<TiffValueCollection<uint>>(TiffValueCollection.Empty<uint>());
            }

            return Reader.ReadLongFieldAsync(entry, sizeLimit, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Long"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<uint> ReadLongField(TiffTag tag)
            => ReadLongField(tag, int.MaxValue);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Long"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<uint> ReadLongField(TiffTag tag, int sizeLimit)
        {
            if (Reader is null)
            {
                throw new InvalidOperationException();
            }

            if (ImageFileDirectory is null)
            {
                throw new InvalidOperationException();
            }

            TiffImageFileDirectoryEntry entry = ImageFileDirectory.FindEntry(tag);
            if (entry.Tag == TiffTag.None)
            {
                return TiffValueCollection.Empty<uint>();
            }

            return Reader.ReadLongField(entry, sizeLimit);
        }

        #endregion

        #region SLong

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SLong"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<int>> ReadSLongFieldAsync(TiffTag tag, CancellationToken cancellationToken = default)
            => ReadSLongFieldAsync(tag, int.MaxValue, cancellationToken);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SLong"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<int>> ReadSLongFieldAsync(TiffTag tag, int sizeLimit, CancellationToken cancellationToken = default)
        {
            if (Reader is null)
            {
                throw new InvalidOperationException();
            }

            if (ImageFileDirectory is null)
            {
                throw new InvalidOperationException();
            }

            TiffImageFileDirectoryEntry entry = ImageFileDirectory.FindEntry(tag);
            if (entry.Tag == TiffTag.None)
            {
                return new ValueTask<TiffValueCollection<int>>(TiffValueCollection.Empty<int>());
            }

            return Reader.ReadSLongFieldAsync(entry, sizeLimit, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SLong"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<int> ReadSLongField(TiffTag tag)
        {
            if (Reader is null)
            {
                throw new InvalidOperationException();
            }

            if (ImageFileDirectory is null)
            {
                throw new InvalidOperationException();
            }

            TiffImageFileDirectoryEntry entry = ImageFileDirectory.FindEntry(tag);
            if (entry.Tag == TiffTag.None)
            {
                return TiffValueCollection.Empty<int>();
            }

            return Reader.ReadSLongField(entry);
        }

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SLong"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<int> ReadSLongField(TiffTag tag, int sizeLimit)
        {
            if (Reader is null)
            {
                throw new InvalidOperationException();
            }

            if (ImageFileDirectory is null)
            {
                throw new InvalidOperationException();
            }

            TiffImageFileDirectoryEntry entry = ImageFileDirectory.FindEntry(tag);
            if (entry.Tag == TiffTag.None)
            {
                return TiffValueCollection.Empty<int>();
            }

            return Reader.ReadSLongField(entry, sizeLimit);
        }

        #endregion

        #region Long8

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Long8"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<ulong>> ReadLong8FieldAsync(TiffTag tag, CancellationToken cancellationToken = default)
            => ReadLong8FieldAsync(tag, int.MaxValue, cancellationToken);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Long8"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<ulong>> ReadLong8FieldAsync(TiffTag tag, int sizeLimit, CancellationToken cancellationToken = default)
        {
            if (Reader is null)
            {
                throw new InvalidOperationException();
            }

            if (ImageFileDirectory is null)
            {
                throw new InvalidOperationException();
            }

            TiffImageFileDirectoryEntry entry = ImageFileDirectory.FindEntry(tag);
            if (entry.Tag == TiffTag.None)
            {
                return new ValueTask<TiffValueCollection<ulong>>(TiffValueCollection.Empty<ulong>());
            }

            return Reader.ReadLong8FieldAsync(entry, sizeLimit, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Long8"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<ulong> ReadLong8Field(TiffTag tag)
            => ReadLong8Field(tag, int.MaxValue);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Long8"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<ulong> ReadLong8Field(TiffTag tag, int sizeLimit)
        {
            if (Reader is null)
            {
                throw new InvalidOperationException();
            }

            if (ImageFileDirectory is null)
            {
                throw new InvalidOperationException();
            }

            TiffImageFileDirectoryEntry entry = ImageFileDirectory.FindEntry(tag);
            if (entry.Tag == TiffTag.None)
            {
                return TiffValueCollection.Empty<ulong>();
            }

            return Reader.ReadLong8Field(entry, sizeLimit);
        }

        #endregion

        #region SLong8

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SLong8"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<long>> ReadSLong8FieldAsync(TiffTag tag, CancellationToken cancellationToken = default)
            => ReadSLong8FieldAsync(tag, int.MaxValue, cancellationToken);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SLong8"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<long>> ReadSLong8FieldAsync(TiffTag tag, int sizeLimit, CancellationToken cancellationToken = default)
        {
            if (Reader is null)
            {
                throw new InvalidOperationException();
            }

            if (ImageFileDirectory is null)
            {
                throw new InvalidOperationException();
            }

            TiffImageFileDirectoryEntry entry = ImageFileDirectory.FindEntry(tag);
            if (entry.Tag == TiffTag.None)
            {
                return new ValueTask<TiffValueCollection<long>>(TiffValueCollection.Empty<long>());
            }

            return Reader.ReadSLong8FieldAsync(entry, sizeLimit, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SLong8"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<long> ReadSLong8Field(TiffTag tag)
            => ReadSLong8Field(tag);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SLong8"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<long> ReadSLong8Field(TiffTag tag, int sizeLimit)
        {
            if (Reader is null)
            {
                throw new InvalidOperationException();
            }

            if (ImageFileDirectory is null)
            {
                throw new InvalidOperationException();
            }

            TiffImageFileDirectoryEntry entry = ImageFileDirectory.FindEntry(tag);
            if (entry.Tag == TiffTag.None)
            {
                return TiffValueCollection.Empty<long>();
            }

            return Reader.ReadSLong8Field(entry, sizeLimit);
        }

        #endregion

        #region Rational

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Rational"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<TiffRational>> ReadRationalFieldAsync(TiffTag tag, CancellationToken cancellationToken = default)
            => ReadRationalFieldAsync(tag, int.MaxValue, cancellationToken);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Rational"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<TiffRational>> ReadRationalFieldAsync(TiffTag tag, int sizeLimit, CancellationToken cancellationToken = default)
        {
            if (Reader is null)
            {
                throw new InvalidOperationException();
            }

            if (ImageFileDirectory is null)
            {
                throw new InvalidOperationException();
            }

            TiffImageFileDirectoryEntry entry = ImageFileDirectory.FindEntry(tag);
            if (entry.Tag == TiffTag.None)
            {
                return new ValueTask<TiffValueCollection<TiffRational>>(TiffValueCollection.Empty<TiffRational>());
            }

            return Reader.ReadRationalFieldAsync(entry, sizeLimit, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Rational"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<TiffRational> ReadRationalField(TiffTag tag)
            => ReadRationalField(tag, int.MaxValue);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Rational"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<TiffRational> ReadRationalField(TiffTag tag, int sizeLimit)
        {
            if (Reader is null)
            {
                throw new InvalidOperationException();
            }

            if (ImageFileDirectory is null)
            {
                throw new InvalidOperationException();
            }

            TiffImageFileDirectoryEntry entry = ImageFileDirectory.FindEntry(tag);
            if (entry.Tag == TiffTag.None)
            {
                return TiffValueCollection.Empty<TiffRational>();
            }

            return Reader.ReadRationalField(entry, sizeLimit);
        }

        #endregion

        #region SRational

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SRational"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<TiffSRational>> ReadSRationalFieldAsync(TiffTag tag, CancellationToken cancellationToken = default)
            => ReadSRationalFieldAsync(tag, int.MaxValue, cancellationToken);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SRational"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<TiffSRational>> ReadSRationalFieldAsync(TiffTag tag, int sizeLimit, CancellationToken cancellationToken = default)
        {
            if (Reader is null)
            {
                throw new InvalidOperationException();
            }

            if (ImageFileDirectory is null)
            {
                throw new InvalidOperationException();
            }

            TiffImageFileDirectoryEntry entry = ImageFileDirectory.FindEntry(tag);
            if (entry.Tag == TiffTag.None)
            {
                return new ValueTask<TiffValueCollection<TiffSRational>>(TiffValueCollection.Empty<TiffSRational>());
            }

            return Reader.ReadSRationalFieldAsync(entry, sizeLimit, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SRational"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<TiffSRational> ReadSRationalField(TiffTag tag)
            => ReadSRationalField(tag, int.MaxValue);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SRational"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<TiffSRational> ReadSRationalField(TiffTag tag, int sizeLimit)
        {
            if (Reader is null)
            {
                throw new InvalidOperationException();
            }

            if (ImageFileDirectory is null)
            {
                throw new InvalidOperationException();
            }

            TiffImageFileDirectoryEntry entry = ImageFileDirectory.FindEntry(tag);
            if (entry.Tag == TiffTag.None)
            {
                return TiffValueCollection.Empty<TiffSRational>();
            }

            return Reader.ReadSRationalField(entry, sizeLimit);
        }

        #endregion

        #region IFD

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.IFD"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<TiffStreamOffset>> ReadIFDFieldAsync(TiffTag tag, CancellationToken cancellationToken = default)
            => ReadIFDFieldAsync(tag, int.MaxValue, cancellationToken);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.IFD"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<TiffStreamOffset>> ReadIFDFieldAsync(TiffTag tag, int sizeLimit, CancellationToken cancellationToken = default)
        {
            if (Reader is null)
            {
                throw new InvalidOperationException();
            }

            if (ImageFileDirectory is null)
            {
                throw new InvalidOperationException();
            }

            TiffImageFileDirectoryEntry entry = ImageFileDirectory.FindEntry(tag);
            if (entry.Tag == TiffTag.None)
            {
                return new ValueTask<TiffValueCollection<TiffStreamOffset>>(TiffValueCollection.Empty<TiffStreamOffset>());
            }

            return Reader.ReadIFDFieldAsync(entry, sizeLimit, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.IFD"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<TiffStreamOffset> ReadIFDField(TiffTag tag)
            => ReadIFDField(tag, int.MaxValue);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.IFD"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<TiffStreamOffset> ReadIFDField(TiffTag tag, int sizeLimit)
        {
            if (Reader is null)
            {
                throw new InvalidOperationException();
            }

            if (ImageFileDirectory is null)
            {
                throw new InvalidOperationException();
            }

            TiffImageFileDirectoryEntry entry = ImageFileDirectory.FindEntry(tag);
            if (entry.Tag == TiffTag.None)
            {
                return TiffValueCollection.Empty<TiffStreamOffset>();
            }

            return Reader.ReadIFDField(entry, sizeLimit);
        }

        #endregion

        #region IFD8

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.IFD8"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<TiffStreamOffset>> ReadIFD8FieldAsync(TiffTag tag, CancellationToken cancellationToken = default)
            => ReadIFD8FieldAsync(tag, cancellationToken);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.IFD8"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<TiffStreamOffset>> ReadIFD8FieldAsync(TiffTag tag, int sizeLimit, CancellationToken cancellationToken = default)
        {
            if (Reader is null)
            {
                throw new InvalidOperationException();
            }

            if (ImageFileDirectory is null)
            {
                throw new InvalidOperationException();
            }

            TiffImageFileDirectoryEntry entry = ImageFileDirectory.FindEntry(tag);
            if (entry.Tag == TiffTag.None)
            {
                return new ValueTask<TiffValueCollection<TiffStreamOffset>>(TiffValueCollection.Empty<TiffStreamOffset>());
            }

            return Reader.ReadIFD8FieldAsync(entry, sizeLimit, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.IFD8"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<TiffStreamOffset> ReadIFD8Field(TiffTag tag)
            => ReadIFD8Field(tag, int.MaxValue);

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.IFD8"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <param name="sizeLimit">The maximum number of values to read.</param>
        /// <returns>The values read.</returns>
        public TiffValueCollection<TiffStreamOffset> ReadIFD8Field(TiffTag tag, int sizeLimit)
        {
            if (Reader is null)
            {
                throw new InvalidOperationException();
            }

            if (ImageFileDirectory is null)
            {
                throw new InvalidOperationException();
            }

            TiffImageFileDirectoryEntry entry = ImageFileDirectory.FindEntry(tag);
            if (entry.Tag == TiffTag.None)
            {
                return TiffValueCollection.Empty<TiffStreamOffset>();
            }

            return Reader.ReadIFD8Field(entry, sizeLimit);
        }

        #endregion
    }

    /// <summary>
    /// Extension methods to read tag values from IFD.
    /// </summary>
    public static partial class TiffTagReaderExtensions { }
}
