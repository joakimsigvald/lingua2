using System.Collections.Generic;

namespace Lingua.Core
{
    public interface IGrammar
    {
        (Translation[] Translations, IReason Reason) Reduce(IList<Translation[]> possibilities);
        IEnumerable<Translation> Arrange(IEnumerable<Translation> translations);
    }
}