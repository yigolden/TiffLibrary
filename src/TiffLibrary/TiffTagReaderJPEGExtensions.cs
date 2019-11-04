using System.Threading.Tasks;

namespace TiffLibrary
{
    public static partial class TiffTagReaderExtensions
    {
    
        #region JPEGTables

        /// <summary>
        /// Read the values of <see cref="TiffTag.JPEGTables"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<byte[]> ReadJPEGTablesAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<byte>> valueTask = tagReader.ReadByteFieldAsync(TiffTag.JPEGTables);
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
        /// Read the values of <see cref="TiffTag.JPEGTables"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static byte[] ReadJPEGTables(this TiffTagReader tagReader)
        {
            TiffValueCollection<byte> result = tagReader.ReadByteField(TiffTag.JPEGTables);
            return result.GetOrCreateArray();
        }

        #endregion
    
        #region JPEGProc

        /// <summary>
        /// Read the values of <see cref="TiffTag.JPEGProc"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<ushort> ReadJPEGProcAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync(TiffTag.JPEGProc);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<ushort> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<ushort>(result.IsEmpty ? (ushort)1 : result.FirstOrDefault);
            }

            return new ValueTask<ushort>(TransformValueTaskAsync(valueTask));

            static async Task<ushort> TransformValueTaskAsync(ValueTask<TiffValueCollection<ushort>> valueTask)
            {
                TiffValueCollection<ushort> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? (ushort)1 : result.FirstOrDefault;
            }
        }
        
        /// <summary>
        /// Read the values of <see cref="TiffTag.JPEGProc"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static ushort ReadJPEGProc(this TiffTagReader tagReader)
        {
            TiffValueCollection<ushort> result = tagReader.ReadShortField(TiffTag.JPEGProc);
            return result.IsEmpty ? (ushort)1 : result.FirstOrDefault;
        }

        #endregion
    
        #region JPEGInterchangeFormat

        /// <summary>
        /// Read the values of <see cref="TiffTag.JPEGInterchangeFormat"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<uint> ReadJPEGInterchangeFormatAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<uint>> valueTask = tagReader.ReadLongFieldAsync(TiffTag.JPEGInterchangeFormat);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<uint> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<uint>(result.IsEmpty ? 0 : result.FirstOrDefault);
            }

            return new ValueTask<uint>(TransformValueTaskAsync(valueTask));

            static async Task<uint> TransformValueTaskAsync(ValueTask<TiffValueCollection<uint>> valueTask)
            {
                TiffValueCollection<uint> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? 0 : result.FirstOrDefault;
            }
        }
        
        /// <summary>
        /// Read the values of <see cref="TiffTag.JPEGInterchangeFormat"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static uint ReadJPEGInterchangeFormat(this TiffTagReader tagReader)
        {
            TiffValueCollection<uint> result = tagReader.ReadLongField(TiffTag.JPEGInterchangeFormat);
            return result.IsEmpty ? 0 : result.FirstOrDefault;
        }

        #endregion
    
        #region JPEGInterchangeFormatLength

        /// <summary>
        /// Read the values of <see cref="TiffTag.JPEGInterchangeFormatLength"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<uint> ReadJPEGInterchangeFormatLengthAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<uint>> valueTask = tagReader.ReadLongFieldAsync(TiffTag.JPEGInterchangeFormatLength);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<uint> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<uint>(result.IsEmpty ? 0 : result.FirstOrDefault);
            }

            return new ValueTask<uint>(TransformValueTaskAsync(valueTask));

            static async Task<uint> TransformValueTaskAsync(ValueTask<TiffValueCollection<uint>> valueTask)
            {
                TiffValueCollection<uint> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? 0 : result.FirstOrDefault;
            }
        }
        
        /// <summary>
        /// Read the values of <see cref="TiffTag.JPEGInterchangeFormatLength"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static uint ReadJPEGInterchangeFormatLength(this TiffTagReader tagReader)
        {
            TiffValueCollection<uint> result = tagReader.ReadLongField(TiffTag.JPEGInterchangeFormatLength);
            return result.IsEmpty ? 0 : result.FirstOrDefault;
        }

        #endregion
    
        #region JPEGRestartInterval

        /// <summary>
        /// Read the values of <see cref="TiffTag.JPEGRestartInterval"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<ushort> ReadJPEGRestartIntervalAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync(TiffTag.JPEGRestartInterval);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<ushort> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<ushort>(result.IsEmpty ? (ushort)0 : result.FirstOrDefault);
            }

            return new ValueTask<ushort>(TransformValueTaskAsync(valueTask));

            static async Task<ushort> TransformValueTaskAsync(ValueTask<TiffValueCollection<ushort>> valueTask)
            {
                TiffValueCollection<ushort> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? (ushort)0 : result.FirstOrDefault;
            }
        }
        
        /// <summary>
        /// Read the values of <see cref="TiffTag.JPEGRestartInterval"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static ushort ReadJPEGRestartInterval(this TiffTagReader tagReader)
        {
            TiffValueCollection<ushort> result = tagReader.ReadShortField(TiffTag.JPEGRestartInterval);
            return result.IsEmpty ? (ushort)0 : result.FirstOrDefault;
        }

        #endregion
    
        #region JPEGQTables

        /// <summary>
        /// Read the values of <see cref="TiffTag.JPEGQTables"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<uint[]> ReadJPEGQTablesAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<uint>> valueTask = tagReader.ReadLongFieldAsync(TiffTag.JPEGQTables);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<uint> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<uint[]>(result.GetOrCreateArray());
            }

            return new ValueTask<uint[]>(TransformValueTaskAsync(valueTask));

            static async Task<uint[]> TransformValueTaskAsync(ValueTask<TiffValueCollection<uint>> valueTask)
            {
                TiffValueCollection<uint> result = await valueTask.ConfigureAwait(false);
                return result.GetOrCreateArray();
            }
        }
        
        /// <summary>
        /// Read the values of <see cref="TiffTag.JPEGQTables"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static uint[] ReadJPEGQTables(this TiffTagReader tagReader)
        {
            TiffValueCollection<uint> result = tagReader.ReadLongField(TiffTag.JPEGQTables);
            return result.GetOrCreateArray();
        }

        #endregion
    
        #region JPEGDCTables

        /// <summary>
        /// Read the values of <see cref="TiffTag.JPEGDCTables"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<uint[]> ReadJPEGDCTablesAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<uint>> valueTask = tagReader.ReadLongFieldAsync(TiffTag.JPEGDCTables);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<uint> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<uint[]>(result.GetOrCreateArray());
            }

            return new ValueTask<uint[]>(TransformValueTaskAsync(valueTask));

            static async Task<uint[]> TransformValueTaskAsync(ValueTask<TiffValueCollection<uint>> valueTask)
            {
                TiffValueCollection<uint> result = await valueTask.ConfigureAwait(false);
                return result.GetOrCreateArray();
            }
        }
        
        /// <summary>
        /// Read the values of <see cref="TiffTag.JPEGDCTables"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static uint[] ReadJPEGDCTables(this TiffTagReader tagReader)
        {
            TiffValueCollection<uint> result = tagReader.ReadLongField(TiffTag.JPEGDCTables);
            return result.GetOrCreateArray();
        }

        #endregion
    
        #region JPEGACTables

        /// <summary>
        /// Read the values of <see cref="TiffTag.JPEGACTables"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<uint[]> ReadJPEGACTablesAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<uint>> valueTask = tagReader.ReadLongFieldAsync(TiffTag.JPEGACTables);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<uint> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<uint[]>(result.GetOrCreateArray());
            }

            return new ValueTask<uint[]>(TransformValueTaskAsync(valueTask));

            static async Task<uint[]> TransformValueTaskAsync(ValueTask<TiffValueCollection<uint>> valueTask)
            {
                TiffValueCollection<uint> result = await valueTask.ConfigureAwait(false);
                return result.GetOrCreateArray();
            }
        }
        
        /// <summary>
        /// Read the values of <see cref="TiffTag.JPEGACTables"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static uint[] ReadJPEGACTables(this TiffTagReader tagReader)
        {
            TiffValueCollection<uint> result = tagReader.ReadLongField(TiffTag.JPEGACTables);
            return result.GetOrCreateArray();
        }

        #endregion
    
    }
}
