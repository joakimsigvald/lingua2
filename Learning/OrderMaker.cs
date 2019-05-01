using System;
using System.Collections.Generic;
using System.Linq;
using Lingua.Core;
using Lingua.Core.Extensions;

namespace Lingua.Learning
{
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

        public (ITranslation[] translations, byte[] order, string unmatched, string hidden) 
            SelectAndOrderTranslations(ITranslation[] translations)
        {
            var order = MakeOrder(translations).ToArray();
            return (translations, order, Unmatched, string.Join(",", _hidden));
        }

        private string Unmatched
        {
            get
            {
                var unmatchedIndices = _matched.Select((c, i) => (c, i))
                    .Where(tuple => tuple.c == Tab)
                    .Select(tuple => tuple.i);
                var unmatched = new string(_translated.Select((c, i) => (c, i))
                    .Join(unmatchedIndices, tuple => tuple.i, i => i, (tuple, i) => tuple.c)
                    .ToArray());
                var words = SplitWords(unmatched);
                return string.Join(",", words);
            }
        }

        private static IEnumerable<string> SplitWords(string text)
            => text.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

        private byte[] MakeOrder(ITranslation[] translations)
        {
            var words = translations.Select(t => t.Output).ToArray();
            var remainingWords = words.Cast<string?>().ToList();
            var orderedIndexedWords = words
                .OrderByDescending(word => word.Length)
                .Select(word => (word, index: Match(word)))
                .OrderBy(tuple => tuple.index)
                .SkipWhile(tuple => tuple.index < 0)
                .ToArray();
            if (!IsAllTranslated)
                return new byte[0];
            var prevIndex = -1;
            var order = orderedIndexedWords.Select(
                tuple => (byte) (prevIndex = MatchNextWord(remainingWords, tuple.word, prevIndex + 1)))
                .ToArray();
            return order;
        }

        private bool IsAllTranslated
            => _translated.CountSymbols() == _matched.CountSymbols();

        private static int MatchNextWord(List<string?> remainingWords, string word, int startIndex)
        {
            var index = GetNextIndex(remainingWords, word, startIndex);
            if (index >= 0)
                remainingWords[index] = null;
            return index;
        }

        private static int GetNextIndex(List<string?> remainingWords, string word, int startIndex)
        {
            if (startIndex == remainingWords.Count)
                return remainingWords.LastIndexOf(word);
            if (remainingWords[startIndex] == word)
                return startIndex;
            var horizon = Math.Min(startIndex, remainingWords.Count - startIndex);
            if (horizon > 1)
                for (int offset = 1; offset < horizon; offset++)
                {
                    if (remainingWords[startIndex - offset - 1] == word)
                        return startIndex - offset - 1;
                    if (remainingWords[startIndex + offset] == word)
                        return startIndex + offset;
                }
            return horizon >= startIndex
                ? remainingWords.IndexOf(word, startIndex + horizon)
                : remainingWords.LastIndexOf(word, startIndex - horizon - 1);
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