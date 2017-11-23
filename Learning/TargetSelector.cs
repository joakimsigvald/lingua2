using System;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Learning
{
    using Core;
    using Core.Tokens;

    public class TargetSelector
    {
        private const char FirstSymbol = '!';
        private const char Space = (char)32;
        private string _translated;
        private int _nextPosition = 1;

        public static TranslationTarget SelectTarget(
            TranslationTreeNode possibilities
            , IList<Token> toTokens) 
            => possibilities == null 
            ? null 
            : new TargetSelector(toTokens).SelectTarget(possibilities);

        private TargetSelector(ICollection<Token> toTokens)
        {
            if (toTokens.Count >= Space)
                throw new NotImplementedException($"Text to long. Can only assign {Space - 1} positions. Need to revise current algorithm");
            _translated = string.Join(" ", toTokens.Select(t => t.Value)).ToLower();
        }

        private TranslationTarget SelectTarget(TranslationTreeNode possibilities)
        {
            var translations = SelectTranslations(possibilities.Children);
            return new TranslationTarget
            {
                Translations = translations?.ToArray(),
                Order = GetOrder()
            };
        }

        private byte[] GetOrder()
            => _translated
            .Where(c => c < Space)
            .Select(c => (byte)c)
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
            ReplaceWithNextPosition(possibilities.Translation.Output);
            return SelectTranslations(possibilities.Children)?.Prepend(possibilities.Translation);
        }

        private void ReplaceWithNextPosition(string output)
        {
            if (!string.IsNullOrEmpty(output))
               _translated = _translated.Replace(output.ToLower(), $"{(char) _nextPosition++}");
        }
    }
}