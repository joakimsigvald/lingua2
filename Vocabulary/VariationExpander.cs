using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Lingua.Vocabulary
{
    public static class VariationExpander
    {
        public static Specification Expand(string pattern)
        {
            var parts = pattern.Split('<');
            var wordPattern = parts[0];
            var modifiers = parts.Skip(1).FirstOrDefault();
            parts = wordPattern.Split('/');
            var connector = parts.Skip(1).FirstOrDefault();
            var variations = GetVariations(parts[0]).ToArray();
            var incompleteCompound = GetIncompleteCompound(variations[0], connector);
            return new Specification(variations
                , incompleteCompound, modifiers);
        }

        private static IEnumerable<string> GetVariations(string wordPattern)
            => wordPattern.Split('!').SelectMany(GetVariationGroup);

        private static IEnumerable<string> GetVariationGroup(string groupPattern)
        {
            var parts = groupPattern.Split(':');
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
            var reduction = GetReduction(modifier);
            var reductionCount = GetReductionCount(reduction);
            var suffix = modifier.Substring(reduction.Length);
            return stem.Substring(0, stem.Length - reductionCount) + suffix;
        }

        private static string GetReduction(string modifier)
            => Regex.Match(modifier, @"(_\d|_+)").Value;

        private static int GetReductionCount(string reduction)
            => string.IsNullOrEmpty(reduction)
                ? 0
                : int.TryParse($"{reduction.Last()}", out int count)
                    ? count
                    : reduction.Length;
    }
}