using System;
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

        public static IEnumerable<TItem> Interleave<TItem>(this IList<TItem> primary, IList<TItem> secondary, bool modulate = false) 
            => primary.Count > 1 && secondary.Any() 
            ? DoInterleave(primary, secondary, modulate)
            : primary;

        private static IEnumerable<TItem> DoInterleave<TItem>(this IList<TItem> primary, IList<TItem> secondary, bool modulate = false)
        {
            var index = 0;
            yield return primary.First();
            foreach (var item in primary.Skip(1))
            {
                if (index < secondary.Count || modulate)
                    yield return secondary[index % secondary.Count];
                yield return item;
                index++;
            }
        }

        public static IEnumerable<TItem> NotNull<TItem>(this IEnumerable<TItem> items)
            where TItem : class
            => items.Where(item => item != null);


        public static void MoveToBeginning<TItem>(this IList<TItem> items, TItem item)
        {
            items.Remove(item);
            items.Insert(0, item);
        }

        public static void Deconstruct<TValue>(this IEnumerable<TValue> sequence, out TValue item1, out TValue item2, out TValue item3)
        {
            using (var enumerator = sequence.GetEnumerator())
            {
                enumerator.MoveNext();
                item1 = enumerator.Current;
                enumerator.MoveNext();
                item2 = enumerator.Current;
                enumerator.MoveNext();
                item3 = enumerator.Current;
            }
        }
    }
}