using System.Collections.Generic;

namespace Lingua.Core
{
    public interface ISynonymResolver
    {
        ITranslation Resolve(IGrammaton subject, IEnumerable<ITranslation> previous);
    }
}