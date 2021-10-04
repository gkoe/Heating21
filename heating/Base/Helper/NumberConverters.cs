
using System.Globalization;

namespace Base.Helper
{
    public static class NumberConverters
    {
        /// <summary>
        /// Wandelt einen String der eine Doublezahl darstellt in den Doublewert um.
        /// Zuerst wird versucht, die Zahl ohne Cultureberücksichtigung umzuwandeln.
        /// Dann wird das normal Parse versucht.
        /// </summary>
        /// <param name="doubleText"></param>
        /// <returns>Doublezahl oder null, falls Parsen schief ging</returns>
        //public static double? ParseInvariantDouble(string doubleText)
        //{
        //    if (doubleText.ToLower().Contains("nan"))
        //    {
        //        return null;
        //    }
        //    if (double.TryParse(doubleText, NumberStyles.Float, CultureInfo.InvariantCulture, out double valueCulture))
        //    {
        //        return valueCulture;
        //    }
        //    if (double.TryParse(doubleText, out double value))
        //    {
        //        return value;
        //    }
        //    return null;
        //}


    }
}
