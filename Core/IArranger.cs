using System.Collections.Generic;

namespace Lingua.Core
{
    public interface IArranger
    {
        IGrammaton[] Arrange(IEnumerable<IGrammaton> grammatons);
    }
}