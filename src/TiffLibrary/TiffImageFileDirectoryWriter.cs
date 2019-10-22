using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace TiffLibrary
{
    /// <summary>
    /// A writer class that write IFD entries into TIFF stream.
    /// </summary>
    public sealed partial class TiffImageFileDirectoryWriter : IDisposable
    {
        private TiffFileWriter _writer;
        private List<TiffImageFileDirectoryEntry> _entries;

        /// <summary>
        /// Gets the TIFF file writer.
        /// </summary>
        public TiffFileWriter FileWriter => _writer;

        internal TiffImageFileDirectoryWriter(TiffFileWriter writer)
        {
            _writer = writer;
            _entries = new List<TiffImageFileDirectoryEntry>();
        }

        /// <summary>
        /// Writes the IFD into the TIFF stream.
        /// </summary>
        /// <returns>The offset of the IFD in the stream.</returns>
        public async Task<TiffStreamOffset> FlushAsync()
        {
            EnsureNotDisposed();

            await _writer.AlignToWordBoundaryAsync().ConfigureAwait(false);
            TiffStreamOffset position = _writer.Position;

            await WriteEntries().ConfigureAwait(false);

            return position;
        }

        /// <summary>
        /// Writes the IFD into the TIFF stream. Update the specified IFD to point its "Next IFD Offset" field to the IFD just written.
        /// </summary>
        /// <param name="previousIfdOffset">The specified IFD to update.</param>
        /// <returns>The offset of the IFD in the stream.</returns>
        public async Task<TiffStreamOffset> FlushAsync(TiffStreamOffset previousIfdOffset)
        {
            EnsureNotDisposed();

            await _writer.AlignToWordBoundaryAsync().ConfigureAwait(false);
            TiffStreamOffset position = _writer.Position;

            await WriteEntries().ConfigureAwait(false);

            if (previousIfdOffset.IsZero)
            {
                _writer.SetFirstImageFileDirectoryOffset(position);
            }
            else
            {
                await _writer.UpdateImageFileDirectoryNextOffsetFieldAsync(previousIfdOffset, position).ConfigureAwait(false);
            }

            return position;
        }

        private async Task WriteEntries()
        {
            _entries.Sort(TiffImageFileDirectoryEntryComparer.Instance);

            var buffer = _writer.InternalBuffer;
            Stream stream = _writer.InnerStream;

            if (_writer.UseBigTiff)
            {
                Unsafe.WriteUnaligned(ref buffer[0], (long)(uint)_entries.Count);
            }
            else
            {
                Unsafe.WriteUnaligned(ref buffer[0], checked((ushort)_entries.Count));
            }
            await stream.WriteAsync(buffer, 0, _writer.OperationContext.ByteCountOfImageFileDirectoryCountField).ConfigureAwait(false);
            _writer.AdvancePosition(_writer.OperationContext.ByteCountOfImageFileDirectoryCountField);

            foreach (TiffImageFileDirectoryEntry entry in _entries)
            {
                int bytesWritten = entry.Write(_writer.OperationContext, buffer);
                await stream.WriteAsync(buffer, 0, bytesWritten).ConfigureAwait(false);
                _writer.AdvancePosition(bytesWritten);
            }

            Unsafe.WriteUnaligned(ref buffer[0], (long)0);

            await stream.WriteAsync(buffer, 0, _writer.OperationContext.ByteCountOfValueOffsetField).ConfigureAwait(false);
            _writer.AdvancePosition(_writer.OperationContext.ByteCountOfValueOffsetField);
        }


        #region Disposiable support

        private void EnsureNotDisposed()
        {
            if (_writer is null)
            {
                ThrowObjectDisposedException();
            }
        }

        private static void ThrowObjectDisposedException()
        {
            throw new ObjectDisposedException(nameof(TiffImageFileDirectoryWriter));
        }

        /// <summary>
        /// Dispose this instance.
        /// </summary>
        public void Dispose()
        {
            _writer = null;
        }

        #endregion
    }
}
