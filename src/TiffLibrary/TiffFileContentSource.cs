using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;
using System.Threading.Tasks;

namespace TiffLibrary
{
    /// <summary>
    /// Provides methods to create <see cref="TiffFileContentReader"/> of TIFF file.
    /// </summary>
    public interface ITiffFileContentSource : IDisposable
    {
        /// <summary>
        /// Opens a <see cref="TiffFileContentReader"/> to read bytes from TIFF file source.
        /// </summary>
        /// <returns>A instance of <see cref="TiffFileContentReader"/>.</returns>
        TiffFileContentReader OpenReader();

        /// <summary>
        /// Opens a <see cref="TiffFileContentReader"/> to read bytes from TIFF file source.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A instance of <see cref="TiffFileContentReader"/>.</returns>
        ValueTask<TiffFileContentReader> OpenReaderAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        ValueTask DisposeAsync();
    }

    /// <summary>
    /// The base class for opening <see cref="TiffFileContentReader"/> of specified TIFF file.
    /// </summary>
    public abstract class TiffFileContentSource : ITiffFileContentSource, IAsyncDisposable
    {
        /// <inheritdoc />
        public abstract TiffFileContentReader OpenReader();

        /// <inheritdoc />
        public virtual ValueTask<TiffFileContentReader> OpenReaderAsync(CancellationToken cancellationToken = default)
            => new ValueTask<TiffFileContentReader>(OpenReader());

        /// <summary>
        /// Create a <see cref="TiffFileContentSource"/> instance from the specified TIFF file name.
        /// </summary>
        /// <param name="fileName">The file name of the TIFF file.</param>
        /// <param name="preferAsync">Whether asynchronous APIs should be preferred.</param>
        /// <returns>A <see cref="TiffFileContentSource"/> that provides bytes from the <see cref="FileStream"/> of this file.</returns>
        public static TiffFileContentSource Create(string fileName, bool preferAsync = true)
        {
            if (fileName is null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            return new TiffFileStreamContentSource(fileName, preferAsync);
        }

        /// <summary>
        /// Wraps <paramref name="stream"/> as <see cref="TiffFileContentSource"/>. <see cref="TiffFileReader"/> created from this instance must not be accessed concurrently.
        /// </summary>
        /// <param name="stream">The stream to wrap.</param>
        /// <param name="leaveOpen">True to dispose the stream when <see cref="TiffFileContentSource"/> instance is disposed; otherwise, false.</param>
        /// <returns>A <see cref="TiffFileContentSource"/> that provides bytes from the <see cref="Stream"/> instance specified.</returns>
        public static TiffFileContentSource Create(Stream stream, bool leaveOpen)
        {
            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            return new TiffStreamContentSource(stream, leaveOpen);
        }

        /// <summary>
        /// Create a <see cref="TiffFileContentSource"/> instance from the specified buffer.
        /// </summary>
        /// <param name="memory">The buffer of the TIFF file.</param>
        /// <returns>A <see cref="TiffFileContentSource"/> that provides bytes from the buffer.</returns>
        public static TiffFileContentSource Create(ReadOnlyMemory<byte> memory)
        {
            return new TiffMemoryContentSource(memory);
        }

        /// <summary>
        /// Create a <see cref="TiffFileContentSource"/> instance from the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer of the TIFF file.</param>
        /// <param name="offset">The offset of the buffer.</param>
        /// <param name="count">The number of bytes in the buffer.</param>
        /// <returns>A <see cref="TiffFileContentSource"/> that provides bytes from the buffer.</returns>
        public static TiffFileContentSource Create(byte[] buffer, int offset, int count)
        {
            if (buffer is null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }
            if ((uint)offset >= (uint)buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(buffer));
            }
            if ((uint)(offset + count) > (uint)buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            return new TiffMemoryContentSource(buffer.AsMemory(offset, count));
        }

        /// <summary>
        /// Wraps <paramref name="memoryMappedFile"/> as <see cref="TiffFileContentSource"/>.
        /// </summary>
        /// <param name="memoryMappedFile">The memory-mapped file to wrap.</param>
        /// <param name="leaveOpen">True to dispose the memory-mapped file when <see cref="TiffFileContentSource"/> instance is disposed; otherwise, false.</param>
        /// <returns>A <see cref="TiffFileContentSource"/> that provides bytes from the <see cref="MemoryMappedFile"/> instance specified.</returns>
        public static TiffFileContentSource Create(MemoryMappedFile memoryMappedFile, bool leaveOpen)
        {
            if (memoryMappedFile is null)
            {
                throw new ArgumentNullException(nameof(memoryMappedFile));
            }

            return new TiffMemoryMappedFileContentSource(memoryMappedFile, leaveOpen);
        }

        #region IDisposable Support

        /// <inheritdoc />
        protected abstract void Dispose(bool disposing);

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public virtual ValueTask DisposeAsync()
        {
            Dispose();
            return default;
        }

        #endregion
    }
}
