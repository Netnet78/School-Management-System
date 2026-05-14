using System.Text;

namespace SchoolManagement.Core.Shared.Extensions
{
    public static class StringExtensions
    {
        public static string RemoveHiddenSpaces(this string text, bool includeAcutualSpace = false)
        {
            char[] blockedCharacters = ['\u200b', '\u200c', '\u200d'];

            StringBuilder sb = new(text.Length);

            foreach (char c in text.Trim())
            {
                if (!blockedCharacters.Contains(c))
                {
                    if (includeAcutualSpace && c == ' ')
                    {
                        continue;
                    }

                    sb.Append(c);
                }
            }

            return sb.ToString();
        }
    }
}