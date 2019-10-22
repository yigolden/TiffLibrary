using System.Threading.Tasks;

namespace TiffLibrary
{
    public static partial class TiffTagReaderExtensions
    {
    
        #region Artist

        /// <summary>
        /// Read the values of <see cref="TiffTag.Artist"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<string> ReadArtistAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<string>> valueTask = tagReader.ReadASCIIFieldAsync(TiffTag.Artist);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<string> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<string>(result.FirstOrDefault);
            }

            return new ValueTask<string>(TransformValueTaskAsync(valueTask));

            static async Task<string> TransformValueTaskAsync(ValueTask<TiffValueCollection<string>> valueTask)
            {
                TiffValueCollection<string> result = await valueTask.ConfigureAwait(false);
                return result.FirstOrDefault;
            }
        }

        #endregion
    
        #region BitsPerSample

        /// <summary>
        /// Read the values of <see cref="TiffTag.BitsPerSample"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffValueCollection<ushort>> ReadBitsPerSampleAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync(TiffTag.BitsPerSample);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<ushort> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffValueCollection<ushort>>(result.IsEmpty ? new TiffValueCollection<ushort>(1) : result);
            }

            return new ValueTask<TiffValueCollection<ushort>>(TransformValueTaskAsync(valueTask));

            static async Task<TiffValueCollection<ushort>> TransformValueTaskAsync(ValueTask<TiffValueCollection<ushort>> valueTask)
            {
                TiffValueCollection<ushort> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? new TiffValueCollection<ushort>(1) : result;
            }
        }

        #endregion
    
        #region CellLength

        /// <summary>
        /// Read the values of <see cref="TiffTag.CellLength"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<ushort?> ReadCellLengthAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync(TiffTag.CellLength);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<ushort> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<ushort?>(result.IsEmpty ? default(ushort?) : result.FirstOrDefault);
            }

            return new ValueTask<ushort?>(TransformValueTaskAsync(valueTask));

            static async Task<ushort?> TransformValueTaskAsync(ValueTask<TiffValueCollection<ushort>> valueTask)
            {
                TiffValueCollection<ushort> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? default(ushort?) : result.FirstOrDefault;
            }
        }

        #endregion
    
        #region CellWidth

        /// <summary>
        /// Read the values of <see cref="TiffTag.CellWidth"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<ushort?> ReadCellWidthAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync(TiffTag.CellWidth);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<ushort> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<ushort?>(result.IsEmpty ? default(ushort?) : result.FirstOrDefault);
            }

            return new ValueTask<ushort?>(TransformValueTaskAsync(valueTask));

            static async Task<ushort?> TransformValueTaskAsync(ValueTask<TiffValueCollection<ushort>> valueTask)
            {
                TiffValueCollection<ushort> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? default(ushort?) : result.FirstOrDefault;
            }
        }

        #endregion
    
        #region ColorMap

        /// <summary>
        /// Read the values of <see cref="TiffTag.ColorMap"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<ushort[]> ReadColorMapAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync(TiffTag.ColorMap);
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
    
        #region Compression

        /// <summary>
        /// Read the values of <see cref="TiffTag.Compression"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffCompression> ReadCompressionAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync(TiffTag.Compression);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<ushort> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffCompression>(result.IsEmpty ? TiffCompression.Unspecified : (TiffCompression)result.FirstOrDefault);
            }

            return new ValueTask<TiffCompression>(TransformValueTaskAsync(valueTask));

            static async Task<TiffCompression> TransformValueTaskAsync(ValueTask<TiffValueCollection<ushort>> valueTask)
            {
                TiffValueCollection<ushort> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? TiffCompression.Unspecified : (TiffCompression)result.FirstOrDefault;
            }
        }

        #endregion
    
        #region Copyright

        /// <summary>
        /// Read the values of <see cref="TiffTag.Copyright"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<string> ReadCopyrightAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<string>> valueTask = tagReader.ReadASCIIFieldAsync(TiffTag.Copyright);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<string> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<string>(result.FirstOrDefault);
            }

            return new ValueTask<string>(TransformValueTaskAsync(valueTask));

            static async Task<string> TransformValueTaskAsync(ValueTask<TiffValueCollection<string>> valueTask)
            {
                TiffValueCollection<string> result = await valueTask.ConfigureAwait(false);
                return result.FirstOrDefault;
            }
        }

        #endregion
    
        #region DateTime

        /// <summary>
        /// Read the values of <see cref="TiffTag.DateTime"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<string> ReadDateTimeAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<string>> valueTask = tagReader.ReadASCIIFieldAsync(TiffTag.DateTime);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<string> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<string>(result.FirstOrDefault);
            }

            return new ValueTask<string>(TransformValueTaskAsync(valueTask));

            static async Task<string> TransformValueTaskAsync(ValueTask<TiffValueCollection<string>> valueTask)
            {
                TiffValueCollection<string> result = await valueTask.ConfigureAwait(false);
                return result.FirstOrDefault;
            }
        }

        #endregion
    
        #region ExtraSamples

        /// <summary>
        /// Read the values of <see cref="TiffTag.ExtraSamples"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffValueCollection<TiffExtraSample>> ReadExtraSamplesAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync(TiffTag.ExtraSamples);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<ushort> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffValueCollection<TiffExtraSample>>(result.ConvertAll(i => (TiffExtraSample)i));
            }

            return new ValueTask<TiffValueCollection<TiffExtraSample>>(TransformValueTaskAsync(valueTask));

            static async Task<TiffValueCollection<TiffExtraSample>> TransformValueTaskAsync(ValueTask<TiffValueCollection<ushort>> valueTask)
            {
                TiffValueCollection<ushort> result = await valueTask.ConfigureAwait(false);
                return result.ConvertAll(i => (TiffExtraSample)i);
            }
        }

        #endregion
    
        #region FillOrder

        /// <summary>
        /// Read the values of <see cref="TiffTag.FillOrder"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffFillOrder> ReadFillOrderAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync(TiffTag.FillOrder);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<ushort> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffFillOrder>(result.IsEmpty ? TiffFillOrder.HigherOrderBitsFirst : (TiffFillOrder)result.FirstOrDefault);
            }

            return new ValueTask<TiffFillOrder>(TransformValueTaskAsync(valueTask));

            static async Task<TiffFillOrder> TransformValueTaskAsync(ValueTask<TiffValueCollection<ushort>> valueTask)
            {
                TiffValueCollection<ushort> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? TiffFillOrder.HigherOrderBitsFirst : (TiffFillOrder)result.FirstOrDefault;
            }
        }

        #endregion
    
        #region FreeByteCounts

        /// <summary>
        /// Read the values of <see cref="TiffTag.FreeByteCounts"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<uint?> ReadFreeByteCountsAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<uint>> valueTask = tagReader.ReadLongFieldAsync(TiffTag.FreeByteCounts);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<uint> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<uint?>(result.IsEmpty ? default(uint?) : result.FirstOrDefault);
            }

            return new ValueTask<uint?>(TransformValueTaskAsync(valueTask));

            static async Task<uint?> TransformValueTaskAsync(ValueTask<TiffValueCollection<uint>> valueTask)
            {
                TiffValueCollection<uint> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? default(uint?) : result.FirstOrDefault;
            }
        }

        #endregion
    
        #region FreeOffsets

        /// <summary>
        /// Read the values of <see cref="TiffTag.FreeOffsets"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<uint?> ReadFreeOffsetsAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<uint>> valueTask = tagReader.ReadLongFieldAsync(TiffTag.FreeOffsets);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<uint> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<uint?>(result.IsEmpty ? default(uint?) : result.FirstOrDefault);
            }

            return new ValueTask<uint?>(TransformValueTaskAsync(valueTask));

            static async Task<uint?> TransformValueTaskAsync(ValueTask<TiffValueCollection<uint>> valueTask)
            {
                TiffValueCollection<uint> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? default(uint?) : result.FirstOrDefault;
            }
        }

        #endregion
    
        #region GrayResponseCurve

        /// <summary>
        /// Read the values of <see cref="TiffTag.GrayResponseCurve"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffValueCollection<ushort>> ReadGrayResponseCurveAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync(TiffTag.GrayResponseCurve);
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
    
        #region GrayResponseUnit

        /// <summary>
        /// Read the values of <see cref="TiffTag.GrayResponseUnit"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffGrayResponseUnit> ReadGrayResponseUnitAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync(TiffTag.GrayResponseUnit);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<ushort> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffGrayResponseUnit>(result.IsEmpty ? TiffGrayResponseUnit.Hundredths : (TiffGrayResponseUnit)result.FirstOrDefault);
            }

            return new ValueTask<TiffGrayResponseUnit>(TransformValueTaskAsync(valueTask));

            static async Task<TiffGrayResponseUnit> TransformValueTaskAsync(ValueTask<TiffValueCollection<ushort>> valueTask)
            {
                TiffValueCollection<ushort> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? TiffGrayResponseUnit.Hundredths : (TiffGrayResponseUnit)result.FirstOrDefault;
            }
        }

        #endregion
    
        #region HostComputer

        /// <summary>
        /// Read the values of <see cref="TiffTag.HostComputer"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<string> ReadHostComputerAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<string>> valueTask = tagReader.ReadASCIIFieldAsync(TiffTag.HostComputer);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<string> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<string>(result.FirstOrDefault);
            }

            return new ValueTask<string>(TransformValueTaskAsync(valueTask));

            static async Task<string> TransformValueTaskAsync(ValueTask<TiffValueCollection<string>> valueTask)
            {
                TiffValueCollection<string> result = await valueTask.ConfigureAwait(false);
                return result.FirstOrDefault;
            }
        }

        #endregion
    
        #region ImageDescription

        /// <summary>
        /// Read the values of <see cref="TiffTag.ImageDescription"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<string> ReadImageDescriptionAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<string>> valueTask = tagReader.ReadASCIIFieldAsync(TiffTag.ImageDescription);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<string> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<string>(result.FirstOrDefault);
            }

            return new ValueTask<string>(TransformValueTaskAsync(valueTask));

            static async Task<string> TransformValueTaskAsync(ValueTask<TiffValueCollection<string>> valueTask)
            {
                TiffValueCollection<string> result = await valueTask.ConfigureAwait(false);
                return result.FirstOrDefault;
            }
        }

        #endregion
    
        #region ImageLength

        /// <summary>
        /// Read the values of <see cref="TiffTag.ImageLength"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<ulong> ReadImageLengthAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ulong>> valueTask = tagReader.ReadLong8FieldAsync(TiffTag.ImageLength);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<ulong> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<ulong>(result.FirstOrDefault);
            }

            return new ValueTask<ulong>(TransformValueTaskAsync(valueTask));

            static async Task<ulong> TransformValueTaskAsync(ValueTask<TiffValueCollection<ulong>> valueTask)
            {
                TiffValueCollection<ulong> result = await valueTask.ConfigureAwait(false);
                return result.FirstOrDefault;
            }
        }

        #endregion
    
        #region ImageWidth

        /// <summary>
        /// Read the values of <see cref="TiffTag.ImageWidth"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<ulong> ReadImageWidthAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ulong>> valueTask = tagReader.ReadLong8FieldAsync(TiffTag.ImageWidth);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<ulong> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<ulong>(result.FirstOrDefault);
            }

            return new ValueTask<ulong>(TransformValueTaskAsync(valueTask));

            static async Task<ulong> TransformValueTaskAsync(ValueTask<TiffValueCollection<ulong>> valueTask)
            {
                TiffValueCollection<ulong> result = await valueTask.ConfigureAwait(false);
                return result.FirstOrDefault;
            }
        }

        #endregion
    
        #region Make

        /// <summary>
        /// Read the values of <see cref="TiffTag.Make"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<string> ReadMakeAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<string>> valueTask = tagReader.ReadASCIIFieldAsync(TiffTag.Make);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<string> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<string>(result.FirstOrDefault);
            }

            return new ValueTask<string>(TransformValueTaskAsync(valueTask));

            static async Task<string> TransformValueTaskAsync(ValueTask<TiffValueCollection<string>> valueTask)
            {
                TiffValueCollection<string> result = await valueTask.ConfigureAwait(false);
                return result.FirstOrDefault;
            }
        }

        #endregion
    
        #region MaxSampleValue

        /// <summary>
        /// Read the values of <see cref="TiffTag.MaxSampleValue"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffValueCollection<ushort>> ReadMaxSampleValueAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync(TiffTag.MaxSampleValue);
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
    
        #region MinSampleValue

        /// <summary>
        /// Read the values of <see cref="TiffTag.MinSampleValue"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffValueCollection<ushort>> ReadMinSampleValueAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync(TiffTag.MinSampleValue);
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
    
        #region Model

        /// <summary>
        /// Read the values of <see cref="TiffTag.Model"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<string> ReadModelAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<string>> valueTask = tagReader.ReadASCIIFieldAsync(TiffTag.Model);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<string> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<string>(result.FirstOrDefault);
            }

            return new ValueTask<string>(TransformValueTaskAsync(valueTask));

            static async Task<string> TransformValueTaskAsync(ValueTask<TiffValueCollection<string>> valueTask)
            {
                TiffValueCollection<string> result = await valueTask.ConfigureAwait(false);
                return result.FirstOrDefault;
            }
        }

        #endregion
    
        #region NewSubfileType

        /// <summary>
        /// Read the values of <see cref="TiffTag.NewSubfileType"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffNewSubfileType> ReadNewSubfileTypeAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<uint>> valueTask = tagReader.ReadLongFieldAsync(TiffTag.NewSubfileType);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<uint> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffNewSubfileType>((TiffNewSubfileType)result.FirstOrDefault);
            }

            return new ValueTask<TiffNewSubfileType>(TransformValueTaskAsync(valueTask));

            static async Task<TiffNewSubfileType> TransformValueTaskAsync(ValueTask<TiffValueCollection<uint>> valueTask)
            {
                TiffValueCollection<uint> result = await valueTask.ConfigureAwait(false);
                return (TiffNewSubfileType)result.FirstOrDefault;
            }
        }

        #endregion
    
        #region Orientation

        /// <summary>
        /// Read the values of <see cref="TiffTag.Orientation"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffOrientation> ReadOrientationAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync(TiffTag.Orientation);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<ushort> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffOrientation>(result.IsEmpty ? TiffOrientation.TopLeft : (TiffOrientation)result.FirstOrDefault);
            }

            return new ValueTask<TiffOrientation>(TransformValueTaskAsync(valueTask));

            static async Task<TiffOrientation> TransformValueTaskAsync(ValueTask<TiffValueCollection<ushort>> valueTask)
            {
                TiffValueCollection<ushort> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? TiffOrientation.TopLeft : (TiffOrientation)result.FirstOrDefault;
            }
        }

        #endregion
    
        #region PhotometricInterpretation

        /// <summary>
        /// Read the values of <see cref="TiffTag.PhotometricInterpretation"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffPhotometricInterpretation?> ReadPhotometricInterpretationAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync(TiffTag.PhotometricInterpretation);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<ushort> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffPhotometricInterpretation?>(result.IsEmpty ? default(TiffPhotometricInterpretation?) : (TiffPhotometricInterpretation)result.FirstOrDefault);
            }

            return new ValueTask<TiffPhotometricInterpretation?>(TransformValueTaskAsync(valueTask));

            static async Task<TiffPhotometricInterpretation?> TransformValueTaskAsync(ValueTask<TiffValueCollection<ushort>> valueTask)
            {
                TiffValueCollection<ushort> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? default(TiffPhotometricInterpretation?) : (TiffPhotometricInterpretation)result.FirstOrDefault;
            }
        }

        #endregion
    
        #region PlanarConfiguration

        /// <summary>
        /// Read the values of <see cref="TiffTag.PlanarConfiguration"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffPlanarConfiguration> ReadPlanarConfigurationAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync(TiffTag.PlanarConfiguration);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<ushort> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffPlanarConfiguration>(result.IsEmpty ? TiffPlanarConfiguration.Chunky : (TiffPlanarConfiguration)result.FirstOrDefault);
            }

            return new ValueTask<TiffPlanarConfiguration>(TransformValueTaskAsync(valueTask));

            static async Task<TiffPlanarConfiguration> TransformValueTaskAsync(ValueTask<TiffValueCollection<ushort>> valueTask)
            {
                TiffValueCollection<ushort> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? TiffPlanarConfiguration.Chunky : (TiffPlanarConfiguration)result.FirstOrDefault;
            }
        }

        #endregion
    
        #region ResolutionUnit

        /// <summary>
        /// Read the values of <see cref="TiffTag.ResolutionUnit"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffResolutionUnit> ReadResolutionUnitAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync(TiffTag.ResolutionUnit);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<ushort> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffResolutionUnit>(result.IsEmpty ? TiffResolutionUnit.Inch : (TiffResolutionUnit)result.FirstOrDefault);
            }

            return new ValueTask<TiffResolutionUnit>(TransformValueTaskAsync(valueTask));

            static async Task<TiffResolutionUnit> TransformValueTaskAsync(ValueTask<TiffValueCollection<ushort>> valueTask)
            {
                TiffValueCollection<ushort> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? TiffResolutionUnit.Inch : (TiffResolutionUnit)result.FirstOrDefault;
            }
        }

        #endregion
    
        #region RowsPerStrip

        /// <summary>
        /// Read the values of <see cref="TiffTag.RowsPerStrip"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<uint> ReadRowsPerStripAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<uint>> valueTask = tagReader.ReadLongFieldAsync(TiffTag.RowsPerStrip);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<uint> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<uint>(result.FirstOrDefault);
            }

            return new ValueTask<uint>(TransformValueTaskAsync(valueTask));

            static async Task<uint> TransformValueTaskAsync(ValueTask<TiffValueCollection<uint>> valueTask)
            {
                TiffValueCollection<uint> result = await valueTask.ConfigureAwait(false);
                return result.FirstOrDefault;
            }
        }

        #endregion
    
        #region SamplesPerPixel

        /// <summary>
        /// Read the values of <see cref="TiffTag.SamplesPerPixel"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<ushort> ReadSamplesPerPixelAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync(TiffTag.SamplesPerPixel);
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

        #endregion
    
        #region Software

        /// <summary>
        /// Read the values of <see cref="TiffTag.Software"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<string> ReadSoftwareAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<string>> valueTask = tagReader.ReadASCIIFieldAsync(TiffTag.Software);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<string> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<string>(result.FirstOrDefault);
            }

            return new ValueTask<string>(TransformValueTaskAsync(valueTask));

            static async Task<string> TransformValueTaskAsync(ValueTask<TiffValueCollection<string>> valueTask)
            {
                TiffValueCollection<string> result = await valueTask.ConfigureAwait(false);
                return result.FirstOrDefault;
            }
        }

        #endregion
    
        #region StripByteCounts

        /// <summary>
        /// Read the values of <see cref="TiffTag.StripByteCounts"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffValueCollection<ulong>> ReadStripByteCountsAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ulong>> valueTask = tagReader.ReadLong8FieldAsync(TiffTag.StripByteCounts);
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

        #endregion
    
        #region StripOffsets

        /// <summary>
        /// Read the values of <see cref="TiffTag.StripOffsets"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffValueCollection<ulong>> ReadStripOffsetsAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ulong>> valueTask = tagReader.ReadLong8FieldAsync(TiffTag.StripOffsets);
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

        #endregion
    
        #region SubFileType

        /// <summary>
        /// Read the values of <see cref="TiffTag.SubFileType"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffSubfileType?> ReadSubFileTypeAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync(TiffTag.SubFileType);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<ushort> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffSubfileType?>(result.IsEmpty ? default(TiffSubfileType?) : (TiffSubfileType)result.FirstOrDefault);
            }

            return new ValueTask<TiffSubfileType?>(TransformValueTaskAsync(valueTask));

            static async Task<TiffSubfileType?> TransformValueTaskAsync(ValueTask<TiffValueCollection<ushort>> valueTask)
            {
                TiffValueCollection<ushort> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? default(TiffSubfileType?) : (TiffSubfileType)result.FirstOrDefault;
            }
        }

        #endregion
    
        #region Threshholding

        /// <summary>
        /// Read the values of <see cref="TiffTag.Threshholding"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffThreshholding> ReadThreshholdingAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync(TiffTag.Threshholding);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<ushort> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffThreshholding>(result.IsEmpty ? TiffThreshholding.NoThreshholding : (TiffThreshholding)result.FirstOrDefault);
            }

            return new ValueTask<TiffThreshholding>(TransformValueTaskAsync(valueTask));

            static async Task<TiffThreshholding> TransformValueTaskAsync(ValueTask<TiffValueCollection<ushort>> valueTask)
            {
                TiffValueCollection<ushort> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? TiffThreshholding.NoThreshholding : (TiffThreshholding)result.FirstOrDefault;
            }
        }

        #endregion
    
        #region XResolution

        /// <summary>
        /// Read the values of <see cref="TiffTag.XResolution"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffRational?> ReadXResolutionAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<TiffRational>> valueTask = tagReader.ReadRationalFieldAsync(TiffTag.XResolution);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<TiffRational> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffRational?>(result.IsEmpty ? default(TiffRational?) : result.FirstOrDefault);
            }

            return new ValueTask<TiffRational?>(TransformValueTaskAsync(valueTask));

            static async Task<TiffRational?> TransformValueTaskAsync(ValueTask<TiffValueCollection<TiffRational>> valueTask)
            {
                TiffValueCollection<TiffRational> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? default(TiffRational?) : result.FirstOrDefault;
            }
        }

        #endregion
    
        #region YResolution

        /// <summary>
        /// Read the values of <see cref="TiffTag.YResolution"/>.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffRational?> ReadYResolutionAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<TiffRational>> valueTask = tagReader.ReadRationalFieldAsync(TiffTag.YResolution);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<TiffRational> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffRational?>(result.IsEmpty ? default(TiffRational?) : result.FirstOrDefault);
            }

            return new ValueTask<TiffRational?>(TransformValueTaskAsync(valueTask));

            static async Task<TiffRational?> TransformValueTaskAsync(ValueTask<TiffValueCollection<TiffRational>> valueTask)
            {
                TiffValueCollection<TiffRational> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? default(TiffRational?) : result.FirstOrDefault;
            }
        }

        #endregion
    
    }
}
