using System.Threading.Tasks;

namespace TiffLibrary.Exif
{
    public static partial class TiffTagExifReaderExtensions
    {
    
        #region GPSVersionID

        /// <summary>
        /// Read the values of GPSVersionID tag.
        /// Field description: Indicates the version of GPSInfoIFD.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<byte[]> ReadGpsVersionIDAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<byte>> valueTask = tagReader.ReadByteFieldAsync((TiffTag)0x0000);
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
        /// Read the values of GPSVersionID tag.
        /// Field description: Indicates the version of GPSInfoIFD.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static byte[] ReadGpsVersionID(this TiffTagReader tagReader)
        {
            TiffValueCollection<byte> result = tagReader.ReadByteField((TiffTag)0x0000);
            return result.GetOrCreateArray();
        }

        #endregion
    
        #region GPSLatitudeReference

        /// <summary>
        /// Read the values of GPSLatitudeReference tag.
        /// Field description: Indicates whether the latitude is north or south latitude. The ASCII value 'N' indicates north latitude, and 'S' is south latitude.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<string?> ReadGpsLatitudeReferenceAsync(this TiffTagReader tagReader)
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
        /// Read the values of GPSLatitudeReference tag.
        /// Field description: Indicates whether the latitude is north or south latitude. The ASCII value 'N' indicates north latitude, and 'S' is south latitude.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static string? ReadGpsLatitudeReference(this TiffTagReader tagReader)
        {
            TiffValueCollection<string> result = tagReader.ReadASCIIField((TiffTag)0x0001);
            return result.GetFirstOrDefault();
        }

        #endregion
    
        #region GPSLatitude

        /// <summary>
        /// Read the values of GPSLatitude tag.
        /// Field description: Indicates the latitude. The latitude is expressed as three RATIONAL values giving the degrees, minutes, and seconds, respectively. If latitude is expressed as degrees, minutes and seconds, a typical format would be dd/1,mm/1,ss/1. When degrees and minutes are used and, for example, fractions of minutes are given up to two decimal places, the format would be dd/1,mmmm/100,0/1.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffRational[]> ReadGpsLatitudeAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<TiffRational>> valueTask = tagReader.ReadRationalFieldAsync((TiffTag)0x0002);
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
        
        /// <summary>
        /// Read the values of GPSLatitude tag.
        /// Field description: Indicates the latitude. The latitude is expressed as three RATIONAL values giving the degrees, minutes, and seconds, respectively. If latitude is expressed as degrees, minutes and seconds, a typical format would be dd/1,mm/1,ss/1. When degrees and minutes are used and, for example, fractions of minutes are given up to two decimal places, the format would be dd/1,mmmm/100,0/1.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffRational[] ReadGpsLatitude(this TiffTagReader tagReader)
        {
            TiffValueCollection<TiffRational> result = tagReader.ReadRationalField((TiffTag)0x0002);
            return result.GetOrCreateArray();
        }

        #endregion
    
        #region GPSLongitudeReference

        /// <summary>
        /// Read the values of GPSLongitudeReference tag.
        /// Field description: Indicates whether the longitude is east or west longitude. ASCII 'E' indicates east longitude, and 'W' is west longitude.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<string?> ReadGpsLongitudeReferenceAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<string>> valueTask = tagReader.ReadASCIIFieldAsync((TiffTag)0x0003);
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
        /// Read the values of GPSLongitudeReference tag.
        /// Field description: Indicates whether the longitude is east or west longitude. ASCII 'E' indicates east longitude, and 'W' is west longitude.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static string? ReadGpsLongitudeReference(this TiffTagReader tagReader)
        {
            TiffValueCollection<string> result = tagReader.ReadASCIIField((TiffTag)0x0003);
            return result.GetFirstOrDefault();
        }

        #endregion
    
        #region GPSLongitude

