using System.Collections.Generic;

namespace Lingua.Core
{
    public interface IGrammar
    {
        (ITranslation[] Translations, IReason Reason) Reduce(IList<ITranslation[]> possibilities);
        IEnumerable<ITranslation> Arrange(IEnumerable<ITranslation> translations);
    }
}