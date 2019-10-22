using System.Threading.Tasks;

namespace TiffLibrary
{
    public static partial class TiffTagReaderExtensions
    {
    
        #region InkSet

        /// <summary>
        /// Read the values of <see cref="TiffTag.InkSet"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffInkSet> ReadInkSetAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync(TiffTag.InkSet);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<ushort> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffInkSet>(result.IsEmpty ? TiffInkSet.CMYK : (TiffInkSet)result.FirstOrDefault);
            }

            return new ValueTask<TiffInkSet>(TransformValueTaskAsync(valueTask));

            static async Task<TiffInkSet> TransformValueTaskAsync(ValueTask<TiffValueCollection<ushort>> valueTask)
            {
                TiffValueCollection<ushort> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? TiffInkSet.CMYK : (TiffInkSet)result.FirstOrDefault;
            }
        }

        #endregion
    
        #region NumberOfInks

        /// <summary>
        /// Read the values of <see cref="TiffTag.NumberOfInks"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<ushort> ReadNumberOfInksAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync(TiffTag.NumberOfInks);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<ushort> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<ushort>(result.IsEmpty ? (ushort)4 : result.FirstOrDefault);
            }

            return new ValueTask<ushort>(TransformValueTaskAsync(valueTask));

            static async Task<ushort> TransformValueTaskAsync(ValueTask<TiffValueCollection<ushort>> valueTask)
            {
                TiffValueCollection<ushort> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? (ushort)4 : result.FirstOrDefault;
            }
        }

        #endregion
    
        #region InkNames

        /// <summary>
        /// Read the values of <see cref="TiffTag.InkNames"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<string[]> ReadInkNamesAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<string>> valueTask = tagReader.ReadASCIIFieldAsync(TiffTag.InkNames);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<string> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<string[]>(result.GetOrCreateArray());
            }

            return new ValueTask<string[]>(TransformValueTaskAsync(valueTask));

            static async Task<string[]> TransformValueTaskAsync(ValueTask<TiffValueCollection<string>> valueTask)
            {
                TiffValueCollection<string> result = await valueTask.ConfigureAwait(false);
                return result.GetOrCreateArray();
            }
        }

        #endregion
    
        #region DotRange

        /// <summary>
        /// Read the values of <see cref="TiffTag.DotRange"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<ushort[]> ReadDotRangeAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync(TiffTag.DotRange);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<ushort> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<ushort[]>(result.GetOrCreateArray());
            }

            return new ValueTask<ushort[]>(TransformValueTaskAsync(valueTask));

            static async Task<ushort[]> TransformValueTaskAsync(ValueTask<TiffValueCollection<ushort>> valueTask)
            {
                TiffValueCollection<ushort> result = await valueTask.ConfigureAwait(false);
                return result.GetOrCreateArray();
            }
        }

        #endregion
    
        #region TargetPrinter

        /// <summary>
        /// Read the values of <see cref="TiffTag.TargetPrinter"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffValueCollection<ushort>> ReadTargetPrinterAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync(TiffTag.TargetPrinter);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<ushort> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffValueCollection<ushort>>(result);
            }

            return new ValueTask<TiffValueCollection<ushort>>(TransformValueTaskAsync(valueTask));

            static async Task<TiffValueCollection<ushort>> TransformValueTaskAsync(ValueTask<TiffValueCollection<ushort>> valueTask)
            {
                TiffValueCollection<ushort> result = await valueTask.ConfigureAwait(false);
                return result;
            }
        }

        #endregion
    
    }
}
