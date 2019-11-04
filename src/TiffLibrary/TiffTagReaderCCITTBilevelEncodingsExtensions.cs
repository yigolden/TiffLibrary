using System.Threading.Tasks;

namespace TiffLibrary
{
    public static partial class TiffTagReaderExtensions
    {
    
        #region T4Options

        /// <summary>
        /// Read the values of <see cref="TiffTag.T4Options"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffT4Options> ReadT4OptionsAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<uint>> valueTask = tagReader.ReadLongFieldAsync(TiffTag.T4Options);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<uint> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffT4Options>(result.IsEmpty ? TiffT4Options.None : (TiffT4Options)result.FirstOrDefault);
            }

            return new ValueTask<TiffT4Options>(TransformValueTaskAsync(valueTask));

            static async Task<TiffT4Options> TransformValueTaskAsync(ValueTask<TiffValueCollection<uint>> valueTask)
            {
                TiffValueCollection<uint> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? TiffT4Options.None : (TiffT4Options)result.FirstOrDefault;
            }
        }
        
        /// <summary>
        /// Read the values of <see cref="TiffTag.T4Options"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffT4Options ReadT4Options(this TiffTagReader tagReader)
        {
            TiffValueCollection<uint> result = tagReader.ReadLongField(TiffTag.T4Options);
            return result.IsEmpty ? TiffT4Options.None : (TiffT4Options)result.FirstOrDefault;
        }

        #endregion
    
        #region T6Options

        /// <summary>
        /// Read the values of <see cref="TiffTag.T6Options"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffT6Options> ReadT6OptionsAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<uint>> valueTask = tagReader.ReadLongFieldAsync(TiffTag.T6Options);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<uint> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffT6Options>(result.IsEmpty ? TiffT6Options.None : (TiffT6Options)result.FirstOrDefault);
            }

            return new ValueTask<TiffT6Options>(TransformValueTaskAsync(valueTask));

            static async Task<TiffT6Options> TransformValueTaskAsync(ValueTask<TiffValueCollection<uint>> valueTask)
            {
                TiffValueCollection<uint> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? TiffT6Options.None : (TiffT6Options)result.FirstOrDefault;
            }
        }
        
        /// <summary>
        /// Read the values of <see cref="TiffTag.T6Options"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffT6Options ReadT6Options(this TiffTagReader tagReader)
        {
            TiffValueCollection<uint> result = tagReader.ReadLongField(TiffTag.T6Options);
            return result.IsEmpty ? TiffT6Options.None : (TiffT6Options)result.FirstOrDefault;
        }

        #endregion
    
    }
}
