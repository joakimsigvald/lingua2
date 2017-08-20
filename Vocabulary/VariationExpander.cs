using System.Collections.Generic;
using System.Linq;
using Lingua.Core;

namespace Lingua.Vocabulary
{
    public static class VariationExpander
    {
        public static IEnumerable<string> Expand(string pattern)
        {
            var parts = pattern.Split(':');
            var stem = parts[0].Split('|')[0];
            return Expand("", parts[0])
                .Concat(parts.Skip(1).SelectMany(modifier => Expand(stem, modifier)));
        }

        private static IEnumerable<string> Expand(string stem, string modifier)
        {
            var parts = modifier.Split('|');
            var prev = stem;
            return parts.Length == 1
                ? new[] {Modify(stem, modifier)}
                : parts.Select(part => prev = Modify(prev, part));
        }

        private static string Modify(string stem, string modifier)
        {
            var suffix = modifier.TrimStart('_');
            return stem.Substring(0, stem.Length + suffix.Length - modifier.Length) + suffix;
        }
    }
}