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
            var parts = pattern.Split(',');
            var synonyms = parts.Select(p => ExpandTranslation(p)).ToArray();
            for (int i = 0; i < synonyms.Length; i++)
                synonyms[i].Synonyms = synonyms;
            return synonyms.First();
        }

        public static Specification ExpandTranslation(string pattern)
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
            var variations = ExpandVariations(variationsPattern);
            var incompleteCompound = GetIncompleteCompound(variations[0], connector);
            return new Specification(variations, incompleteCompound, modifiers);
        }

        private static string[] ExpandVariations(string variationsPattern)
        {
            var words = variationsPattern.Split('+');
            var wordVariations = words
                .Select(word => GetVariations(word).ToArray())
                .ToArray();
            return MergeVariations(wordVariations);
        }

        private static string[] MergeVariations(IList<string[]> wordVariations) 
            => wordVariations.First()
            .Select((_, i) => string.Join(' ', wordVariations.Select(w => w[i % w.Length])))
            .ToArray();

        private static IEnumerable<string> GetVariations(string variationsPattern)
            => new Process(variationsPattern).GetVariations();

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

        private class Process
        {
            private static readonly char[] Divisors = { ':', '!', '|' };

            private readonly string _variationsPattern;
            private string _stem;
            private string _current = "";
            private string _previous = "";
            private char _prevC = (char) 0;
            private int _multiplier = 1;
            private string _variation;

            public Process(string variationsPattern)
            {
                _variationsPattern = variationsPattern;
            }

            public IEnumerable<string> GetVariations()
            {
                foreach (var c in _variationsPattern)
                {
                    if (char.IsDigit(c))
                    {
                        switch (_prevC)
                        {
                            case '_':
                                _previous = _previous.Substring(..^(c - 49));
                                break;
                            case ' ':
                                break;
                            default:
                                throw new Exception("Invalid pattern: " + _variationsPattern);
                        }
                    }
                    else if (Divisors.Contains(c))
                    {
                        foreach (var variation in GetPreviousVariations())
                            yield return variation;
                        InitNextVariation(c);
                    }
                    else if (c == '_')
                        _previous = _previous.Substring(..^1);
                    else
                        _current += c;
                    _prevC = c;
                }
                foreach (var variation in GetPreviousVariations())
                    yield return variation;
            }

            private void InitNextVariation(char c)
            {
                _previous = GetPrevious(c);
                _multiplier = char.IsDigit(_prevC) ? _prevC - 48 : 1;
            }

            private IEnumerable<string> GetPreviousVariations()
            {
                _variation = CreateVariation();
                for (var i = 0; i < _multiplier; i++)
                    yield return _variation;
            }

            private string GetPrevious(char c)
            {
                switch (c)
                {
                    case ':':
                        return _stem;
                    case '!':
                        return string.Empty;
                    case '|':
                        return _variation;
                    default: throw new NotImplementedException();
                }
            }

            private string CreateVariation()
            {
                var retVal = _previous.Trim() + _current.Trim();
                _stem ??= retVal;
                _current = "";
                return retVal;
            }
        }
    }
}