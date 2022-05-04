
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Base.Validations
{
    public class CheckPasswordRules : ValidationAttribute
    {
        /// <summary>
        /// Kennwort muss mindestens 8 Zeichen lang sein
        /// und zumindest Großbuchstaben/Kleinbuchstaben, Ziffern 
        /// und Sonderzeichen enthalten
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool IsValid(object value)
        {
            if (string.IsNullOrEmpty((string)value))
            {
                return true;
            }
            string password = (string)value;
            ErrorMessage = GetErrorMessageForPasswordRules(password);
            return string.IsNullOrEmpty(ErrorMessage);
        }

        /// <summary>
        /// Das übergebene Passwort wird auf die korrekte Struktur hin überprüft.
        /// Es muss sowohl Groß- als auch Kleinbuchstaben, Ziffern und Sonderzeichen
        /// enthalten und mindestens 8 Zeichen lang sein.
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string GetErrorMessageForPasswordRules(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return "password is empty";
            }
            if (password.Length < 8)
            {
                return $"password has a length of {password.Length} chars (minimum 8)";
            }
            var upperLetters = password.Count(l => char.IsLetter(l) && char.IsUpper(l));
            var lowerLetters = password.Count(l => char.IsLetter(l) && char.IsLower(l));
            var digits = password.Count(l => char.IsDigit(l));
            var specialChars = password.Length - upperLetters - lowerLetters - digits;
            List<string> missingParts = new();
            if (upperLetters == 0) missingParts.Add("upper letters");
            if (lowerLetters == 0) missingParts.Add("lower letters");
            if (digits == 0) missingParts.Add("digits");
            if (specialChars == 0) missingParts.Add("special chars");
            if (upperLetters == 0 || lowerLetters == 0 || digits == 0 || specialChars == 0)
            {
                return "password must contain " + string.Join(", ", missingParts.ToArray());
            }
            return "";
        }
    }
}
