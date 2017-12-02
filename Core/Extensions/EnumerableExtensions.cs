﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Core.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<TItem> Append<TItem>(this IEnumerable<TItem> items, TItem item)
            => items.Concat(new[] {item});

        public static void ForEach<TItem>(this IEnumerable<TItem> items, Action<TItem> action)
            => items.AsList().ForEach(action);

        private static List<TItem> AsList<TItem>(this IEnumerable<TItem> items)
            => items as List<TItem> ?? new List<TItem>(items);

        public static TValue SafeGetValue<TKey, TValue>(this IDictionary<TKey, TValue> map, TKey key)
            => key != null && map.TryGetValue(key, out TValue value) ? value : default(TValue);

        public static IEnumerable<TItem> Prepend<TItem>(this IEnumerable<TItem> items, TItem item)
            => new []{item}.Concat(items);

        public static IEnumerable<TItem> NotNull<TItem>(this IEnumerable<TItem> items)
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
    }
}