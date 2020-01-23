using System.Threading.Tasks;

namespace TiffLibrary.Exif
{
    public static partial class TiffTagExifReaderExtensions
    {
    
        #region InteroperabilityIndex

        /// <summary>
        /// Read the values of InteroperabilityIndex tag.
        /// Field description: Indicates the identification of the Interoperability rule.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<string?> ReadInteroperabilityIndexAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<string>> valueTask = tagReader.ReadASCIIFieldAsync((TiffTag)0x0001);
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
        /// Read the values of InteroperabilityIndex tag.
        /// Field description: Indicates the identification of the Interoperability rule.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static string? ReadInteroperabilityIndex(this TiffTagReader tagReader)
        {
            TiffValueCollection<string> result = tagReader.ReadASCIIField((TiffTag)0x0001);
            return result.GetFirstOrDefault();
        }

        #endregion
    
    }
}
