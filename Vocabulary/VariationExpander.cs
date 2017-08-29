using System;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Vocabulary
{
    public static class VariationExpander
    {
        public static Tuple<IEnumerable<string>, string> Expand(string pattern)
        {
            var parts = pattern.Split('/');
            var wordPart = parts[0];
            var connector = parts.Length == 1 ? null : parts[1];
            parts = wordPart.Split(':');
            var stem = parts[0].Split('|')[0];
            return new Tuple<IEnumerable<string>, string>(Expand("", parts[0])
                .Concat(parts.Skip(1).SelectMany(modifier => Expand(stem, modifier)))
                , Modify(stem, connector));
        }

        private static IEnumerable<string> Expand(string stem, string modifier)
        {
            var parts = modifier.Split('|');
            if (parts.Length == 1)
                return new[] {Modify(stem, modifier)};
            var prev = stem;
            return parts.Select(part => prev = Modify(prev, part));
        }

        private static string Modify(string stem, string modifier)
        {
            if (modifier == null)
                return null;
            var suffix = modifier.TrimStart('_');
            return stem.Substring(0, stem.Length + suffix.Length - modifier.Length) + suffix;
        }
    }
}