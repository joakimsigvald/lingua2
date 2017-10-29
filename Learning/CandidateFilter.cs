using System.Collections.Generic;
using System.Linq;

namespace Lingua.Learning
{
    using Core;
    using Core.Tokens;

    public static class CandidateFilter
    {
        public static IEnumerable<Translation[]> FilterCandidates(
            ICollection<Translation[]> candidates
            , Token[] toTokens)
            => candidates == null || candidates.Count != toTokens.Length
                ? candidates
                : candidates.Select((c, i) => FilterTranslations(c, toTokens, i).ToArray());

        private static IEnumerable<Translation> FilterTranslations(
            IEnumerable<Translation> translations,
            Token[] toTokens, int index)
            => translations.Where(t => Matches(t, toTokens, index));

        private static bool Matches(
            Translation translation,
            Token[] toTokens, int index)
            => translation.Output == string.Join(" ", toTokens.Skip(index).Take(translation.WordCount).Select(t => t.Value));
    }
}