        /// <summary>
        /// Read the values of GPSLongitude tag.
        /// Field description: Indicates the longitude. The longitude is expressed as three RATIONAL values giving the degrees, minutes, and seconds, respectively. If longitude is expressed as degrees, minutes and seconds, a typical format would be ddd/1,mm/1,ss/1. When degrees and minutes are used and, for example, fractions of minutes are given up to two decimal places, the format would be ddd/1,mmmm/100,0/1.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffRational[]> ReadGpsLongitudeAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<TiffRational>> valueTask = tagReader.ReadRationalFieldAsync((TiffTag)0x0004);
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
        
        /// <summary>
        /// Read the values of GPSLongitude tag.
        /// Field description: Indicates the longitude. The longitude is expressed as three RATIONAL values giving the degrees, minutes, and seconds, respectively. If longitude is expressed as degrees, minutes and seconds, a typical format would be ddd/1,mm/1,ss/1. When degrees and minutes are used and, for example, fractions of minutes are given up to two decimal places, the format would be ddd/1,mmmm/100,0/1.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffRational[] ReadGpsLongitude(this TiffTagReader tagReader)
        {
            TiffValueCollection<TiffRational> result = tagReader.ReadRationalField((TiffTag)0x0004);
            return result.GetOrCreateArray();
        }

        #endregion
    
        #region GPSAltitudeReference

        /// <summary>
        /// Read the values of GPSAltitudeReference tag.
        /// Field description: Indicates the altitude used as the reference altitude. If the reference is sea level and the altitude is above sea level, 0 is given. If the altitude is below sea level, a value of 1 is given and the altitude is indicated as an absolute value in the GPSAltitude tag. The reference unit is meters. Note that this tag is BYTE type, unlike other reference tags.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffGpsAltitudeReference> ReadGpsAltitudeReferenceAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<byte>> valueTask = tagReader.ReadByteFieldAsync((TiffTag)0x0005);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<byte> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffGpsAltitudeReference>((TiffGpsAltitudeReference)result.GetFirstOrDefault());
            }

            return new ValueTask<TiffGpsAltitudeReference>(TransformValueTaskAsync(valueTask));

