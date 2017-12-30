using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Lingua.Core.Extensions;
using Lingua.Grammar;

namespace Lingua.Learning
{
    using Core;

    public static class TargetSelector
    {
        public static TranslationTarget SelectTarget(IList<ITranslation[]> possibilities, string translated)
        {
            if (possibilities == null)
                return null;
            var arrangementAndUntranslated = GetArrangementAndUnmatched(possibilities, translated);
            var arrangement = arrangementAndUntranslated.arrangement;
            return new TranslationTarget
            {
                Arrangement = arrangement,
                IsFullyTranslated = arrangement.Order.Any(),
                Unmatched = arrangementAndUntranslated.unmatched,
                Translations = possibilities.Select((p, i) => p.Single(t => t.Code == arrangement.Code[i])).ToArray()
            };
        }

        private static (Arrangement arrangement, string unmatched) GetArrangementAndUnmatched(IEnumerable<ITranslation[]> possibilities, string translated)
        {
            var orderAndUntranslated = MakeOrder(possibilities, translated);
            return (
                new Arrangement(orderAndUntranslated.code, orderAndUntranslated.order)
                , orderAndUntranslated.untranslated);
        }

        private static (byte[] order, ushort[] code, string untranslated) MakeOrder(IEnumerable<ITranslation[]> possibilities, string translated)
        {
            var filteredPossibilities = FilterPossibilities(possibilities, translated);
            var possibleSequences = filteredPossibilities.Expand();
            return possibleSequences
                .Select(ps => new OrderMaker(translated).GetOrderAndUntranslated(ps))
                .OrderBy(o => o.untranslated.Length)
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
        private string _translated;

        public OrderMaker(string translate) => _translated = translate;

        public (byte[] order, ushort[] codes, string untranslated) GetOrderAndUntranslated(ITranslation[] translations)
        {
            var words = translations.Select(t => t.Output).ToArray();
            var code = translations.Select(t => t.Code).ToArray();
            var order = MakeOrder(words).ToArray();
            return (order, code, TrimWords(_translated));
        }

        private static string TrimWords(string text)
             => Regex.Replace(text.Trim(), "\\s+", " ");

        private IEnumerable<byte> MakeOrder(string[] words)
        {
            var orderedIndexedWords = words
                .OrderByDescending(word => word.Length)
                .Select(word => (word: word, index: Remove(word)))
                .OrderBy(tuple => tuple.index)
                .SkipWhile(tuple => tuple.index < 0)
                .ToArray();
            if (!string.IsNullOrWhiteSpace(_translated))
                yield break;
            var prevIndex = -1;
            foreach (var tuple in orderedIndexedWords)
            {
                yield return (byte)(prevIndex = GetNextIndex(words, tuple.word, prevIndex + 1));
            }
        }

        private static int GetNextIndex(string[] words, string word, int startIndex)
        {
            var index = Array.IndexOf(words, word, startIndex);
            return index < 0 ? Array.IndexOf(words, word) : index;
        }

        private int Remove(string input)
        {
            var index = _translated.IndexOfIgnoreCase(input);
            if (index >= 0)
                _translated = _translated.Remove(index, input.Length);
            return index;
        }
    }
}