using System;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Core
{
    public static class Extensions
    {
        /*
        public static string Interleave(this IList<string> primary, IList<string> secondary)
            => secondary.Any()
                ? primary.First() + secondary.Interleave(primary.Skip(1).ToList())
                : string.Join("", primary);

        public static IEnumerable<IList<TItem>> Split<TItem>(this IList<TItem> items, Func<TItem, bool> predicate)
        {
            var prevIndex = -1;
            var splitters = items.Where(predicate).ToList();
            return splitters.Select(items.IndexOf)
                .Append(items.Count)
                .Select(splitIndex => Grab(items, prevIndex + 1, prevIndex = splitIndex));
        }
                */

        public static IEnumerable<TItem> Append<TItem>(this IEnumerable<TItem> items, TItem item)
            => items.Concat(new[] {item});

        /*
        public static IList<TItem> Grab<TItem>(this IList<TItem> items, int fromIndex, int toIndex)
            => items.Take(toIndex).Skip(fromIndex).ToList();
            */
        public static void ForEach<TItem>(this IEnumerable<TItem> items, Action<TItem> action)
            => items.AsList().ForEach(action);

        private static List<TItem> AsList<TItem>(this IEnumerable<TItem> items)
            => items as List<TItem> ?? new List<TItem>(items);

        public static TValue SafeGetValue<TKey, TValue>(this IDictionary<TKey, TValue> map, TKey key)
            => key != null && map.TryGetValue(key, out TValue value) ? value : default(TValue);
        /*
        public static IEnumerable<TItem> PopAll<TItem>(this Stack<TItem> stack)
        {
            while (stack.Any())
                yield return stack.Pop();
        }
        public static void PushAll<TItem>(this Stack<TItem> stack, IEnumerable<TItem> items)
            => items.ForEach(stack.Push);

        public static string End(this string text, int count)
            => count < 0 
            ? text.Substring(-count)
            : text.Substring(text.Length - count);
        */

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

        public static IEnumerable<TItem> Interleave<TItem>(this IList<TItem> primary, IList<TItem> secondary)
            => primary.Any()
                ? Interleave(secondary, primary.Skip(1).ToArray()).Prepend(primary.First())
                : secondary;

        public static IEnumerable<TItem> NotNull<TItem>(this IEnumerable<TItem> items)
            where TItem : class
            => items.Where(item => item != null);
    }
}