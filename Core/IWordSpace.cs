using System.Collections.Generic;

namespace Lingua.Core
{
    public interface IWordSpace : IEnumerable<ITranslationCandidate>
    {
        IGrammatonCandidate[] GrammatonCandidates { get; }
    }
}