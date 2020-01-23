using System.Threading.Tasks;

namespace TiffLibrary.Exif
{
    public static partial class TiffTagExifReaderExtensions
    {
    
        #region ExposureTime

        /// <summary>
        /// Read the values of ExposureTime tag.
        /// Field description: Exposure time, given in seconds.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffRational?> ReadExifExposureTimeAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<TiffRational>> valueTask = tagReader.ReadRationalFieldAsync((TiffTag)0x829A);
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
        /// Read the values of ExposureTime tag.
        /// Field description: Exposure time, given in seconds.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffRational? ReadExifExposureTime(this TiffTagReader tagReader)
        {
            TiffValueCollection<TiffRational> result = tagReader.ReadRationalField((TiffTag)0x829A);
            return result.IsEmpty ? default(TiffRational?) : result.GetFirstOrDefault();
        }

        #endregion
    
        #region FNumber

        /// <summary>
        /// Read the values of FNumber tag.
        /// Field description: The F number.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffRational?> ReadExifFNumberAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<TiffRational>> valueTask = tagReader.ReadRationalFieldAsync((TiffTag)0x829D);
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
        /// Read the values of FNumber tag.
        /// Field description: The F number.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffRational? ReadExifFNumber(this TiffTagReader tagReader)
        {
            TiffValueCollection<TiffRational> result = tagReader.ReadRationalField((TiffTag)0x829D);
            return result.IsEmpty ? default(TiffRational?) : result.GetFirstOrDefault();
        }

        #endregion
    
        #region ExposureProgram

        /// <summary>
        /// Read the values of ExposureProgram tag.
        /// Field description: The class of the program used by the camera to set exposure when the picture is taken.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffExifExposureProgram> ReadExifExposureProgramAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync((TiffTag)0x8822);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<ushort> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffExifExposureProgram>((TiffExifExposureProgram)result.GetFirstOrDefault());
            }

            return new ValueTask<TiffExifExposureProgram>(TransformValueTaskAsync(valueTask));

