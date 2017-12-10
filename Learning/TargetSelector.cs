using Lingua.Core.WordClasses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Learning
{
    using Core;
    using Core.Extensions;
    using Core.Tokens;
    using Grammar;

    public class TargetSelector
    {
        private const char FirstSymbol = '!';
        private const char Space = (char) 32;
        private string _translated;
        private string _nextReplacement = "";
        private string _unmatched = "";
        private int _nextPosition = 1;
        private bool _previousIsAbbreviation;

        public static TranslationTarget SelectTarget(
            TranslationTreeNode possibilities
            , string translated)
        {
            translated = translated.ToLower();
            if (possibilities == null)
                return null;
            var target = new TargetSelector(translated).SelectFirstTarget(possibilities);
            return target.IsFullyTranslated && target.Arrangement.IsInOrder
                ? target
                : SelectBestTarget(possibilities, translated)
                  ?? throw new MissingTranslations(translated, GetUntranslated(possibilities).ToList());
        }

        private static IEnumerable<Translation> GetUntranslated(TranslationTreeNode possibilities)
        {
            while (possibilities.Children.Any())
            {
                if (possibilities.Translation.From is Unclassified)
                    yield return possibilities.Translation;
                possibilities = possibilities.Children.First();
            }
        }

        private static TranslationTarget SelectBestTarget(TranslationTreeNode possibilities, string translated)
        {
            var candidateSequences = GetCandidateSequences(possibilities.Children, translated).ToArray();
            var targetCandidates = candidateSequences
                .Select(sequence => GenerateTarget(sequence, translated))
                .Where(tt => tt.IsFullyTranslated)
                .OrderBy(target => target.Unmatched.Length)
                .ThenBy(target => target.Arrangement.Deviation)
                .ToArray();
            return targetCandidates.First();
        }

        private static TranslationTarget GenerateTarget(IEnumerable<Translation> sequence, string translated)
            => new TargetSelector(translated).GenerateTarget(sequence.ToArray());

        private static IEnumerable<IEnumerable<Translation>> GetCandidateSequences(
            IList<TranslationTreeNode> candidates, string translated)
        {
            if (!candidates.Any())
                return new[] {new Translation[0]};
            var orderedCandidates = candidates.OrderBy(cand => cand.Translation.Output.Length).ToList();
            var firstCandidates = orderedCandidates.Where(tn => MatchesTranslated(translated, tn.Translation.Output))
                .OrderByDescending(tn => tn.Translation.Output.Length)
                .ToList();
            if (!firstCandidates.Any())
                firstCandidates.Add(orderedCandidates.First());
            return firstCandidates
                .SelectMany(fc => GetCandidateSequences(fc.Children, translated)
                    .Select(sequence => sequence.Prepend(fc.Translation)));
        }

        private TargetSelector(string translated) => _translated = translated;

        private TranslationTarget SelectFirstTarget(TranslationTreeNode possibilities)
        {
            var translations = SelectTranslations(possibilities.Children)?.ToArray();
            return CreateTarget(translations);
        }

        private TranslationTarget GenerateTarget(Translation[] translations)
        {
            translations.ForEach(TryReplaceWithNextPosition);
            return CreateTarget(translations);
        }

        private TranslationTarget CreateTarget(Translation[] translations)
            => new TranslationTarget
            {
                Translations = translations,
                IsFullyTranslated = IsFullyTranslated,
                Unmatched = _unmatched,
                Arrangement = translations == null
                    ? null
                    : new Arrangement(Encoder.Encode(translations).ToArray(), GetOrder())
            };

        private byte[] GetOrder()
            => _translated
                .Where(c => c < Space)
                .Select(c => (byte) (c - 1))
                .ToArray();

        private IEnumerable<Translation> SelectTranslations(
            ICollection<TranslationTreeNode> candidates)
        {
            if (!candidates.Any())
                return IsFullyTranslated
                    ? new Translation[0]
                    : null;
            var matchingCandidates = candidates.Where(tn => MatchesTranslated(_translated, tn.Translation.Output))
                .OrderByDescending(tn => tn.Translation.Output.Length);
            return matchingCandidates.Concat(candidates)
                .Select(FilterPossibilities)
                .FirstOrDefault();
        }

        private bool IsFullyTranslated => _translated.All(c => c < FirstSymbol);

        private static bool MatchesTranslated(string translated, string output)
            => string.IsNullOrEmpty(output) || translated.Contains(output.ToLower());

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
            var replaced = ReplaceWithNextPosition(ShortenEllipsisToFit(output), out _translated);
            if (replaced)
                _unmatched += output;
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