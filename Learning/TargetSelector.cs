using System;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Learning
{
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
                return _translated.All(c => c < FirstSymbol) ? new Translation[0] : null;
            var matchingCandidates = candidates.Where(tn => _translated.Contains(tn.Translation.Output.ToLower()))
                .OrderByDescending(tn => tn.Translation.Output.Length)
                .ToArray();
            return matchingCandidates.Append(candidates.First())
                .Select(FilterPossibilities)
                .NotNull()
                .FirstOrDefault();
        }

        private IEnumerable<Translation> FilterPossibilities(TranslationTreeNode possibilities)
        {
            TryReplaceWithNextPosition(possibilities.Translation.Output);
            return SelectTranslations(possibilities.Children)?.Prepend(possibilities.Translation);
        }

        private void TryReplaceWithNextPosition(string output)
        {
            if (_nextPosition >= Space - 1)
                throw new Exception("Testcase too long, ran out of positions to assign");
            _nextReplacement += (char) _nextPosition++;
            if (string.IsNullOrEmpty(output))
                return;
            _translated = ReplaceWithNextPosition(ShortenEllipsisToFit(output));
            _nextReplacement = "";
        }

        private string ReplaceWithNextPosition(string output) 
            => _translated.ReplaceFirst(output.ToLower(), _nextReplacement);

        private string ShortenEllipsisToFit(string output)
            => output == "..." && !_translated.Contains(output) ? ".." : output;
    }
}