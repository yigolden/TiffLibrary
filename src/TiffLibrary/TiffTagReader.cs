using System;
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

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Byte"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<byte>> ReadByteFieldAsync(TiffTag tag)
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
                return new ValueTask<TiffValueCollection<byte>>(TiffValueCollection<byte>.Empty);
            }

            return Reader.ReadByteFieldAsync(entry);
        }

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SByte"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<sbyte>> ReadSByteFieldAsync(TiffTag tag)
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
                return new ValueTask<TiffValueCollection<sbyte>>(TiffValueCollection<sbyte>.Empty);
            }

            return Reader.ReadSByteFieldAsync(entry);
        }

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.ASCII"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<string>> ReadASCIIFieldAsync(TiffTag tag)
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
                return new ValueTask<TiffValueCollection<string>>(TiffValueCollection<string>.Empty);
            }

            return Reader.ReadASCIIFieldAsync(entry);
        }

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Short"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<ushort>> ReadShortFieldAsync(TiffTag tag)
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
                return new ValueTask<TiffValueCollection<ushort>>(TiffValueCollection<ushort>.Empty);
            }

            return Reader.ReadShortFieldAsync(entry);
        }

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SShort"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<short>> ReadSShortFieldAsync(TiffTag tag)
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
                return new ValueTask<TiffValueCollection<short>>(TiffValueCollection<short>.Empty);
            }

            return Reader.ReadSShortFieldAsync(entry);
        }

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Long"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<uint>> ReadLongFieldAsync(TiffTag tag)
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
                return new ValueTask<TiffValueCollection<uint>>(TiffValueCollection<uint>.Empty);
            }

            return Reader.ReadLongFieldAsync(entry);
        }

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SLong"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<int>> ReadSLongFieldAsync(TiffTag tag)
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
                return new ValueTask<TiffValueCollection<int>>(TiffValueCollection<int>.Empty);
            }

            return Reader.ReadSLongFieldAsync(entry);
        }

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Long8"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<ulong>> ReadLong8FieldAsync(TiffTag tag)
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
                return new ValueTask<TiffValueCollection<ulong>>(TiffValueCollection<ulong>.Empty);
            }

            return Reader.ReadLong8FieldAsync(entry);
        }

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SLong8"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<long>> ReadSLong8FieldAsync(TiffTag tag)
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
                return new ValueTask<TiffValueCollection<long>>(TiffValueCollection<long>.Empty);
            }

            return Reader.ReadSLong8FieldAsync(entry);
        }

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.Rational"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<TiffRational>> ReadRationalFieldAsync(TiffTag tag)
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
                return new ValueTask<TiffValueCollection<TiffRational>>(TiffValueCollection<TiffRational>.Empty);
            }

            return Reader.ReadRationalFieldAsync(entry);
        }

        /// <summary>
        /// Read values of type <see cref="TiffFieldType.SRational"/> from the specified tag.
        /// </summary>
        /// <param name="tag">The tag to read.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the values are read and return the read values.</returns>
        public ValueTask<TiffValueCollection<TiffSRational>> ReadSRationalFieldAsync(TiffTag tag)
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
                return new ValueTask<TiffValueCollection<TiffSRational>>(TiffValueCollection<TiffSRational>.Empty);
            }

            return Reader.ReadSRationalFieldAsync(entry);
        }
    }

    /// <summary>
    /// Extension methods to read tag values from IFD.
    /// </summary>
    public static partial class TiffTagReaderExtensions { }
}
