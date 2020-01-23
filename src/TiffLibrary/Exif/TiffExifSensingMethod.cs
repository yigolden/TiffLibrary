using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiffLibrary.Exif
{
    /// <summary>
    /// Indicates the image sensor type on the camera or input device.
    /// </summary>
    public enum TiffExifSensingMethod : ushort
    {
        /// <summary>
        /// Not defined
        /// </summary>
        NotDefined = 1,

        /// <summary>
        /// One-chip color area sensor
        /// </summary>
        OneChipColorAreaSensor = 2,

        /// <summary>
        /// Two-chip color area sensor
        /// </summary>
        TwoChipColorAreaSensor = 3,

        /// <summary>
        /// Three-chip color area sensor
        /// </summary>
        ThreeChipColorAreaSensor = 4,

        /// <summary>
        /// Color sequential area sensor
        /// </summary>
        ColorSequentialAreaSensor = 5,

        /// <summary>
        /// Trilinear sensor
        /// </summary>
        TrilinearSensor = 7,

        /// <summary>
        /// Color sequential linear sensor
        /// </summary>
        ColorSequentialLinearSensor = 8,
    }
}
