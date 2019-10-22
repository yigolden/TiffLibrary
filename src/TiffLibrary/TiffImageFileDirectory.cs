using System;
using System.Collections;
using System.Collections.Generic;

namespace TiffLibrary
{

#pragma warning disable CA1710 // CA1710: Identifiers should have correct suffix
    /// <summary>
    /// A image file directory in the TIFF file.
    /// </summary>
    public sealed class TiffImageFileDirectory : IReadOnlyList<TiffImageFileDirectoryEntry>
#pragma warning restore CA1710 // CA1710: Identifiers should have correct suffix
    {
        private TiffImageFileDirectoryEntry[] _entries;
        internal long _nextOffset;

        internal TiffImageFileDirectoryEntry[] Entries => _entries;

        /// <summary>
        /// The offset of the next IFD.
        /// </summary>
        public TiffStreamOffset NextOffset => new TiffStreamOffset(_nextOffset);

        internal TiffImageFileDirectory(int entryCount)
        {
            _entries = new TiffImageFileDirectoryEntry[entryCount];
        }

        #region Collection Support

        /// <summary>
        /// The the IFD entry of the specified index.
        /// </summary>
        /// <param name="index">The IFD entry index.</param>
        /// <returns>The IFD Entry</returns>
        public TiffImageFileDirectoryEntry this[int index] => _entries[index];

        /// <summary>
        /// The number of entries in this IFD.
        /// </summary>
        public int Count => _entries.Length;

        /// <summary>
        /// Gets a enumerator of IFD entries.
        /// </summary>
        /// <returns>A enumerator of IFD entries.</returns>
        public IEnumerator<TiffImageFileDirectoryEntry> GetEnumerator() => ((IEnumerable<TiffImageFileDirectoryEntry>)_entries).GetEnumerator();

        /// <summary>
        /// Gets a enumerator of IFD entries.
        /// </summary>
        /// <returns>A enumerator of IFD entries.</returns>
        IEnumerator IEnumerable.GetEnumerator() => _entries.GetEnumerator();

        #endregion

        #region Contains & FindEntry

        /// <summary>
        /// Check whether entries of this IFD contains the specified tag.
        /// </summary>
        /// <param name="tag">The specified tag.</param>
        /// <returns>True is entries of this IFD contains the specified tag; otherwise, false.</returns>
        public bool Contains(TiffTag tag)
        {
            foreach (TiffImageFileDirectoryEntry entry in _entries)
            {
                if (entry.Tag == tag)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Find the entry of the specified tag in the entries.
        /// </summary>
        /// <param name="tag">The specified tag.</param>
        /// <returns>The entry of the specified tag. Returns default(TiffImageFileDirectoryEntry) is the entry is not found.</returns>
        public TiffImageFileDirectoryEntry FindEntry(TiffTag tag)
        {
            foreach (TiffImageFileDirectoryEntry entry in _entries)
            {
                if (entry.Tag == tag)
                {
                    return entry;
                }
            }

            return default;
        }

        /// <summary>
        /// Find the first entry that satisfied the specified delegate.
        /// </summary>
        /// <param name="predicate">A delegate to check the each entry.</param>
        /// <returns>The entry found. Returns default(TiffImageFileDirectoryEntry) is the entry is not found.</returns>
        public TiffImageFileDirectoryEntry FindEntry(Func<TiffImageFileDirectoryEntry, bool> predicate)
        {
            foreach (TiffImageFileDirectoryEntry entry in _entries)
            {
                if (predicate(entry))
                {
                    return entry;
                }
            }

            return default;
        }
        #endregion
    }
}
