using System.Collections.Generic;
using System.Linq;

namespace Lingua.Testing
{
    using Core;
    using Core.Tokens;

    public static class CandidateFilter
    {
        public static IEnumerable<Translation[]> FilterCandidates(ICollection<Translation[]> candidates,
            IReadOnlyList<Token> toTokens)
            => candidates == null || candidates.Count != toTokens.Count
                ? candidates
                : candidates.Select((c, i) => FilterTranslations(c, toTokens[i]).ToArray());

        private static IEnumerable<Translation> FilterTranslations(
            IEnumerable<Translation> translations,
            Token toToken)
            => translations.Where(t => t.Output == toToken.Value);
    }
}