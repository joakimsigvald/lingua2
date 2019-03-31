using System.Collections.Generic;

namespace Lingua.Core
{
    public interface IGrammar
    {
        ReductionResult Reduce(IList<ITranslation[]> possibilities);
        int Evaluate(ITranslation[] translations);
    }
}