            static async Task<TiffGpsAltitudeReference> TransformValueTaskAsync(ValueTask<TiffValueCollection<byte>> valueTask)
            {
                TiffValueCollection<byte> result = await valueTask.ConfigureAwait(false);
                return (TiffGpsAltitudeReference)result.GetFirstOrDefault();
            }
        }
        
        /// <summary>
        /// Read the values of GPSAltitudeReference tag.
        /// Field description: Indicates the altitude used as the reference altitude. If the reference is sea level and the altitude is above sea level, 0 is given. If the altitude is below sea level, a value of 1 is given and the altitude is indicated as an absolute value in the GPSAltitude tag. The reference unit is meters. Note that this tag is BYTE type, unlike other reference tags.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffGpsAltitudeReference ReadGpsAltitudeReference(this TiffTagReader tagReader)
        {
            TiffValueCollection<byte> result = tagReader.ReadByteField((TiffTag)0x0005);
            return (TiffGpsAltitudeReference)result.GetFirstOrDefault();
        }

        #endregion
    
        #region GPSAltitude

        /// <summary>
        /// Read the values of GPSAltitude tag.
        /// Field description: Indicates the altitude based on the reference in GPSAltitudeRef. Altitude is expressed as one RATIONAL value. The reference unit is meters.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffRational?> ReadGpsAltitudeAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<TiffRational>> valueTask = tagReader.ReadRationalFieldAsync((TiffTag)0x0006);
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
        /// Read the values of GPSAltitude tag.
        /// Field description: Indicates the altitude based on the reference in GPSAltitudeRef. Altitude is expressed as one RATIONAL value. The reference unit is meters.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffRational? ReadGpsAltitude(this TiffTagReader tagReader)
        {
            TiffValueCollection<TiffRational> result = tagReader.ReadRationalField((TiffTag)0x0006);
            return result.IsEmpty ? default(TiffRational?) : result.GetFirstOrDefault();
        }

        #endregion
    
        #region GPSTimeStamp

        /// <summary>
        /// Read the values of GPSTimeStamp tag.
        /// Field description: Indicates the time as UTC (Coordinated Universal Time). TimeStamp is expressed as three RATIONAL values giving the hour, minute, and second.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffRational[]> ReadGpsTimeStampAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<TiffRational>> valueTask = tagReader.ReadRationalFieldAsync((TiffTag)0x0007);
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
        
        /// <summary>
        /// Read the values of GPSTimeStamp tag.
        /// Field description: Indicates the time as UTC (Coordinated Universal Time). TimeStamp is expressed as three RATIONAL values giving the hour, minute, and second.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffRational[] ReadGpsTimeStamp(this TiffTagReader tagReader)
        {
            TiffValueCollection<TiffRational> result = tagReader.ReadRationalField((TiffTag)0x0007);
            return result.GetOrCreateArray();
        }

        #endregion
    
        #region GPSSatellites

        /// <summary>
        /// Read the values of GPSSatellites tag.
        /// Field description: Indicates the GPS satellites used for measurements. This tag can be used to describe the number of satellites, their ID number, angle of elevation, azimuth, SNR and other information in ASCII notation. The format is not specified. If the GPS receiver is incapable of taking measurements, value of the tag shall be set to NULL.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffValueCollection<string>> ReadGpsSatellitesAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<string>> valueTask = tagReader.ReadASCIIFieldAsync((TiffTag)0x0008);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<string> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffValueCollection<string>>(result);
            }

            return new ValueTask<TiffValueCollection<string>>(TransformValueTaskAsync(valueTask));

            static async Task<TiffValueCollection<string>> TransformValueTaskAsync(ValueTask<TiffValueCollection<string>> valueTask)
            {
                TiffValueCollection<string> result = await valueTask.ConfigureAwait(false);
                return result;
            }
        }
        
        /// <summary>
        /// Read the values of GPSSatellites tag.
        /// Field description: Indicates the GPS satellites used for measurements. This tag can be used to describe the number of satellites, their ID number, angle of elevation, azimuth, SNR and other information in ASCII notation. The format is not specified. If the GPS receiver is incapable of taking measurements, value of the tag shall be set to NULL.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffValueCollection<string> ReadGpsSatellites(this TiffTagReader tagReader)
        {
            TiffValueCollection<string> result = tagReader.ReadASCIIField((TiffTag)0x0008);
            return result;
        }

        #endregion
    
        #region GPSStatus

        /// <summary>
        /// Read the values of GPSStatus tag.
        /// Field description: Indicates the status of the GPS receiver when the image is recorded. 'A' = Measurement is in progress. 'V' = Measurement is Interoperability.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<string?> ReadGpsStatusAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<string>> valueTask = tagReader.ReadASCIIFieldAsync((TiffTag)0x0009);
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
        /// Read the values of GPSStatus tag.
        /// Field description: Indicates the status of the GPS receiver when the image is recorded. 'A' = Measurement is in progress. 'V' = Measurement is Interoperability.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static string? ReadGpsStatus(this TiffTagReader tagReader)
        {
            TiffValueCollection<string> result = tagReader.ReadASCIIField((TiffTag)0x0009);
            return result.GetFirstOrDefault();
        }

        #endregion
    
        #region GPSMeasureMode

        /// <summary>
        /// Read the values of GPSMeasureMode tag.
        /// Field description: Indicates the GPS measurement mode. '2' = 2-dimensional measurement. '3' = 3-dimensional measurement.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<string?> ReadGpsMeasureModeAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<string>> valueTask = tagReader.ReadASCIIFieldAsync((TiffTag)0x000A);
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
        /// Read the values of GPSMeasureMode tag.
        /// Field description: Indicates the GPS measurement mode. '2' = 2-dimensional measurement. '3' = 3-dimensional measurement.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static string? ReadGpsMeasureMode(this TiffTagReader tagReader)
        {
            TiffValueCollection<string> result = tagReader.ReadASCIIField((TiffTag)0x000A);
            return result.GetFirstOrDefault();
        }

        #endregion
    
        #region GPSDilutionOfPrecision

        /// <summary>
        /// Read the values of GPSDilutionOfPrecision tag.
        /// Field description: Indicates the GPS DOP (data degree of precision). An HDOP value is written during two-dimensional measurement, and PDOP during three-dimensional measurement.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffRational?> ReadGpsDilutionOfPrecisionAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<TiffRational>> valueTask = tagReader.ReadRationalFieldAsync((TiffTag)0x000B);
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
        /// Read the values of GPSDilutionOfPrecision tag.
        /// Field description: Indicates the GPS DOP (data degree of precision). An HDOP value is written during two-dimensional measurement, and PDOP during three-dimensional measurement.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffRational? ReadGpsDilutionOfPrecision(this TiffTagReader tagReader)
        {
            TiffValueCollection<TiffRational> result = tagReader.ReadRationalField((TiffTag)0x000B);
            return result.IsEmpty ? default(TiffRational?) : result.GetFirstOrDefault();
        }

        #endregion
    
        #region GPSSpeedReference

        /// <summary>
        /// Read the values of GPSSpeedReference tag.
        /// Field description: Indicates the unit used to express the GPS receiver speed of movement. 'K' = Kilometers per hour. 'M' = Miles per hour. 'N' = Knots.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<string?> ReadGpsSpeedReferenceAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<string>> valueTask = tagReader.ReadASCIIFieldAsync((TiffTag)0x000C);
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
        /// Read the values of GPSSpeedReference tag.
        /// Field description: Indicates the unit used to express the GPS receiver speed of movement. 'K' = Kilometers per hour. 'M' = Miles per hour. 'N' = Knots.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static string? ReadGpsSpeedReference(this TiffTagReader tagReader)
        {
            TiffValueCollection<string> result = tagReader.ReadASCIIField((TiffTag)0x000C);
            return result.GetFirstOrDefault();
        }

        #endregion
    
        #region GPSSpeed

        /// <summary>
        /// Read the values of GPSSpeed tag.
        /// Field description: Indicates the speed of GPS receiver movement.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffRational?> ReadGpsSpeedAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<TiffRational>> valueTask = tagReader.ReadRationalFieldAsync((TiffTag)0x000D);
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
        /// Read the values of GPSSpeed tag.
        /// Field description: Indicates the speed of GPS receiver movement.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffRational? ReadGpsSpeed(this TiffTagReader tagReader)
        {
            TiffValueCollection<TiffRational> result = tagReader.ReadRationalField((TiffTag)0x000D);
            return result.IsEmpty ? default(TiffRational?) : result.GetFirstOrDefault();
        }

        #endregion
    
        #region GPSTrackReference

        /// <summary>
        /// Read the values of GPSTrackReference tag.
        /// Field description: Indicates the reference for giving the direction of GPS receiver movement. 'T' = True direction. 'M' = Magnetic direction.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<string?> ReadGpsTrackReferenceAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<string>> valueTask = tagReader.ReadASCIIFieldAsync((TiffTag)0x000E);
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
        /// Read the values of GPSTrackReference tag.
        /// Field description: Indicates the reference for giving the direction of GPS receiver movement. 'T' = True direction. 'M' = Magnetic direction.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static string? ReadGpsTrackReference(this TiffTagReader tagReader)
        {
            TiffValueCollection<string> result = tagReader.ReadASCIIField((TiffTag)0x000E);
            return result.GetFirstOrDefault();
        }

        #endregion
    
        #region GPSTrack

        /// <summary>
        /// Read the values of GPSTrack tag.
        /// Field description: Indicates the direction of GPS receiver movement. The range of values is from 0.00 to 359.99.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffRational?> ReadGpsTrackAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<TiffRational>> valueTask = tagReader.ReadRationalFieldAsync((TiffTag)0x000F);
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
        /// Read the values of GPSTrack tag.
        /// Field description: Indicates the direction of GPS receiver movement. The range of values is from 0.00 to 359.99.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffRational? ReadGpsTrack(this TiffTagReader tagReader)
        {
            TiffValueCollection<TiffRational> result = tagReader.ReadRationalField((TiffTag)0x000F);
            return result.IsEmpty ? default(TiffRational?) : result.GetFirstOrDefault();
        }

        #endregion
    
        #region GPSImageDirectionReference

        /// <summary>
        /// Read the values of GPSImageDirectionReference tag.
        /// Field description: Indicates the reference for giving the direction of the image when it is captured. 'T' = True direction. 'M' = Magnetic direction.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<string?> ReadGpsImageDirectionReferenceAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<string>> valueTask = tagReader.ReadASCIIFieldAsync((TiffTag)0x0010);
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
        /// Read the values of GPSImageDirectionReference tag.
        /// Field description: Indicates the reference for giving the direction of the image when it is captured. 'T' = True direction. 'M' = Magnetic direction.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static string? ReadGpsImageDirectionReference(this TiffTagReader tagReader)
        {
            TiffValueCollection<string> result = tagReader.ReadASCIIField((TiffTag)0x0010);
            return result.GetFirstOrDefault();
        }

        #endregion
    
        #region GPSImageDirection

        /// <summary>
        /// Read the values of GPSImageDirection tag.
        /// Field description: Indicates the direction of the image when it was captured. The range of values is from 0.00 to 359.99.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffRational?> ReadGpsImageDirectionAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<TiffRational>> valueTask = tagReader.ReadRationalFieldAsync((TiffTag)0x000F);
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
        /// Read the values of GPSImageDirection tag.
        /// Field description: Indicates the direction of the image when it was captured. The range of values is from 0.00 to 359.99.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffRational? ReadGpsImageDirection(this TiffTagReader tagReader)
        {
            TiffValueCollection<TiffRational> result = tagReader.ReadRationalField((TiffTag)0x000F);
            return result.IsEmpty ? default(TiffRational?) : result.GetFirstOrDefault();
        }

        #endregion
    
        #region GPSMapDatum

        /// <summary>
        /// Read the values of GPSMapDatum tag.
        /// Field description: Indicates the geodetic survey data used by the GPS receiver. If the survey data is restricted to Japan, the value of this tag is 'TOKYO' or 'WGS-84'. If a GPS Info tag is recorded, it is strongly recommended that this tag be recorded.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<string?> ReadGpsMapDatumAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<string>> valueTask = tagReader.ReadASCIIFieldAsync((TiffTag)0x0012);
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
        /// Read the values of GPSMapDatum tag.
        /// Field description: Indicates the geodetic survey data used by the GPS receiver. If the survey data is restricted to Japan, the value of this tag is 'TOKYO' or 'WGS-84'. If a GPS Info tag is recorded, it is strongly recommended that this tag be recorded.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static string? ReadGpsMapDatum(this TiffTagReader tagReader)
        {
            TiffValueCollection<string> result = tagReader.ReadASCIIField((TiffTag)0x0012);
            return result.GetFirstOrDefault();
        }

        #endregion
    
        #region GPSDestinationLatitudeReference

        /// <summary>
        /// Read the values of GPSDestinationLatitudeReference tag.
        /// Field description: Indicates whether the latitude of the destination point is north or south latitude. 'N' = North latitude. 'S' = South latitude.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<string?> ReadGpsDestinationLatitudeReferenceAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<string>> valueTask = tagReader.ReadASCIIFieldAsync((TiffTag)0x0013);
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
        /// Read the values of GPSDestinationLatitudeReference tag.
        /// Field description: Indicates whether the latitude of the destination point is north or south latitude. 'N' = North latitude. 'S' = South latitude.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static string? ReadGpsDestinationLatitudeReference(this TiffTagReader tagReader)
        {
            TiffValueCollection<string> result = tagReader.ReadASCIIField((TiffTag)0x0013);
            return result.GetFirstOrDefault();
        }

        #endregion
    
        #region GPSDestinationLatitude

        /// <summary>
        /// Read the values of GPSDestinationLatitude tag.
        /// Field description: Indicates the latitude of the destination point. The latitude is expressed as three RATIONAL values giving the degrees, minutes, and seconds, respectively. If latitude is expressed as degrees, minutes and seconds, a typical format would be dd/1,mm/1,ss/1. When degrees and minutes are used and, for example, fractions of minutes are given up to two decimal places, the format would be dd/1,mmmm/100,0/1.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffRational[]> ReadGpsDestinationLatitudeAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<TiffRational>> valueTask = tagReader.ReadRationalFieldAsync((TiffTag)0x0014);
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
        
        /// <summary>
        /// Read the values of GPSDestinationLatitude tag.
        /// Field description: Indicates the latitude of the destination point. The latitude is expressed as three RATIONAL values giving the degrees, minutes, and seconds, respectively. If latitude is expressed as degrees, minutes and seconds, a typical format would be dd/1,mm/1,ss/1. When degrees and minutes are used and, for example, fractions of minutes are given up to two decimal places, the format would be dd/1,mmmm/100,0/1.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffRational[] ReadGpsDestinationLatitude(this TiffTagReader tagReader)
        {
            TiffValueCollection<TiffRational> result = tagReader.ReadRationalField((TiffTag)0x0014);
            return result.GetOrCreateArray();
        }

        #endregion
    
        #region GPSDestinationLongitudeReference

        /// <summary>
        /// Read the values of GPSDestinationLongitudeReference tag.
        /// Field description: Indicates whether the longitude of the destination point is east or west longitude. 'E' = East longitude. 'W' = West longitude.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<string?> ReadGpsDestinationLongitudeReferenceAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<string>> valueTask = tagReader.ReadASCIIFieldAsync((TiffTag)0x0015);
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
        /// Read the values of GPSDestinationLongitudeReference tag.
        /// Field description: Indicates whether the longitude of the destination point is east or west longitude. 'E' = East longitude. 'W' = West longitude.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static string? ReadGpsDestinationLongitudeReference(this TiffTagReader tagReader)
        {
            TiffValueCollection<string> result = tagReader.ReadASCIIField((TiffTag)0x0015);
            return result.GetFirstOrDefault();
        }

        #endregion
    
        #region GPSDestinationLongitude

        /// <summary>
        /// Read the values of GPSDestinationLongitude tag.
        /// Field description: Indicates the longitude of the destination point. The longitude is expressed as three RATIONAL values giving the degrees, minutes, and seconds, respectively. If longitude is expressed as degrees, minutes and seconds, a typical format would be ddd/1,mm/1,ss/1. When degrees and minutes are used and, for example, fractions of minutes are given up to two decimal places, the format would be ddd/1,mmmm/100,0/1.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffRational[]> ReadGpsDestinationLongitudeAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<TiffRational>> valueTask = tagReader.ReadRationalFieldAsync((TiffTag)0x0014);
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
        
        /// <summary>
        /// Read the values of GPSDestinationLongitude tag.
        /// Field description: Indicates the longitude of the destination point. The longitude is expressed as three RATIONAL values giving the degrees, minutes, and seconds, respectively. If longitude is expressed as degrees, minutes and seconds, a typical format would be ddd/1,mm/1,ss/1. When degrees and minutes are used and, for example, fractions of minutes are given up to two decimal places, the format would be ddd/1,mmmm/100,0/1.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffRational[] ReadGpsDestinationLongitude(this TiffTagReader tagReader)
        {
            TiffValueCollection<TiffRational> result = tagReader.ReadRationalField((TiffTag)0x0014);
            return result.GetOrCreateArray();
        }

        #endregion
    
        #region GPSDestinationBearingReference

        /// <summary>
        /// Read the values of GPSDestinationBearingReference tag.
        /// Field description: Indicates the reference used for giving the bearing to the destination point. 'T' = True direction. 'M' = Magnetic direction.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<string?> ReadGpsDestinationBearingReferenceAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<string>> valueTask = tagReader.ReadASCIIFieldAsync((TiffTag)0x0017);
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
        /// Read the values of GPSDestinationBearingReference tag.
        /// Field description: Indicates the reference used for giving the bearing to the destination point. 'T' = True direction. 'M' = Magnetic direction.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static string? ReadGpsDestinationBearingReference(this TiffTagReader tagReader)
        {
            TiffValueCollection<string> result = tagReader.ReadASCIIField((TiffTag)0x0017);
            return result.GetFirstOrDefault();
        }

        #endregion
    
        #region GPSDestinationBearing

        /// <summary>
        /// Read the values of GPSDestinationBearing tag.
        /// Field description: Indicates the bearing to the destination point. The range of values is from 0.00 to 359.99.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffRational?> ReadGpsDestinationBearingAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<TiffRational>> valueTask = tagReader.ReadRationalFieldAsync((TiffTag)0x0018);
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
        /// Read the values of GPSDestinationBearing tag.
        /// Field description: Indicates the bearing to the destination point. The range of values is from 0.00 to 359.99.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffRational? ReadGpsDestinationBearing(this TiffTagReader tagReader)
        {
            TiffValueCollection<TiffRational> result = tagReader.ReadRationalField((TiffTag)0x0018);
            return result.IsEmpty ? default(TiffRational?) : result.GetFirstOrDefault();
        }

        #endregion
    
        #region GPSDestinationDistanceReference

        /// <summary>
        /// Read the values of GPSDestinationDistanceReference tag.
        /// Field description: Indicates the unit used to express the distance to the destination point. 'K' = Kilometers. 'M' = Miles. 'N' = Knots.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<string?> ReadGpsDestinationDistanceReferenceAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<string>> valueTask = tagReader.ReadASCIIFieldAsync((TiffTag)0x0019);
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
        /// Read the values of GPSDestinationDistanceReference tag.
        /// Field description: Indicates the unit used to express the distance to the destination point. 'K' = Kilometers. 'M' = Miles. 'N' = Knots.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static string? ReadGpsDestinationDistanceReference(this TiffTagReader tagReader)
        {
            TiffValueCollection<string> result = tagReader.ReadASCIIField((TiffTag)0x0019);
            return result.GetFirstOrDefault();
        }

        #endregion
    
        #region GPSDestinationDistance

        /// <summary>
        /// Read the values of GPSDestinationDistance tag.
        /// Field description: Indicates the distance to the destination point.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffRational?> ReadGpsDestinationDistanceAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<TiffRational>> valueTask = tagReader.ReadRationalFieldAsync((TiffTag)0x001A);
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
        /// Read the values of GPSDestinationDistance tag.
        /// Field description: Indicates the distance to the destination point.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffRational? ReadGpsDestinationDistance(this TiffTagReader tagReader)
        {
            TiffValueCollection<TiffRational> result = tagReader.ReadRationalField((TiffTag)0x001A);
            return result.IsEmpty ? default(TiffRational?) : result.GetFirstOrDefault();
        }

        #endregion
    
        #region GPSProcessingMethod

        /// <summary>
        /// Read the values of GPSProcessingMethod tag.
        /// Field description: A character string recording the name of the method used for location finding. The first byte indicates the character code used, and this is followed by the name of the method. Since the Type is not ASCII, NULL termination is not necessary.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<byte[]> ReadGpsProcessingMethodAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<byte>> valueTask = tagReader.ReadByteFieldAsync((TiffTag)0x001B);
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
        /// Read the values of GPSProcessingMethod tag.
        /// Field description: A character string recording the name of the method used for location finding. The first byte indicates the character code used, and this is followed by the name of the method. Since the Type is not ASCII, NULL termination is not necessary.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static byte[] ReadGpsProcessingMethod(this TiffTagReader tagReader)
        {
            TiffValueCollection<byte> result = tagReader.ReadByteField((TiffTag)0x001B);
            return result.GetOrCreateArray();
        }

        #endregion
    
        #region GPSAreaInformation

        /// <summary>
        /// Read the values of GPSAreaInformation tag.
        /// Field description: A character string recording the name of the GPS area. The first byte indicates the character code used, and this is followed by the name of the GPS area. Since the Type is not ASCII, NULL termination is not necessary.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<byte[]> ReadGpsAreaInformationAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<byte>> valueTask = tagReader.ReadByteFieldAsync((TiffTag)0x001C);
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
        /// Read the values of GPSAreaInformation tag.
        /// Field description: A character string recording the name of the GPS area. The first byte indicates the character code used, and this is followed by the name of the GPS area. Since the Type is not ASCII, NULL termination is not necessary.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static byte[] ReadGpsAreaInformation(this TiffTagReader tagReader)
        {
            TiffValueCollection<byte> result = tagReader.ReadByteField((TiffTag)0x001C);
            return result.GetOrCreateArray();
        }

        #endregion
    
        #region GPSDateStamp

        /// <summary>
        /// Read the values of GPSDateStamp tag.
        /// Field description: A character string recording date and time information relative to UTC (Coordinated Universal Time). The format is "YYYY:MM:DD." The length of the string is 11 bytes including NULL.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<string?> ReadGpsDateStampAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<string>> valueTask = tagReader.ReadASCIIFieldAsync((TiffTag)0x001D);
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
        /// Read the values of GPSDateStamp tag.
        /// Field description: A character string recording date and time information relative to UTC (Coordinated Universal Time). The format is "YYYY:MM:DD." The length of the string is 11 bytes including NULL.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static string? ReadGpsDateStamp(this TiffTagReader tagReader)
        {
            TiffValueCollection<string> result = tagReader.ReadASCIIField((TiffTag)0x001D);
            return result.GetFirstOrDefault();
        }

        #endregion
    
        #region GPSDifferential

        /// <summary>
        /// Read the values of GPSDifferential tag.
        /// Field description: Indicates whether differential correction is applied to the GPS receiver. 0 = Measurement without differential correction. 1 = Differential correction applied.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<ushort?> ReadGpsDifferentialAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync((TiffTag)0x001E);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<ushort> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<ushort?>(result.IsEmpty ? default(ushort?) : result.GetFirstOrDefault());
            }

            return new ValueTask<ushort?>(TransformValueTaskAsync(valueTask));

            static async Task<ushort?> TransformValueTaskAsync(ValueTask<TiffValueCollection<ushort>> valueTask)
            {
                TiffValueCollection<ushort> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? default(ushort?) : result.GetFirstOrDefault();
            }
        }
        
        /// <summary>
        /// Read the values of GPSDifferential tag.
        /// Field description: Indicates whether differential correction is applied to the GPS receiver. 0 = Measurement without differential correction. 1 = Differential correction applied.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static ushort? ReadGpsDifferential(this TiffTagReader tagReader)
        {
            TiffValueCollection<ushort> result = tagReader.ReadShortField((TiffTag)0x001E);
            return result.IsEmpty ? default(ushort?) : result.GetFirstOrDefault();
        }

        #endregion
    
    }
}
