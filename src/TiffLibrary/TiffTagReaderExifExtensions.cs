using System.Threading;
using System.Threading.Tasks;

namespace TiffLibrary
{
    public static partial class TiffTagReaderExtensions
    {
    
        #region ExifIfd

        /// <summary>
        /// Read the values of <see cref="TiffTag.ExifIfd"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffStreamOffset> ReadExifIfdAsync(this TiffTagReader tagReader, CancellationToken cancellationToken = default)
        {
            ValueTask<TiffValueCollection<TiffStreamOffset>> valueTask = tagReader.ReadIFD8FieldAsync(TiffTag.ExifIfd, cancellationToken);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<TiffStreamOffset> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffStreamOffset>(result.GetFirstOrDefault());
            }

            return new ValueTask<TiffStreamOffset>(TransformValueTaskAsync(valueTask));

            static async Task<TiffStreamOffset> TransformValueTaskAsync(ValueTask<TiffValueCollection<TiffStreamOffset>> valueTask)
            {
                TiffValueCollection<TiffStreamOffset> result = await valueTask.ConfigureAwait(false);
                return result.GetFirstOrDefault();
            }
        }
        
        /// <summary>
        /// Read the values of <see cref="TiffTag.ExifIfd"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffStreamOffset ReadExifIfd(this TiffTagReader tagReader)
        {
            TiffValueCollection<TiffStreamOffset> result = tagReader.ReadIFD8Field(TiffTag.ExifIfd);
            return result.GetFirstOrDefault();
        }

        #endregion
    
        #region GpsIfd

        /// <summary>
        /// Read the values of <see cref="TiffTag.GpsIfd"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffStreamOffset> ReadGpsIfdAsync(this TiffTagReader tagReader, CancellationToken cancellationToken = default)
        {
            ValueTask<TiffValueCollection<TiffStreamOffset>> valueTask = tagReader.ReadIFD8FieldAsync(TiffTag.GpsIfd, cancellationToken);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<TiffStreamOffset> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffStreamOffset>(result.GetFirstOrDefault());
            }

            return new ValueTask<TiffStreamOffset>(TransformValueTaskAsync(valueTask));

            static async Task<TiffStreamOffset> TransformValueTaskAsync(ValueTask<TiffValueCollection<TiffStreamOffset>> valueTask)
            {
                TiffValueCollection<TiffStreamOffset> result = await valueTask.ConfigureAwait(false);
                return result.GetFirstOrDefault();
            }
        }
        
        /// <summary>
        /// Read the values of <see cref="TiffTag.GpsIfd"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffStreamOffset ReadGpsIfd(this TiffTagReader tagReader)
        {
            TiffValueCollection<TiffStreamOffset> result = tagReader.ReadIFD8Field(TiffTag.GpsIfd);
            return result.GetFirstOrDefault();
        }

        #endregion
    
        #region InteroperabilityIfd

        /// <summary>
        /// Read the values of <see cref="TiffTag.InteroperabilityIfd"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffStreamOffset> ReadInteroperabilityIfdAsync(this TiffTagReader tagReader, CancellationToken cancellationToken = default)
        {
            ValueTask<TiffValueCollection<TiffStreamOffset>> valueTask = tagReader.ReadIFD8FieldAsync(TiffTag.InteroperabilityIfd, cancellationToken);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<TiffStreamOffset> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffStreamOffset>(result.GetFirstOrDefault());
            }

            return new ValueTask<TiffStreamOffset>(TransformValueTaskAsync(valueTask));

            static async Task<TiffStreamOffset> TransformValueTaskAsync(ValueTask<TiffValueCollection<TiffStreamOffset>> valueTask)
            {
                TiffValueCollection<TiffStreamOffset> result = await valueTask.ConfigureAwait(false);
                return result.GetFirstOrDefault();
            }
        }
        
        /// <summary>
        /// Read the values of <see cref="TiffTag.InteroperabilityIfd"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffStreamOffset ReadInteroperabilityIfd(this TiffTagReader tagReader)
        {
            TiffValueCollection<TiffStreamOffset> result = tagReader.ReadIFD8Field(TiffTag.InteroperabilityIfd);
            return result.GetFirstOrDefault();
        }

        #endregion
    
    }
}
