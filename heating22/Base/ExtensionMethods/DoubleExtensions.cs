
using System;
using System.Globalization;

namespace Base.ExtensionMethods
{
    public static class DoubleExtensions
    {

        /// <summary>
        /// Stellt sicher, dass der Wertebereich für double eingehelten wird.
        /// NotANumber und Negativ unendlich wird zu double.MinValue
        /// positiv unendlich zu double.MaxValue
        /// </summary>
        /// <param name="value"></param>
        public static double ToLegalDouble(this double value)
        {
            if (double.IsNaN(value) || double.IsNegativeInfinity(value))
            {
                return double.MinValue;
            }
            if (double.IsPositiveInfinity(value))
            {
                return double.MaxValue;
            }
            return value;
        }

        public static string ToGermanString(this double value, int decimals = 2)
        {
            double roundedValue = Math.Round(value, decimals);
            string specifier = "G";
            CultureInfo culture = CultureInfo.CreateSpecificCulture("de-DE");
            return roundedValue.ToString(specifier, culture);
        }

    }
}
