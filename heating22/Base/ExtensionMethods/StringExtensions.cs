
using System;
using System.Globalization;
using System.Text;

namespace Base.ExtensionMethods
{
    public static class StringExtensions
    {
        public static string RemoveChars(this string text, string charsToRemove)
        {
            var result = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                char ch = text[i];
                if (!charsToRemove.Contains(ch))
                {
                    result.Append(ch);
                }
            }
            return result.ToString();
        }


        /// <summary>
        /// Der String wird auf die Länge maxLength gekürzt.
        /// Wenn er gekürzt werden musste, werden die letzten drei
        /// Buchstaben mit ... ersetzt.
        /// </summary>
        /// <param name="text">zu kürzender Text</param>
        /// <param name="maxLength">Maximale Länge des Ergebnistextes (>=3)</param>
        /// <returns></returns>
        public static string Shorten(this string text, int maxLength = 10)
        {
            if (maxLength < 3)
            {
                throw new ArgumentOutOfRangeException(nameof(maxLength), "Zielstring muss mindestens drei Zeichen lang sein");
            }

            if (String.IsNullOrEmpty(text))
            {
                return text;
            }

            if (text.Length <= maxLength)
            {
                return text;
            }

            return $"{text.Remove(maxLength - 3)}...";
        }

        /// <summary>
        /// Der Inhalt des Strings wird umgedreht.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string Reverse(this string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text), "text is null!");
            }
            char[] reversed = new char[text.Length];
            int j = 0;
            for (int i = text.Length - 1; i >= 0; i--)
            {
                reversed[j] = text[i];
                j++;
            }
            return new string(reversed);
        }

        /// <summary>
        /// Wandelt Texte, die Gleitkommazahlen enthalten, die nach Cultures englisch und deutsch gültig sind um
        ///     Als Dezimaltrenner sind , und . erlaubt. 
        ///     Als Tausendertrenner sind die komplementären Zeichen zulässig.
        /// Interne Vereinbarung: Gibt es einen Separator, ist der letzte immer ein Dezimaltrenner (sonst würde
        /// z.B. 3,140 im englischen Format als 3140 umgewandelt)
        /// </summary>
        /// <param name="doubleText">Doublezahl im Textformat</param>
        /// <returns>umgewandelte Zahl oder null, falls die Umwandlung nicht möglich war</returns>
        public static double? TryParseToDouble(this string doubleText)
        {
            if (doubleText == null)
            {
                throw new ArgumentNullException(nameof(doubleText), "DoubleText is null!");
            }
            var text = new StringBuilder();
            var separatorFound = false;
            for (int i = doubleText.Length - 1; i >= 0; i--)  // von hinten nach vorne, um letztes Trennzeichen zu erkennen
            {
                var ch = doubleText[i];
                if (char.IsDigit(ch) || (i == 0 && ch=='-'))
                {
                    text.Append(ch);
                }
                else if (ch == '.' || ch == ',')  // Trennzeichen ==> letztes verarbeiten, Rest ignorieren
                {
                    if (!separatorFound)
                    {
                        separatorFound = true;
                        text.Append(',');
                    }
                }
                else  // keinTrennzeichen und keine Ziffer
                {
                    return null;
                }
            }
            if (separatorFound && text.Length == 1)
            {
                return null;   // nur Trennzeichen
            }
            //if (double.TryParse(text.ToString(), NumberStyles.Float, new CultureInfo("en-US"), out double valueCulture))
            //{
            //    return valueCulture;
            //}
            if (double.TryParse(text.ToString().Reverse(), NumberStyles.Float, new CultureInfo("de-DE"), out double value))
            {
                return value;
            }
            return null;
        }


        /// <summary>
        /// Ein Datumstext im deutschen Format wird geparst
        /// </summary>
        /// <param name="text">Datumstext</param>
        /// <returns>Datum oder null im Fehlerfall</returns>
        public static DateTime? TryParseGermanDateTime(this string text)
        {
            var deCultureInfo = new CultureInfo("de-DE");
            try
            {
                return DateTime.Parse(text, deCultureInfo);
            }
            catch (Exception)
            {
                return null;
            }
        }


        /// <summary>
        /// try to parse an integer
        /// </summary>
        /// <param name="str"></param>
        /// <returns>int or null</returns>
        public static int? ParseToInt(this string str)
        {
            if (int.TryParse(str, out int value))
                return value;
            else
                return null;
        }

    }
}
