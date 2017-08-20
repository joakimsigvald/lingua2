using Lingua.Core.Tokens;

namespace Lingua.Core
{
    public interface IThesaurus
    {
        Translation[] Translate(Token token);
    }
}