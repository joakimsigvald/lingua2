using Lingua.Core.Tokens;

namespace Lingua.Core
{
    public interface IThesaurus
    {
        ITranslation[] Translate(Token token);
        bool TryExpand(string word, out string expanded);
    }
}