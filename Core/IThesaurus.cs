using Lingua.Core.Tokens;

namespace Lingua.Core
{
    public interface IThesaurus
    {
        Translation[] Translate(Token token);
        bool TryExpand(string word, out string expanded);
    }
}