using System;
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
            return Expand(parts[0], parts.Skip(1).FirstOrDefault());
        }

        private static Specification Expand(string wordPattern, string modifiers)
        {
            var parts = wordPattern.Split('/');
            return Expand(parts[0], modifiers, parts.Skip(1).FirstOrDefault());
        }

        private static Specification Expand(string variationsPattern, string modifiers, string connector)
        {
            var variations = GetVariations(variationsPattern).ToArray();
            var incompleteCompound = GetIncompleteCompound(variations[0], connector);
            return new Specification(variations, incompleteCompound, modifiers);
        }

        private static IEnumerable<string> GetVariations(string variationsPattern)
        {
            string stem = null;
            var current = "";
            var previous = "";
            var prevC = (char)0;
            foreach (var c in variationsPattern)
            {
                switch (c)
                {
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        if (prevC == '_')
                            previous = previous.Substring(0, previous.Length - (c - 49));
                        else throw new Exception("Invalid pattern: " + variationsPattern);
                        break;
                    case '_':
                        previous = previous.Substring(0, previous.Length - 1);
                        break;
                    case ':':
                        yield return CreateVariation(previous, ref current, ref stem);
                        previous = stem;
                        break;
                    case '!':
                        yield return CreateVariation(previous, ref current, ref stem);
                        previous = "";
                        break;
                    case '|':
                        yield return previous = CreateVariation(previous, ref current, ref stem);
                        break;
                    default:
                        current += c;
                        break;
                }
                prevC = c;
            }
            yield return CreateVariation(previous, ref current, ref stem);
        }

        private static string CreateVariation(string previous, ref string current, ref string stem)
        {
            var retVal = previous.Trim() + current.Trim();
            stem = stem ?? retVal;
            current = "";
            return retVal;
        }

        private static string GetIncompleteCompound(string stem, string connector)
            => Modify(stem, connector);

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