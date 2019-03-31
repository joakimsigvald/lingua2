using System.Collections.Generic;

namespace Lingua.Core
{
    public interface IGrammar
    {
        ReductionResult Reduce(IList<ITranslation[]> possibilities);
        ReductionResult Evaluate(ITranslation[] translations);
    }
}