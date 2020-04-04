using System.Threading;
using System.Threading.Tasks;

namespace TiffLibrary
{
    public static partial class TiffTagReaderExtensions
    {
    
        #region SubIFDs

        /// <summary>
        /// Read the values of <see cref="TiffTag.SubIFDs"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffValueCollection<TiffStreamOffset>> ReadSubIFDsAsync(this TiffTagReader tagReader, CancellationToken cancellationToken = default)
        {
            ValueTask<TiffValueCollection<TiffStreamOffset>> valueTask = tagReader.ReadIFD8FieldAsync(TiffTag.SubIFDs, cancellationToken);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<TiffStreamOffset> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffValueCollection<TiffStreamOffset>>(result);
            }

            return new ValueTask<TiffValueCollection<TiffStreamOffset>>(TransformValueTaskAsync(valueTask));

            static async Task<TiffValueCollection<TiffStreamOffset>> TransformValueTaskAsync(ValueTask<TiffValueCollection<TiffStreamOffset>> valueTask)
            {
                TiffValueCollection<TiffStreamOffset> result = await valueTask.ConfigureAwait(false);
                return result;
            }
        }
        
        /// <summary>
        /// Read the values of <see cref="TiffTag.SubIFDs"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffValueCollection<TiffStreamOffset> ReadSubIFDs(this TiffTagReader tagReader)
        {
            TiffValueCollection<TiffStreamOffset> result = tagReader.ReadIFD8Field(TiffTag.SubIFDs);
            return result;
        }

        #endregion
    
        #region ClipPath

        /// <summary>
        /// Read the values of <see cref="TiffTag.ClipPath"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<byte[]> ReadClipPathAsync(this TiffTagReader tagReader, CancellationToken cancellationToken = default)
        {
            ValueTask<TiffValueCollection<byte>> valueTask = tagReader.ReadByteFieldAsync(TiffTag.ClipPath, cancellationToken);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<byte> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<byte[]>(result.GetOrCreateArray());
            }

            return new ValueTask<byte[]>(TransformValueTaskAsync(valueTask));

            static async Task<byte[]> TransformValueTaskAsync(ValueTask<TiffValueCollection<byte>> valueTask)
            {
                TiffValueCollection<byte> result = await valueTask.ConfigureAwait(false);
                return result.GetOrCreateArray();
            }
        }
        
        /// <summary>
        /// Read the values of <see cref="TiffTag.ClipPath"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static byte[] ReadClipPath(this TiffTagReader tagReader)
        {
            TiffValueCollection<byte> result = tagReader.ReadByteField(TiffTag.ClipPath);
            return result.GetOrCreateArray();
        }

        #endregion
    
        #region XClipPathUnits

        /// <summary>
        /// Read the values of <see cref="TiffTag.XClipPathUnits"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<int?> ReadXClipPathUnitsAsync(this TiffTagReader tagReader, CancellationToken cancellationToken = default)
        {
            ValueTask<TiffValueCollection<int>> valueTask = tagReader.ReadSLongFieldAsync(TiffTag.XClipPathUnits, cancellationToken);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<int> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<int?>(result.IsEmpty ? default(int?) : result.GetFirstOrDefault());
            }

            return new ValueTask<int?>(TransformValueTaskAsync(valueTask));

            static async Task<int?> TransformValueTaskAsync(ValueTask<TiffValueCollection<int>> valueTask)
            {
                TiffValueCollection<int> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? default(int?) : result.GetFirstOrDefault();
            }
        }
        
        /// <summary>
        /// Read the values of <see cref="TiffTag.XClipPathUnits"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static int? ReadXClipPathUnits(this TiffTagReader tagReader)
        {
            TiffValueCollection<int> result = tagReader.ReadSLongField(TiffTag.XClipPathUnits);
            return result.IsEmpty ? default(int?) : result.GetFirstOrDefault();
        }

        #endregion
    
        #region YClipPathUnits

        /// <summary>
        /// Read the values of <see cref="TiffTag.YClipPathUnits"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<int?> ReadYClipPathUnitsAsync(this TiffTagReader tagReader, CancellationToken cancellationToken = default)
        {
            ValueTask<TiffValueCollection<int>> valueTask = tagReader.ReadSLongFieldAsync(TiffTag.YClipPathUnits, cancellationToken);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<int> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<int?>(result.IsEmpty ? default(int?) : result.GetFirstOrDefault());
            }

            return new ValueTask<int?>(TransformValueTaskAsync(valueTask));

            static async Task<int?> TransformValueTaskAsync(ValueTask<TiffValueCollection<int>> valueTask)
            {
                TiffValueCollection<int> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? default(int?) : result.GetFirstOrDefault();
            }
        }
        
        /// <summary>
        /// Read the values of <see cref="TiffTag.YClipPathUnits"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static int? ReadYClipPathUnits(this TiffTagReader tagReader)
        {
            TiffValueCollection<int> result = tagReader.ReadSLongField(TiffTag.YClipPathUnits);
            return result.IsEmpty ? default(int?) : result.GetFirstOrDefault();
        }

        #endregion
    
        #region Indexed

        /// <summary>
        /// Read the values of <see cref="TiffTag.Indexed"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that fires if the user want to stop the current task.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<ushort> ReadIndexedAsync(this TiffTagReader tagReader, CancellationToken cancellationToken = default)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync(TiffTag.Indexed, cancellationToken);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<ushort> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<ushort>(result.GetFirstOrDefault());
            }

            return new ValueTask<ushort>(TransformValueTaskAsync(valueTask));

            static async Task<ushort> TransformValueTaskAsync(ValueTask<TiffValueCollection<ushort>> valueTask)
            {
                TiffValueCollection<ushort> result = await valueTask.ConfigureAwait(false);
                return result.GetFirstOrDefault();
            }
        }
        
        /// <summary>
        /// Read the values of <see cref="TiffTag.Indexed"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static ushort ReadIndexed(this TiffTagReader tagReader)
        {
            TiffValueCollection<ushort> result = tagReader.ReadShortField(TiffTag.Indexed);
            return result.GetFirstOrDefault();
        }

        #endregion
    
    }
}
