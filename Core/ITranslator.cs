using System.Collections.Generic;

namespace Lingua.Core
{
    public interface ITranslator
    {
        TranslationResult Translate(string text);
        IList<ITranslation[]> Decompose(string original);
    }
}