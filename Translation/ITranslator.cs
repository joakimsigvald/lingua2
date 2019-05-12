using Lingua.Core;

namespace Lingua.Translation
{
    public interface ITranslator
    {
        TranslationResult Translate(string text);
        IDecomposition Decompose(string original);
    }
}