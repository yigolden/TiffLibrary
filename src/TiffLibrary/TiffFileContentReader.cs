using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace TiffLibrary
{
    /// <summary>
    /// Provides methods to read bytes from TIFF file source.
    /// </summary>
    public abstract class TiffFileContentReader : IDisposable
    {
        /// <summary>
        /// Read bytes from TIFF file source.
        /// </summary>
        /// <param name="offset">The offset in the file.</param>
        /// <param name="buffer">The buffer to hold bytes.</param>
        /// <returns>The count of bytes read from file.</returns>
        public abstract ValueTask<int> ReadAsync(long offset, ArraySegment<byte> buffer);

        /// <summary>
        /// Read bytes from TIFF file source.
        /// </summary>
        /// <param name="offset">The offset in the file.</param>
        /// <param name="buffer">The buffer to hold bytes.</param>
        /// <returns>The count of bytes read from file.</returns>
        public abstract ValueTask<int> ReadAsync(long offset, Memory<byte> buffer);

        /// <summary>
        /// Dispose this instance.
        /// </summary>
        /// <returns>A <see cref="ValueTask"/> that completes when the instance is disposed.</returns>
        public virtual ValueTask DisposeAsync()
        {
            Dispose(true);
#pragma warning disable CA1816 // CA1816: Call GC.SuppressFinalize correctly
            GC.SuppressFinalize(this);
#pragma warning restore CA1816
            return default;
        }

        #region IDisposable Support

        /// <summary>
        /// Dispose this instance.
        /// </summary>
        /// <param name="disposing">Whether to dispose managed resources.</param>
        protected abstract void Dispose(bool disposing);

        /// <summary>
        /// Dispose this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

    }
}
