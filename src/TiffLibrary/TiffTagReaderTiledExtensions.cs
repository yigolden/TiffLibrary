using System.Threading;
using System.Threading.Tasks;

namespace TiffLibrary
{
    public static partial class TiffTagReaderExtensions
    {
    
        #region TileWidth

        /// <summary>
        /// Read the values of <see cref="TiffTag.TileWidth"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<uint?> ReadTileWidthAsync(this TiffTagReader tagReader, CancellationToken cancellationToken = default)
        {
            ValueTask<TiffValueCollection<uint>> valueTask = tagReader.ReadLongFieldAsync(TiffTag.TileWidth, cancellationToken);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<uint> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<uint?>(result.IsEmpty ? default(uint?) : result.GetFirstOrDefault());
            }

            return new ValueTask<uint?>(TransformValueTaskAsync(valueTask));

            static async Task<uint?> TransformValueTaskAsync(ValueTask<TiffValueCollection<uint>> valueTask)
            {
                TiffValueCollection<uint> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? default(uint?) : result.GetFirstOrDefault();
            }
        }
        
        /// <summary>
        /// Read the values of <see cref="TiffTag.TileWidth"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static uint? ReadTileWidth(this TiffTagReader tagReader)
        {
            TiffValueCollection<uint> result = tagReader.ReadLongField(TiffTag.TileWidth);
            return result.IsEmpty ? default(uint?) : result.GetFirstOrDefault();
        }

        #endregion
    
        #region TileLength

        /// <summary>
        /// Read the values of <see cref="TiffTag.TileLength"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<uint?> ReadTileLengthAsync(this TiffTagReader tagReader, CancellationToken cancellationToken = default)
        {
            ValueTask<TiffValueCollection<uint>> valueTask = tagReader.ReadLongFieldAsync(TiffTag.TileLength, cancellationToken);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<uint> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<uint?>(result.IsEmpty ? default(uint?) : result.GetFirstOrDefault());
            }

            return new ValueTask<uint?>(TransformValueTaskAsync(valueTask));

            static async Task<uint?> TransformValueTaskAsync(ValueTask<TiffValueCollection<uint>> valueTask)
            {
                TiffValueCollection<uint> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? default(uint?) : result.GetFirstOrDefault();
            }
        }
        
        /// <summary>
        /// Read the values of <see cref="TiffTag.TileLength"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static uint? ReadTileLength(this TiffTagReader tagReader)
        {
            TiffValueCollection<uint> result = tagReader.ReadLongField(TiffTag.TileLength);
            return result.IsEmpty ? default(uint?) : result.GetFirstOrDefault();
        }

        #endregion
    
        #region TileOffsets

        /// <summary>
        /// Read the values of <see cref="TiffTag.TileOffsets"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffValueCollection<ulong>> ReadTileOffsetsAsync(this TiffTagReader tagReader, CancellationToken cancellationToken = default)
        {
            ValueTask<TiffValueCollection<ulong>> valueTask = tagReader.ReadLong8FieldAsync(TiffTag.TileOffsets, cancellationToken);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<ulong> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffValueCollection<ulong>>(result);
            }

            return new ValueTask<TiffValueCollection<ulong>>(TransformValueTaskAsync(valueTask));

            static async Task<TiffValueCollection<ulong>> TransformValueTaskAsync(ValueTask<TiffValueCollection<ulong>> valueTask)
            {
                TiffValueCollection<ulong> result = await valueTask.ConfigureAwait(false);
                return result;
            }
        }
        
        /// <summary>
        /// Read the values of <see cref="TiffTag.TileOffsets"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffValueCollection<ulong> ReadTileOffsets(this TiffTagReader tagReader)
        {
            TiffValueCollection<ulong> result = tagReader.ReadLong8Field(TiffTag.TileOffsets);
            return result;
        }

        #endregion
    
        #region TileByteCounts

        /// <summary>
        /// Read the values of <see cref="TiffTag.TileByteCounts"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffValueCollection<ulong>> ReadTileByteCountsAsync(this TiffTagReader tagReader, CancellationToken cancellationToken = default)
        {
            ValueTask<TiffValueCollection<ulong>> valueTask = tagReader.ReadLong8FieldAsync(TiffTag.TileByteCounts, cancellationToken);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<ulong> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffValueCollection<ulong>>(result);
            }

            return new ValueTask<TiffValueCollection<ulong>>(TransformValueTaskAsync(valueTask));

            static async Task<TiffValueCollection<ulong>> TransformValueTaskAsync(ValueTask<TiffValueCollection<ulong>> valueTask)
            {
                TiffValueCollection<ulong> result = await valueTask.ConfigureAwait(false);
                return result;
            }
        }
        
        /// <summary>
        /// Read the values of <see cref="TiffTag.TileByteCounts"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffValueCollection<ulong> ReadTileByteCounts(this TiffTagReader tagReader)
        {
            TiffValueCollection<ulong> result = tagReader.ReadLong8Field(TiffTag.TileByteCounts);
            return result;
        }

        #endregion
    
    }
}
