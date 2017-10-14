using System;
using System.Collections.Generic;

namespace Lingua.Core
{
    public interface IGrammar
    {
        (IEnumerable<Translation> Translations, IReason Reason) Reduce(TranslationTreeNode possibilities);
        IEnumerable<Translation> Arrange(IEnumerable<Translation> translations);
    }
}