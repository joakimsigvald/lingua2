namespace Lingua.Core
{
    public interface IGrammaton
    {
        ITranslation[] Translations { get; }
        ushort Code { get; }
        byte WordCount { get; }
    }
}