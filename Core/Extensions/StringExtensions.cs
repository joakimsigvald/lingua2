﻿using System;

namespace Lingua.Core.Extensions
{
    public static class StringExtensions
    {
        public static bool ReplaceFirst(this string text, string old, string replacement, out string updatedText)
        {
            updatedText = text;
            var index = text.IndexOf(old, StringComparison.Ordinal);
            if (index < 0)
                return false;
            updatedText = Splice(text, replacement, index, old.Length);
            return true;
        }

        private static string Splice(string text, string replacement, int index, int deleteCount) 
            => index < 0
            ? text
            : text.Substring(0, index) + replacement + text.Substring(index + deleteCount);

        public static string Start(this string text, int count)
            => count >= 0
                ? text.Substring(0, count)
                : text.Substring(0, text.Length + count);

        public static string Capitalize(this string word)
            => AlterInitial(word, char.ToUpper);

        public static string Decapitalize(this string word)
            => AlterInitial(word, char.ToLower);

        private static string AlterInitial(string word, Func<char, char> alter)
            => string.IsNullOrEmpty(word) ? word : alter(word[0]) + word.Substring(1);
    }
}