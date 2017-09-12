using System;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Core
{
    public static class Extensions
    {
        public static IEnumerable<TItem> Append<TItem>(this IEnumerable<TItem> items, TItem item)
            => items.Concat(new[] {item});

        public static void ForEach<TItem>(this IEnumerable<TItem> items, Action<TItem> action)
            => items.AsList().ForEach(action);

        private static List<TItem> AsList<TItem>(this IEnumerable<TItem> items)
            => items as List<TItem> ?? new List<TItem>(items);

        public static TValue SafeGetValue<TKey, TValue>(this IDictionary<TKey, TValue> map, TKey key)
            => key != null && map.TryGetValue(key, out TValue value) ? value : default(TValue);

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

        public static IEnumerable<TItem> Prepend<TItem>(this IEnumerable<TItem> items, TItem item)
            => new []{item}.Concat(items);

        public static IEnumerable<TItem> Interleave<TItem>(this IEnumerable<TItem> primary, IList<TItem> secondary, bool modulate = false) 
            => !secondary.Any() ? primary : DoInterleave(primary, secondary, modulate);

        private static IEnumerable<TItem> DoInterleave<TItem>(this IEnumerable<TItem> primary, IList<TItem> secondary, bool modulate = false)
        {
            var index = 0;
            foreach (var item in primary)
            {
                yield return item;
                if (index < secondary.Count)
                    yield return secondary[index];
                else if (modulate)
                    yield return secondary[index % secondary.Count];
                index++;
            }
        }

        public static IEnumerable<TItem> NotNull<TItem>(this IEnumerable<TItem> items)
            where TItem : class
            => items.Where(item => item != null);
    }
}