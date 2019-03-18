using System.Collections.Generic;
using Lingua.Core;

namespace Lingua.Core
{
    public interface IArranger
    {
        ITranslation[] Arrange(IEnumerable<ITranslation> translations);
    }
}