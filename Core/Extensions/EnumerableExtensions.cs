using System;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Core.Extensions
{
    public static class EnumerableExtensions
    {
        public static void ForEach<TItem>(this IEnumerable<TItem> items, Action<TItem> action)
            => items.AsList().ForEach(action);

        private static List<TItem> AsList<TItem>(this IEnumerable<TItem> items)
            => items as List<TItem> ?? new List<TItem>(items);

        public static TValue SafeGetValue<TKey, TValue>(this IDictionary<TKey, TValue> map, TKey key)
            => key != null && map.TryGetValue(key, out TValue value) ? value : default;

        public static IEnumerable<TItem> ExceptNull<TItem>(this IEnumerable<TItem> items)
            where TItem : class
            => items.Where(item => item != null);


        public static void MoveToBeginning<TItem>(this IList<TItem> items, TItem item)
        {
            items.Remove(item);
            items.Insert(0, item);
        }

        public static bool IsSegmentOf<TItem>(this IList<TItem> a, IList<TItem> b)
            => a.Count <= b.Count && Enumerable.Range(0, b.Count - a.Count + 1)
                   .Any(offset => b.Skip(offset).Take(a.Count).SequenceEqual(a));

        /// <summary>
        /// Takes a number of sets of values and produce all possible different sequences by taking one value from each set
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public static IEnumerable<TValue[]> Expand<TValue>(this IEnumerable<IEnumerable<TValue>> sets)
            => Expand(sets.ToList(), 0)
            .Select(seq => seq.ToArray());

        public static IEnumerable<TValue> Except<TValue>(this IEnumerable<TValue> items, TValue exceptItem)
            => items.Where(item => !item.Equals(exceptItem));

        private static IEnumerable<IEnumerable<TValue>> Expand<TValue>(
            IList<IEnumerable<TValue>> sets, int offset)
            => sets.Count > offset
                ? sets[offset].SelectMany(first => Expand(sets, offset + 1)
                    .Select(rest => rest.Prepend(first)))
                : new[] {new TValue[0]};
    }
}