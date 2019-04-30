using Lingua.Core;
using System.Collections.Generic;

namespace Lingua.Translation
{
    public interface ITranslator
    {
        TranslationResult Translate(string text);
        IList<IGrammaton[]> Decompose(string original);
    }
}