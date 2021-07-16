
using System.Text;

namespace Common.Helper
{
    public static class StingUtils
    {
        public static string RemoveChars(this string text, string charsToRemove)
        {
            StringBuilder result = new StringBuilder();
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


    }
}
