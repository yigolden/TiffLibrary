using System.Threading.Tasks;

namespace TiffLibrary
{
    public static partial class TiffTagReaderExtensions
    {
    
        #region WhitePoint

        /// <summary>
        /// Read the values of <see cref="TiffTag.WhitePoint"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffRational[]> ReadWhitePointAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<TiffRational>> valueTask = tagReader.ReadRationalFieldAsync(TiffTag.WhitePoint);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<TiffRational> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffRational[]>(result.GetOrCreateArray());
            }

            return new ValueTask<TiffRational[]>(TransformValueTaskAsync(valueTask));

            static async Task<TiffRational[]> TransformValueTaskAsync(ValueTask<TiffValueCollection<TiffRational>> valueTask)
            {
                TiffValueCollection<TiffRational> result = await valueTask.ConfigureAwait(false);
                return result.GetOrCreateArray();
            }
        }

        #endregion
    
        #region PrimaryChromaticities

        /// <summary>
        /// Read the values of <see cref="TiffTag.PrimaryChromaticities"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffRational[]> ReadPrimaryChromaticitiesAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<TiffRational>> valueTask = tagReader.ReadRationalFieldAsync(TiffTag.PrimaryChromaticities);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<TiffRational> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffRational[]>(result.GetOrCreateArray());
            }

            return new ValueTask<TiffRational[]>(TransformValueTaskAsync(valueTask));

            static async Task<TiffRational[]> TransformValueTaskAsync(ValueTask<TiffValueCollection<TiffRational>> valueTask)
            {
                TiffValueCollection<TiffRational> result = await valueTask.ConfigureAwait(false);
                return result.GetOrCreateArray();
            }
        }

        #endregion
    
        #region TransferFunction

        /// <summary>
        /// Read the values of <see cref="TiffTag.TransferFunction"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<ushort[]> ReadTransferFunctionAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync(TiffTag.TransferFunction);
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
    
        #region TransferRange

        /// <summary>
        /// Read the values of <see cref="TiffTag.TransferRange"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<ushort[]> ReadTransferRangeAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync(TiffTag.TransferRange);
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
    
        #region ReferenceBlackWhite

        /// <summary>
        /// Read the values of <see cref="TiffTag.ReferenceBlackWhite"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffRational[]> ReadReferenceBlackWhiteAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<TiffRational>> valueTask = tagReader.ReadRationalFieldAsync(TiffTag.ReferenceBlackWhite);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<TiffRational> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffRational[]>(result.GetOrCreateArray());
            }

            return new ValueTask<TiffRational[]>(TransformValueTaskAsync(valueTask));

            static async Task<TiffRational[]> TransformValueTaskAsync(ValueTask<TiffValueCollection<TiffRational>> valueTask)
            {
                TiffValueCollection<TiffRational> result = await valueTask.ConfigureAwait(false);
                return result.GetOrCreateArray();
            }
        }

        #endregion
    
    }
}
