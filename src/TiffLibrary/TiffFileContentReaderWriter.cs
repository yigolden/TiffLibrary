using System;
using System.Threading;
using System.Threading.Tasks;

namespace TiffLibrary
{
    /// <summary>
    /// Provides methods to read bytes from TIFF file and write bytes into TIFF file stream.
    /// </summary>
    public abstract class TiffFileContentReaderWriter : TiffFileContentReader
    {
        /// <summary>
        /// Write bytes into TIFF file stream.
        /// </summary>
        /// <param name="offset">The offset in the stream.</param>
        /// <param name="buffer">The buffer to write.</param>
        public virtual void Write(TiffStreamOffset offset, ArraySegment<byte> buffer)
            => Write(offset, buffer.AsMemory());

        /// <summary>
        /// Write bytes into TIFF file stream.
        /// </summary>
        /// <param name="offset">The offset in the stream.</param>
        /// <param name="buffer">The buffer to write.</param>
        public abstract void Write(TiffStreamOffset offset, ReadOnlyMemory<byte> buffer);

        /// <summary>
        /// Write bytes into TIFF file stream.
        /// </summary>
        /// <param name="offset">The offset in the stream.</param>
        /// <param name="buffer">The buffer to write.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that fires when the users has requested to stop the IO process.</param>
        public virtual ValueTask WriteAsync(TiffStreamOffset offset, ArraySegment<byte> buffer, CancellationToken cancellationToken = default)
            => WriteAsync(offset, buffer.AsMemory(), cancellationToken);

        /// <summary>
        /// Clears all buffers for this writer and causes any buffered data to be written to the underlying device.
        /// </summary>
        public abstract void Flush();

        /// <summary>
        /// Clears all buffers for this writer and causes any buffered data to be written to the underlying device.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that fires when the users has requested to stop the IO process.</param>
        public virtual ValueTask FlushAsync(CancellationToken cancellationToken = default)
        {
            Flush();
            return default;
        }

        /// <summary>
        /// Write bytes into TIFF file stream.
        /// </summary>
        /// <param name="offset">The offset in the stream.</param>
        /// <param name="buffer">The buffer to write.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that fires when the users has requested to stop the IO process.</param>
        public virtual ValueTask WriteAsync(TiffStreamOffset offset, ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            Write(offset, buffer);
            return default;
        }
    }
}
