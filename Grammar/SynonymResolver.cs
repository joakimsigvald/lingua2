using Lingua.Core;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Grammar
{
    public class SynonymResolver : ISynonymResolver
    {
        private static readonly IDictionary<string, SynonymIndicator> _indicators 
            = LoadIndicators().ToDictionary(ind => ind.Word);

        public ITranslation Resolve(ITranslation[] candidates, IEnumerable<ITranslation> preceeding, IEnumerable<ITranslation> next)
            => (candidates.Length == 1
                ? null
                : candidates.FirstOrDefault(t => HasIndicators(t, preceeding, next)))
            ?? candidates[0];

        private bool HasIndicators(ITranslation candidate, IEnumerable<ITranslation> preceeding, IEnumerable<ITranslation> next)
        {
            var previousWord = preceeding.LastOrDefault()?.Output.ToLower();
            var nextWords = next.Select(w => w.Output.ToLower()).ToArray();
            return _indicators.TryGetValue(candidate.Output.ToLower(), out var indicator)
                && indicator.Matches(previousWord, nextWords);
        }

        private static IEnumerable<SynonymIndicator> LoadIndicators() 
            => LoaderBase.ReadFile("SynonymIndicators.txt")
            .Select(SynonymIndicator.Create);
    }
}