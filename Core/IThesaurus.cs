using Lingua.Core.Tokens;

namespace Lingua.Core
{
    public interface IThesaurus
    {
        ITranslation[] Translate(Token token);
    }
}