using System.Collections.Generic;
using System.Linq;

namespace Lingua.Learning
{
    using Core;
    using Core.Tokens;

    public static class CandidateFilter
    {
        public static IEnumerable<Translation> FilterCandidates(
            TranslationTreeNode possibilities
            , IEnumerable<Token> toTokens)
            => possibilities == null
                ? null
                : FilterCandidates(possibilities.Children
                    , string.Join(" ", toTokens.Select(t => t.Value)).ToLower());

        private static IEnumerable<Translation> FilterCandidates(
            ICollection<TranslationTreeNode> candidates
            , string translated)
        {
            if (!candidates.Any())
                return string.IsNullOrWhiteSpace(translated) ? new Translation[0] : null;
            var matchingCandidates = candidates.Where(tn => translated.Contains(tn.Translation.Output.ToLower()))
                .OrderByDescending(tn => tn.Translation.Output.Length)
                .ToArray();
            return matchingCandidates.Append(candidates.First())
                    .Select(tn => FilterPossibilities(tn, translated))
                    .NotNull()
                    .FirstOrDefault();
        }

        private static IEnumerable<Translation> FilterPossibilities(
            TranslationTreeNode possibilities
            , string translated)
            => FilterCandidates(possibilities.Children,
                    Remove(translated, possibilities.Translation.Output))
                ?.Prepend(possibilities.Translation);

        private static string Remove(string translated, string output)
            => string.IsNullOrEmpty(output) ? translated : translated.Replace(output.ToLower(), "");
    }
}