            static async Task<TiffExifExposureProgram> TransformValueTaskAsync(ValueTask<TiffValueCollection<ushort>> valueTask)
            {
                TiffValueCollection<ushort> result = await valueTask.ConfigureAwait(false);
                return (TiffExifExposureProgram)result.GetFirstOrDefault();
            }
        }
        
        /// <summary>
        /// Read the values of ExposureProgram tag.
        /// Field description: The class of the program used by the camera to set exposure when the picture is taken.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffExifExposureProgram ReadExifExposureProgram(this TiffTagReader tagReader)
        {
            TiffValueCollection<ushort> result = tagReader.ReadShortField((TiffTag)0x8822);
            return (TiffExifExposureProgram)result.GetFirstOrDefault();
        }

        #endregion
    
        #region SpectralSensitivity

        /// <summary>
        /// Read the values of SpectralSensitivity tag.
        /// Field description: Indicates the spectral sensitivity of each channel of the camera used. The tag value is an ASCII string compatible with the standard developed by the ASTM Technical committee.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffValueCollection<string>> ReadExifSpectralSensitivityAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<string>> valueTask = tagReader.ReadASCIIFieldAsync((TiffTag)0x8824);
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
        /// Read the values of SpectralSensitivity tag.
        /// Field description: Indicates the spectral sensitivity of each channel of the camera used. The tag value is an ASCII string compatible with the standard developed by the ASTM Technical committee.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffValueCollection<string> ReadExifSpectralSensitivity(this TiffTagReader tagReader)
        {
            TiffValueCollection<string> result = tagReader.ReadASCIIField((TiffTag)0x8824);
            return result;
        }

        #endregion
    
        #region ISOSpeedRatings

        /// <summary>
        /// Read the values of ISOSpeedRatings tag.
        /// Field description: Indicates the ISO Speed and ISO Latitude of the camera or input device as specified in ISO 12232.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffValueCollection<ushort>> ReadExifISOSpeedRatingsAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync((TiffTag)0x8827);
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
        
        /// <summary>
        /// Read the values of ISOSpeedRatings tag.
        /// Field description: Indicates the ISO Speed and ISO Latitude of the camera or input device as specified in ISO 12232.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffValueCollection<ushort> ReadExifISOSpeedRatings(this TiffTagReader tagReader)
        {
            TiffValueCollection<ushort> result = tagReader.ReadShortField((TiffTag)0x8827);
            return result;
        }

        #endregion
    
        #region OECF

        /// <summary>
        /// Read the values of OECF tag.
        /// Field description: Indicates the Opto-Electric Conversion Function (OECF) specified in ISO 14524.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<byte[]> ReadExifOECFAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<byte>> valueTask = tagReader.ReadByteFieldAsync((TiffTag)0x8828);
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
        /// Read the values of OECF tag.
        /// Field description: Indicates the Opto-Electric Conversion Function (OECF) specified in ISO 14524.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static byte[] ReadExifOECF(this TiffTagReader tagReader)
        {
            TiffValueCollection<byte> result = tagReader.ReadByteField((TiffTag)0x8828);
            return result.GetOrCreateArray();
        }

        #endregion
    
        #region ExifVersion

        /// <summary>
        /// Read the values of ExifVersion tag.
        /// Field description: The version of the supported Exif standard.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<byte[]> ReadExifVersionAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<byte>> valueTask = tagReader.ReadByteFieldAsync((TiffTag)0x9000);
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
        /// Read the values of ExifVersion tag.
        /// Field description: The version of the supported Exif standard.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static byte[] ReadExifVersion(this TiffTagReader tagReader)
        {
            TiffValueCollection<byte> result = tagReader.ReadByteField((TiffTag)0x9000);
            return result.GetOrCreateArray();
        }

        #endregion
    
        #region DateTimeOriginal

        /// <summary>
        /// Read the values of DateTimeOriginal tag.
        /// Field description: The date and time when the original image data was generated. For a digital still camera, this is the date and time the picture was taken or recorded. The format is "YYYY:MM:DD HH:MM:SS" with time shown in 24-hour format, and the date and time separated by one blank character (hex 20).
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<string?> ReadExifDateTimeOriginalAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<string>> valueTask = tagReader.ReadASCIIFieldAsync((TiffTag)0x9003);
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
        /// Read the values of DateTimeOriginal tag.
        /// Field description: The date and time when the original image data was generated. For a digital still camera, this is the date and time the picture was taken or recorded. The format is "YYYY:MM:DD HH:MM:SS" with time shown in 24-hour format, and the date and time separated by one blank character (hex 20).
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static string? ReadExifDateTimeOriginal(this TiffTagReader tagReader)
        {
            TiffValueCollection<string> result = tagReader.ReadASCIIField((TiffTag)0x9003);
            return result.GetFirstOrDefault();
        }

        #endregion
    
        #region DateTimeDigitized

        /// <summary>
        /// Read the values of DateTimeDigitized tag.
        /// Field description: The date and time when the image was stored as digital data. If, for example, an image was captured by a digital still camera, and at the same time the file was recorded, then the DateTimeOriginal and DateTimeDigitized will have the same contents. The format is "YYYY:MM:DD HH:MM:SS" with time shown in 24-hour format, and the date and time separated by one blank character (hex 20).
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<string?> ReadExifDateTimeDigitizedAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<string>> valueTask = tagReader.ReadASCIIFieldAsync((TiffTag)0x9004);
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
        /// Read the values of DateTimeDigitized tag.
        /// Field description: The date and time when the image was stored as digital data. If, for example, an image was captured by a digital still camera, and at the same time the file was recorded, then the DateTimeOriginal and DateTimeDigitized will have the same contents. The format is "YYYY:MM:DD HH:MM:SS" with time shown in 24-hour format, and the date and time separated by one blank character (hex 20).
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static string? ReadExifDateTimeDigitized(this TiffTagReader tagReader)
        {
            TiffValueCollection<string> result = tagReader.ReadASCIIField((TiffTag)0x9004);
            return result.GetFirstOrDefault();
        }

        #endregion
    
        #region ComponentsConfiguration

        /// <summary>
        /// Read the values of ComponentsConfiguration tag.
        /// Field description: Specific to compressed data; specifies the channels and complements PhotometricInterpretation.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<byte[]> ReadExifComponentsConfigurationAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<byte>> valueTask = tagReader.ReadByteFieldAsync((TiffTag)0x9101);
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
        /// Read the values of ComponentsConfiguration tag.
        /// Field description: Specific to compressed data; specifies the channels and complements PhotometricInterpretation.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static byte[] ReadExifComponentsConfiguration(this TiffTagReader tagReader)
        {
            TiffValueCollection<byte> result = tagReader.ReadByteField((TiffTag)0x9101);
            return result.GetOrCreateArray();
        }

        #endregion
    
        #region CompressedBitsPerPixel

        /// <summary>
        /// Read the values of CompressedBitsPerPixel tag.
        /// Field description: Specific to compressed data; states the compressed bits per pixel. The compression mode used for a compressed image is indicated in unit bits per pixel.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffRational?> ReadExifCompressedBitsPerPixelAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<TiffRational>> valueTask = tagReader.ReadRationalFieldAsync((TiffTag)0x9102);
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
        /// Read the values of CompressedBitsPerPixel tag.
        /// Field description: Specific to compressed data; states the compressed bits per pixel. The compression mode used for a compressed image is indicated in unit bits per pixel.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffRational? ReadExifCompressedBitsPerPixel(this TiffTagReader tagReader)
        {
            TiffValueCollection<TiffRational> result = tagReader.ReadRationalField((TiffTag)0x9102);
            return result.IsEmpty ? default(TiffRational?) : result.GetFirstOrDefault();
        }

        #endregion
    
        #region ShutterSpeedValue

        /// <summary>
        /// Read the values of ShutterSpeedValue tag.
        /// Field description: Shutter speed. The unit is the APEX (Additive System of Photographic Exposure) setting.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffSRational?> ReadExifShutterSpeedValueAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<TiffSRational>> valueTask = tagReader.ReadSRationalFieldAsync((TiffTag)0x9201);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<TiffSRational> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffSRational?>(result.IsEmpty ? default(TiffSRational?) : result.GetFirstOrDefault());
            }

            return new ValueTask<TiffSRational?>(TransformValueTaskAsync(valueTask));

            static async Task<TiffSRational?> TransformValueTaskAsync(ValueTask<TiffValueCollection<TiffSRational>> valueTask)
            {
                TiffValueCollection<TiffSRational> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? default(TiffSRational?) : result.GetFirstOrDefault();
            }
        }
        
        /// <summary>
        /// Read the values of ShutterSpeedValue tag.
        /// Field description: Shutter speed. The unit is the APEX (Additive System of Photographic Exposure) setting.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffSRational? ReadExifShutterSpeedValue(this TiffTagReader tagReader)
        {
            TiffValueCollection<TiffSRational> result = tagReader.ReadSRationalField((TiffTag)0x9201);
            return result.IsEmpty ? default(TiffSRational?) : result.GetFirstOrDefault();
        }

        #endregion
    
        #region ApertureValue

        /// <summary>
        /// Read the values of ApertureValue tag.
        /// Field description: The lens aperture. The unit is the APEX (Additive System of Photographic Exposure) setting.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffRational?> ReadExifApertureValueAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<TiffRational>> valueTask = tagReader.ReadRationalFieldAsync((TiffTag)0x9202);
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
        /// Read the values of ApertureValue tag.
        /// Field description: The lens aperture. The unit is the APEX (Additive System of Photographic Exposure) setting.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffRational? ReadExifApertureValue(this TiffTagReader tagReader)
        {
            TiffValueCollection<TiffRational> result = tagReader.ReadRationalField((TiffTag)0x9202);
            return result.IsEmpty ? default(TiffRational?) : result.GetFirstOrDefault();
        }

        #endregion
    
        #region BrightnessValue

        /// <summary>
        /// Read the values of BrightnessValue tag.
        /// Field description: The value of brightness. The unit is the APEX (Additive System of Photographic Exposure) setting. Ordinarily it is given in the range of -99.99 to 99.99. Note that if the numerator of the recorded value is hex FFFFFFFF, Unknown shall be indicated.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffSRational?> ReadExifBrightnessValueAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<TiffSRational>> valueTask = tagReader.ReadSRationalFieldAsync((TiffTag)0x9203);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<TiffSRational> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffSRational?>(result.IsEmpty ? default(TiffSRational?) : result.GetFirstOrDefault());
            }

            return new ValueTask<TiffSRational?>(TransformValueTaskAsync(valueTask));

            static async Task<TiffSRational?> TransformValueTaskAsync(ValueTask<TiffValueCollection<TiffSRational>> valueTask)
            {
                TiffValueCollection<TiffSRational> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? default(TiffSRational?) : result.GetFirstOrDefault();
            }
        }
        
        /// <summary>
        /// Read the values of BrightnessValue tag.
        /// Field description: The value of brightness. The unit is the APEX (Additive System of Photographic Exposure) setting. Ordinarily it is given in the range of -99.99 to 99.99. Note that if the numerator of the recorded value is hex FFFFFFFF, Unknown shall be indicated.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffSRational? ReadExifBrightnessValue(this TiffTagReader tagReader)
        {
            TiffValueCollection<TiffSRational> result = tagReader.ReadSRationalField((TiffTag)0x9203);
            return result.IsEmpty ? default(TiffSRational?) : result.GetFirstOrDefault();
        }

        #endregion
    
        #region ExposureBiasValue

        /// <summary>
        /// Read the values of ExposureBiasValue tag.
        /// Field description: The exposure bias. The unit is the APEX (Additive System of Photographic Exposure) setting. Ordinarily it is given in the range of -99.99 to 99.99.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffSRational?> ReadExifExposureBiasValueAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<TiffSRational>> valueTask = tagReader.ReadSRationalFieldAsync((TiffTag)0x9204);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<TiffSRational> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffSRational?>(result.IsEmpty ? default(TiffSRational?) : result.GetFirstOrDefault());
            }

            return new ValueTask<TiffSRational?>(TransformValueTaskAsync(valueTask));

            static async Task<TiffSRational?> TransformValueTaskAsync(ValueTask<TiffValueCollection<TiffSRational>> valueTask)
            {
                TiffValueCollection<TiffSRational> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? default(TiffSRational?) : result.GetFirstOrDefault();
            }
        }
        
        /// <summary>
        /// Read the values of ExposureBiasValue tag.
        /// Field description: The exposure bias. The unit is the APEX (Additive System of Photographic Exposure) setting. Ordinarily it is given in the range of -99.99 to 99.99.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffSRational? ReadExifExposureBiasValue(this TiffTagReader tagReader)
        {
            TiffValueCollection<TiffSRational> result = tagReader.ReadSRationalField((TiffTag)0x9204);
            return result.IsEmpty ? default(TiffSRational?) : result.GetFirstOrDefault();
        }

        #endregion
    
        #region MaxApertureValue

        /// <summary>
        /// Read the values of MaxApertureValue tag.
        /// Field description: The smallest F number of the lens. The unit is the APEX (Additive System of Photographic Exposure) setting. Ordinarily it is given in the range of 00.00 to 99.99, but it is not limited to this range.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffRational?> ReadExifMaxApertureValueAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<TiffRational>> valueTask = tagReader.ReadRationalFieldAsync((TiffTag)0x9205);
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
        /// Read the values of MaxApertureValue tag.
        /// Field description: The smallest F number of the lens. The unit is the APEX (Additive System of Photographic Exposure) setting. Ordinarily it is given in the range of 00.00 to 99.99, but it is not limited to this range.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffRational? ReadExifMaxApertureValue(this TiffTagReader tagReader)
        {
            TiffValueCollection<TiffRational> result = tagReader.ReadRationalField((TiffTag)0x9205);
            return result.IsEmpty ? default(TiffRational?) : result.GetFirstOrDefault();
        }

        #endregion
    
        #region SubjectDistance

        /// <summary>
        /// Read the values of SubjectDistance tag.
        /// Field description: The distance to the subject, given in meters. Note that if the numerator of the recorded value is hex FFFFFFFF, Infinity shall be indicated; and if the numerator is 0, Distance unknown shall be indicated.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffRational?> ReadExifSubjectDistanceAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<TiffRational>> valueTask = tagReader.ReadRationalFieldAsync((TiffTag)0x9206);
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
        /// Read the values of SubjectDistance tag.
        /// Field description: The distance to the subject, given in meters. Note that if the numerator of the recorded value is hex FFFFFFFF, Infinity shall be indicated; and if the numerator is 0, Distance unknown shall be indicated.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffRational? ReadExifSubjectDistance(this TiffTagReader tagReader)
        {
            TiffValueCollection<TiffRational> result = tagReader.ReadRationalField((TiffTag)0x9206);
            return result.IsEmpty ? default(TiffRational?) : result.GetFirstOrDefault();
        }

        #endregion
    
        #region MeteringMode

        /// <summary>
        /// Read the values of MeteringMode tag.
        /// Field description: The metering mode.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffExifMeteringMode> ReadExifMeteringModeAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync((TiffTag)0x9207);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<ushort> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffExifMeteringMode>((TiffExifMeteringMode)result.GetFirstOrDefault());
            }

            return new ValueTask<TiffExifMeteringMode>(TransformValueTaskAsync(valueTask));

            static async Task<TiffExifMeteringMode> TransformValueTaskAsync(ValueTask<TiffValueCollection<ushort>> valueTask)
            {
                TiffValueCollection<ushort> result = await valueTask.ConfigureAwait(false);
                return (TiffExifMeteringMode)result.GetFirstOrDefault();
            }
        }
        
        /// <summary>
        /// Read the values of MeteringMode tag.
        /// Field description: The metering mode.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffExifMeteringMode ReadExifMeteringMode(this TiffTagReader tagReader)
        {
            TiffValueCollection<ushort> result = tagReader.ReadShortField((TiffTag)0x9207);
            return (TiffExifMeteringMode)result.GetFirstOrDefault();
        }

        #endregion
    
        #region LightSource

        /// <summary>
        /// Read the values of LightSource tag.
        /// Field description: The kind of light source.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffExifLightSource> ReadExifLightSourceAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync((TiffTag)0x9208);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<ushort> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffExifLightSource>((TiffExifLightSource)result.GetFirstOrDefault());
            }

            return new ValueTask<TiffExifLightSource>(TransformValueTaskAsync(valueTask));

            static async Task<TiffExifLightSource> TransformValueTaskAsync(ValueTask<TiffValueCollection<ushort>> valueTask)
            {
                TiffValueCollection<ushort> result = await valueTask.ConfigureAwait(false);
                return (TiffExifLightSource)result.GetFirstOrDefault();
            }
        }
        
        /// <summary>
        /// Read the values of LightSource tag.
        /// Field description: The kind of light source.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffExifLightSource ReadExifLightSource(this TiffTagReader tagReader)
        {
            TiffValueCollection<ushort> result = tagReader.ReadShortField((TiffTag)0x9208);
            return (TiffExifLightSource)result.GetFirstOrDefault();
        }

        #endregion
    
        #region Flash

        /// <summary>
        /// Read the values of Flash tag.
        /// Field description: Indicates the status of flash when the image was shot.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffExifFlash?> ReadExifFlashAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync((TiffTag)0x9209);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<ushort> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffExifFlash?>(result.IsEmpty ? default(TiffExifFlash?) : new TiffExifFlash(result.GetFirstOrDefault()));
            }

            return new ValueTask<TiffExifFlash?>(TransformValueTaskAsync(valueTask));

            static async Task<TiffExifFlash?> TransformValueTaskAsync(ValueTask<TiffValueCollection<ushort>> valueTask)
            {
                TiffValueCollection<ushort> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? default(TiffExifFlash?) : new TiffExifFlash(result.GetFirstOrDefault());
            }
        }
        
        /// <summary>
        /// Read the values of Flash tag.
        /// Field description: Indicates the status of flash when the image was shot.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffExifFlash? ReadExifFlash(this TiffTagReader tagReader)
        {
            TiffValueCollection<ushort> result = tagReader.ReadShortField((TiffTag)0x9209);
            return result.IsEmpty ? default(TiffExifFlash?) : new TiffExifFlash(result.GetFirstOrDefault());
        }

        #endregion
    
        #region FocalLength

        /// <summary>
        /// Read the values of FocalLength tag.
        /// Field description: The actual focal length of the lens, in mm.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffRational?> ReadExifFocalLengthAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<TiffRational>> valueTask = tagReader.ReadRationalFieldAsync((TiffTag)0x920A);
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
        /// Read the values of FocalLength tag.
        /// Field description: The actual focal length of the lens, in mm.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffRational? ReadExifFocalLength(this TiffTagReader tagReader)
        {
            TiffValueCollection<TiffRational> result = tagReader.ReadRationalField((TiffTag)0x920A);
            return result.IsEmpty ? default(TiffRational?) : result.GetFirstOrDefault();
        }

        #endregion
    
        #region SubjectArea

        /// <summary>
        /// Read the values of SubjectArea tag.
        /// Field description: Indicates the location and area of the main subject in the overall scene.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<ushort[]> ReadExifSubjectAreaAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync((TiffTag)0x9214);
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
        
        /// <summary>
        /// Read the values of SubjectArea tag.
        /// Field description: Indicates the location and area of the main subject in the overall scene.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static ushort[] ReadExifSubjectArea(this TiffTagReader tagReader)
        {
            TiffValueCollection<ushort> result = tagReader.ReadShortField((TiffTag)0x9214);
            return result.GetOrCreateArray();
        }

        #endregion
    
        #region MakerNote

        /// <summary>
        /// Read the values of MakerNote tag.
        /// Field description: Manufacturer specific information.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<byte[]> ReadExifMakerNoteAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<byte>> valueTask = tagReader.ReadByteFieldAsync((TiffTag)0x927C);
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
        /// Read the values of MakerNote tag.
        /// Field description: Manufacturer specific information.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static byte[] ReadExifMakerNote(this TiffTagReader tagReader)
        {
            TiffValueCollection<byte> result = tagReader.ReadByteField((TiffTag)0x927C);
            return result.GetOrCreateArray();
        }

        #endregion
    
        #region UserComment

        /// <summary>
        /// Read the values of UserComment tag.
        /// Field description: Keywords or comments on the image; complements ImageDescription. The character code used in the UserComment tag is identified based on an ID code in a fixed 8-byte area at the start of the tag data area.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<byte[]> ReadExifUserCommentAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<byte>> valueTask = tagReader.ReadByteFieldAsync((TiffTag)0x9286);
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
        /// Read the values of UserComment tag.
        /// Field description: Keywords or comments on the image; complements ImageDescription. The character code used in the UserComment tag is identified based on an ID code in a fixed 8-byte area at the start of the tag data area.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static byte[] ReadExifUserComment(this TiffTagReader tagReader)
        {
            TiffValueCollection<byte> result = tagReader.ReadByteField((TiffTag)0x9286);
            return result.GetOrCreateArray();
        }

        #endregion
    
        #region SubsecTime

        /// <summary>
        /// Read the values of SubsecTime tag.
        /// Field description: A tag used to record fractions of seconds for the DateTime tag.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffValueCollection<string>> ReadExifSubsecTimeAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<string>> valueTask = tagReader.ReadASCIIFieldAsync((TiffTag)0x9290);
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
        /// Read the values of SubsecTime tag.
        /// Field description: A tag used to record fractions of seconds for the DateTime tag.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffValueCollection<string> ReadExifSubsecTime(this TiffTagReader tagReader)
        {
            TiffValueCollection<string> result = tagReader.ReadASCIIField((TiffTag)0x9290);
            return result;
        }

        #endregion
    
        #region SubsecTimeOriginal

        /// <summary>
        /// Read the values of SubsecTimeOriginal tag.
        /// Field description: A tag used to record fractions of seconds for the DateTimeOriginal tag.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffValueCollection<string>> ReadExifSubsecTimeOriginalAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<string>> valueTask = tagReader.ReadASCIIFieldAsync((TiffTag)0x9291);
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
        /// Read the values of SubsecTimeOriginal tag.
        /// Field description: A tag used to record fractions of seconds for the DateTimeOriginal tag.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffValueCollection<string> ReadExifSubsecTimeOriginal(this TiffTagReader tagReader)
        {
            TiffValueCollection<string> result = tagReader.ReadASCIIField((TiffTag)0x9291);
            return result;
        }

        #endregion
    
        #region SubsecTimeDigitized

        /// <summary>
        /// Read the values of SubsecTimeDigitized tag.
        /// Field description: A tag used to record fractions of seconds for the DateTimeDigitized tag.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffValueCollection<string>> ReadExifSubsecTimeDigitizedAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<string>> valueTask = tagReader.ReadASCIIFieldAsync((TiffTag)0x9292);
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
        /// Read the values of SubsecTimeDigitized tag.
        /// Field description: A tag used to record fractions of seconds for the DateTimeDigitized tag.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffValueCollection<string> ReadExifSubsecTimeDigitized(this TiffTagReader tagReader)
        {
            TiffValueCollection<string> result = tagReader.ReadASCIIField((TiffTag)0x9292);
            return result;
        }

        #endregion
    
        #region FlashpixVersion

        /// <summary>
        /// Read the values of FlashpixVersion tag.
        /// Field description: The Flashpix format version supported by a FPXR file.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<byte[]> ReadExifFlashpixVersionAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<byte>> valueTask = tagReader.ReadByteFieldAsync((TiffTag)0xA000);
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
        /// Read the values of FlashpixVersion tag.
        /// Field description: The Flashpix format version supported by a FPXR file.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static byte[] ReadExifFlashpixVersion(this TiffTagReader tagReader)
        {
            TiffValueCollection<byte> result = tagReader.ReadByteField((TiffTag)0xA000);
            return result.GetOrCreateArray();
        }

        #endregion
    
        #region ColorSpace

        /// <summary>
        /// Read the values of ColorSpace tag.
        /// Field description: The color space information tag is always recorded as the color space specifier.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<ushort?> ReadExifColorSpaceAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync((TiffTag)0xA001);
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
        /// Read the values of ColorSpace tag.
        /// Field description: The color space information tag is always recorded as the color space specifier.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static ushort? ReadExifColorSpace(this TiffTagReader tagReader)
        {
            TiffValueCollection<ushort> result = tagReader.ReadShortField((TiffTag)0xA001);
            return result.IsEmpty ? default(ushort?) : result.GetFirstOrDefault();
        }

        #endregion
    
        #region PixelXDimension

        /// <summary>
        /// Read the values of PixelXDimension tag.
        /// Field description: Specific to compressed data; the valid width of the meaningful image.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<uint?> ReadExifPixelXDimensionAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<uint>> valueTask = tagReader.ReadLongFieldAsync((TiffTag)0xA002);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<uint> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<uint?>(result.IsEmpty ? default(uint?) : result.GetFirstOrDefault());
            }

            return new ValueTask<uint?>(TransformValueTaskAsync(valueTask));

            static async Task<uint?> TransformValueTaskAsync(ValueTask<TiffValueCollection<uint>> valueTask)
            {
                TiffValueCollection<uint> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? default(uint?) : result.GetFirstOrDefault();
            }
        }
        
        /// <summary>
        /// Read the values of PixelXDimension tag.
        /// Field description: Specific to compressed data; the valid width of the meaningful image.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static uint? ReadExifPixelXDimension(this TiffTagReader tagReader)
        {
            TiffValueCollection<uint> result = tagReader.ReadLongField((TiffTag)0xA002);
            return result.IsEmpty ? default(uint?) : result.GetFirstOrDefault();
        }

        #endregion
    
        #region PixelYDimension

        /// <summary>
        /// Read the values of PixelYDimension tag.
        /// Field description: Specific to compressed data; the valid height of the meaningful image.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<uint?> ReadExifPixelYDimensionAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<uint>> valueTask = tagReader.ReadLongFieldAsync((TiffTag)0xA003);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<uint> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<uint?>(result.IsEmpty ? default(uint?) : result.GetFirstOrDefault());
            }

            return new ValueTask<uint?>(TransformValueTaskAsync(valueTask));

            static async Task<uint?> TransformValueTaskAsync(ValueTask<TiffValueCollection<uint>> valueTask)
            {
                TiffValueCollection<uint> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? default(uint?) : result.GetFirstOrDefault();
            }
        }
        
        /// <summary>
        /// Read the values of PixelYDimension tag.
        /// Field description: Specific to compressed data; the valid height of the meaningful image.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static uint? ReadExifPixelYDimension(this TiffTagReader tagReader)
        {
            TiffValueCollection<uint> result = tagReader.ReadLongField((TiffTag)0xA003);
            return result.IsEmpty ? default(uint?) : result.GetFirstOrDefault();
        }

        #endregion
    
        #region RelatedSoundFile

        /// <summary>
        /// Read the values of RelatedSoundFile tag.
        /// Field description: Used to record the name of an audio file related to the image data.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<string?> ReadExifRelatedSoundFileAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<string>> valueTask = tagReader.ReadASCIIFieldAsync((TiffTag)0xA004);
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
        /// Read the values of RelatedSoundFile tag.
        /// Field description: Used to record the name of an audio file related to the image data.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static string? ReadExifRelatedSoundFile(this TiffTagReader tagReader)
        {
            TiffValueCollection<string> result = tagReader.ReadASCIIField((TiffTag)0xA004);
            return result.GetFirstOrDefault();
        }

        #endregion
    
        #region FlashEnergy

        /// <summary>
        /// Read the values of FlashEnergy tag.
        /// Field description: Indicates the strobe energy at the time the image is captured, as measured in Beam Candle Power Seconds.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffRational?> ReadExifFlashEnergyAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<TiffRational>> valueTask = tagReader.ReadRationalFieldAsync((TiffTag)0xA20B);
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
        /// Read the values of FlashEnergy tag.
        /// Field description: Indicates the strobe energy at the time the image is captured, as measured in Beam Candle Power Seconds.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffRational? ReadExifFlashEnergy(this TiffTagReader tagReader)
        {
            TiffValueCollection<TiffRational> result = tagReader.ReadRationalField((TiffTag)0xA20B);
            return result.IsEmpty ? default(TiffRational?) : result.GetFirstOrDefault();
        }

        #endregion
    
        #region SpatialFrequencyResponse

        /// <summary>
        /// Read the values of SpatialFrequencyResponse tag.
        /// Field description: Records the camera or input device spatial frequency table and SFR values in the direction of image width, image height, and diagonal direction, as specified in ISO 12233.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<byte[]> ReadExifSpatialFrequencyResponseAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<byte>> valueTask = tagReader.ReadByteFieldAsync((TiffTag)0xA20C);
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
        /// Read the values of SpatialFrequencyResponse tag.
        /// Field description: Records the camera or input device spatial frequency table and SFR values in the direction of image width, image height, and diagonal direction, as specified in ISO 12233.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static byte[] ReadExifSpatialFrequencyResponse(this TiffTagReader tagReader)
        {
            TiffValueCollection<byte> result = tagReader.ReadByteField((TiffTag)0xA20C);
            return result.GetOrCreateArray();
        }

        #endregion
    
        #region FocalPlaneXResolution

        /// <summary>
        /// Read the values of FocalPlaneXResolution tag.
        /// Field description: Indicates the number of pixels in the image width (X) direction per FocalPlaneResolutionUnit on the camera focal plane.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffRational?> ReadExifFocalPlaneXResolutionAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<TiffRational>> valueTask = tagReader.ReadRationalFieldAsync((TiffTag)0xA20E);
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
        /// Read the values of FocalPlaneXResolution tag.
        /// Field description: Indicates the number of pixels in the image width (X) direction per FocalPlaneResolutionUnit on the camera focal plane.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffRational? ReadExifFocalPlaneXResolution(this TiffTagReader tagReader)
        {
            TiffValueCollection<TiffRational> result = tagReader.ReadRationalField((TiffTag)0xA20E);
            return result.IsEmpty ? default(TiffRational?) : result.GetFirstOrDefault();
        }

        #endregion
    
        #region FocalPlaneYResolution

        /// <summary>
        /// Read the values of FocalPlaneYResolution tag.
        /// Field description: Indicates the number of pixels in the image height (Y) direction per FocalPlaneResolutionUnit on the camera focal plane.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffRational?> ReadExifFocalPlaneYResolutionAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<TiffRational>> valueTask = tagReader.ReadRationalFieldAsync((TiffTag)0xA20F);
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
        /// Read the values of FocalPlaneYResolution tag.
        /// Field description: Indicates the number of pixels in the image height (Y) direction per FocalPlaneResolutionUnit on the camera focal plane.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffRational? ReadExifFocalPlaneYResolution(this TiffTagReader tagReader)
        {
            TiffValueCollection<TiffRational> result = tagReader.ReadRationalField((TiffTag)0xA20F);
            return result.IsEmpty ? default(TiffRational?) : result.GetFirstOrDefault();
        }

        #endregion
    
        #region FocalPlaneResolutionUnit

        /// <summary>
        /// Read the values of FocalPlaneResolutionUnit tag.
        /// Field description: Indicates the unit for measuring FocalPlaneXResolution and FocalPlaneYResolution.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffResolutionUnit> ReadExifFocalPlaneResolutionUnitAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync((TiffTag)0xA210);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<ushort> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffResolutionUnit>(result.IsEmpty ? TiffResolutionUnit.Inch : (TiffResolutionUnit)result.GetFirstOrDefault());
            }

            return new ValueTask<TiffResolutionUnit>(TransformValueTaskAsync(valueTask));

            static async Task<TiffResolutionUnit> TransformValueTaskAsync(ValueTask<TiffValueCollection<ushort>> valueTask)
            {
                TiffValueCollection<ushort> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? TiffResolutionUnit.Inch : (TiffResolutionUnit)result.GetFirstOrDefault();
            }
        }
        
        /// <summary>
        /// Read the values of FocalPlaneResolutionUnit tag.
        /// Field description: Indicates the unit for measuring FocalPlaneXResolution and FocalPlaneYResolution.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffResolutionUnit ReadExifFocalPlaneResolutionUnit(this TiffTagReader tagReader)
        {
            TiffValueCollection<ushort> result = tagReader.ReadShortField((TiffTag)0xA210);
            return result.IsEmpty ? TiffResolutionUnit.Inch : (TiffResolutionUnit)result.GetFirstOrDefault();
        }

        #endregion
    
        #region SubjectLocation

        /// <summary>
        /// Read the values of SubjectLocation tag.
        /// Field description: Indicates the location of the main subject in the scene.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<ushort[]> ReadExifSubjectLocationAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync((TiffTag)0xA214);
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
        
        /// <summary>
        /// Read the values of SubjectLocation tag.
        /// Field description: Indicates the location of the main subject in the scene.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static ushort[] ReadExifSubjectLocation(this TiffTagReader tagReader)
        {
            TiffValueCollection<ushort> result = tagReader.ReadShortField((TiffTag)0xA214);
            return result.GetOrCreateArray();
        }

        #endregion
    
        #region ExposureIndex

        /// <summary>
        /// Read the values of ExposureIndex tag.
        /// Field description: Indicates the exposure index selected on the camera or input device at the time the image is captured.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffRational?> ReadExifExposureIndexAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<TiffRational>> valueTask = tagReader.ReadRationalFieldAsync((TiffTag)0xA215);
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
        /// Read the values of ExposureIndex tag.
        /// Field description: Indicates the exposure index selected on the camera or input device at the time the image is captured.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffRational? ReadExifExposureIndex(this TiffTagReader tagReader)
        {
            TiffValueCollection<TiffRational> result = tagReader.ReadRationalField((TiffTag)0xA215);
            return result.IsEmpty ? default(TiffRational?) : result.GetFirstOrDefault();
        }

        #endregion
    
        #region SensingMethod

        /// <summary>
        /// Read the values of SensingMethod tag.
        /// Field description: Indicates the image sensor type on the camera or input device.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffExifSensingMethod?> ReadExifSensingMethodAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync((TiffTag)0xA217);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<ushort> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffExifSensingMethod?>(result.IsEmpty ? default(TiffExifSensingMethod?) : (TiffExifSensingMethod)result.GetFirstOrDefault());
            }

            return new ValueTask<TiffExifSensingMethod?>(TransformValueTaskAsync(valueTask));

            static async Task<TiffExifSensingMethod?> TransformValueTaskAsync(ValueTask<TiffValueCollection<ushort>> valueTask)
            {
                TiffValueCollection<ushort> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? default(TiffExifSensingMethod?) : (TiffExifSensingMethod)result.GetFirstOrDefault();
            }
        }
        
        /// <summary>
        /// Read the values of SensingMethod tag.
        /// Field description: Indicates the image sensor type on the camera or input device.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffExifSensingMethod? ReadExifSensingMethod(this TiffTagReader tagReader)
        {
            TiffValueCollection<ushort> result = tagReader.ReadShortField((TiffTag)0xA217);
            return result.IsEmpty ? default(TiffExifSensingMethod?) : (TiffExifSensingMethod)result.GetFirstOrDefault();
        }

        #endregion
    
        #region FileSource

        /// <summary>
        /// Read the values of FileSource tag.
        /// Field description: Indicates the image source. If a DSC (Digital Still Camera) recorded the image, this tag will always be set to 3, indicating that the image was recorded on a DSC.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<byte> ReadExifFileSourceAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<byte>> valueTask = tagReader.ReadByteFieldAsync((TiffTag)0xA300);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<byte> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<byte>(result.IsEmpty ? (byte)3 : result.GetFirstOrDefault());
            }

            return new ValueTask<byte>(TransformValueTaskAsync(valueTask));

            static async Task<byte> TransformValueTaskAsync(ValueTask<TiffValueCollection<byte>> valueTask)
            {
                TiffValueCollection<byte> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? (byte)3 : result.GetFirstOrDefault();
            }
        }
        
        /// <summary>
        /// Read the values of FileSource tag.
        /// Field description: Indicates the image source. If a DSC (Digital Still Camera) recorded the image, this tag will always be set to 3, indicating that the image was recorded on a DSC.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static byte ReadExifFileSource(this TiffTagReader tagReader)
        {
            TiffValueCollection<byte> result = tagReader.ReadByteField((TiffTag)0xA300);
            return result.IsEmpty ? (byte)3 : result.GetFirstOrDefault();
        }

        #endregion
    
        #region SceneType

        /// <summary>
        /// Read the values of SceneType tag.
        /// Field description: Indicates the type of scene. If a DSC recorded the image, this tag value shall always be set to 1, indicating that the image was directly photographed.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<byte> ReadExifSceneTypeAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<byte>> valueTask = tagReader.ReadByteFieldAsync((TiffTag)0xA301);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<byte> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<byte>(result.IsEmpty ? (byte)1 : result.GetFirstOrDefault());
            }

            return new ValueTask<byte>(TransformValueTaskAsync(valueTask));

            static async Task<byte> TransformValueTaskAsync(ValueTask<TiffValueCollection<byte>> valueTask)
            {
                TiffValueCollection<byte> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? (byte)1 : result.GetFirstOrDefault();
            }
        }
        
        /// <summary>
        /// Read the values of SceneType tag.
        /// Field description: Indicates the type of scene. If a DSC recorded the image, this tag value shall always be set to 1, indicating that the image was directly photographed.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static byte ReadExifSceneType(this TiffTagReader tagReader)
        {
            TiffValueCollection<byte> result = tagReader.ReadByteField((TiffTag)0xA301);
            return result.IsEmpty ? (byte)1 : result.GetFirstOrDefault();
        }

        #endregion
    
        #region CFAPattern

        /// <summary>
        /// Read the values of CFAPattern tag.
        /// Field description: Indicates the color filter array (CFA) geometric pattern of the image sensor when a one-chip color area sensor is used.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<byte[]> ReadExifCFAPatternAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<byte>> valueTask = tagReader.ReadByteFieldAsync((TiffTag)0xA302);
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
        /// Read the values of CFAPattern tag.
        /// Field description: Indicates the color filter array (CFA) geometric pattern of the image sensor when a one-chip color area sensor is used.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static byte[] ReadExifCFAPattern(this TiffTagReader tagReader)
        {
            TiffValueCollection<byte> result = tagReader.ReadByteField((TiffTag)0xA302);
            return result.GetOrCreateArray();
        }

        #endregion
    
        #region CustomRendered

        /// <summary>
        /// Read the values of CustomRendered tag.
        /// Field description: Indicates the use of special processing on image data, such as rendering geared to output.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffExifCustomRendered> ReadExifCustomRenderedAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync((TiffTag)0xA401);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<ushort> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffExifCustomRendered>((TiffExifCustomRendered)result.GetFirstOrDefault());
            }

            return new ValueTask<TiffExifCustomRendered>(TransformValueTaskAsync(valueTask));

            static async Task<TiffExifCustomRendered> TransformValueTaskAsync(ValueTask<TiffValueCollection<ushort>> valueTask)
            {
                TiffValueCollection<ushort> result = await valueTask.ConfigureAwait(false);
                return (TiffExifCustomRendered)result.GetFirstOrDefault();
            }
        }
        
        /// <summary>
        /// Read the values of CustomRendered tag.
        /// Field description: Indicates the use of special processing on image data, such as rendering geared to output.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffExifCustomRendered ReadExifCustomRendered(this TiffTagReader tagReader)
        {
            TiffValueCollection<ushort> result = tagReader.ReadShortField((TiffTag)0xA401);
            return (TiffExifCustomRendered)result.GetFirstOrDefault();
        }

        #endregion
    
        #region ExposureMode

        /// <summary>
        /// Read the values of ExposureMode tag.
        /// Field description: Indicates the exposure mode set when the image was shot.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffExifExposureMode?> ReadExifExposureModeAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync((TiffTag)0xA402);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<ushort> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffExifExposureMode?>(result.IsEmpty ? default(TiffExifExposureMode?) : (TiffExifExposureMode)result.GetFirstOrDefault());
            }

            return new ValueTask<TiffExifExposureMode?>(TransformValueTaskAsync(valueTask));

            static async Task<TiffExifExposureMode?> TransformValueTaskAsync(ValueTask<TiffValueCollection<ushort>> valueTask)
            {
                TiffValueCollection<ushort> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? default(TiffExifExposureMode?) : (TiffExifExposureMode)result.GetFirstOrDefault();
            }
        }
        
        /// <summary>
        /// Read the values of ExposureMode tag.
        /// Field description: Indicates the exposure mode set when the image was shot.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffExifExposureMode? ReadExifExposureMode(this TiffTagReader tagReader)
        {
            TiffValueCollection<ushort> result = tagReader.ReadShortField((TiffTag)0xA402);
            return result.IsEmpty ? default(TiffExifExposureMode?) : (TiffExifExposureMode)result.GetFirstOrDefault();
        }

        #endregion
    
        #region WhiteBalance

        /// <summary>
        /// Read the values of WhiteBalance tag.
        /// Field description: Indicates the white balance mode set when the image was shot.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffExifWhiteBalance?> ReadExifWhiteBalanceAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync((TiffTag)0xA403);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<ushort> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffExifWhiteBalance?>(result.IsEmpty ? default(TiffExifWhiteBalance?) : (TiffExifWhiteBalance)result.GetFirstOrDefault());
            }

            return new ValueTask<TiffExifWhiteBalance?>(TransformValueTaskAsync(valueTask));

            static async Task<TiffExifWhiteBalance?> TransformValueTaskAsync(ValueTask<TiffValueCollection<ushort>> valueTask)
            {
                TiffValueCollection<ushort> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? default(TiffExifWhiteBalance?) : (TiffExifWhiteBalance)result.GetFirstOrDefault();
            }
        }
        
        /// <summary>
        /// Read the values of WhiteBalance tag.
        /// Field description: Indicates the white balance mode set when the image was shot.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffExifWhiteBalance? ReadExifWhiteBalance(this TiffTagReader tagReader)
        {
            TiffValueCollection<ushort> result = tagReader.ReadShortField((TiffTag)0xA403);
            return result.IsEmpty ? default(TiffExifWhiteBalance?) : (TiffExifWhiteBalance)result.GetFirstOrDefault();
        }

        #endregion
    
        #region DigitalZoomRatio

        /// <summary>
        /// Read the values of DigitalZoomRatio tag.
        /// Field description: Indicates the digital zoom ratio when the image was shot. If the numerator of the recorded value is 0, this indicates that digital zoom was not used.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffRational?> ReadExifDigitalZoomRatioAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<TiffRational>> valueTask = tagReader.ReadRationalFieldAsync((TiffTag)0xA404);
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
        /// Read the values of DigitalZoomRatio tag.
        /// Field description: Indicates the digital zoom ratio when the image was shot. If the numerator of the recorded value is 0, this indicates that digital zoom was not used.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffRational? ReadExifDigitalZoomRatio(this TiffTagReader tagReader)
        {
            TiffValueCollection<TiffRational> result = tagReader.ReadRationalField((TiffTag)0xA404);
            return result.IsEmpty ? default(TiffRational?) : result.GetFirstOrDefault();
        }

        #endregion
    
        #region FocalLengthIn35mmFilm

        /// <summary>
        /// Read the values of FocalLengthIn35mmFilm tag.
        /// Field description: Indicates the equivalent focal length assuming a 35mm film camera, in mm. A value of 0 means the focal length is unknown. Note that this tag differs from the FocalLength tag.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<ushort?> ReadExifFocalLengthIn35mmFilmAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync((TiffTag)0xA405);
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
        /// Read the values of FocalLengthIn35mmFilm tag.
        /// Field description: Indicates the equivalent focal length assuming a 35mm film camera, in mm. A value of 0 means the focal length is unknown. Note that this tag differs from the FocalLength tag.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static ushort? ReadExifFocalLengthIn35mmFilm(this TiffTagReader tagReader)
        {
            TiffValueCollection<ushort> result = tagReader.ReadShortField((TiffTag)0xA405);
            return result.IsEmpty ? default(ushort?) : result.GetFirstOrDefault();
        }

        #endregion
    
        #region SceneCaptureType

        /// <summary>
        /// Read the values of SceneCaptureType tag.
        /// Field description: Indicates the type of scene that was shot. It can also be used to record the mode in which the image was shot. Note that this differs from the SceneType tag.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffExifSceneCaptureType> ReadExifSceneCaptureTypeAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync((TiffTag)0xA406);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<ushort> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffExifSceneCaptureType>((TiffExifSceneCaptureType)result.GetFirstOrDefault());
            }

            return new ValueTask<TiffExifSceneCaptureType>(TransformValueTaskAsync(valueTask));

            static async Task<TiffExifSceneCaptureType> TransformValueTaskAsync(ValueTask<TiffValueCollection<ushort>> valueTask)
            {
                TiffValueCollection<ushort> result = await valueTask.ConfigureAwait(false);
                return (TiffExifSceneCaptureType)result.GetFirstOrDefault();
            }
        }
        
        /// <summary>
        /// Read the values of SceneCaptureType tag.
        /// Field description: Indicates the type of scene that was shot. It can also be used to record the mode in which the image was shot. Note that this differs from the SceneType tag.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffExifSceneCaptureType ReadExifSceneCaptureType(this TiffTagReader tagReader)
        {
            TiffValueCollection<ushort> result = tagReader.ReadShortField((TiffTag)0xA406);
            return (TiffExifSceneCaptureType)result.GetFirstOrDefault();
        }

        #endregion
    
        #region GainControl

        /// <summary>
        /// Read the values of GainControl tag.
        /// Field description: Indicates the degree of overall image gain adjustment.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffExifGainControl?> ReadExifGainControlAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync((TiffTag)0xA407);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<ushort> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffExifGainControl?>(result.IsEmpty ? default(TiffExifGainControl?) : (TiffExifGainControl)result.GetFirstOrDefault());
            }

            return new ValueTask<TiffExifGainControl?>(TransformValueTaskAsync(valueTask));

            static async Task<TiffExifGainControl?> TransformValueTaskAsync(ValueTask<TiffValueCollection<ushort>> valueTask)
            {
                TiffValueCollection<ushort> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? default(TiffExifGainControl?) : (TiffExifGainControl)result.GetFirstOrDefault();
            }
        }
        
        /// <summary>
        /// Read the values of GainControl tag.
        /// Field description: Indicates the degree of overall image gain adjustment.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffExifGainControl? ReadExifGainControl(this TiffTagReader tagReader)
        {
            TiffValueCollection<ushort> result = tagReader.ReadShortField((TiffTag)0xA407);
            return result.IsEmpty ? default(TiffExifGainControl?) : (TiffExifGainControl)result.GetFirstOrDefault();
        }

        #endregion
    
        #region Contrast

        /// <summary>
        /// Read the values of Contrast tag.
        /// Field description: Indicates the direction of contrast processing applied by the camera when the image was shot.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffExifContrast> ReadExifContrastAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync((TiffTag)0xA408);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<ushort> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffExifContrast>((TiffExifContrast)result.GetFirstOrDefault());
            }

            return new ValueTask<TiffExifContrast>(TransformValueTaskAsync(valueTask));

            static async Task<TiffExifContrast> TransformValueTaskAsync(ValueTask<TiffValueCollection<ushort>> valueTask)
            {
                TiffValueCollection<ushort> result = await valueTask.ConfigureAwait(false);
                return (TiffExifContrast)result.GetFirstOrDefault();
            }
        }
        
        /// <summary>
        /// Read the values of Contrast tag.
        /// Field description: Indicates the direction of contrast processing applied by the camera when the image was shot.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffExifContrast ReadExifContrast(this TiffTagReader tagReader)
        {
            TiffValueCollection<ushort> result = tagReader.ReadShortField((TiffTag)0xA408);
            return (TiffExifContrast)result.GetFirstOrDefault();
        }

        #endregion
    
        #region Saturation

        /// <summary>
        /// Read the values of Saturation tag.
        /// Field description: Indicates the direction of saturation processing applied by the camera when the image was shot.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffExifContrast> ReadExifSaturationAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync((TiffTag)0xA409);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<ushort> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffExifContrast>((TiffExifContrast)result.GetFirstOrDefault());
            }

            return new ValueTask<TiffExifContrast>(TransformValueTaskAsync(valueTask));

            static async Task<TiffExifContrast> TransformValueTaskAsync(ValueTask<TiffValueCollection<ushort>> valueTask)
            {
                TiffValueCollection<ushort> result = await valueTask.ConfigureAwait(false);
                return (TiffExifContrast)result.GetFirstOrDefault();
            }
        }
        
        /// <summary>
        /// Read the values of Saturation tag.
        /// Field description: Indicates the direction of saturation processing applied by the camera when the image was shot.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffExifContrast ReadExifSaturation(this TiffTagReader tagReader)
        {
            TiffValueCollection<ushort> result = tagReader.ReadShortField((TiffTag)0xA409);
            return (TiffExifContrast)result.GetFirstOrDefault();
        }

        #endregion
    
        #region Sharpness

        /// <summary>
        /// Read the values of Sharpness tag.
        /// Field description: Indicates the direction of sharpness processing applied by the camera when the image was shot.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffExifSharpness> ReadExifSharpnessAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync((TiffTag)0xA40A);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<ushort> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffExifSharpness>((TiffExifSharpness)result.GetFirstOrDefault());
            }

            return new ValueTask<TiffExifSharpness>(TransformValueTaskAsync(valueTask));

            static async Task<TiffExifSharpness> TransformValueTaskAsync(ValueTask<TiffValueCollection<ushort>> valueTask)
            {
                TiffValueCollection<ushort> result = await valueTask.ConfigureAwait(false);
                return (TiffExifSharpness)result.GetFirstOrDefault();
            }
        }
        
        /// <summary>
        /// Read the values of Sharpness tag.
        /// Field description: Indicates the direction of sharpness processing applied by the camera when the image was shot.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffExifSharpness ReadExifSharpness(this TiffTagReader tagReader)
        {
            TiffValueCollection<ushort> result = tagReader.ReadShortField((TiffTag)0xA40A);
            return (TiffExifSharpness)result.GetFirstOrDefault();
        }

        #endregion
    
        #region DeviceSettingDescription

        /// <summary>
        /// Read the values of DeviceSettingDescription tag.
        /// Field description: This tag indicates information on the picture-taking conditions of a particular camera model. The tag is used only to indicate the picture-taking conditions in the reader.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<byte[]> ReadExifDeviceSettingDescriptionAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<byte>> valueTask = tagReader.ReadByteFieldAsync((TiffTag)0xA40B);
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
        /// Read the values of DeviceSettingDescription tag.
        /// Field description: This tag indicates information on the picture-taking conditions of a particular camera model. The tag is used only to indicate the picture-taking conditions in the reader.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static byte[] ReadExifDeviceSettingDescription(this TiffTagReader tagReader)
        {
            TiffValueCollection<byte> result = tagReader.ReadByteField((TiffTag)0xA40B);
            return result.GetOrCreateArray();
        }

        #endregion
    
        #region SubjectDistanceRange

        /// <summary>
        /// Read the values of SubjectDistanceRange tag.
        /// Field description: Indicates the distance to the subject.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<TiffExifSubjectDistanceRange?> ReadExifSubjectDistanceRangeAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<ushort>> valueTask = tagReader.ReadShortFieldAsync((TiffTag)0xA40C);
            if (valueTask.IsCompletedSuccessfully)
            {
                TiffValueCollection<ushort> result = valueTask.GetAwaiter().GetResult();
                return new ValueTask<TiffExifSubjectDistanceRange?>(result.IsEmpty ? default(TiffExifSubjectDistanceRange?) : (TiffExifSubjectDistanceRange)result.GetFirstOrDefault());
            }

            return new ValueTask<TiffExifSubjectDistanceRange?>(TransformValueTaskAsync(valueTask));

            static async Task<TiffExifSubjectDistanceRange?> TransformValueTaskAsync(ValueTask<TiffValueCollection<ushort>> valueTask)
            {
                TiffValueCollection<ushort> result = await valueTask.ConfigureAwait(false);
                return result.IsEmpty ? default(TiffExifSubjectDistanceRange?) : (TiffExifSubjectDistanceRange)result.GetFirstOrDefault();
            }
        }
        
        /// <summary>
        /// Read the values of SubjectDistanceRange tag.
        /// Field description: Indicates the distance to the subject.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static TiffExifSubjectDistanceRange? ReadExifSubjectDistanceRange(this TiffTagReader tagReader)
        {
            TiffValueCollection<ushort> result = tagReader.ReadShortField((TiffTag)0xA40C);
            return result.IsEmpty ? default(TiffExifSubjectDistanceRange?) : (TiffExifSubjectDistanceRange)result.GetFirstOrDefault();
        }

        #endregion
    
        #region ImageUniqueID

        /// <summary>
        /// Read the values of ImageUniqueID tag.
        /// Field description: Indicates an identifier assigned uniquely to each image. It is recorded as an ASCII string equivalent to hexadecimal notation and 128-bit fixed length.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>A <see cref="ValueTask{TiffValueCollection}"/> that completes when the value of the tag is read and return the read values.</returns>
        public static ValueTask<string?> ReadExifImageUniqueIDAsync(this TiffTagReader tagReader)
        {
            ValueTask<TiffValueCollection<string>> valueTask = tagReader.ReadASCIIFieldAsync((TiffTag)0xA420);
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
        /// Read the values of ImageUniqueID tag.
        /// Field description: Indicates an identifier assigned uniquely to each image. It is recorded as an ASCII string equivalent to hexadecimal notation and 128-bit fixed length.
        /// </summary>
        /// <param name="tagReader">The tag reader to use.</param>
        /// <returns>The values read.</returns>
        public static string? ReadExifImageUniqueID(this TiffTagReader tagReader)
        {
            TiffValueCollection<string> result = tagReader.ReadASCIIField((TiffTag)0xA420);
            return result.GetFirstOrDefault();
        }

        #endregion
    
    }
}
