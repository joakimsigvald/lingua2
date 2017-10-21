namespace Lingua.Core
{
    public interface ITranslator
    {
        TranslationResult Translate(string text);
    }
}
