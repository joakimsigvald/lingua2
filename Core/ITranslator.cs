namespace Lingua.Core
{
    public interface ITranslator
    {
        (string translation, IReason reason) Translate(string text);
    }
}
