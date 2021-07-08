using System;

namespace TiffLibrary.Exif
{
    /// <summary>
    /// The kind of light source.
    /// </summary>
    [CLSCompliant(false)]
    public enum TiffExifLightSource : ushort
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Daylight
        /// </summary>
        Daylight = 1,

        /// <summary>
        /// Fluorescent
        /// </summary>
        Fluorescent = 2,

        /// <summary>
        /// Tungsten (incandescent light)
        /// </summary>
        Tungsten = 3,

        /// <summary>
        /// Flash
        /// </summary>
        Flash = 4,

        /// <summary>
        /// Fine weather
        /// </summary>
        FineWeather = 9,

        /// <summary>
        /// Cloudy weather
        /// </summary>
        CloudyWeather = 10,

        /// <summary>
        /// Shade
        /// </summary>
        Shade = 11,

        /// <summary>
        /// Daylight fluorescent (D 5700 - 7100K)
        /// </summary>
        DaylightFluorescent = 12,

        /// <summary>
        /// Day white fluorescent (N 4600 - 5400K)
        /// </summary>
        DayWhiteFluorescent = 13,

        /// <summary>
        /// Cool white fluorescent (W 3900 - 4500K)
        /// </summary>
        CoolWhiteFluorescent = 14,

        /// <summary>
        /// White fluorescent (WW 3200 - 3700K)
        /// </summary>
        WhiteFluorescent = 15,

        /// <summary>
        /// Standard light A
        /// </summary>
        StandardLightA = 17,

        /// <summary>
        /// Standard light B
        /// </summary>
        StandardLightB = 18,

        /// <summary>
        /// Standard light C
        /// </summary>
        StandardLightC = 19,

        /// <summary>
        /// D55
        /// </summary>
        D55 = 20,

        /// <summary>
        /// D65
        /// </summary>
        D65 = 21,

        /// <summary>
        /// D75
        /// </summary>
        D75 = 22,

        /// <summary>
        /// D50
        /// </summary>
        D50 = 23,

        /// <summary>
        /// ISO studio tungsten
        /// </summary>
        ISOStudioTungsten = 26,

        /// <summary>
        /// Other light source
        /// </summary>
        Other = 255,
    }
}
