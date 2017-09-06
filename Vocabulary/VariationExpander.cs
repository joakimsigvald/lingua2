using System;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Vocabulary
{
    public static class VariationExpander
    {


        public static Specification Expand(string pattern)
        {
            var parts = pattern.Split('<');
            var modifiers = parts.Skip(1).FirstOrDefault();
            parts = pattern.Split('/');
            var connector = parts.Skip(1).FirstOrDefault();
            var variations = GetVariations(parts[0]).ToArray();
            var incompleteCompound = GetIncompleteCompound(variations[0], connector);
            return new Specification(variations
                , incompleteCompound, modifiers);
        }

        private static IEnumerable<string> GetVariations(string wordPattern)
        {
            var parts = wordPattern.Split(':');
            var stem = parts[0].Split('|')[0];
            return Expand("", parts[0])
                .Concat(parts.Skip(1).SelectMany(modifier => Expand(stem, modifier)))
                .ToArray();
        }

        private static string GetIncompleteCompound(string stem, string connector)
            => Modify(stem, connector);

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