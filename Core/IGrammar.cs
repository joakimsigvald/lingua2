using System.Collections.Generic;

namespace Lingua.Core
{
    public interface IGrammar
    {
        ITranslation[] Reduce(IList<ITranslation[]> possibilities);
    }
}