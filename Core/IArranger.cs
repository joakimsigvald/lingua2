using System.Collections.Generic;

namespace Lingua.Core
{
    public interface IArranger
    {
        ITranslation[] Arrange(IEnumerable<ITranslation> translations);
    }
}