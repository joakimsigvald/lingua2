using System.Collections.Generic;

namespace Lingua.Core
{
    public interface IGrammar
    {
        (Translation[] Translations, IReason Reason) Reduce(TranslationTreeNode possibilities);
        Translation[] Arrange(IEnumerable<Translation> translations);
    }
}