using Lingua.Core;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Grammar
{
    public class SynonymResolver : ISynonymResolver
    {
        public ITranslation Resolve(ITranslation[] candidates, IEnumerable<ITranslation> preceeding, IEnumerable<ITranslation> next) 
            => (candidates.Length == 1
                ? null
                : candidates.FirstOrDefault(t => HasIndicators(t, preceeding, next)))
            ?? candidates[0];

        private bool HasIndicators(ITranslation candidate, IEnumerable<ITranslation> preceeding, IEnumerable<ITranslation> next)
        {
            var previousWord = preceeding.LastOrDefault()?.Output.ToLower();
            var nextWords = next.Select(w => w.Output.ToLower()).ToArray();
            //var previousWords = previous.Select(t => t.Output.ToLower()).ToArray();
            //var followingWords = following.Select(t => t.Output.ToLower()).ToArray();
            return candidate.Output.ToLower() switch
            {
                "om" => previousWord == "beslut",
                "fattas" => previousWord == "handläggning",
                "av" => previousWord == "fattas",
                "detta" => previousWord == "trots",
                "vad" => nextWords.Any(w => w == "gäller"),
                _ => false
            };
        }
    }
}