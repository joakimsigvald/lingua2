using Lingua.Core.Tokens;

namespace Lingua.Core
{
    public interface IGrammaton
    {
        ITranslation[] Translations { get; }
        ushort Code { get; }
        byte WordCount { get; }
        string Input { get; }
        bool IsCapitalized { get; }
    }
}