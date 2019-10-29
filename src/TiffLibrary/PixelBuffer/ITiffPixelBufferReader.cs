using System.Threading;
using System.Threading.Tasks;

namespace TiffLibrary
{
    /// <summary>
    /// Represents a reader object capable of copying 2-dimensional pixel data from its storage into a specified <see cref="TiffPixelBufferWriter{TPixel}"/>.
    /// </summary>
    /// <typeparam name="TPixel">The pixel type.</typeparam>
    public interface ITiffPixelBufferReader<TPixel> where TPixel : unmanaged
    {
        /// <summary>
        /// The number of columns in the region the reader object provides.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// The number of rows in the region the reader object provides.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Copy the 2-dimensional pixel data into <paramref name="destination"/>, after skipping some rows and columns specified in <paramref name="offset"/>.
        /// </summary>
        /// <param name="offset">The number rows and columns to skip. X represents the number of columns to skip; Y represents the number of rows to skip.</param>
        /// <param name="destination">The destination writer. It also limits the number of rows and columns to copy.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask"/> that completes when all the requested pixels are copied.</returns>
        ValueTask ReadAsync(TiffPoint offset, TiffPixelBufferWriter<TPixel> destination, CancellationToken cancellationToken);
    }
}
