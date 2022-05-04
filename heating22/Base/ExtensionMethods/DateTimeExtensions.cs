
using System;

namespace Base.ExtensionMethods
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Setzt auf den aktuellen DateTime Stunden und Minuten.
        /// Sekunden werden auf 0 gesetzt.
        /// Datum bleibt unverändert
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="hours"></param>
        /// <param name="minutes"></param>
        /// <returns></returns>
        public static DateTime SetHoursAndMinutes(this DateTime dateTime, int hours, int minutes)
        {
            DateTime result = DateTime.Parse($"{dateTime.ToShortDateString()} {hours}:{minutes}");
            return result;
        }
    }
}
