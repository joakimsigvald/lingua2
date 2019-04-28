using System.Collections.Generic;

namespace Lingua.Core
{
    public interface ICapitalizer
    {
        IEnumerable<ITranslation> Capitalize(ITranslation[] arrangedTranslations, IGrammaton[] original);
    }
}