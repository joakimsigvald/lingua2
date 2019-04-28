using System.Collections.Generic;

namespace Lingua.Core
{
    public interface ISynonymResolver
    {
        ITranslation Resolve(ITranslation[] candidates, IEnumerable<ITranslation> preceeding, IEnumerable<ITranslation> next);
    }
}