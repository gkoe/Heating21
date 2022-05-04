
using System;
using System.Globalization;

namespace Base.Helper
{
    public class DateTimeHelpers
    {
        /// <summary>
        /// Unix-Timestamp in DateTime umwandeln
        /// </summary>
        /// <param name="unixTimeStamp"></param>
        /// <returns></returns>
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }

        public static DateTime ParseGermanDateTimeString(string text)
        {
            if (text.Contains('/'))
            {
                throw new FormatException("text isn't a german datetime");
            }
            CultureInfo provider = new CultureInfo("de-DE");
            return DateTime.Parse(text, provider);
        }



    }
}
