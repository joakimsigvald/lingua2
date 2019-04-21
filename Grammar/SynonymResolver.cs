using Lingua.Core;
using System.Collections.Generic;
using System.Linq;

namespace Lingua.Grammar
{
    public class SynonymResolver : ISynonymResolver
    {
        public ITranslation Resolve(IGrammaton grammaton, IEnumerable<ITranslation> previous)
        {
            if (grammaton.Translations.Length == 1)
                return grammaton.Translations[0];
            return grammaton.Translations.FirstOrDefault(t => HasIndicators(t, previous)) ?? grammaton.Translations.First();
        }

        private bool HasIndicators(ITranslation candidate, IEnumerable<ITranslation> previous)
        {
            var previousWords = previous.Select(t => t.Output.ToLower()).ToArray();
            switch (candidate.Output.ToLower())
            {
                case "om": return previousWords.Any(w => w == "beslut");
                case "fattas": return previousWords.Any(w => w == "handläggning");
                case "av": return previousWords.Any(w => w == "fattas");
                default: return false;
            }
        }
    }
}