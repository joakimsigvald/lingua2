using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Lingua.Core.WordClasses;

namespace Lingua.Learning
{
    using Core.Tokens;
    using Core.Extensions;
    using Grammar;
    using Core;

    public class TargetSelector
    {
        private const char FirstSymbol = '!';
        private const char Space = (char) 32;
        private string _translated;
        private string _nextReplacement = "";
        private int _nextPosition = 1;
        private bool _previousIsAbbreviation;
        private List<Translation> _untranslated = new List<Translation>();

        public static TranslationTarget SelectTarget(
            TranslationTreeNode possibilities
            , string translated)
            => possibilities == null
                ? null
                : new TargetSelector(translated).SelectTarget(possibilities);

        private TargetSelector(string translated) => _translated = translated.ToLower();

        private TranslationTarget SelectTarget(TranslationTreeNode possibilities)
        {
            var translations = SelectTranslations(possibilities.Children)?.ToArray();
            return new TranslationTarget
            {
                Translations = translations,
                Arrangement = translations == null
                    ? null
                    : new Arrangement(Encoder.Encode(translations).ToArray(), GetOrder())
            };
        }

        private byte[] GetOrder()
            => _translated
                .Where(c => c < Space)
                .Select(c => (byte) (c - 1))
                .ToArray();

        private IEnumerable<Translation> SelectTranslations(
            ICollection<TranslationTreeNode> candidates)
        {
            if (!candidates.Any())
                return _translated.All(c => c < FirstSymbol)
                    ? new Translation[0]
                    : throw new MissingTranslations(_translated, _untranslated);
            var matchingCandidates = candidates.Where(tn => _translated.Contains(tn.Translation.Output.ToLower()))
                .OrderByDescending(tn => tn.Translation.Output.Length)
                .ToArray();
            return matchingCandidates.Append(candidates.First())
                .Select(FilterPossibilities)
                .FirstOrDefault();
        }

        private IEnumerable<Translation> FilterPossibilities(TranslationTreeNode possibilities)
        {
            var translation = possibilities.Translation;
            TryReplaceWithNextPosition(translation);
            _previousIsAbbreviation = translation.From is Abbreviation;
            return SelectTranslations(possibilities.Children)?.Prepend(translation);
        }

        private void TryReplaceWithNextPosition(Translation translation)
        {
            var output = translation.Output;
            if (_nextPosition >= Space - 1)
                throw new Exception("Testcase too long, ran out of positions to assign");
            if (_previousIsAbbreviation && output == ".")
                return;
            _nextReplacement += (char) _nextPosition++;
            if (string.IsNullOrEmpty(output))
                return;
            var foundMatch = ReplaceWithNextPosition(ShortenEllipsisToFit(output), out _translated);
            if (!foundMatch && !string.IsNullOrEmpty(output) && translation.From is Unclassified)
                _untranslated.Add(translation);
            _nextReplacement = "";
        }

        private bool ReplaceWithNextPosition(string output, out string updated)
            => Replace($" {output} ", $" {_nextReplacement} ", out updated)
               || Replace($"{output} ", $"{_nextReplacement} ", out updated)
               || Replace($" {output} ", $" {_nextReplacement}", out updated)
               || Replace($"{output}", $"{_nextReplacement}", out updated);

        private bool Replace(string output, string replacement, out string updated) 
            => _translated.ReplaceFirst(output.ToLower(), replacement, out updated);

        private string ShortenEllipsisToFit(string output)
            => output == "..." && !_translated.Contains(output) ? ".." : output;
    }
}