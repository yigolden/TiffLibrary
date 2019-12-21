using System.Threading.Tasks;

namespace TiffLibrary
{
    public static partial class TiffTagReaderExtensions
    {
    
        #region DocumentName

        /// <summary>
        /// Read the values of <see cref="TiffTag.DocumentName"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<string?> ReadDocumentNameAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<string>> valueTask = tagReader.ReadASCIIFieldAsync(TiffTag.DocumentName);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<string> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<string?>(result.GetFirstOrDefault());
            }

            return new ValueTask<string?>(TransformValueTaskAsync(valueTask));

            static async Task<string?> TransformValueTaskAsync(ValueTask<TiffValueCollection<string>> valueTask)
            {
                TiffValueCollection<string> result = await valueTask.ConfigureAwait(false);
                return result.GetFirstOrDefault();
            }
        }
        
        /// <summary>
        /// Read the values of <see cref="TiffTag.DocumentName"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static string? ReadDocumentName(this TiffTagReader tagReader)
        {
            TiffValueCollection<string> result = tagReader.ReadASCIIField(TiffTag.DocumentName);
            return result.GetFirstOrDefault();
        }

        #endregion
    
        #region PageName

        /// <summary>
        /// Read the values of <see cref="TiffTag.PageName"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<string?> ReadPageNameAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<string>> valueTask = tagReader.ReadASCIIFieldAsync(TiffTag.PageName);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<string> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<string?>(result.GetFirstOrDefault());
            }

            return new ValueTask<string?>(TransformValueTaskAsync(valueTask));

            static async Task<string?> TransformValueTaskAsync(ValueTask<TiffValueCollection<string>> valueTask)
            {
                TiffValueCollection<string> result = await valueTask.ConfigureAwait(false);
                return result.GetFirstOrDefault();
            }
        }
        
        /// <summary>
        /// Read the values of <see cref="TiffTag.PageName"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static string? ReadPageName(this TiffTagReader tagReader)
        {
            TiffValueCollection<string> result = tagReader.ReadASCIIField(TiffTag.PageName);
            return result.GetFirstOrDefault();
        }

        #endregion
    
        #region PageNumber

        /// <summary>
        /// Read the values of <see cref="TiffTag.PageNumber"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffPageNumber> ReadPageNumberAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync(TiffTag.PageNumber);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<ushort> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffPageNumber>(result.Count == 0 ? default(TiffPageNumber) : (result.Count == 1 ? new TiffPageNumber(result.GetFirstOrDefault(), 0) : new TiffPageNumber(result.GetFirstOrDefault(), result[1])));
            }

            return new ValueTask<TiffPageNumber>(TransformValueTaskAsync(valueTask));

            static async Task<TiffPageNumber> TransformValueTaskAsync(ValueTask<TiffValueCollection<ushort>> valueTask)
            {
                TiffValueCollection<ushort> result = await valueTask.ConfigureAwait(false);
                return result.Count == 0 ? default(TiffPageNumber) : (result.Count == 1 ? new TiffPageNumber(result.GetFirstOrDefault(), 0) : new TiffPageNumber(result.GetFirstOrDefault(), result[1]));
            }
        }
        
        /// <summary>
        /// Read the values of <see cref="TiffTag.PageNumber"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffPageNumber ReadPageNumber(this TiffTagReader tagReader)
        {
            TiffValueCollection<ushort> result = tagReader.ReadShortField(TiffTag.PageNumber);
            return result.Count == 0 ? default(TiffPageNumber) : (result.Count == 1 ? new TiffPageNumber(result.GetFirstOrDefault(), 0) : new TiffPageNumber(result.GetFirstOrDefault(), result[1]));
        }

        #endregion
    
        #region XPosition

        /// <summary>
        /// Read the values of <see cref="TiffTag.XPosition"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffRational?> ReadXPositionAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<TiffRational>> valueTask = tagReader.ReadRationalFieldAsync(TiffTag.XPosition);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<TiffRational> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffRational?>(result.IsEmpty ? default(TiffRational?) : result.GetFirstOrDefault());
            }

            return new ValueTask<TiffRational?>(TransformValueTaskAsync(valueTask));

            static async Task<TiffRational?> TransformValueTaskAsync(ValueTask<TiffValueCollection<TiffRational>> valueTask)
            {
                TiffValueCollection<TiffRational> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? default(TiffRational?) : result.GetFirstOrDefault();
            }
        }
        
        /// <summary>
        /// Read the values of <see cref="TiffTag.XPosition"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffRational? ReadXPosition(this TiffTagReader tagReader)
        {
            TiffValueCollection<TiffRational> result = tagReader.ReadRationalField(TiffTag.XPosition);
            return result.IsEmpty ? default(TiffRational?) : result.GetFirstOrDefault();
        }

        #endregion
    
        #region YPosition

        /// <summary>
        /// Read the values of <see cref="TiffTag.YPosition"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffRational?> ReadYPositionAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<TiffRational>> valueTask = tagReader.ReadRationalFieldAsync(TiffTag.YPosition);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<TiffRational> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffRational?>(result.IsEmpty ? default(TiffRational?) : result.GetFirstOrDefault());
            }

            return new ValueTask<TiffRational?>(TransformValueTaskAsync(valueTask));

            static async Task<TiffRational?> TransformValueTaskAsync(ValueTask<TiffValueCollection<TiffRational>> valueTask)
            {
                TiffValueCollection<TiffRational> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? default(TiffRational?) : result.GetFirstOrDefault();
            }
        }
        
        /// <summary>
        /// Read the values of <see cref="TiffTag.YPosition"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffRational? ReadYPosition(this TiffTagReader tagReader)
        {
            TiffValueCollection<TiffRational> result = tagReader.ReadRationalField(TiffTag.YPosition);
            return result.IsEmpty ? default(TiffRational?) : result.GetFirstOrDefault();
        }

        #endregion
    
    }
}
