using System.Threading.Tasks;

namespace TiffLibrary
{
    public static partial class TiffTagReaderExtensions
    {
    
        #region Predictor

        /// <summary>
        /// Read the values of <see cref="TiffTag.Predictor"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffPredictor> ReadPredictorAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync(TiffTag.Predictor);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<ushort> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffPredictor>(result.IsEmpty ? TiffPredictor.None : (TiffPredictor)result.FirstOrDefault);
            }

            return new ValueTask<TiffPredictor>(TransformValueTaskAsync(valueTask));

            static async Task<TiffPredictor> TransformValueTaskAsync(ValueTask<TiffValueCollection<ushort>> valueTask)
            {
                TiffValueCollection<ushort> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? TiffPredictor.None : (TiffPredictor)result.FirstOrDefault;
            }
        }

        #endregion
    
    }
}
