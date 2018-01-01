using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Lingua.Learning
{
    using Core;
    using Core.Extensions;
    using Grammar;

    public static class TargetSelector
    {
        public static TranslationTarget SelectTarget(IList<ITranslation[]> possibilities, string translated)
        {
            if (possibilities == null)
                return null;
            var orderedTranslations = SelectAndOrderTranslations(possibilities, translated);
            var translations = orderedTranslations.translations;
            var arrangement = CreateArrangement(translations, orderedTranslations.order);
            return new TranslationTarget
            {
                Arrangement = arrangement,
                IsFullyTranslated = arrangement.Order.Any(),
                Unmatched = orderedTranslations.unmatched,
                Translations = translations
            };
        }

        private static Arrangement CreateArrangement(IEnumerable<ITranslation> translations, byte[] order)
            => new Arrangement(translations.Select(t => t.Code).ToArray(), order);

        private static (ITranslation[] translations, byte[] order, string unmatched, string hidden) SelectAndOrderTranslations(IEnumerable<ITranslation[]> possibilities, string translated)
        {
            var filteredPossibilities = FilterPossibilities(possibilities, translated).ToList();
            var possibleSequences = new Expander(filteredPossibilities).Expand(out var _);
            return possibleSequences
                .Select(ps => new OrderMaker(translated).SelectAndOrderTranslations(ps))
                .OrderBy(o => o.unmatched.Length)
                .ThenBy(o => o.hidden.Length)
                .First();
        }

        private static IEnumerable<ITranslation[]> FilterPossibilities(
            IEnumerable<ITranslation[]> possibilities, string translated)
            => possibilities.Select(p => SelectAlternatives(p, translated));

        private static ITranslation[] SelectAlternatives(ITranslation[] alternatives, string translated)
        {
            var matchningAlternatives = alternatives.Where(t => translated.ContainsIgnoreCase(t.Output))
                .OrderByDescending(t => t.Output.Length);
            var nonMatchingAlternative = alternatives
                .Where(t => !translated.ContainsIgnoreCase(t.Output)).Take(1);
            return matchningAlternatives
                .Concat(nonMatchingAlternative)
                .ToArray();
        }
    }

    public class OrderMaker
    {
        private const char Tab = '\u0009';
        private readonly string _translated;
        private string _matched;
        private readonly List<string> _hidden = new List<string>();

        public OrderMaker(string translate)
        {
            _translated = translate;
            _matched = new string(Tab, _translated.Length);
        }

        public (ITranslation[] translations, byte[] order, string unmatched, string hidden) SelectAndOrderTranslations(ITranslation[] translations)
        {
            var words = translations.Select(t => t.Output).ToArray();
            var order = MakeOrder(words).ToArray();
            return (translations, order, Unmatched, string.Join(",", _hidden));
        }

        private string Unmatched
        {
            get
            {
                var unmatchedIndices = _matched.Select((c, i) => (c: c, i: i))
                    .Where(tuple => tuple.c == Tab)
                    .Select(tuple => tuple.i);
                var unmatched = new string(_translated.Select((c, i) => (c: c, i: i))
                    .Join(unmatchedIndices, tuple => tuple.i, i => i, (tuple, i) => tuple.c)
                    .ToArray());
                var words = SplitWords(unmatched);
                return string.Join(",", words);
            }
        }

        private static IEnumerable<string> SplitWords(string text)
            => Regex.Split(text.Trim().ToLower(), "\\s+");

        private IEnumerable<byte> MakeOrder(string[] words)
        {
            var orderedIndexedWords = words
                .OrderByDescending(word => word.Length)
                .Select(word => (word: word, index: Match(word)))
                .OrderBy(tuple => tuple.index)
                .SkipWhile(tuple => tuple.index < 0)
                .ToArray();
            if (!IsAllTranslated)
                return new byte[0];
            var prevIndex = -1;
            return orderedIndexedWords.Select(
                tuple => (byte) (prevIndex = GetNextIndex(words, tuple.word, prevIndex + 1)));
        }

        private bool IsAllTranslated
            => _translated.CountSymbols() == _matched.CountSymbols();

        private static int GetNextIndex(string[] words, string word, int startIndex)
        {
            var index = Array.IndexOf(words, word, startIndex);
            return index < 0 ? Array.IndexOf(words, word) : index;
        }

        private int Match(string word)
        {
            if (string.IsNullOrEmpty(word))
                return -1;
            var index = GetUnmatchedIndex(word);
            if (index >= 0)
                _matched = _matched.Substring(0, index) + word + _matched.Substring(index + word.Length);
            else _hidden.Add(word);
            return index;
        }

        private int GetUnmatchedIndex(string word)
        {
            var startIndex = 0;
            int index;
            do
            {
                index = _translated.IndexOfIgnoreCase(word, startIndex);
                if (index < 0)
                    return index;
                startIndex = index + _matched.Substring(index, word.Length).TrimEnd().Length;
            } while (startIndex > index);
            return index;
        }
    }
}