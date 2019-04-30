using Lingua.Core;
using System.Collections.Generic;

namespace Lingua.Translation
{
    public interface ICapitalizer
    {
        IEnumerable<ITranslation> Capitalize(ITranslation[] arrangedTranslations, IGrammaton[] original);
    }
}