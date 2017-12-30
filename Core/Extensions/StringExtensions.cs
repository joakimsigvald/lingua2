using System;

namespace Lingua.Core.Extensions
{
    public static class StringExtensions
    {
        public static string Start(this string text, int count)
            => count >= 0
                ? text.Substring(0, count)
                : text.Substring(0, text.Length + count);

        public static string Capitalize(this string word)
            => AlterInitial(word, char.ToUpper);

        public static string Decapitalize(this string word)
            => AlterInitial(word, char.ToLower);

        public static bool ContainsIgnoreCase(this string str, string part)
            => str.IndexOfIgnoreCase(part) >= 0;

        public static int IndexOfIgnoreCase(this string str, string part, int startIndex = 0)
            => str.IndexOf(part, startIndex, StringComparison.OrdinalIgnoreCase);

        private static string AlterInitial(string word, Func<char, char> alter)
            => string.IsNullOrEmpty(word) ? word : alter(word[0]) + word.Substring(1);
    }
}