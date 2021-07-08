using System;
using System.Threading;
using System.Threading.Tasks;

namespace TiffLibrary
{
    /// <summary>
    /// Provides methods to read bytes from TIFF file source.
    /// </summary>
    public abstract class TiffFileContentReader : IAsyncDisposable, IDisposable
    {
        /// <summary>
        /// Read bytes from TIFF file source.
        /// </summary>
        /// <param name="offset">The offset in the file.</param>
        /// <param name="buffer">The buffer to hold bytes.</param>
        /// <returns>The count of bytes read from file.</returns>
        public virtual int Read(TiffStreamOffset offset, ArraySegment<byte> buffer)
            => Read(offset, buffer.AsMemory());

        /// <summary>
        /// Read bytes from TIFF file source.
        /// </summary>
        /// <param name="offset">The offset in the file.</param>
        /// <param name="buffer">The buffer to hold bytes.</param>
        /// <returns>The count of bytes read from file.</returns>
        public abstract int Read(TiffStreamOffset offset, Memory<byte> buffer);

        /// <summary>
        /// Read bytes from TIFF file source.
        /// </summary>
        /// <param name="offset">The offset in the file.</param>
        /// <param name="buffer">The buffer to hold bytes.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that fires when the users has requested to stop the IO process.</param>
        /// <returns>The count of bytes read from file.</returns>
        public virtual ValueTask<int> ReadAsync(TiffStreamOffset offset, ArraySegment<byte> buffer, CancellationToken cancellationToken = default)
            => ReadAsync(offset, buffer.AsMemory(), cancellationToken);

        /// <summary>
        /// Read bytes from TIFF file source.
        /// </summary>
        /// <param name="offset">The offset in the file.</param>
        /// <param name="buffer">The buffer to hold bytes.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that fires when the users has requested to stop the IO process.</param>
        /// <returns>The count of bytes read from file.</returns>
        public virtual ValueTask<int> ReadAsync(TiffStreamOffset offset, Memory<byte> buffer, CancellationToken cancellationToken = default)
            => new ValueTask<int>(Read(offset, buffer));

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        public virtual ValueTask DisposeAsync()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            return default;
        }

        #region IDisposable Support

        /// <summary>
        /// Dispose this instance.
        /// </summary>
        /// <param name="disposing">Whether to dispose managed resources.</param>
        protected abstract void Dispose(bool disposing);

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

    }
}
